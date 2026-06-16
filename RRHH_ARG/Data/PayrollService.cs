using ClosedXML.Excel;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.EntityFrameworkCore;
using RRHH_ARG.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace RRHH_ARG.Data
{
    public class PayrollService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;

        public PayrollService(IDbContextFactory<ApplicationDbContext> dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public async Task<Nomina> GetPeriodoAsync(int anio, int mes, int quincena)
        {
            using var context = _dbFactory.CreateDbContext();
            string periodoCalculado = $"P{((mes - 1) * 2) + quincena}";

            return await context.Nominas
                .FirstOrDefaultAsync(p => p.Anno == anio && p.Mes == mes && p.Periodo == periodoCalculado);
        }

        public async Task<List<string>> GetPeriodosGeneradosAsync(int anio)
        {
            using var context = _dbFactory.CreateDbContext();
            // Busca en la base de datos y devuelve solo los nombres de los periodos (ej. "P1", "P2")
            return await context.Nominas
                .Where(n => n.Anno == anio)
                .Select(n => n.Periodo)
                .ToListAsync();
        }

        public async Task ToggleBloqueoAsync(int nominaId, bool bloquear)
        {
            using var context = _dbFactory.CreateDbContext();
            var periodo = await context.Nominas.FindAsync(nominaId);
            if (periodo != null)
            {
                periodo.Bloqueado = bloquear;
                await context.SaveChangesAsync();
            }
        }

        public async Task<Nomina> GenerarNuevaNominaAsync(int anio, int mes, int quincena)
        {
            using var context = _dbFactory.CreateDbContext();

            DateTime fechaInicio = new DateTime(anio, mes, quincena == 1 ? 1 : 16);
            int diasDelMes = DateTime.DaysInMonth(anio, mes);
            DateTime fechaFin = quincena == 1 ? new DateTime(anio, mes, 15) : new DateTime(anio, mes, diasDelMes);
            string periodoCalculado = $"P{((mes - 1) * 2) + quincena}";

            int diasTotalesQuincena = 15;

            var periodo = await context.Nominas
                .FirstOrDefaultAsync(p => p.Anno == anio && p.Mes == mes && p.Periodo == periodoCalculado);

            if (periodo == null)
            {
                periodo = new Nomina
                {
                    Periodo = periodoCalculado,
                    Anno = anio,
                    Mes = mes,
                    PeriodoInicio = fechaInicio,
                    PeriodoFin = fechaFin,
                    Estado = "ABIERTA",
                    Bloqueado = false,
                    TotalSueldosNetos = 0
                };
                context.Nominas.Add(periodo);
                await context.SaveChangesAsync();
            }

            var nominasExistentes = await context.NominaEmpleados.Where(n => n.NominaId == periodo.NominaId).ToListAsync();
            var empIdsConRecibo = nominasExistentes.Select(n => n.EMP_ID).ToList();

            var empleadosInvolucrados = await context.Empleados
                .Where(e => e.StaId == 1 || (e.EmpFechaBaja != null && e.EmpFechaBaja >= fechaInicio) || empIdsConRecibo.Contains(e.EmpId))
                .Select(e => new { e.EmpId, e.StaId, e.SAE_MENSUAL, e.SAE_QUINCENAL, e.SAE_SDI, e.EmpFechaBaja })
                .ToListAsync();

            var catalogoConceptos = await context.NominaConceptos.ToDictionaryAsync(c => c.ConceptoId, c => c);

            // =========================================================================
            // OPTIMIZACIÓN: CARGAR TODOS LOS ADEUDOS DEL PASADO EN 1 SOLO VIAJE A LA BD
            // =========================================================================
            int numPeriodoActual = ((mes - 1) * 2) + quincena;
            int numPeriodoAnterior = numPeriodoActual - 1;
            int anioAnterior = anio;

            if (numPeriodoAnterior == 0)
            {
                numPeriodoAnterior = 24;
                anioAnterior = anio - 1;
            }

            string periodoAnteriorFiltro = $"P{numPeriodoAnterior}";

            // Traemos un Diccionario donde la llave es el EmpId y el valor es su Deuda
            var adeudosPasados = await (from neOld in context.NominaEmpleados
                                        join nOld in context.Nominas on neOld.NominaId equals nOld.NominaId
                                        where nOld.Anno == anioAnterior && nOld.Periodo == periodoAnteriorFiltro
                                        select new { neOld.EMP_ID, neOld.Adeudado })
                                        .ToDictionaryAsync(x => x.EMP_ID, x => x.Adeudado);
            // =========================================================================


            foreach (var emp in empleadosInvolucrados)
            {
                bool debeTenerReciboNuevo = emp.StaId == 1 || (emp.EmpFechaBaja != null && emp.EmpFechaBaja.Value.Date >= fechaInicio.Date);

                if (debeTenerReciboNuevo && !nominasExistentes.Any(n => n.EMP_ID == emp.EmpId))
                {
                    decimal diasPagar = 0;
                    if (emp.EmpFechaBaja != null && emp.EmpFechaBaja.Value.Date <= fechaFin.Date)
                    {
                        diasPagar = (decimal)(emp.EmpFechaBaja.Value.Date - fechaInicio.Date).TotalDays + 1;
                        if (diasPagar < 0) diasPagar = 0;
                    }
                    else
                    {
                        diasPagar = diasTotalesQuincena;
                    }

                    // Asignación ultra rápida desde la memoria
                    decimal adeudoDelEmpleado = adeudosPasados.ContainsKey(emp.EmpId) ? adeudosPasados[emp.EmpId] : 0;

                    var nuevoRecibo = new NominaEmpleado
                    {
                        NominaId = periodo.NominaId,
                        EMP_ID = emp.EmpId,
                        Salario = emp.SAE_QUINCENAL ?? 0,
                        DiasPagados = diasPagar,
                        Percepciones = 0,
                        Deducciones = 0,
                        Pagado = 0,
                        AdeudoAnterior = adeudoDelEmpleado, // <--- Se asigna en milisegundos
                        Estado = "NO_REGISTRADO"
                    };

                    context.NominaEmpleados.Add(nuevoRecibo);
                    await context.SaveChangesAsync();

                    var beneficiosContrato = await context.Set<EmpleadoBeneficio>().Where(b => b.EmpId == emp.EmpId).ToListAsync();

                    foreach (var ben in beneficiosContrato)
                    {
                        decimal montoAInyectar = (quincena == 1) ? ben.MontoQ1 : ben.MontoQ2;
                        if (montoAInyectar > 0)
                        {
                            context.NominaEmpleadoDetalles.Add(new NominaEmpleadoDetalle
                            {
                                NominaEmpleadoId = nuevoRecibo.NominaEmpleadoId,
                                ConceptoId = ben.ConId,
                                Importe = montoAInyectar,
                                Total = montoAInyectar,
                                Comentarios = "Fijo Contractual"
                            });
                        }
                    }

                    nominasExistentes.Add(nuevoRecibo);
                }
            }
            await context.SaveChangesAsync();

            var conceptosDeIncidencia = await context.Set<TipoIncidencia>().Where(t => t.ConId != null).Select(t => t.ConId.Value).Distinct().ToListAsync();

            DateTime inicioBusqueda = fechaInicio.Date;
            DateTime finBusqueda = fechaFin.Date;

            var incidenciasQuincena = await (from inc in context.Incidencium
                                             join hin in context.HistorialIncidencium on inc.IncId equals hin.IncId
                                             join din in context.DetalleIncidencium on inc.DinId equals din.DinId
                                             join tin in context.Set<TipoIncidencia>() on din.TinId equals tin.TinId
                                             join cat in context.NominaConceptos on tin.ConId equals cat.ConceptoId
                                             where inc.IncFecha != null
                                                && inc.IncFecha.Value.Date >= inicioBusqueda
                                                && inc.IncFecha.Value.Date <= finBusqueda
                                                && tin.ConId != null
                                             select new
                                             {
                                                 EmpId = hin.EmpId,
                                                 IncId = inc.IncId, // <--- Rescatamos el ID de la incidencia maestra
                                                 ConceptoId = tin.ConId.Value,
                                                 TipoConcepto = cat.Tipo,
                                                 NombreConcepto = cat.Concepto,
                                                 CantidadReal = din.DinCantidad
                                             }).ToListAsync();

            var vacacionesQuincena = await context.Set<VacacionTomada>().Where(v => v.FechaPago >= fechaInicio && v.FechaPago <= fechaFin).ToListAsync();

            foreach (var recibo in nominasExistentes)
            {
                var emp = empleadosInvolucrados.FirstOrDefault(e => e.EmpId == recibo.EMP_ID);
                if (emp == null) continue;

                decimal sdi = emp.SAE_SDI ?? 0;
                decimal salarioDiarioNormal = (emp.SAE_QUINCENAL ?? 0) / 15m;

                var fallasEmpleado = incidenciasQuincena.Where(i => i.EmpId == emp.EmpId).GroupBy(i => new { i.ConceptoId, i.TipoConcepto, i.NombreConcepto }).ToList();

                foreach (var grupo in fallasEmpleado)
                {
                    decimal sumaCantidades = grupo.Sum(x => x.CantidadReal);
                    decimal totalEnDinero = 0;
                    string nombreUpper = grupo.Key.NombreConcepto.ToUpper();

                    if (nombreUpper.Contains("HORAS EXTRAS")) totalEnDinero = Math.Round(sumaCantidades * ((sdi / 8m) * 2m), 2);
                    else if (nombreUpper.Contains("FESTIVO") || nombreUpper.Contains("DESCANSO")) totalEnDinero = Math.Round(sumaCantidades * (sdi * 3m), 2);
                    else if (nombreUpper.Contains("DOMINICAL")) totalEnDinero = Math.Round(sumaCantidades * (salarioDiarioNormal * 0.25m), 2);
                    else if (nombreUpper.Contains("BAJA") || nombreUpper.Contains("FALTA") || nombreUpper.Contains("SIN GOCE")) totalEnDinero = Math.Round(sumaCantidades * sdi, 2);

                    // TRAZABILIDAD: Concatenamos todos los folios involucrados en este concepto
                    var listaFolios = grupo.Select(x => $"INC-{x.IncId}");
                    string comentarioFolios = "Ref: " + string.Join(", ", listaFolios);

                    context.NominaEmpleadoDetalles.Add(new NominaEmpleadoDetalle
                    {
                        NominaEmpleadoId = recibo.NominaEmpleadoId,
                        ConceptoId = grupo.Key.ConceptoId,
                        Cantidad = sumaCantidades,
                        Importe = totalEnDinero,
                        Total = totalEnDinero,
                        Comentarios = comentarioFolios // <--- Guardamos las referencias agrupadas
                    });
                }

                var primaEmpleado = vacacionesQuincena.Where(v => v.EmpleadoId == emp.EmpId).ToList();
                if (primaEmpleado.Any())
                {
                    decimal sumaDiasPrima = primaEmpleado.Sum(v => v.DiasVacaciones);
                    decimal montoPrimaReal = Math.Round(sumaDiasPrima * sdi * 0.25m, 2);
                    context.NominaEmpleadoDetalles.Add(new NominaEmpleadoDetalle { NominaEmpleadoId = recibo.NominaEmpleadoId, ConceptoId = 1, Cantidad = sumaDiasPrima, Importe = montoPrimaReal, Total = montoPrimaReal });
                }
            }

            await context.SaveChangesAsync();

            // ==============================================================================================
            // RECALCULO INTELIGENTE DE TODA LA SÁBANA 
            // ==============================================================================================
            var todosLosDetalles = await context.NominaEmpleadoDetalles
                .Where(d => d.NominaEmpleado.NominaId == periodo.NominaId)
                .ToListAsync();

            foreach (var recibo in nominasExistentes)
            {
                var detallesRecibo = todosLosDetalles.Where(d => d.NominaEmpleadoId == recibo.NominaEmpleadoId).ToList();
                decimal customSdSueldo = (recibo.Salario) / (decimal)diasTotalesQuincena;
                decimal sueldoProporcional = Math.Round(customSdSueldo * recibo.DiasPagados, 2);

                // Separamos Percepciones (P) de Deducciones (D)
                recibo.Percepciones = sueldoProporcional + detallesRecibo.Where(d => catalogoConceptos.ContainsKey(d.ConceptoId) && catalogoConceptos[d.ConceptoId].Tipo == "P").Sum(d => d.Total);
                recibo.Deducciones = detallesRecibo.Where(d => catalogoConceptos.ContainsKey(d.ConceptoId) && catalogoConceptos[d.ConceptoId].Tipo == "D").Sum(d => d.Total);
                recibo.Pagado = (recibo.Percepciones - recibo.Deducciones) + recibo.AdeudoAnterior;
                context.NominaEmpleados.Update(recibo);
            }
            await context.SaveChangesAsync();

            periodo.TotalSueldosNetos = nominasExistentes.Sum(n => n.Pagado);
            context.Nominas.Update(periodo);
            await context.SaveChangesAsync();

            return periodo;
        }

        public async Task<List<NominaConceptos>> GetConceptosAsync()
        {
            using var context = _dbFactory.CreateDbContext();
            return await context.NominaConceptos.OrderBy(c => c.ConceptoId).ToListAsync();
        }

        public async Task<List<NominaMatrizDTO>> GetMatrizNominaAsync(int nominaId)
        {
            using var context = _dbFactory.CreateDbContext();

            var query = from n in context.NominaEmpleados
                        join e in context.Empleados on n.EMP_ID equals e.EmpId
                        join p in context.Personas on e.PerId equals p.PerId
                        join s in context.Subsidiaria on p.SubId equals s.SubId into subJoin
                        from s in subJoin.DefaultIfEmpty()
                        where n.NominaId == nominaId
                        orderby p.PerApellidoPaterno, p.PerNombre
                        select new NominaMatrizDTO
                        {
                            NomId = n.NominaEmpleadoId,
                            EmpId = e.EmpId,
                            SubId = p.SubId ?? 0,
                            NombreSubsidiaria = s != null ? s.SubNombre : "Sin Subsidiaria",
                            Banco = e.Banco ?? "Sin Banco",  // <--- LÍNEA NUEVA
                            CuentaBancaria = e.CuentaBancaria,
                            NombreCompleto = p.PerApellidoPaterno + " " + (p.PerApellidoMaterno ?? "") + " " + p.PerNombre,
                            SaeMensualPactado = n.Salario * 2,
                            SaeQuincenalPactado = n.Salario,
                            SaeSdi = n.Salario / 15,
                            DiasPagados = n.DiasPagados,
                            TotalPercepciones = n.Percepciones,
                            TotalDeducciones = n.Deducciones,
                            NominaFiscalPagada = 0,
                            ComplementoTercero = 0,
                            AdeudoAnterior = n.AdeudoAnterior,
                            TotalPagado = n.Pagado,
                            PagoAplicado = n.PagoAplicado, // <--- NUEVO
                            Adeudado = n.Adeudado,
                            Estado = n.Estado
                        };

            var matriz = await query.ToListAsync();
            var nomIds = matriz.Select(m => m.NomId).ToList();

            var importes = await context.NominaEmpleadoDetalles.Where(ni => nomIds.Contains(ni.NominaEmpleadoId)).ToListAsync();

            foreach (var fila in matriz)
            {
                fila.Conceptos = importes.Where(ni => ni.NominaEmpleadoId == fila.NomId).GroupBy(ni => ni.ConceptoId)
                    .ToDictionary(g => g.Key, g => new CeldaNominaDTO { Cantidad = g.Sum(x => x.Cantidad), Total = g.Sum(x => x.Total) });
            }

            return matriz;
        }

        public async Task<List<NominaImporteDTO>> GetDesgloseParaEdicionAsync(int nomId)
        {
            using var context = _dbFactory.CreateDbContext();

            var recibo = await context.NominaEmpleados.FindAsync(nomId);
            var periodo = recibo != null ? await context.Nominas.FindAsync(recibo.NominaId) : null;

            var conceptosCat = await context.NominaConceptos.OrderBy(c => c.ConceptoId).ToListAsync();
            var importesActuales = await context.NominaEmpleadoDetalles.Where(ni => ni.NominaEmpleadoId == nomId).ToListAsync();

            var desglose = new List<NominaImporteDTO>();

            foreach (var c in conceptosCat)
            {
                var imp = importesActuales.FirstOrDefault(i => i.ConceptoId == c.ConceptoId);
                int? incId = null;

                // 1. Aquí capturamos tu cadena "Ref: INC-96, INC-98..."
                string comentarioOficial = imp?.Comentarios;

                if (c.CeldaEditable && recibo != null && periodo != null)
                {
                    var tipoInc = await context.Set<TipoIncidencia>().FirstOrDefaultAsync(t => t.ConId == c.ConceptoId);
                    if (tipoInc != null)
                    {
                        var incOficial = await (from h in context.HistorialIncidencium
                                                join i in context.Incidencium on h.IncId equals i.IncId
                                                join d in context.DetalleIncidencium on i.DinId equals d.DinId
                                                where h.EmpId == recibo.EMP_ID && d.TinId == tipoInc.TinId && i.IncFecha >= periodo.PeriodoInicio && i.IncFecha <= periodo.PeriodoFin
                                                select new { i.IncId, d.DinDescripcion }).FirstOrDefaultAsync();

                        if (incOficial != null)
                        {
                            incId = incOficial.IncId;

                            // ========================================================
                            // EL ESCUDO PROTECTOR
                            // Solo usamos la descripción de RH si nuestra celda NO tiene ya los folios agrupados
                            // ========================================================
                            if (string.IsNullOrWhiteSpace(comentarioOficial) || !comentarioOficial.Contains("Ref: INC-"))
                            {
                                comentarioOficial = incOficial.DinDescripcion;
                            }
                        }
                    }
                }

                desglose.Add(new NominaImporteDTO
                {
                    NdeId = imp?.Id ?? 0,
                    NomId = nomId,
                    ConId = c.ConceptoId,
                    NombreConcepto = c.Concepto,
                    Tipo = c.Tipo,
                    Clasificacion = c.Clasificacion ?? "REGULAR",
                    RequiereCantidad = c.CeldaEditable,
                    EsMontoDirecto = c.EsMontoDirecto,
                    Cantidad = imp?.Cantidad,
                    Importe = imp?.Importe ?? 0,
                    Total = imp?.Total ?? 0,
                    Comentario = comentarioOficial, // ¡Ahora pasará a la pantalla sano y salvo!
                    IncidenciaId = incId
                });
            }

            return desglose;
        }

        public async Task GuardarReciboAsync(int nomId, decimal diasPagados, decimal pagoAplicado, List<NominaImporteDTO> desglose, string nuevoEstado)
        {
            using var context = _dbFactory.CreateDbContext();
            using var transaction = await context.Database.BeginTransactionAsync();

            try
            {
                var recibo = await context.NominaEmpleados.FindAsync(nomId);
                if (recibo == null) return;

                var periodo = await context.Nominas.FindAsync(recibo.NominaId);
                if (periodo != null && periodo.Bloqueado) throw new Exception("La sábana está bloqueada. No se pueden guardar cambios.");

                DateTime fechaIncidencia = periodo != null ? periodo.PeriodoFin : DateTime.Now.Date;

                int quincena = periodo != null && periodo.PeriodoFin.Day == 15 ? 1 : 2;
                int mesReal = periodo != null ? periodo.Mes : DateTime.Now.Month;
                int numPeriodo = ((mesReal - 1) * 2) + quincena;

                string nombreMes = periodo != null ? new DateTime(periodo.Anno, periodo.Mes, 1).ToString("MMMM", new CultureInfo("es-MX")) : "";
                if (nombreMes.Length > 0) nombreMes = char.ToUpper(nombreMes[0]) + nombreMes.Substring(1);
                string descOficial = $"Prenómina {nombreMes} P{numPeriodo}";

                recibo.DiasPagados = diasPagados;

                foreach (var item in desglose)
                {
                    if ((item.Total == 0 && item.Importe == 0 && item.Cantidad == null) && item.NdeId == 0) continue;

                    // =========================================================
                    // PASO A: ACTUALIZAR LA SÁBANA DE NÓMINA 
                    // =========================================================
                    var existente = await context.NominaEmpleadoDetalles
                        .FirstOrDefaultAsync(d => d.NominaEmpleadoId == nomId && d.ConceptoId == item.ConId);

                    if (existente != null)
                    {
                        existente.Cantidad = item.Cantidad;
                        existente.Importe = item.Importe;
                        existente.Total = item.Total;
                        existente.Comentarios = item.Comentario;
                        context.NominaEmpleadoDetalles.Update(existente);
                    }
                    else
                    {
                        context.NominaEmpleadoDetalles.Add(new NominaEmpleadoDetalle
                        {
                            NominaEmpleadoId = nomId,
                            ConceptoId = item.ConId,
                            Cantidad = item.Cantidad,
                            Importe = item.Importe,
                            Total = item.Total,
                            Comentarios = item.Comentario
                        });
                    }
                }

                // ¡Guardamos los detalles para evitar choques en el contexto!
                await context.SaveChangesAsync();

                // =========================================================
                // PASO B: SINCRONIZAR DE REGRESO A RECURSOS HUMANOS (Trazabilidad)
                // =========================================================
                var conceptosConCantidad = desglose.Where(d => d.RequiereCantidad).ToList();

                if (conceptosConCantidad.Any())
                {
                    // OBTENEMOS EL MÁXIMO UNA SOLA VEZ AL INICIO
                    int currentDinId = await context.DetalleIncidencium.AnyAsync() ? await context.DetalleIncidencium.MaxAsync(d => d.DinId) : 0;
                    int currentIncId = await context.Incidencium.AnyAsync() ? await context.Incidencium.MaxAsync(i => i.IncId) : 0;
                    int currentHinId = await context.HistorialIncidencium.AnyAsync() ? await context.HistorialIncidencium.MaxAsync(h => h.HinId) : 0;

                    foreach (var item in conceptosConCantidad)
                    {
                        var tipoIncidencia = await context.Set<TipoIncidencia>().FirstOrDefaultAsync(t => t.ConId == item.ConId);
                        if (tipoIncidencia != null && periodo != null)
                        {
                            decimal cantidadFinal = item.Cantidad ?? 0m;

                            // 1. Buscamos TODAS las incidencias de este concepto en la quincena
                            var incidenciasViejas = await (from h in context.HistorialIncidencium
                                                           join i in context.Incidencium on h.IncId equals i.IncId
                                                           join d in context.DetalleIncidencium on i.DinId equals d.DinId
                                                           where h.EmpId == recibo.EMP_ID && d.TinId == tipoIncidencia.TinId && i.IncFecha >= periodo.PeriodoInicio && i.IncFecha <= periodo.PeriodoFin
                                                           select new { Hist = h, Inc = i, Det = d }).ToListAsync();

                            // 2. Sumamos lo que RH había reportado originalmente
                            decimal sumaRH = incidenciasViejas.Sum(x => x.Det.DinCantidad);

                            // 3. LA REGLA DE ORO: Si Finanzas no alteró el número, ¡NO TOCAMOS NADA DE RH!
                            if (cantidadFinal == sumaRH)
                            {
                                // Como los números cuadran, pasamos de largo para no ensuciar las descripciones originales de RH
                                continue;
                            }

                            // 4. SI FINANZAS ALTERÓ EL NÚMERO (Override):
                            // Borramos el historial fragmentado viejo para no duplicar sumas
                            foreach (var vieja in incidenciasViejas)
                            {
                                context.HistorialIncidencium.Remove(vieja.Hist);
                                context.Incidencium.Remove(vieja.Inc);
                                context.DetalleIncidencium.Remove(vieja.Det);
                            }

                            // Creamos UNA SOLA incidencia nueva con el valor consolidado por Finanzas
                            if (cantidadFinal > 0)
                            {
                                currentDinId++;
                                currentIncId++;
                                currentHinId++;

                                // Limpiamos el texto por si traía la cadena "Ref: INC-..."
                                // Limpiamos el texto por si traía la cadena "Ref: INC-..." y le inyectamos el periodo
                                string descripcionLimpia = string.IsNullOrWhiteSpace(item.Comentario) || item.Comentario.Contains("Ref: INC-")
                                    ? $"Modificado/Consolidado por Finanzas desde {descOficial}."
                                    : $"Consolidado por Finanzas en {descOficial}: {item.Comentario}";

                                var det = new DetalleIncidencium
                                {
                                    DinId = currentDinId,
                                    TinId = tipoIncidencia.TinId,
                                    DinCantidad = cantidadFinal,
                                    DinDescripcion = descripcionLimpia
                                };
                                context.DetalleIncidencium.Add(det);

                                var inc = new Incidencium
                                {
                                    IncId = currentIncId,
                                    DinId = det.DinId,
                                    IncFecha = fechaIncidencia
                                };
                                context.Incidencium.Add(inc);

                                context.HistorialIncidencium.Add(new HistorialIncidencium
                                {
                                    HinId = currentHinId,
                                    IncId = inc.IncId,
                                    EmpId = recibo.EMP_ID
                                });
                            }
                        }
                    }
                    await context.SaveChangesAsync();
                }

                // =========================================================
                // PASO C: RECALCULAR TOTALES FINALES
                // =========================================================
                int diasTotalesQuincena = periodo != null ? (periodo.PeriodoFin.Day == 15 ? 15 : (periodo.PeriodoFin.Day - 15)) : 15;
                decimal customSdSueldo = (recibo.Salario) / (decimal)diasTotalesQuincena;
                decimal sueldoProporcional = Math.Round(customSdSueldo * (recibo.DiasPagados), 2);

                // Sumamos Percepciones (Sueldo + Conceptos Tipo 'P')
                recibo.Percepciones = sueldoProporcional + desglose.Where(d => d.Tipo == "P").Sum(d => d.Total);

                // Sumamos Deducciones (Conceptos Tipo 'D')
                recibo.Deducciones = desglose.Where(d => d.Tipo == "D").Sum(d => d.Total);

                // EL NETO: Percepciones MENOS Deducciones MÁS el Adeudo que arrastraba
                recibo.Pagado = (recibo.Percepciones - recibo.Deducciones) + recibo.AdeudoAnterior;

                // --- LÓGICA DE ADEUDO ---
                recibo.PagoAplicado = pagoAplicado;
                recibo.Adeudado = recibo.Pagado - pagoAplicado;
                // ------------------------

                recibo.Estado = nuevoEstado;
                context.NominaEmpleados.Update(recibo);
                await context.SaveChangesAsync();

                // ========================================================
                // AQUÍ ES DONDE SE ACTUALIZA EL GRAN TOTAL DE LA SÁBANA
                // ========================================================
                if (periodo != null)
                {
                    decimal granTotal = await context.NominaEmpleados
                        .Where(ne => ne.NominaId == periodo.NominaId)
                        .SumAsync(ne => ne.Pagado);

                    periodo.TotalSueldosNetos = granTotal;
                    context.Nominas.Update(periodo);
                    await context.SaveChangesAsync(); // Guardamos el total en la tabla principal
                }

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<List<Subsidiarium>> GetSubsidiariasAsync()
        {
            using var context = _dbFactory.CreateDbContext();
            return await context.Subsidiaria.OrderBy(s => s.SubNombre).ToListAsync();
        }

        public async Task EliminarReciboAsync(int nomId)
        {
            using var context = _dbFactory.CreateDbContext();

            var recibo = await context.NominaEmpleados.FindAsync(nomId);
            if (recibo == null) return;

            int idDeLaSabana = recibo.NominaId;

            // 1. Borramos el desglose de conceptos
            var importesVinculados = await context.NominaEmpleadoDetalles.Where(ni => ni.NominaEmpleadoId == nomId).ToListAsync();
            if (importesVinculados.Any()) context.NominaEmpleadoDetalles.RemoveRange(importesVinculados);

            // 2. Borramos el recibo del empleado
            context.NominaEmpleados.Remove(recibo);
            await context.SaveChangesAsync();

            // ========================================================
            // NUEVO: RECALCULAR EL TOTAL DE LA SÁBANA TRAS LA ELIMINACIÓN
            // ========================================================
            var periodo = await context.Nominas.FindAsync(idDeLaSabana);
            if (periodo != null)
            {
                decimal nuevoGranTotal = await context.NominaEmpleados
                    .Where(ne => ne.NominaId == idDeLaSabana)
                    .SumAsync(ne => ne.Pagado);

                periodo.TotalSueldosNetos = nuevoGranTotal;
                context.Nominas.Update(periodo);
                await context.SaveChangesAsync();
            }
        }

        public async Task EliminarSabanaCompletaAsync(int nominaId)
        {
            using var context = _dbFactory.CreateDbContext();
            var nomina = await context.Nominas.FindAsync(nominaId);
            if (nomina == null) return;
            if (nomina.Bloqueado) throw new Exception("No se puede eliminar una sábana que está bloqueada/aprobada.");

            // 1. Buscamos a todos los empleados atados a esta sábana
            var empleadosIds = await context.NominaEmpleados
                .Where(ne => ne.NominaId == nominaId)
                .Select(ne => ne.NominaEmpleadoId)
                .ToListAsync();

            if (empleadosIds.Any())
            {
                // 2. CAZAFANTASMAS: Borramos explícitamente todos los detalles matemáticos huérfanos
                var detallesFantasmas = await context.NominaEmpleadoDetalles
                    .Where(d => empleadosIds.Contains(d.NominaEmpleadoId))
                    .ToListAsync();

                if (detallesFantasmas.Any())
                {
                    context.NominaEmpleadoDetalles.RemoveRange(detallesFantasmas);
                }

                // 3. Borramos los recibos de los empleados
                var recibos = await context.NominaEmpleados
                    .Where(ne => ne.NominaId == nominaId)
                    .ToListAsync();

                context.NominaEmpleados.RemoveRange(recibos);
            }

            // 4. Finalmente, borramos la cabecera de la sábana
            context.Nominas.Remove(nomina);

            // Guardamos todos los cambios de golpe
            await context.SaveChangesAsync();
        }

       
        // =================================================================
        // PROYECCIÓN FINANCIERA (MIXTO: REALIDAD + SIMULACIÓN)
        // =================================================================
        public async Task<ProyeccionAnualDTO> GetProyeccionFinancieraAsync(int anio)
        {
            using var context = _dbFactory.CreateDbContext();
            var proyeccion = new ProyeccionAnualDTO { Anio = anio };

            var nominasReales = await context.Nominas.Where(n => n.Anno == anio).ToListAsync();
            var empleadosActivos = await context.Empleados
                .Where(e => e.StaId == 1)
                .Select(e => new { e.EmpId, SueldoQuincenal = e.SAE_QUINCENAL ?? 0 })
                .ToListAsync();

            var beneficios = await context.Set<EmpleadoBeneficio>().ToListAsync();
            var catalogoTipos = await context.NominaConceptos.ToDictionaryAsync(c => c.ConceptoId, c => c.Tipo);

            var hoy = DateTime.Now;
            int mesActual = hoy.Month;
            int quincenaActual = hoy.Day <= 15 ? 1 : 2;

            for (int m = 1; m <= 12; m++)
            {
                for (int q = 1; q <= 2; q++)
                {
                    int numPeriodo = ((m - 1) * 2) + q;
                    string periodoFiltro = $"P{numPeriodo}";
                    var nominaReal = nominasReales.FirstOrDefault(n => n.Periodo == periodoFiltro);
                    string nombreMes = new DateTime(2000, m, 1).ToString("MMM", new CultureInfo("es-MX")).ToUpper();

                    if (nominaReal != null && nominaReal.TotalSueldosNetos > 0)
                    {
                        // RESCATE HISTÓRICO: Leemos Percepciones y Deducciones directas de NominaEmpleado
                        var recibosHistoricos = await context.NominaEmpleados
                            .Where(ne => ne.NominaId == nominaReal.NominaId)
                            .Select(ne => new { ne.Percepciones, ne.Deducciones })
                            .ToListAsync();

                        proyeccion.Quincenas.Add(new ProyeccionQuincenaDTO
                        {
                            Mes = m,
                            Quincena = q,
                            NombrePeriodo = $"{nombreMes} P{numPeriodo}",
                            TotalPercepciones = recibosHistoricos.Sum(r => r.Percepciones),
                            TotalDeducciones = recibosHistoricos.Sum(r => r.Deducciones),
                            GranTotalFijo = nominaReal.TotalSueldosNetos, // El Total de la cabecera Nomina
                            EsProyeccion = false
                        });
                    }
                    else
                    {
                        bool esQuincenaFuturaOActual = (anio > hoy.Year) || (anio == hoy.Year && (m > mesActual || (m == mesActual && q >= quincenaActual)));

                        if (esQuincenaFuturaOActual)
                        {
                            // En la simulación futura, el Sueldo Pactado entra como Percepción Base
                            decimal perSim = empleadosActivos.Sum(e => e.SueldoQuincenal);
                            decimal dedSim = 0;

                            foreach (var emp in empleadosActivos)
                            {
                                var bEmp = beneficios.Where(b => b.EmpId == emp.EmpId);
                                foreach (var b in bEmp)
                                {
                                    decimal monto = q == 1 ? b.MontoQ1 : b.MontoQ2;
                                    if (catalogoTipos.TryGetValue(b.ConId, out var tipo))
                                    {
                                        if (tipo == "P") perSim += monto; else if (tipo == "D") dedSim += monto;
                                    }
                                }
                            }

                            proyeccion.Quincenas.Add(new ProyeccionQuincenaDTO
                            {
                                Mes = m,
                                Quincena = q,
                                NombrePeriodo = $"{nombreMes} P{numPeriodo}",
                                TotalPercepciones = perSim,
                                TotalDeducciones = dedSim,
                                GranTotalFijo = null,
                                EsProyeccion = true
                            });
                        }
                        else
                        {
                            proyeccion.Quincenas.Add(new ProyeccionQuincenaDTO
                            {
                                Mes = m,
                                Quincena = q,
                                NombrePeriodo = $"{nombreMes} P{numPeriodo}",
                                TotalPercepciones = 0,
                                TotalDeducciones = 0,
                                GranTotalFijo = null,
                                EsProyeccion = false
                            });
                        }
                    }
                }
            }
            return proyeccion;
        }

        // =================================================================
        // VISOR HISTÓRICO (BÚSQUEDA Y FILTRADO GLOBAL)
        // =================================================================

        public async Task<List<VisorNominaDTO>> GetHistorialVisorAsync(string busquedaNombre, string subsidiaria, int anio, string periodoFiltro)
        {
            using var context = _dbFactory.CreateDbContext();

            var query = from ne in context.NominaEmpleados
                        join n in context.Nominas on ne.NominaId equals n.NominaId
                        join e in context.Empleados on ne.EMP_ID equals e.EmpId
                        join p in context.Personas on e.PerId equals p.PerId
                        join s in context.Subsidiaria on p.SubId equals s.SubId into subJoin
                        from s in subJoin.DefaultIfEmpty()
                        where n.TotalSueldosNetos > 0 && n.Anno == anio // FILTRO DE AÑO
                        select new { ne, n, e, p, s };

            // Filtro por Nombre
            if (!string.IsNullOrWhiteSpace(busquedaNombre))
            {
                var searchTerms = busquedaNombre.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                foreach (var term in searchTerms)
                {
                    query = query.Where(q =>
                        q.p.PerNombre.ToLower().Contains(term) ||
                        q.p.PerApellidoPaterno.ToLower().Contains(term) ||
                        q.p.PerApellidoMaterno.ToLower().Contains(term));
                }
            }

            // Filtro por Subsidiaria
            if (!string.IsNullOrWhiteSpace(subsidiaria))
            {
                query = query.Where(q => q.s != null && q.s.SubNombre == subsidiaria);
            }

            // Filtro por Periodo (Ej: "P1", "P2")
            if (!string.IsNullOrWhiteSpace(periodoFiltro))
            {
                query = query.Where(q => q.n.Periodo == periodoFiltro);
            }

            var data = await query.OrderByDescending(q => q.n.PeriodoInicio).ThenBy(q => q.p.PerApellidoPaterno).ToListAsync();

            var nomIds = data.Select(d => d.ne.NominaEmpleadoId).ToList();
            var detallesBd = await (from d in context.NominaEmpleadoDetalles
                                    join c in context.NominaConceptos on d.ConceptoId equals c.ConceptoId
                                    where nomIds.Contains(d.NominaEmpleadoId) && d.Total > 0
                                    select new { d.NominaEmpleadoId, c.Concepto, c.Tipo, d.Total }).ToListAsync();

            var result = new List<VisorNominaDTO>();

            foreach (var item in data)
            {
                int quincenaCalculada = item.n.PeriodoFin.Day <= 15 ? 1 : 2;

                var row = new VisorNominaDTO
                {
                    NomId = item.ne.NominaEmpleadoId,
                    PeriodoNombre = item.n.Periodo,
                    Mes = item.n.Mes,
                    Quincena = quincenaCalculada,
                    Anio = item.n.Anno,
                    FechaInicio = item.n.PeriodoInicio,
                    Empleado = $"{item.p.PerApellidoPaterno} {(item.p.PerApellidoMaterno ?? "")} {item.p.PerNombre}".Trim(),
                    Subsidiaria = item.s != null ? item.s.SubNombre : "Sin Subsidiaria",
                    Banco = item.e.Banco ?? "Sin Banco",

                    // ========================================================
                    // MAPEO DE LAS NUEVAS COLUMNAS DESDE NominaEmpleado (ne)
                    // ========================================================
                    AdeudoAnterior = item.ne.AdeudoAnterior,
                    PagoAplicado = item.ne.PagoAplicado,
                    Adeudado = item.ne.Adeudado,

                    // Dejamos los totales en 0 temporalmente, los calcularemos con la verdad absoluta abajo
                    TotalPercepciones = 0,
                    TotalDeducciones = 0,
                    Neto = 0,
                    Detalles = new List<VisorDetalleDTO>()
                };

                // 1. Calculamos e inyectamos el Sueldo Base (Tipo P) - CORREGIDO A BASE 15 SIEMPRE
                decimal sueldoProp = Math.Round((item.ne.Salario / 15m) * item.ne.DiasPagados, 2);
                row.Detalles.Add(new VisorDetalleDTO { Concepto = "1. Sueldo Base Quincenal", Tipo = "P", Total = sueldoProp });

                // 2. Extraemos todos los conceptos de NominaEmpleadoDetalle (Horas extra, primas, etc.)
                var misDetalles = detallesBd.Where(d => d.NominaEmpleadoId == row.NomId).ToList();
                foreach (var d in misDetalles)
                {
                    row.Detalles.Add(new VisorDetalleDTO { Concepto = d.Concepto, Tipo = d.Tipo, Total = d.Total });
                }

                // ====================================================================
                // 3. Sumamos en vivo (Agregando el AdeudoAnterior al Neto a Pagar)
                // ====================================================================
                row.TotalPercepciones = row.Detalles.Where(d => d.Tipo == "P").Sum(d => d.Total);
                row.TotalDeducciones = row.Detalles.Where(d => d.Tipo == "D").Sum(d => d.Total);

                // El Neto ahora cuadrará con la Sábana porque considera la deuda que arrastraba
                row.Neto = (row.TotalPercepciones - row.TotalDeducciones) + row.AdeudoAnterior;

                result.Add(row);
            }

            return result;
        }

        // =================================================================
        // GENERACIÓN DE REPORTES (EXCEL Y PDF)


public async Task<byte[]> GenerarReporteExcelAsync(int anio, string periodoFiltro, string empleadoNombre)
    {
        // 1. Reutilizamos tu consulta del Visor
        var data = await GetHistorialVisorAsync(empleadoNombre, "", anio, periodoFiltro);
        if (!data.Any()) return Array.Empty<byte>();

        // 2. Extraemos los conceptos dinámicos de la BD
        var columnasDinamicas = data.SelectMany(r => r.Detalles)
            .GroupBy(d => d.Concepto)
            .Select(g => new { Concepto = g.Key, Tipo = g.First().Tipo })
            .OrderBy(x => x.Tipo == "P" ? 0 : 1)
            .ThenBy(x => x.Concepto)
            .ToList();

        // 3. Inicializamos el libro de ClosedXML
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Historial Nómina");

        // Color corporativo azul de tu diseño de la sábana
        var colorNavy = XLColor.FromHtml("#253C60");

        // --- CONSTRUCCIÓN DE ENCABEZADOS ---
        int colIndex = 1;
        worksheet.Cell(1, colIndex++).Value = "Periodo";
        worksheet.Cell(1, colIndex++).Value = "Empleado";
        worksheet.Cell(1, colIndex++).Value = "Subsidiaria";
        worksheet.Cell(1, colIndex++).Value = "Banco";

        var sueldoCol = columnasDinamicas.FirstOrDefault(c => c.Concepto.ToUpper().Contains("SUELDO BASE"));
        if (sueldoCol != null) worksheet.Cell(1, colIndex++).Value = "Base Qnal";

        worksheet.Cell(1, colIndex++).Value = "T. Percepciones";
        worksheet.Cell(1, colIndex++).Value = "T. Deducciones";
        worksheet.Cell(1, colIndex++).Value = "Deuda Anterior";
        worksheet.Cell(1, colIndex++).Value = "Neto Pagar";
        worksheet.Cell(1, colIndex++).Value = "Pago Aplicado";
        worksheet.Cell(1, colIndex++).Value = "Adeudo";

        foreach (var col in columnasDinamicas.Where(c => !c.Concepto.ToUpper().Contains("SUELDO BASE")))
        {
            worksheet.Cell(1, colIndex++).Value = col.Concepto.ToUpper();
        }

        // Aplicar estilos profesionales al encabezado (Negrita, fondo azul y texto blanco)
        var rangoEncabezado = worksheet.Range(1, 1, 1, colIndex - 1);
        rangoEncabezado.Style.Font.Bold = true;
        rangoEncabezado.Style.Font.FontColor = XLColor.White;
        rangoEncabezado.Style.Fill.BackgroundColor = colorNavy;
        rangoEncabezado.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        // --- LLENADO DE FILAS DE EMPLEADOS ---
        int rowIndex = 2;
        foreach (var row in data)
        {
            colIndex = 1;
            worksheet.Cell(rowIndex, colIndex++).Value = row.PeriodoNombre;
            worksheet.Cell(rowIndex, colIndex++).Value = row.Empleado;
            worksheet.Cell(rowIndex, colIndex++).Value = row.Subsidiaria;
            worksheet.Cell(rowIndex, colIndex++).Value = row.Banco;

            if (sueldoCol != null)
            {
                var dSueldo = row.Detalles.FirstOrDefault(x => x.Concepto == sueldoCol.Concepto);
                var cell = worksheet.Cell(rowIndex, colIndex++);
                cell.Value = dSueldo != null ? dSueldo.Total : 0;
                cell.Style.NumberFormat.Format = "$#,##0.00"; // Formato contable real
            }

            // Mapeo de Totales y Adeudos con formato numérico de moneda
            decimal[] totales = { row.TotalPercepciones, row.TotalDeducciones, row.AdeudoAnterior, row.Neto, row.PagoAplicado, row.Adeudado };
            foreach (var val in totales)
            {
                var cell = worksheet.Cell(rowIndex, colIndex++);
                cell.Value = val;
                cell.Style.NumberFormat.Format = "$#,##0.00";
            }

            // Conceptos dinámicos de la quincena
            foreach (var col in columnasDinamicas.Where(c => !c.Concepto.ToUpper().Contains("SUELDO BASE")))
            {
                var d = row.Detalles.FirstOrDefault(x => x.Concepto == col.Concepto);
                var cell = worksheet.Cell(rowIndex, colIndex++);
                cell.Value = d != null ? d.Total : 0;
                cell.Style.NumberFormat.Format = "$#,##0.00";
            }

            rowIndex++;
        }

        // Autoajustar el ancho de todas las columnas mágicamente según su contenido
        worksheet.Columns(1, colIndex - 1).AdjustToContents();

        // Guardar en un MemoryStream para enviarlo a la UI de Blazor
        using var ms = new MemoryStream();
        workbook.SaveAs(ms);
        return ms.ToArray();
    }
}

        public class NominaImporteDTO
    {
        public int NdeId { get; set; }
        public int NomId { get; set; }
        public int ConId { get; set; }
        public string NombreConcepto { get; set; }
        public string Tipo { get; set; }
        public bool RequiereCantidad { get; set; }
        public decimal? Cantidad { get; set; }
        public decimal Importe { get; set; }
        public decimal Total { get; set; }
        public string? Comentario { get; set; }
        public int? IncidenciaId { get; set; }
        public string Clasificacion { get; set; }
        public bool EsMontoDirecto { get; set; }
    }

    public class CeldaNominaDTO
    {
        public decimal? Cantidad { get; set; }
        public decimal Total { get; set; }
        public bool IsEditing { get; set; } = false;
        public decimal? ValorEditado { get; set; }
    }

    public class NominaMatrizDTO
    {
        public int NomId { get; set; }
        public int EmpId { get; set; }
        public string NombreCompleto { get; set; }
        public int SubId { get; set; }
        public string NombreSubsidiaria { get; set; }
        public decimal SaeMensualPactado { get; set; }
        public decimal SaeQuincenalPactado { get; set; }
        public decimal SaeSdi { get; set; }
        public decimal DiasPagados { get; set; }
        public decimal TotalPercepciones { get; set; }
        public decimal TotalDeducciones { get; set; }
        public decimal NominaFiscalPagada { get; set; }
        public decimal ComplementoTercero { get; set; }
        public decimal TotalPagado { get; set; }
        public string Estado { get; set; }
        public string Banco { get; set; }
        public string CuentaBancaria { get; set; }
        public decimal PagoAplicado { get; set; }
        public decimal Adeudado { get; set; }
        public Dictionary<int, CeldaNominaDTO> Conceptos { get; set; } = new Dictionary<int, CeldaNominaDTO>();

        public decimal AdeudoAnterior { get; set; }
        public bool IsSaving { get; set; } = false;
    }

    public class ProyeccionQuincenaDTO
    {
        public int Mes { get; set; }
        public int Quincena { get; set; }
        public string NombrePeriodo { get; set; }

        public decimal TotalPercepciones { get; set; }
        public decimal TotalDeducciones { get; set; }

        // Propiedad que guarda el dato duro de la BD para el histórico
        public decimal? GranTotalFijo { get; set; }

        // Si hay GranTotalFijo (pasado) se muestra, si no, se simula con P - D (futuro)
        public decimal GranTotal => GranTotalFijo ?? (TotalPercepciones - TotalDeducciones);

        public bool EsProyeccion { get; set; }
    }

    public class ProyeccionAnualDTO
    {
        public int Anio { get; set; }
        public List<ProyeccionQuincenaDTO> Quincenas { get; set; } = new List<ProyeccionQuincenaDTO>();
        public decimal TotalPagadoReal => Quincenas.Where(q => !q.EsProyeccion).Sum(q => q.GranTotal);
        public decimal TotalProyectado => Quincenas.Where(q => q.EsProyeccion).Sum(q => q.GranTotal);
        public decimal GranTotalAnual => TotalPagadoReal + TotalProyectado;
    }

    // ==========================================
    // DTOs PARA EL VISOR HISTÓRICO DE NÓMINA
    // ==========================================
    public class VisorDetalleDTO
    {
        public string Concepto { get; set; }
        public string Tipo { get; set; } // "P" o "D"
        public decimal Total { get; set; }
    }

    public class VisorNominaDTO
    {
        public int NomId { get; set; }
        public string PeriodoNombre { get; set; } // Ej: P1
        public int Mes { get; set; }
        public int Quincena { get; set; }
        public int Anio { get; set; }
        public string Banco { get; set; }          // <--- AGREGAR
        public DateTime FechaInicio { get; set; }
        public string Empleado { get; set; }
        public string Subsidiaria { get; set; }
        public decimal TotalPercepciones { get; set; }
        public decimal TotalDeducciones { get; set; }
        public decimal Neto { get; set; }
        public decimal AdeudoAnterior { get; set; }
        public decimal PagoAplicado { get; set; }
        public decimal Adeudado { get; set; }
        public List<VisorDetalleDTO> Detalles { get; set; } = new();

        // Propiedad para agrupar visualmente
        public string TituloAgrupador => $"{GetNombreMes(Mes)} - Quincena {Quincena} ({Anio})";

        private string GetNombreMes(int mes) =>
            new DateTime(2000, mes, 1).ToString("MMMM", new CultureInfo("es-MX")).ToUpper();
    }


}