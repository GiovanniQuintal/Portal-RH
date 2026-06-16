using Microsoft.EntityFrameworkCore;
using RRHH_ARG.Models;
using RRHH_ARG.Data;

namespace RRHH_ARG.Services
{
    public class VacacionService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;

        public VacacionService(IDbContextFactory<ApplicationDbContext> dbFactory)
        {
            _dbFactory = dbFactory;
        }

        // 1. OBTENER SALDO TOTAL DISPONIBLE
        public async Task<decimal> GetSaldoDisponibleAsync(int empleadoId)
        {
            using var context = _dbFactory.CreateDbContext();
            var hoy = DateTime.Now;

            // Sumamos solo lo que no ha caducado y es vigente
            return await context.Set<KardexVacacion>()
                .Where(k => k.EmpleadoId == empleadoId && k.IsVigente && k.FechaCaducidad >= hoy)
                .SumAsync(k => k.DiasDerecho - k.DiasTomados);
        }

        // 2. GUARDAR SOLICITUD CON LÓGICA PEPS
        // ==========================================================
        // GUARDAR SOLICITUD (INCIDENCIA OFICIAL + LÓGICA PEPS)
        // ==========================================================
        public async Task RegistrarVacacionesAsync(VacacionSolicitudDTO dto)
        {
            using var context = _dbFactory.CreateDbContext();
            using var transaction = await context.Database.BeginTransactionAsync();

            try
            {
                // ------------------------------------------------------------------
                // PASO 1: CREAR LA INCIDENCIA OFICIAL EN EL ERP
                // ------------------------------------------------------------------

                // A. Crear Detalle
                int nextDinId = await context.DetalleIncidencium.AnyAsync() ? await context.DetalleIncidencium.MaxAsync(d => d.DinId) + 1 : 1;

                var detalle = new DetalleIncidencium
                {
                    DinId = nextDinId,
                    TinId = 2, // <--- OJO: AQUÍ DEBES PONER EL ID QUE CORRESPONDE A "VACACIONES" EN TU CATÁLOGO DE TIPOS
                    DinCantidad = dto.DiasVacaciones,
                    DinDescripcion = $"Vacaciones del {dto.FechaInicial.ToShortDateString()} al {dto.FechaFinal.ToShortDateString()}"
                };
                context.DetalleIncidencium.Add(detalle);
                await context.SaveChangesAsync(); // Guardamos para asegurar el ID

                // B. Crear Incidencia
                int nextIncId = await context.Incidencium.AnyAsync() ? await context.Incidencium.MaxAsync(i => i.IncId) + 1 : 1;

                var incidencia = new Incidencium
                {
                    IncId = nextIncId,
                    DinId = detalle.DinId,
                    IncFecha = DateTime.Now // Fecha en la que se captura
                };
                context.Incidencium.Add(incidencia);
                await context.SaveChangesAsync();

                // C. Crear Historial (Para enlazarlo al empleado)
                int nextHinId = await context.HistorialIncidencium.AnyAsync() ? await context.HistorialIncidencium.MaxAsync(h => h.HinId) + 1 : 1;

                var historial = new HistorialIncidencium
                {
                    HinId = nextHinId,
                    IncId = incidencia.IncId,
                    EmpId = dto.EmpleadoId
                };
                context.HistorialIncidencium.Add(historial);
                await context.SaveChangesAsync();


                // ------------------------------------------------------------------
                // PASO 2: DESCONTAR DEL KARDEX (LÓGICA PEPS)
                // ------------------------------------------------------------------
                decimal diasRestantesPorAplicar = dto.DiasVacaciones;

                var cubetas = await context.Set<KardexVacacion>()
                    .Where(k => k.EmpleadoId == dto.EmpleadoId && k.IsVigente && (k.DiasDerecho - k.DiasTomados) > 0)
                    .OrderBy(k => k.FechaInicio)
                    .ToListAsync();

                foreach (var kubeta in cubetas)
                {
                    if (diasRestantesPorAplicar <= 0) break;

                    decimal disponibleEnEstaKubeta = kubeta.DiasDerecho - kubeta.DiasTomados;
                    decimal aDescontar = Math.Min(disponibleEnEstaKubeta, diasRestantesPorAplicar);

                    // Insertar en vacacion_tomada con el Folio de la Incidencia (IncId)
                    var registro = new VacacionTomada
                    {
                        KardexVacacionId = kubeta.Id,
                        EmpleadoId = dto.EmpleadoId,
                        IncidenciaId = incidencia.IncId, // <--- GUARDAMOS EL FOLIO AQUÍ
                        FechaInicial = dto.FechaInicial,
                        FechaFinal = dto.FechaFinal,
                        FechaPago = dto.FechaPago,
                        DiasDescanso = dto.DiasDescanso,
                        DiasVacaciones = aDescontar,
                        PrimaVacacionalDias = aDescontar * 0.25m,
                        FechaRegistro = DateTime.Now
                    };
                    context.Set<VacacionTomada>().Add(registro);

                    // Actualizar la cubeta
                    kubeta.DiasTomados += aDescontar;
                    context.Set<KardexVacacion>().Update(kubeta);

                    diasRestantesPorAplicar -= aDescontar;
                }

                await context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // ==========================================================
        // OBTENER KARDEX COMPLETO (MAESTRO - DETALLE Y AJUSTE)
        // ==========================================================
        public async Task<KardexResultDTO> GetKardexEmpleadoAsync(int empleadoId)
        {
            using var context = _dbFactory.CreateDbContext();

            // 1. LEER EL AJUSTE HISTÓRICO DEL EMPLEADO
            var empleado = await context.Set<Empleado>()
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.EmpId == empleadoId);

            int ajusteGuardado = empleado != null ? empleado.VacTomadasIniciales : 0;

            // 2. LEER LAS FILAS DEL KARDEX
            var kardexDb = await context.Set<KardexVacacion>()
                .Where(k => k.EmpleadoId == empleadoId)
                .OrderBy(k => k.NumeroAniversario)
                .ToListAsync();

            var detallesDb = await context.Set<VacacionTomada>()
                .Where(v => v.EmpleadoId == empleadoId)
                .ToListAsync();

            var lista = new List<KardexRowDTO>();

            foreach (var k in kardexDb)
            {
                var row = new KardexRowDTO
                {
                    KardexId = k.Id,
                    Aniversario = k.NumeroAniversario,
                    Periodo = k.Periodo,
                    Derecho = k.DiasDerecho,
                    Tomados = k.DiasTomados,
                    // BORRADO: IncidenciaId = k.IncidenciaId, <--- Aquí estaba el error
                    FechaInicio = k.FechaInicio,
                    FechaCaducidad = k.FechaCaducidad,
                    IsVigente = k.IsVigente,
                    Detalles = detallesDb
                        .Where(d => d.KardexVacacionId == k.Id)
                        .Select(d => new KardexDetalleDTO
                        {
                            IncidenciaId = d.IncidenciaId, // <--- COLOCADO AQUÍ (en el Detalle)
                            FechaInicio = d.FechaInicial,
                            FechaFin = d.FechaFinal,
                            DiasDescontados = d.DiasVacaciones,
                            FechaRegistro = d.FechaRegistro
                        }).ToList()
                };
                lista.Add(row);
            }

            // 3. DEVOLVER EL PAQUETE COMPLETO
            return new KardexResultDTO
            {
                Filas = lista,
                AjusteInicial = ajusteGuardado
            };
        }



        // ==========================================================
        // TABLA DE LEY (VACACIONES DIGNAS MÉXICO)
        // ==========================================================
        // ==========================================================
        // TABLA DE LEY (VACACIONES DIGNAS MÉXICO Y LEY ANTERIOR)
        // ==========================================================
        public decimal CalcularDiasPorLey(int anioCumplido, DateTime fechaAniversario)
        {
            // La reforma de Vacaciones Dignas entró en vigor el 1 de enero de 2023
            bool esLeyNueva = fechaAniversario >= new DateTime(2023, 1, 1);

            if (esLeyNueva)
            {
                // NUEVA TABLA (2023 en adelante)
                if (anioCumplido == 1) return 12;
                if (anioCumplido == 2) return 14;
                if (anioCumplido == 3) return 16;
                if (anioCumplido == 4) return 18;
                if (anioCumplido == 5) return 20;
                if (anioCumplido >= 6 && anioCumplido <= 10) return 22;
                if (anioCumplido >= 11 && anioCumplido <= 15) return 24;
                if (anioCumplido >= 16 && anioCumplido <= 20) return 26;
                if (anioCumplido >= 21 && anioCumplido <= 25) return 28;
                return 30; // 26 años o más
            }
            else
            {
                // TABLA ANTIGUA (Hasta el 31 de diciembre de 2022)
                if (anioCumplido == 1) return 6;
                if (anioCumplido == 2) return 8;
                if (anioCumplido == 3) return 10;
                if (anioCumplido == 4) return 12;
                if (anioCumplido >= 5 && anioCumplido <= 9) return 14;
                if (anioCumplido >= 10 && anioCumplido <= 14) return 16;
                if (anioCumplido >= 15 && anioCumplido <= 19) return 18;
                if (anioCumplido >= 20 && anioCumplido <= 24) return 20;
                return 22; // 25 años o más en la ley antigua
            }
        }

        // ==========================================================
        // MOTOR INTELIGENTE DE GENERACIÓN Y AUTO-REPARACIÓN
        // ==========================================================
        public async Task GenerarKardexAutomaticoAsync(int empleadoId)
        {
            using var context = _dbFactory.CreateDbContext();

            var empleado = await context.Set<Empleado>().AsNoTracking().FirstOrDefaultAsync(e => e.EmpId == empleadoId);
            DateTime? fechaIngresoNullable = empleado?.EmpFechaContratacion ?? empleado?.EmpFechaRegistro;
            if (fechaIngresoNullable == null) return;

            DateTime fechaIngreso = fechaIngresoNullable.Value.Date;
            DateTime hoy = DateTime.Now.Date;

            int anosCumplidos = hoy.Year - fechaIngreso.Year;
            if (fechaIngreso > hoy.AddYears(-anosCumplidos)) anosCumplidos--;

            // Si es menor a 1 año, borramos cualquier kardex viejo (por si RH redujo la antigüedad a meses)
            if (anosCumplidos < 1)
            {
                var kardexErroneos = await context.Set<KardexVacacion>().Where(k => k.EmpleadoId == empleadoId).ToListAsync();
                if (kardexErroneos.Any())
                {
                    context.Set<KardexVacacion>().RemoveRange(kardexErroneos);
                    await context.SaveChangesAsync();
                }
                return;
            }

            // Traemos lo que ya existe en BD para analizarlo
            var kardexExistentes = await context.Set<KardexVacacion>()
                .Where(k => k.EmpleadoId == empleadoId)
                .ToListAsync();

            var nuevosRegistros = new List<KardexVacacion>();
            bool huboCambiosEnFechas = false;

            // 1. REPARAR Y CREAR
            for (int i = 1; i <= anosCumplidos; i++)
            {
                DateTime fechaInicioPeriodo = fechaIngreso.AddYears(i - 1);
                DateTime fechaFinPeriodo = fechaIngreso.AddYears(i);
                string periodoCorrecto = $"{fechaInicioPeriodo.Year} - {fechaFinPeriodo.Year}";
                DateTime caducidadCorrecta = fechaFinPeriodo.AddMonths(18);

                // ---> LÍNEA MODIFICADA: Ahora evalúa en qué año cayó este aniversario
                decimal derechoCorrecto = CalcularDiasPorLey(i, fechaFinPeriodo);

                

                var existente = kardexExistentes.FirstOrDefault(k => k.NumeroAniversario == i);

                if (existente != null)
                {
                    // Si RH cambió la fecha de ingreso, actualizamos las fechas de esta fila
                    if (existente.FechaInicio != fechaFinPeriodo || existente.Periodo != periodoCorrecto)
                    {
                        existente.FechaInicio = fechaFinPeriodo;
                        existente.Periodo = periodoCorrecto;
                        existente.FechaCaducidad = caducidadCorrecta;
                        existente.DiasDerecho = derechoCorrecto;
                        context.Set<KardexVacacion>().Update(existente);
                        huboCambiosEnFechas = true;
                    }
                }
                else
                {
                    // No existe, lo creamos
                    nuevosRegistros.Add(new KardexVacacion
                    {
                        EmpleadoId = empleadoId,
                        NumeroAniversario = i,
                        Periodo = periodoCorrecto,
                        DiasDerecho = derechoCorrecto,
                        DiasTomados = 0,
                        FechaInicio = fechaFinPeriodo,
                        FechaCaducidad = caducidadCorrecta,
                        IsVigente = true
                    });
                    huboCambiosEnFechas = true;
                }
            }

            // 2. ELIMINAR SOBRANTES (Si RH le había puesto 5 años y lo corrigió a 2 años)
            var kardexSobrantes = kardexExistentes.Where(k => k.NumeroAniversario > anosCumplidos).ToList();
            if (kardexSobrantes.Any())
            {
                context.Set<KardexVacacion>().RemoveRange(kardexSobrantes);
                huboCambiosEnFechas = true;
            }

            if (nuevosRegistros.Any()) context.Set<KardexVacacion>().AddRange(nuevosRegistros);

            await context.SaveChangesAsync();

            // 3. RECALCULAR PEPS SI SE MOVIERON LAS CUBETAS
            // Si se movió la antigüedad, volvemos a vaciar el saldo en las cubetas correctas
            if (huboCambiosEnFechas)
            {
                int ajusteGuardado = empleado.VacTomadasIniciales;
                await GuardarAjusteYRecalcularAsync(empleadoId, ajusteGuardado);
            }
        }

        // ==========================================================
        // GUARDAR AJUSTE HISTÓRICO Y RECALCULAR PEPS
        // ==========================================================
        public async Task GuardarAjusteYRecalcularAsync(int empleadoId, int diasAjuste)
        {
            using var context = _dbFactory.CreateDbContext();
            using var transaction = await context.Database.BeginTransactionAsync();

            try
            {
                // 1. Guardar el ajuste histórico persistente en el empleado
                var emp = await context.Set<Empleado>().FirstOrDefaultAsync(e => e.EmpId == empleadoId);
                if (emp != null) emp.VacTomadasIniciales = diasAjuste;

                // 2. Sumar TODAS las vacaciones nuevas registradas por el formulario
                var diasNuevosTomados = await context.Set<VacacionTomada>()
                    .Where(v => v.EmpleadoId == empleadoId)
                    .SumAsync(v => v.DiasVacaciones);

                // El total real que el empleado se ha gastado en su vida
                decimal totalConsumido = diasAjuste + diasNuevosTomados;

                // 3. Traer las cubetas del Kardex
                var kardexDb = await context.Set<KardexVacacion>()
                    .Where(k => k.EmpleadoId == empleadoId)
                    .OrderBy(k => k.NumeroAniversario)
                    .ToListAsync();

                // 4. Distribuir el total consumido desde el año más viejo al más nuevo (PEPS)
                foreach (var kubeta in kardexDb)
                {
                    if (totalConsumido <= 0)
                    {
                        kubeta.DiasTomados = 0; // Si ya no hay saldo consumido, la cubeta queda intacta
                        continue;
                    }

                    decimal aDescontar = Math.Min(kubeta.DiasDerecho, totalConsumido);
                    kubeta.DiasTomados = aDescontar;
                    totalConsumido -= aDescontar;
                }

                context.Set<KardexVacacion>().UpdateRange(kardexDb);
                await context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }


}