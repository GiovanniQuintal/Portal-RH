using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RRHH_ARG.Models;
using RRHH_ARG.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClosedXML.Excel;

using static RRHH_ARG.Components.Pages.QhseDashboard;
using static RRHH_ARG.Components.Pages.QhseForm;

namespace RRHH_ARG.Data
{
    public class PuestosUsuarioDTO
    {
        public string NombrePuesto1 { get; set; } = "Sin asignar";
        public byte[]? PdfPuesto1 { get; set; }

        public string? NombrePuesto2 { get; set; }
        public byte[]? PdfPuesto2 { get; set; }
    }

    public class EmpleadoService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly RRHH_ARG.Services.MoodleSyncService _moodleService; //<---22Dic AGREGAR 
        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;

        public EmpleadoService(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, Services.MoodleSyncService moodleService, IDbContextFactory<ApplicationDbContext> dbFactory)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _moodleService = moodleService;
            _dbFactory = dbFactory;
        }

        // =================================================================
        // 1. OBTENER LISTA DE INCIDENCIAS (LECTURA)
        // =================================================================
        public async Task<List<IncidenciaDTO>> GetIncidenciasDTOAsync()
        {
            using var context = _dbFactory.CreateDbContext();

            var query = from inc in context.Incidencium
                        join din in context.DetalleIncidencium on inc.DinId equals din.DinId
                        join hin in context.HistorialIncidencium on inc.IncId equals hin.IncId
                        join emp in context.Empleados on hin.EmpId equals emp.EmpId
                        join per in context.Personas on emp.PerId equals per.PerId
                        join pep in context.PerfilPuesto on per.PepId equals pep.PepId into pepGroup
                        from pep in pepGroup.DefaultIfEmpty()
                        join area in context.Area on pep.AreId equals area.AreId into areaGroup
                        from area in areaGroup.DefaultIfEmpty()
                        orderby inc.IncId descending
                        select new IncidenciaDTO
                        {
                            IncidenciaId = inc.IncId,
                            EmpleadoId = emp.EmpId,
                            NombreEmpleado = per.PerNombre + " " + per.PerApellidoPaterno,
                            AreaEmpleado = area != null ? area.AreNombre : "Sin Área",
                            //Tipo = din.DinTipo,
                            TipoId = din.TinId ?? 0,

                            Tipo = din.TipoIncidenciaRel != null ? din.TipoIncidenciaRel.TinNombre : "Sin Tipo",
                            Cantidad = din.DinCantidad,
                            Descripcion = din.DinDescripcion,
                            NombreArchivo = din.DinArchivo,
                            FechaIncidencia = inc.IncFecha ?? DateTime.Now
                        };

            return await query.AsNoTracking().ToListAsync();
        }

        // =================================================================
        // 2. OBTENER EMPLEADOS PARA EL MODAL (DROPDOWNS)
        // =================================================================
        public async Task<List<Empleado>> GetEmpleadosParaIncidenciasAsync()
        {
            using var context = _dbFactory.CreateDbContext();

            var consulta = from emp in context.Empleados
                           join per in context.Personas on emp.PerId equals per.PerId
                           join sub in context.Subsidiaria on per.SubId equals sub.SubId into subGroup
                           from sub in subGroup.DefaultIfEmpty()
                           join pep in context.PerfilPuesto on per.PepId equals pep.PepId into pepGroup
                           from pep in pepGroup.DefaultIfEmpty()
                           join area in context.Area on pep.AreId equals area.AreId into areaGroup
                           from area in areaGroup.DefaultIfEmpty()
                           select new
                           {
                               Entity = emp,
                               Nombre = per.PerNombre,
                               Apellidos = per.PerApellidoPaterno + " " + per.PerApellidoMaterno,
                               Subsidiaria = sub != null ? sub.SubNombre : "",
                               Area = area != null ? area.AreNombre : ""
                           };

            var datos = await consulta.ToListAsync();
            var listaFinal = new List<Empleado>();

            foreach (var item in datos)
            {
                var e = item.Entity;
                e.Nombre = item.Nombre;
                e.Apellidos = item.Apellidos;
                e.Subsidiaria = item.Subsidiaria;
                e.Area = item.Area;
                listaFinal.Add(e);
            }

            return listaFinal;
        }

        // =================================================================
        // 3. GUARDAR INCIDENCIA (ESCRITURA) Y SINCRONIZAR KARDEX / NÓMINA
        // =================================================================
        public async Task AddIncidenciaAsync(IncidenciaDTO vm)
        {
            using var context = _dbFactory.CreateDbContext();
            using var transaction = await context.Database.BeginTransactionAsync();

            try
            {
                if (vm.EmpleadoId <= 0)
                    throw new Exception("Error: No se ha seleccionado un empleado válido para la incidencia.");

                bool existeEmpleado = await context.Empleados.AnyAsync(e => e.EmpId == vm.EmpleadoId);
                if (!existeEmpleado)
                    throw new Exception("Error: El empleado seleccionado no existe en la base de datos.");

                // 1. CREAR DETALLE INCIDENCIA
                int nextDinId = await context.DetalleIncidencium.AnyAsync() ? await context.DetalleIncidencium.MaxAsync(d => d.DinId) + 1 : 1;
                var detalle = new DetalleIncidencium
                {
                    DinId = nextDinId,
                    TinId = vm.TipoId,
                    DinCantidad = vm.Cantidad,
                    DinDescripcion = vm.Descripcion,
                    DinArchivo = vm.NombreArchivo,
                    DinContenido = vm.ArchivoBytes
                };
                context.DetalleIncidencium.Add(detalle);
                await context.SaveChangesAsync();

                // 2. CREAR INCIDENCIA MAESTRA
                int nextIncId = await context.Incidencium.AnyAsync() ? await context.Incidencium.MaxAsync(i => i.IncId) + 1 : 1;
                var incidencia = new Incidencium
                {
                    IncId = nextIncId,
                    DinId = detalle.DinId,
                    IncFecha = vm.FechaIncidencia
                };
                context.Incidencium.Add(incidencia);
                await context.SaveChangesAsync();

                // 3. CREAR HISTORIAL (Enlace Empleado)
                int nextHinId = await context.HistorialIncidencium.AnyAsync() ? await context.HistorialIncidencium.MaxAsync(h => h.HinId) + 1 : 1;
                var historial = new HistorialIncidencium
                {
                    HinId = nextHinId,
                    IncId = incidencia.IncId,
                    EmpId = vm.EmpleadoId
                };
                context.HistorialIncidencium.Add(historial);
                await context.SaveChangesAsync();

                // =================================================================
                // MAGIA 1: SINCRONIZACIÓN INVERSA (INCIDENCIAS -> KARDEX VACACIONES)
                // =================================================================
                if (vm.TipoId == 2 && vm.Cantidad > 0)
                {
                    decimal diasRestantesPorAplicar = vm.Cantidad;

                    var cubetas = await context.Set<KardexVacacion>()
                        .Where(k => k.EmpleadoId == vm.EmpleadoId && k.IsVigente && (k.DiasDerecho - k.DiasTomados) > 0)
                        .OrderBy(k => k.FechaInicio)
                        .ToListAsync();

                    foreach (var kubeta in cubetas)
                    {
                        if (diasRestantesPorAplicar <= 0) break;

                        decimal disponibleEnEstaKubeta = kubeta.DiasDerecho - kubeta.DiasTomados;
                        decimal aDescontar = Math.Min(disponibleEnEstaKubeta, diasRestantesPorAplicar);

                        var registro = new VacacionTomada
                        {
                            KardexVacacionId = kubeta.Id,
                            EmpleadoId = vm.EmpleadoId,
                            IncidenciaId = incidencia.IncId,
                            FechaInicial = vm.FechaIncidencia,
                            FechaFinal = vm.FechaIncidencia.AddDays((double)aDescontar - 1),
                            FechaPago = DateTime.Now,
                            DiasDescanso = 0,
                            DiasVacaciones = aDescontar,
                            PrimaVacacionalDias = aDescontar * 0.25m,
                            FechaRegistro = DateTime.Now
                        };
                        context.Set<VacacionTomada>().Add(registro);

                        kubeta.DiasTomados += aDescontar;
                        context.Set<KardexVacacion>().Update(kubeta);

                        diasRestantesPorAplicar -= aDescontar;
                    }
                    await context.SaveChangesAsync();
                }

                // =================================================================
                // MAGIA 2: SINCRONIZACIÓN BILATERAL (INCIDENCIAS -> SÁBANA NÓMINA)
                // =================================================================
                if (vm.TipoId != 2) // Excluimos vacaciones de este recálculo financiero
                {
                    await SincronizarPrenominaDesdeIncidenciaAsync(vm.EmpleadoId, vm.FechaIncidencia, context);
                }

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // =================================================================
        // 4. ELIMINAR INCIDENCIA Y ACTUALIZAR SÁBANA
        // =================================================================
        public async Task DeleteIncidenciaAsync(int incidenciaId)
        {
            using var context = _dbFactory.CreateDbContext();
            using var transaction = await context.Database.BeginTransactionAsync();

            try
            {
                var incidencia = await context.Incidencium.FindAsync(incidenciaId);

                if (incidencia != null)
                {
                    DateTime fechaAfectada = incidencia.IncFecha ?? DateTime.Now;

                    var historial = await context.HistorialIncidencium.FirstOrDefaultAsync(h => h.IncId == incidenciaId);
                    int empId = historial?.EmpId ?? 0;

                    int? dinId = incidencia.DinId;

                    // Borramos el historial y la incidencia
                    if (historial != null) context.HistorialIncidencium.Remove(historial);
                    context.Incidencium.Remove(incidencia);
                    await context.SaveChangesAsync(); // Se borra para que el recálculo no la encuentre

                    // Sincronizamos la nómina ahora que la incidencia ya no existe
                    if (empId > 0)
                    {
                        await SincronizarPrenominaDesdeIncidenciaAsync(empId, fechaAfectada, context);
                    }

                    // Finalmente borramos el detalle y el archivo
                    if (dinId != null)
                    {
                        var detalle = await context.DetalleIncidencium.FindAsync(dinId);
                        if (detalle != null) context.DetalleIncidencium.Remove(detalle);
                        await context.SaveChangesAsync();
                    }
                }

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // =================================================================
        // 5. ACTUALIZAR INCIDENCIA Y RECALCULAR SÁBANA
        // =================================================================
        public async Task UpdateIncidenciaAsync(IncidenciaDTO vm)
        {
            using var context = _dbFactory.CreateDbContext();
            using var transaction = await context.Database.BeginTransactionAsync();

            try
            {
                var incidencia = await context.Incidencium.FindAsync(vm.IncidenciaId);

                if (incidencia != null)
                {
                    DateTime fechaAntigua = incidencia.IncFecha ?? DateTime.Now;
                    var historial = await context.HistorialIncidencium.FirstOrDefaultAsync(h => h.IncId == vm.IncidenciaId);
                    int empId = historial?.EmpId ?? 0;

                    incidencia.IncFecha = vm.FechaIncidencia;

                    if (incidencia.DinId != null)
                    {
                        var detalle = await context.DetalleIncidencium.FindAsync(incidencia.DinId);
                        if (detalle != null)
                        {
                            detalle.TinId = vm.TipoId;
                            detalle.DinCantidad = vm.Cantidad;
                            detalle.DinDescripcion = vm.Descripcion;

                            if (vm.ArchivoBytes != null && vm.ArchivoBytes.Length > 0)
                            {
                                detalle.DinArchivo = vm.NombreArchivo;
                                detalle.DinContenido = vm.ArchivoBytes;
                            }
                        }
                    }
                    await context.SaveChangesAsync(); // Guardamos el cambio de cantidad/fecha

                    // Sincronizamos la nómina
                    if (empId > 0 && vm.TipoId != 2)
                    {
                        // Si le cambió la fecha de Enero a Febrero, limpiamos Enero y afectamos Febrero
                        await SincronizarPrenominaDesdeIncidenciaAsync(empId, fechaAntigua, context);

                        if (fechaAntigua.Date != vm.FechaIncidencia.Date)
                        {
                            await SincronizarPrenominaDesdeIncidenciaAsync(empId, vm.FechaIncidencia, context);
                        }
                    }
                }
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // =================================================================
        // MOTOR BIFÁSICO: DE INCIDENCIA A NÓMINA
        // =================================================================
        private async Task SincronizarPrenominaDesdeIncidenciaAsync(int empId, DateTime fechaIncidencia, ApplicationDbContext context)
        {
            // TRUCO: Asegurarnos de quitarle la hora, minuto y segundo a la fecha que llega de RH
            DateTime fechaBuscada = fechaIncidencia.Date;

            // 1. Buscamos la nómina abierta comparando puras fechas de calendario
            var nomina = await context.Nominas
                .FirstOrDefaultAsync(n =>
                    n.PeriodoInicio.Date <= fechaBuscada &&
                    n.PeriodoFin.Date >= fechaBuscada &&
                    !n.Bloqueado);

            if (nomina == null) return; // Si no hay nómina abierta en esta fecha, abortamos.

            // 2. Buscamos el recibo
            var recibo = await context.NominaEmpleados
                .Include(r => r.Empleado)
                .FirstOrDefaultAsync(r => r.NominaId == nomina.NominaId && r.EMP_ID == empId);

            if (recibo == null) return; // Si el empleado fue excluido de la sábana, abortamos.

            // 3. Traemos tipos de incidencia
            var tiposIncidencia = await context.Set<TipoIncidencia>().Where(t => t.ConId != null).ToListAsync();
            var idsTiposInc = tiposIncidencia.Select(t => t.TinId).ToList();

            // Congelamos las fechas de la nómina purificadas (sin horas) para la subconsulta
            DateTime inicioNomina = nomina.PeriodoInicio.Date;
            DateTime finNomina = nomina.PeriodoFin.Date;

            // Buscamos todas las incidencias guardando el IncId para la cadena de texto
            var incidenciasQuincena = await (from inc in context.Incidencium
                                             join hin in context.HistorialIncidencium on inc.IncId equals hin.IncId
                                             join din in context.DetalleIncidencium on inc.DinId equals din.DinId
                                             where hin.EmpId == empId
                                                && inc.IncFecha != null
                                                && inc.IncFecha.Value.Date >= inicioNomina
                                                && inc.IncFecha.Value.Date <= finNomina
                                                && idsTiposInc.Contains(din.TinId ?? 0)
                                             select new { inc.IncId, din.TinId, din.DinCantidad }).ToListAsync();

            var conceptosCatalogo = await context.NominaConceptos.ToDictionaryAsync(c => c.ConceptoId, c => c);
            decimal sdi = recibo.Empleado.SAE_SDI ?? 0;
            decimal salarioDiarioNormal = (recibo.Empleado.SAE_QUINCENAL ?? 0) / 15m;

            var detallesRecibo = await context.NominaEmpleadoDetalles
                .Where(d => d.NominaEmpleadoId == recibo.NominaEmpleadoId).ToListAsync();

            foreach (var tin in tiposIncidencia)
            {
                var d = detallesRecibo.FirstOrDefault(x => x.ConceptoId == tin.ConId.Value);
                if (d != null) { d.Cantidad = 0; d.Total = 0; d.Importe = 0; d.Comentarios = null; }
            }

            var fallasAgrupadas = incidenciasQuincena
                .GroupBy(i => tiposIncidencia.First(t => t.TinId == i.TinId).ConId.Value)
                .ToList();

            foreach (var grupo in fallasAgrupadas)
            {
                int conId = grupo.Key;
                decimal sumaCantidades = grupo.Sum(x => x.DinCantidad);
                if (sumaCantidades == 0) continue;

                decimal totalEnDinero = 0;
                var cat = conceptosCatalogo[conId];
                string nombreUpper = cat.Concepto.ToUpper();

                if (nombreUpper.Contains("HORAS EXTRAS")) totalEnDinero = Math.Round(sumaCantidades * ((sdi / 8m) * 2m), 2);
                else if (nombreUpper.Contains("FESTIVO") || nombreUpper.Contains("DESCANSO")) totalEnDinero = Math.Round(sumaCantidades * (sdi * 3m), 2);
                else if (nombreUpper.Contains("DOMINICAL")) totalEnDinero = Math.Round(sumaCantidades * (salarioDiarioNormal * 0.25m), 2);
                else if (nombreUpper.Contains("BAJA") || nombreUpper.Contains("FALTA") || nombreUpper.Contains("SIN GOCE")) totalEnDinero = Math.Round(sumaCantidades * sdi, 2);

                // Volvemos a concatenar los folios dinámicamente
                var listaFolios = grupo.Select(x => $"INC-{x.IncId}");
                string comentarioFolios = "Ref: " + string.Join(", ", listaFolios);

                var det = detallesRecibo.FirstOrDefault(d => d.ConceptoId == conId);
                if (det != null)
                {
                    det.Cantidad = sumaCantidades;
                    det.Total = totalEnDinero;
                    det.Importe = totalEnDinero;
                    det.Comentarios = comentarioFolios; // <--- Sincroniza las referencias
                }
                else
                {
                    det = new NominaEmpleadoDetalle
                    {
                        NominaEmpleadoId = recibo.NominaEmpleadoId,
                        ConceptoId = conId,
                        Cantidad = sumaCantidades,
                        Total = totalEnDinero,
                        Importe = totalEnDinero,
                        Comentarios = comentarioFolios
                    };
                    context.NominaEmpleadoDetalles.Add(det);
                    detallesRecibo.Add(det);
                }
            }

            // 4. Actualizamos Totales Finales del Recibo
            decimal customSdSueldo = (recibo.Empleado.SAE_QUINCENAL ?? 0) / 15m;
            decimal sueldoProporcional = Math.Round(customSdSueldo * recibo.DiasPagados, 2);

            recibo.Percepciones = sueldoProporcional + detallesRecibo.Where(d => conceptosCatalogo.ContainsKey(d.ConceptoId) && conceptosCatalogo[d.ConceptoId].Tipo == "P").Sum(d => d.Total);
            recibo.Deducciones = detallesRecibo.Where(d => conceptosCatalogo.ContainsKey(d.ConceptoId) && conceptosCatalogo[d.ConceptoId].Tipo == "D").Sum(d => d.Total);

            recibo.Pagado = (recibo.Percepciones - recibo.Deducciones) + recibo.AdeudoAnterior;
            recibo.Adeudado = recibo.Pagado - recibo.PagoAplicado;

            context.NominaEmpleados.Update(recibo);

            // 5. Ajustamos el Gran Total de la Sábana General
            var totalRestante = await context.NominaEmpleados
                .Where(n => n.NominaId == nomina.NominaId && n.NominaEmpleadoId != recibo.NominaEmpleadoId)
                .SumAsync(n => n.Pagado);

            nomina.TotalSueldosNetos = totalRestante + recibo.Pagado;
            context.Nominas.Update(nomina);

            await context.SaveChangesAsync();
        }

        // =================================================================
        // 6. APLICANTES
        // =================================================================
        public async Task<List<AplicanteViewModel>> GetAplicantesViewModelAsync()
        {
            using var context = _dbFactory.CreateDbContext();

            var query = from a in context.Aplicante
                        join p in context.Personas on a.PerId equals p.PerId
                        join puesto in context.PerfilPuesto on p.PepId equals puesto.PepId
                        join area in context.Area on puesto.AreId equals area.AreId
                        join tipo in context.TipoAplicante on a.TipId equals tipo.TipId into tipoGroup
                        from tipoJoin in tipoGroup.DefaultIfEmpty()
                        from puestoJoin in context.PerfilPuesto.Where(pj => pj.PepId == p.PepId).DefaultIfEmpty()
                        from areaJoin in context.Area.Where(aj => aj.AreId == puestoJoin.AreId).DefaultIfEmpty()
                        from subJoin in context.Subsidiaria.Where(s => s.SubId == p.SubId).DefaultIfEmpty()
                        select new AplicanteViewModel
                        {
                            AplId = a.AplId,
                            PerId = p.PerId,
                            Nombre = p.PerNombre,
                            ApellidoPaterno = p.PerApellidoPaterno,
                            ApellidoMaterno = p.PerApellidoMaterno,
                            Genero = p.PerGenero,
                            Email = p.PerEmail,
                            Telefono = p.PerTelefono,
                            FechaNacimiento = p.PerFechaNacimiento,
                            FechaRegistro = p.PerFechaRegistro,
                            Area = areaJoin != null ? areaJoin.AreNombre : "N/A",
                            Empleo = puestoJoin != null ? puestoJoin.PepNombre : "N/A",
                            Subsidiaria = subJoin != null ? subJoin.SubNombre : "N/A",
                            AplStatus = a.AplStatus,
                            FechaInicioEntrevista = a.AplFechaInicioEntrevista,
                            FechaTerminoEntrevista = a.AplFechaTerminoEntrevista,
                            IsPromoted = a.AplIspromoted,
                            TipoEmpleado = tipoJoin != null ? tipoJoin.TipNombre : "N/A",
                            AplCv = a.AplCv
                        };
            return await query.ToListAsync();
        }

        public async Task SaveAplicanteAsync(AplicanteViewModel vm, System.IO.Stream? fileStream = null, string? fileName = null)
        {
            using var context = _dbFactory.CreateDbContext();

            vm.ApellidoPaterno = NormalizarApellido(vm.ApellidoPaterno);
            vm.ApellidoMaterno = NormalizarApellido(vm.ApellidoMaterno);



            var puesto = await context.PerfilPuesto.FirstOrDefaultAsync(p => p.PepNombre == vm.Empleo);
            var sub = await context.Subsidiaria.FirstOrDefaultAsync(s => s.SubNombre == vm.Subsidiaria);
            var tipoApp = await context.TipoAplicante.FirstOrDefaultAsync(t => t.TipNombre == vm.TipoEmpleado);

            byte[]? fileBytes = null;
            if (fileStream != null)
            {
                using (var memoryStream = new System.IO.MemoryStream())
                {
                    await fileStream.CopyToAsync(memoryStream);
                    fileBytes = memoryStream.ToArray();
                }
            }

            if (vm.AplId == 0) // NUEVO APLICANTE
            {
                var nuevaPersona = new Persona
                {
                    PerNombre = vm.Nombre,
                    PerApellidoPaterno = vm.ApellidoPaterno,
                    PerApellidoMaterno = vm.ApellidoMaterno,
                    PerEmail = vm.Email,
                    PerTelefono = vm.Telefono,
                    PerGenero = vm.Genero,
                    PerFechaNacimiento = vm.FechaNacimiento,
                    PerFechaRegistro = vm.FechaRegistro ?? DateTime.Now,
                    PepId = puesto?.PepId,
                    SubId = sub?.SubId
                };
                context.Personas.Add(nuevaPersona);
                await context.SaveChangesAsync();

                var nuevoAplicante = new Aplicante
                {
                    PerId = nuevaPersona.PerId,
                    AplStatus = vm.AplStatus,
                    AplIspromoted = vm.IsPromoted,
                    AplCv = fileName,
                    AplCvArchivo = fileBytes,
                    TipId = tipoApp?.TipId,
                    AplFechaInicioEntrevista = vm.FechaInicioEntrevista,
                    AplFechaTerminoEntrevista = vm.FechaTerminoEntrevista
                };
                if (!nuevoAplicante.AplStatus) nuevoAplicante.AplStatus = true;
                context.Aplicante.Add(nuevoAplicante);
                await context.SaveChangesAsync();
            }
            else // EDICIÓN APLICANTE
            {
                var aplicante = await context.Aplicante.FirstOrDefaultAsync(a => a.AplId == vm.AplId);
                if (aplicante != null)
                {
                    var persona = await context.Personas.FirstOrDefaultAsync(p => p.PerId == aplicante.PerId);
                    if (persona != null)
                    {
                        persona.PerNombre = vm.Nombre; persona.PerApellidoPaterno = vm.ApellidoPaterno; persona.PerApellidoMaterno = vm.ApellidoMaterno;
                        persona.PerEmail = vm.Email; persona.PerTelefono = vm.Telefono; persona.PerGenero = vm.Genero; persona.PerFechaNacimiento = vm.FechaNacimiento;
                        persona.PepId = puesto?.PepId; persona.SubId = sub?.SubId;
                        context.Personas.Update(persona);
                    }
                    aplicante.AplStatus = vm.AplStatus;
                    aplicante.AplIspromoted = vm.IsPromoted;
                    aplicante.TipId = tipoApp?.TipId;
                    aplicante.AplFechaInicioEntrevista = vm.FechaInicioEntrevista;
                    aplicante.AplFechaTerminoEntrevista = vm.FechaTerminoEntrevista;

                    if (fileBytes != null) { aplicante.AplCv = fileName; aplicante.AplCvArchivo = fileBytes; }
                    else if (string.IsNullOrEmpty(vm.AplCv)) { aplicante.AplCv = null; aplicante.AplCvArchivo = null; }

                    context.Aplicante.Update(aplicante);
                    await context.SaveChangesAsync();
                }
            }
        }

        public async Task DeleteCvAplicanteAsync(int aplId)
        {
            using var context = _dbFactory.CreateDbContext();

            var app = await context.Aplicante.FindAsync(aplId);
            if (app != null) { app.AplCv = null; app.AplCvArchivo = null; await context.SaveChangesAsync(); }
        }


        public async Task PromoteAplicanteToEmpleadoAsync(int aplicanteId)
        {
            using var context = _dbFactory.CreateDbContext();

            var aplicante = await context.Aplicante.FirstOrDefaultAsync(a => a.AplId == aplicanteId);
            if (aplicante == null) throw new Exception("El aplicante no existe.");
            if (aplicante.AplIspromoted) throw new Exception("Este aplicante ya fue promovido anteriormente.");

            var persona = await context.Personas.FindAsync(aplicante.PerId);
            if (persona == null) throw new Exception("Error crítico: El aplicante no tiene datos personales asociados.");

            // 1. Crear Usuario Identity
            string emailCorporativo = await GenerarCorreoCorporativoAsync(persona.PerNombre, persona.PerApellidoPaterno);
            string passwordPorDefecto = "Argos" + DateTime.Now.Year + ".";

            var newUser = new ApplicationUser
            {
                UserName = emailCorporativo,
                Email = emailCorporativo,
                EsPrimerIngreso = true,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(newUser, passwordPorDefecto);
            if (!result.Succeeded) throw new Exception($"Error Identity: {string.Join(", ", result.Errors.Select(e => e.Description))}");

            // ROL
            string nombreRol = "Empleado";
            if (await _roleManager.RoleExistsAsync(nombreRol))
            {
                await _userManager.AddToRoleAsync(newUser, nombreRol);
            }

            // 2. Intentar guardar Empleado y Moodle
            try
            {
                // A. Preparar Empleado (Base de datos principal)
                var nuevoEmpleado = new Empleado
                {
                    PerId = aplicante.PerId,
                    StaId = 1,
                    EmpFechaContratacion = DateTime.Now,
                    EmpFechaRegistro = DateTime.Now,
                    TipId = aplicante.TipId,
                    AspNetUserId = newUser.Id // Enlazamos con el usuario recién creado
                };

                context.Empleados.Add(nuevoEmpleado);

                // B. Marcar como promovido
                aplicante.AplIspromoted = true;
                context.Aplicante.Update(aplicante);

                // C. Guardar en Base de Datos Principal
                await context.SaveChangesAsync();

                // D. Si todo salió bien en la BD, creamos en Moodle
                // (Es mejor crear en Moodle al final, porque si Moodle falla es menos crítico que si falla la BD)
                try
                {
                    string passwordMoodle = passwordPorDefecto + "*";
                    await _moodleService.CrearUsuarioMoodleAsync(
                        newUser.UserName,
                        passwordMoodle,
                        persona.PerNombre,           // <--- CORREGIDO (Antes era vm.Nombre)
                        persona.PerApellidoPaterno,  // <--- CORREGIDO (Antes era vm.Apellido)
                        newUser.Email
                    );
                }
                catch (Exception exMoodle)
                {
                    // Logueamos error de Moodle pero NO cancelamos la operación porque el empleado ya se guardó
                    Console.WriteLine("Empleado creado, pero falló Moodle: " + exMoodle.Message);
                }
            }
            catch (Exception ex)
            {
                // 3. ESTRATEGIA DE ROLLBACK (CRÍTICO)
                // Si falló el SaveChangesAsync del Empleado, debemos borrar el usuario de Identity
                // para que no quede "huérfano" y se pueda reintentar la operación.
                await _userManager.DeleteAsync(newUser);

                throw new Exception("Error al promover empleado. Se ha revertido la creación del usuario. Detalle: " + ex.Message);
            }
        }

        // =================================================================
        // 8. CATÁLOGOS
        // =================================================================
        public async Task<List<Area>> GetAreasAsync()
        {
            using var context = _dbFactory.CreateDbContext();
            return await context.Area
                .AsNoTracking()
                .OrderBy(a => a.AreNombre)
                .ToListAsync();
        }

        public async Task<List<TipoAplicante>> GetTiposAplicanteAsync()
        {
            using var context = _dbFactory.CreateDbContext();
            return await context.TipoAplicante
                .AsNoTracking()
                .OrderBy(t => t.TipNombre)
                .ToListAsync();
        }

        public async Task<List<Subsidiarium>> GetSubsidiariasAsync()
        {
            using var context = _dbFactory.CreateDbContext();
            return await context.Subsidiaria
                .AsNoTracking()
                .OrderBy(s => s.SubNombre)
                .ToListAsync();
        }

        public async Task<List<PerfilPuesto>> GetPerfilesPuestoAsync()
        {
            using var context = _dbFactory.CreateDbContext();
            return await context.PerfilPuesto
                .AsNoTracking()
                .Include(p => p.Are)
                .OrderBy(p => p.PepNombre)
                .ToListAsync();
        }

        public async Task<List<Status>> GetStatusCatalogoAsync()
        {
            using var context = _dbFactory.CreateDbContext();
            return await context.Statuses
                .AsNoTracking()
                .OrderBy(s => s.StaTipoStatus)
                .ToListAsync();
        }

        // =================================================================
        // 9. EMPLEADOS VIEW MODEL (BARRA DE PROGRESO DINÁMICA UNIVERSAL)
        // =================================================================
        public async Task<List<EmpleadoViewModel>> GetEmpleadosViewModelAsync()
        {
            using var context = _dbFactory.CreateDbContext();

            // Arreglo base para Becarios/Practicantes
            int[] docsBecariosIds = { 4, 5, 6, 9, 13, 15, 17, 19, 20 };
            int idDocumentoExclusivoBecarios = 19;

            var query = from e in context.Empleados
                        join p in context.Personas on e.PerId equals p.PerId
                        join puesto in context.PerfilPuesto on p.PepId equals puesto.PepId into puestoGroup
                        from puestoJoin in puestoGroup.DefaultIfEmpty()
                        join puesto2 in context.PerfilPuesto on p.PepId2 equals puesto2.PepId into puesto2Group
                        from puesto2Join in puesto2Group.DefaultIfEmpty()
                        join area in context.Area on (puestoJoin == null ? -1 : puestoJoin.AreId) equals area.AreId into areaGroup
                        from areaJoin in areaGroup.DefaultIfEmpty()
                        join area2 in context.Area on (puesto2Join == null ? -1 : puesto2Join.AreId) equals area2.AreId into area2Group
                        from area2Join in area2Group.DefaultIfEmpty()
                        join sub in context.Subsidiaria on p.SubId equals sub.SubId into subGroup
                        from subJoin in subGroup.DefaultIfEmpty()
                        join st in context.Statuses on e.StaId equals st.StaId into statusGroup
                        from stJoin in statusGroup.DefaultIfEmpty()
                        join ti in context.TipoAplicante on e.TipId equals ti.TipId into tipoGroup
                        from tiJoin in tipoGroup.DefaultIfEmpty()
                        select new EmpleadoViewModel
                        {
                            Id = e.EmpId,
                            CodigoEmpleado = e.EmpCodigo,
                            PerId = p.PerId,
                            Nombre = p.PerNombre,
                            ApellidoPaterno = p.PerApellidoPaterno,
                            ApellidoMaterno = p.PerApellidoMaterno,
                            Email = p.PerEmail,
                            Telefono = p.PerTelefono,
                            FechaNacimiento = p.PerFechaNacimiento,
                            Genero = p.PerGenero,
                            Area = areaJoin != null ? areaJoin.AreNombre : "Sin Área",
                            Empleo = puestoJoin != null ? puestoJoin.PepNombre : "Sin Cargo",
                            Area2 = area2Join != null ? area2Join.AreNombre : "",
                            Empleo2 = puesto2Join != null ? puesto2Join.PepNombre : "",
                            Subsidiaria = subJoin != null ? subJoin.SubNombre : "",
                            EstatusReal = stJoin != null ? stJoin.StaTipoStatus : "Activo",
                            TipoReal = tiJoin != null ? tiJoin.TipNombre : "Empleado",
                            StaId = e.StaId ?? 1,
                            SupId = e.SupId,
                            TipId = e.TipId ?? 0,
                            FechaContratacion = e.EmpFechaContratacion,
                            SaeMensual = e.SAE_MENSUAL,
                            SaeQuincenal = e.SAE_QUINCENAL,
                            SaeSdi = e.SAE_SDI,
                            Banco = e.Banco,            // <--- AGREGAR ESTA
                            CuentaBancaria = e.CuentaBancaria,
                            ExpedientesTotales = 0,
                            ExpedientesCompletos = 0
                        };

            var empleados = await query.ToListAsync();

            var expedientesSubidos = await context.Expediente
                .Where(exp => exp.ExpRuta != null && exp.EmpId != null)
                .Select(exp => new { exp.EmpId, exp.DexId, exp.DeuId })
                .ToListAsync();

            var todasLasExclusiones = await context.ExpedienteExclusiones
                .Select(x => new { x.EmpId, x.DexId })
                .ToListAsync();

            var catalogoCompleto = await context.DetalleExpediente.ToListAsync();

            var documentosUnicosGlobal = await context.DetalleExpedienteUnico
                .Select(d => new { d.DeuId, d.EmpId })
                .ToListAsync();

            foreach (var emp in empleados)
            {
                var exclusionesEmp = todasLasExclusiones
                    .Where(x => x.EmpId == emp.Id)
                    .Select(x => x.DexId)
                    .ToList();

                // LÓGICA UNIFICADA: Todos los documentos aplican a todos, excepto si tienen exclusión manual
                var docsAplicablesDelCatalogo = catalogoCompleto.Where(doc =>
                {
                    // 1. Si está excluido manualmente, no se le pide
                    if (exclusionesEmp.Contains(doc.DexId)) return false;

                    // Si sobrevive al filtro de exclusión manual, es obligatorio para esta persona
                    return true;

                }).Select(d => d.DexId).ToList();

                // Documentos Únicos (Específicos de esta persona)
                var unicosDelEmpleado = documentosUnicosGlobal.Where(u => u.EmpId == emp.Id).Select(u => u.DeuId).ToList();

                // Totales: Catálogo filtrado + Documentos Únicos
                emp.ExpedientesTotales = docsAplicablesDelCatalogo.Count + unicosDelEmpleado.Count;

                // Completos: Comparar contra lo que realmente hay en la tabla Expediente
                int subidosCatalogo = expedientesSubidos.Count(x => x.EmpId == emp.Id && x.DexId.HasValue && docsAplicablesDelCatalogo.Contains(x.DexId.Value));
                int subidosUnicos = expedientesSubidos.Count(x => x.EmpId == emp.Id && x.DeuId.HasValue && unicosDelEmpleado.Contains(x.DeuId.Value));

                emp.ExpedientesCompletos = subidosCatalogo + subidosUnicos;
            }

            return empleados.OrderBy(e => e.EstatusReal == "Inactivo").ThenBy(e => e.ApellidoPaterno).ToList();
        }

        //MODIFICADO DIC 22

        // =================================================================
        // MODIFICADO: LECTURA DEL DOBLE ROL (Ahora usando IDbContextFactory para evitar concurrencia)
        public async Task<EmpleadoViewModel> GetEmpleadoByIdAsync(int id)
        {
            using var context = _dbFactory.CreateDbContext();

            // 1. Obtenemos el VM base
            var lista = await GetEmpleadosViewModelAsync();
            var vm = lista.FirstOrDefault(x => x.Id == id) ?? new EmpleadoViewModel();

            // 2. RECUPERAR INFORMACIÓN EXTRA Y ROLES
            if (vm.Id > 0 && vm.PerId > 0)
            {
                var empleadoEntity = await context.Empleados
                    .AsNoTracking()
                    .Include(e => e.SupervisorDirecto)
                        .ThenInclude(s => s.Per)
                    .FirstOrDefaultAsync(e => e.EmpId == vm.Id);

                if (empleadoEntity != null && !string.IsNullOrEmpty(empleadoEntity.AspNetUserId))
                {
                    // =========================================================================
                    // SOLUCIÓN DE CONCURRENCIA: 
                    // Leemos los roles directo de la BD con el Factory en lugar de usar _userManager
                    // =========================================================================
                    var roles = await (from ur in context.UserRoles
                                       join r in context.Roles on ur.RoleId equals r.Id
                                       where ur.UserId == empleadoEntity.AspNetUserId
                                       select r.Name).ToListAsync();

                    // 1. COMBO PRINCIPAL (Rol de Sistema):
                    var rolPrincipalEncontrado = roles.FirstOrDefault(r =>
                        !string.Equals(r, "SUPERVISOR", StringComparison.OrdinalIgnoreCase));

                    if (!string.IsNullOrEmpty(rolPrincipalEncontrado))
                    {
                        vm.RolSeleccionado = rolPrincipalEncontrado;
                    }
                    else
                    {
                        vm.RolSeleccionado = "Empleado";
                    }

                    // 2. COMBO SECUNDARIO (Función de Mando):
                    vm.AsignarRolSupervisor = roles.Any(r =>
                        string.Equals(r, "SUPERVISOR", StringComparison.OrdinalIgnoreCase));

                    // Nombre del Supervisor
                    if (empleadoEntity.SupervisorDirecto != null && empleadoEntity.SupervisorDirecto.Per != null)
                    {
                        vm.SupervisorNombre = $"{empleadoEntity.SupervisorDirecto.Per.PerNombre} {empleadoEntity.SupervisorDirecto.Per.PerApellidoPaterno}";
                    }
                    else
                    {
                        vm.SupervisorNombre = "No Asignado";
                    }

                    // Carga de Contactos de Emergencia
                    vm.ContactosEmergencia = await context.ContactosEmergencias
                        .AsNoTracking()
                        .Where(c => c.PerId == vm.PerId)
                        .Select(c => new ContactoEmergenciaDTO
                        {
                            CemId = c.CemId,
                            CemNombreCompleto = c.CemNombreCompleto,
                            CemParentesco = c.CemParentesco,
                            CemTelefono1 = c.CemTelefono1,
                            CemTelefono2 = c.CemTelefono2
                        })
                        .ToListAsync();

                    // Carga de Info Médica
                    var medico = await context.InformacionMedicas
                        .AsNoTracking()
                        .FirstOrDefaultAsync(m => m.PerId == vm.PerId);

                    if (medico != null)
                    {
                        vm.ImeId = medico.ImeId;
                        vm.ImeTipoSangre = medico.ImeTipoSangre;
                        vm.ImeAlergias = medico.ImeAlergias;
                        vm.ImePadecimientos = medico.ImePadecimientos;
                        vm.ImeObservaciones = medico.ImeObservaciones;
                    }

                    if (vm.ContactosEmergencia.Count == 0)
                    {
                        vm.ContactosEmergencia.Add(new ContactoEmergenciaDTO());
                    }
                }
            }

            if (vm.Id > 0)
            {
                // Verificamos si algún empleado activo lo tiene como supervisor
                vm.TieneSubordinados = await context.Empleados
                    .AnyAsync(e => e.SupId == vm.Id && e.StaId == 1);
            }

            // =========================================================================
            // Cargar Configuración de Conceptos Fijos (Beneficios y Deducciones)
            // =========================================================================
            var conceptosAsignadosDb = await context.Set<EmpleadoBeneficio>().Where(b => b.EmpId == vm.Id).ToListAsync();

            // 1. Buscamos todos los conceptos que sean recurrentes (Beneficios o Deducciones)
            var catalogoRecurrente = await context.Set<NominaConceptos>()
                .Where(c => c.Clasificacion == "BENEFICIO" || c.Clasificacion == "DEDUCCION")
                .OrderBy(c => c.Clasificacion).ThenBy(c => c.ConceptoId)
                .ToListAsync();

            // 2. Limpiamos y llenamos la lista del ViewModel
            vm.Beneficios.Clear();
            foreach (var cat in catalogoRecurrente)
            {
                var asignado = conceptosAsignadosDb.FirstOrDefault(b => b.ConId == cat.ConceptoId);
                vm.Beneficios.Add(new BeneficioEmpleadoDTO
                {
                    ConId = cat.ConceptoId,
                    ConceptoNombre = cat.Concepto,
                    Clasificacion = cat.Clasificacion, // <--- NUEVA LÍNEA: La leemos de la BD
                    MontoQ1 = asignado?.MontoQ1 ?? 0,
                    MontoQ2 = asignado?.MontoQ2 ?? 0
                });
            }

            return vm;
        }

        // =================================================================
        // 10. GUARDAR EMPLEADO (SOPORTE MULTI-ROL)
        // =================================================================
        public async Task SaveEmpleadoAsync(EmpleadoViewModel vm)
        {
            using var context = _dbFactory.CreateDbContext();

            vm.ApellidoPaterno = NormalizarApellido(vm.ApellidoPaterno);
            vm.ApellidoMaterno = NormalizarApellido(vm.ApellidoMaterno);

            int perIdParaGuardar = 0;
            string userIdProcesado = ""; // Guardaremos el ID del usuario aquí

            //var puesto = await context.PerfilPuesto.FirstOrDefaultAsync(p => p.PepNombre == vm.Empleo);
            //PerfilPuesto? puesto2 = null;
            //if (!string.IsNullOrEmpty(vm.Empleo2))
            //{
            //    puesto2 = await context.PerfilPuesto.FirstOrDefaultAsync(p => p.PepNombre == vm.Empleo2);
            //}

            var puesto = await context.PerfilPuesto
        .Include(p => p.Are) // Aseguramos traer el área
        .FirstOrDefaultAsync(p => p.PepNombre == vm.Empleo && p.Are.AreNombre == vm.Area);

            PerfilPuesto? puesto2 = null;
            if (!string.IsNullOrEmpty(vm.Empleo2) && !string.IsNullOrEmpty(vm.Area2))
            {
                puesto2 = await context.PerfilPuesto
                    .Include(p => p.Are)
                    .FirstOrDefaultAsync(p => p.PepNombre == vm.Empleo2 && p.Are.AreNombre == vm.Area2);
            }




            var sub = await context.Subsidiaria.FirstOrDefaultAsync(s => s.SubNombre == vm.Subsidiaria);

            // Determinamos el nombre del Rol Principal (Ej. "RRHH") basado en el Tipo o Selección
            string nombreRolPrincipal = !string.IsNullOrEmpty(vm.RolSeleccionado) ? vm.RolSeleccionado : "Empleado";

            // ==========================================
            // ESCENARIO 1: CREAR NUEVO
            // ==========================================
            if (vm.Id == 0)
            {
                // 1. CREAR USUARIO IDENTITY
                string emailCorporativo = await GenerarCorreoCorporativoAsync(vm.Nombre, vm.ApellidoPaterno);

                var newUser = new ApplicationUser
                {
                    UserName = emailCorporativo,
                    Email = emailCorporativo,
                    EsPrimerIngreso = true,
                    UsuCodigo = null,
                    EmailConfirmed = true
                };
                string passwordPorDefecto = "Argos" + DateTime.Now.Year + ".";
                var result = await _userManager.CreateAsync(newUser, passwordPorDefecto);
                if (!result.Succeeded) throw new Exception($"Error Identity: {string.Join(", ", result.Errors.Select(e => e.Description))}");

                userIdProcesado = newUser.Id; // Guardamos ID

                // CREAR MOODLE
                try
                {
                    string passwordMoodle = passwordPorDefecto + "*";
                    await _moodleService.CrearUsuarioMoodleAsync(
                        newUser.UserName, passwordMoodle,
                        vm.Nombre, vm.ApellidoPaterno, newUser.Email
                    );
                }
                catch (Exception ex) { Console.WriteLine("Error Moodle: " + ex.Message); }

                // 2. CREAR PERSONA
                var nuevaPersona = new Persona
                {
                    PerNombre = vm.Nombre,
                    PerApellidoPaterno = vm.ApellidoPaterno,
                    PerApellidoMaterno = vm.ApellidoMaterno,
                    PerEmail = vm.Email,
                    PerTelefono = vm.Telefono,
                    PerGenero = vm.Genero,
                    PerFechaNacimiento = vm.FechaNacimiento,
                    PerFechaRegistro = DateTime.Now,
                    PepId = puesto?.PepId,
                    PepId2 = puesto2?.PepId,
                    SubId = sub?.SubId
                };
                context.Personas.Add(nuevaPersona);
                await context.SaveChangesAsync();

                perIdParaGuardar = nuevaPersona.PerId;

                // 3. CREAR EMPLEADO
                var nuevoEmpleado = new Empleado
                {
                    PerId = nuevaPersona.PerId,
                    EmpCodigo = vm.CodigoEmpleado,
                    EmpFechaRegistro = DateTime.Now,

                    StaId = vm.StaId > 0 ? vm.StaId : 1,
                    EmpFechaContratacion = vm.FechaContratacion ?? DateTime.Now,
                    SupId = vm.SupId,
                    TipId = vm.TipId > 0 ? vm.TipId : null,
                    AspNetUserId = newUser.Id,
                    SAE_MENSUAL = vm.SaeMensual,
                    SAE_QUINCENAL = vm.SaeQuincenal,
                    SAE_SDI = vm.SaeSdi,
                    Banco = vm.Banco,
                    CuentaBancaria = vm.CuentaBancaria,

                };
                context.Empleados.Add(nuevoEmpleado);
                await context.SaveChangesAsync();

                vm.Id = nuevoEmpleado.EmpId; // Le pasamos el nuevo ID autogenerado al formulario
            }
            // ==========================================
            // ESCENARIO 2: EDITAR EXISTENTE
            // ==========================================
            else
            {
                var empleado = await context.Empleados.FirstOrDefaultAsync(e => e.EmpId == vm.Id);
                if (empleado != null)
                {
                    perIdParaGuardar = empleado.PerId ?? 0;
                    userIdProcesado = empleado.AspNetUserId; // Guardamos ID

                    // Actualizar Persona
                    var persona = await context.Personas.FirstOrDefaultAsync(p => p.PerId == empleado.PerId);
                    if (persona != null)
                    {
                        persona.PerNombre = vm.Nombre;
                        persona.PerApellidoPaterno = vm.ApellidoPaterno;
                        persona.PerApellidoMaterno = vm.ApellidoMaterno;
                        persona.PerEmail = vm.Email;
                        persona.PerTelefono = vm.Telefono;
                        persona.PerGenero = vm.Genero;
                        persona.PerFechaNacimiento = vm.FechaNacimiento;
                        persona.PepId = puesto?.PepId;
                        persona.SubId = sub?.SubId;
                        context.Personas.Update(persona);
                    }


                    // Actualizar Empleado
                    if (vm.StaId > 0) empleado.StaId = vm.StaId;
                    empleado.EmpCodigo = vm.CodigoEmpleado;
                    persona.PepId = puesto?.PepId;
                    persona.PepId2 = puesto2?.PepId;
                    empleado.SupId = vm.SupId;
                    empleado.EmpFechaContratacion = vm.FechaContratacion;
                    empleado.EmpFechaBaja = vm.FechaBaja; // <--- LÍNEA NUEVA
                    if (vm.TipId > 0) empleado.TipId = vm.TipId;

                    // ---> ESTAS 3 LÍNEAS FALTABAN PARA GUARDAR AL EDITAR <---
                    empleado.SAE_MENSUAL = vm.SaeMensual;
                    empleado.SAE_QUINCENAL = vm.SaeQuincenal;
                    empleado.SAE_SDI = vm.SaeSdi;
                    empleado.Banco = vm.Banco;
                    empleado.CuentaBancaria = vm.CuentaBancaria;

                    context.Empleados.Update(empleado);
                    await context.SaveChangesAsync();
                }
            }

            // =========================================================================
            // LÓGICA DE ROLES IDENTITY (SE EJECUTA PARA AMBOS CASOS)
            // AQUI ESTA LA MAGIA DEL DOBLE ROL
            // =========================================================================
            if (!string.IsNullOrEmpty(userIdProcesado))
            {
                var user = await _userManager.FindByIdAsync(userIdProcesado);
                if (user != null)
                {
                    // 1. Limpieza Total: Quitamos roles actuales para evitar conflictos
                    var currentRoles = await _userManager.GetRolesAsync(user);
                    if (currentRoles.Any())
                    {
                        await _userManager.RemoveFromRolesAsync(user, currentRoles);
                    }

                    // 2. Asignar Rol Principal (RRHH, HSE, Empleado, etc.)
                    if (!await _roleManager.RoleExistsAsync(nombreRolPrincipal))
                        await _roleManager.CreateAsync(new IdentityRole(nombreRolPrincipal));

                    await _userManager.AddToRoleAsync(user, nombreRolPrincipal);

                    // 3. GESTIÓN DEL ROL SUPERVISOR (SECUNDARIO)
                    if (vm.AsignarRolSupervisor)
                    {
                        // Si el usuario marcó el combo, agregamos el rol SUPERVISOR adicionalmente
                        if (!await _roleManager.RoleExistsAsync("SUPERVISOR"))
                            await _roleManager.CreateAsync(new IdentityRole("SUPERVISOR"));

                        await _userManager.AddToRoleAsync(user, "SUPERVISOR");
                    }
                    // Si vm.AsignarRolSupervisor es false, simplemente no lo agregamos 
                    // (ya lo borramos en el paso 1 de limpieza).
                }
            }

            // =========================================================================
            // GUARDAR DATOS MÉDICOS Y DE EMERGENCIA (Sin cambios respecto a tu código)
            // =========================================================================
            if (perIdParaGuardar > 0)
            {
                // 1. Contactos Emergencia
                var contactosEnBd = await context.ContactosEmergencias
                                         .Where(c => c.PerId == perIdParaGuardar).ToListAsync();

                foreach (var itemVM in vm.ContactosEmergencia)
                {
                    if (!string.IsNullOrWhiteSpace(itemVM.CemNombreCompleto))
                    {
                        if (itemVM.CemId == 0)
                        {
                            context.ContactosEmergencias.Add(new ContactoEmergencia
                            {
                                PerId = perIdParaGuardar,
                                CemNombreCompleto = itemVM.CemNombreCompleto,
                                CemParentesco = itemVM.CemParentesco,
                                CemTelefono1 = itemVM.CemTelefono1,
                                CemTelefono2 = itemVM.CemTelefono2
                            });
                        }
                        else
                        {
                            var existente = contactosEnBd.FirstOrDefault(c => c.CemId == itemVM.CemId);
                            if (existente != null)
                            {
                                existente.CemNombreCompleto = itemVM.CemNombreCompleto;
                                existente.CemParentesco = itemVM.CemParentesco;
                                existente.CemTelefono1 = itemVM.CemTelefono1;
                                existente.CemTelefono2 = itemVM.CemTelefono2;
                                contactosEnBd.Remove(existente);
                            }
                        }
                    }
                }
                if (contactosEnBd.Count > 0) context.ContactosEmergencias.RemoveRange(contactosEnBd);

                // 2. Info Médica
                var medico = await context.InformacionMedicas.FirstOrDefaultAsync(m => m.PerId == perIdParaGuardar);
                if (medico == null)
                {
                    medico = new InformacionMedica { PerId = perIdParaGuardar };
                    context.InformacionMedicas.Add(medico);
                }
                medico.ImeTipoSangre = vm.ImeTipoSangre;
                medico.ImeAlergias = vm.ImeAlergias;
                medico.ImePadecimientos = vm.ImePadecimientos;
                medico.ImeObservaciones = vm.ImeObservaciones;

                await context.SaveChangesAsync();
            }

            ////// Beneficios

            // Guardar Beneficios (Borrón y cuenta nueva para que sea más fácil)
            var beneficiosViejos = await context.Set<EmpleadoBeneficio>().Where(b => b.EmpId == vm.Id).ToListAsync();
            context.Set<EmpleadoBeneficio>().RemoveRange(beneficiosViejos);

            var nuevosBeneficios = vm.Beneficios
                .Where(b => b.MontoQ1 > 0 || b.MontoQ2 > 0) // Solo guardamos los que tengan dinero
                .Select(b => new EmpleadoBeneficio
                {
                    EmpId = vm.Id,
                    ConId = b.ConId,
                    MontoQ1 = b.MontoQ1,
                    MontoQ2 = b.MontoQ2
                });
            context.Set<EmpleadoBeneficio>().AddRange(nuevosBeneficios);
            await context.SaveChangesAsync();
        }

        // =================================================================
        // 11. EXPEDIENTES (BINARIOS) - CON AJUSTE PARA ID 18 Y 19
        // =================================================================
        public async Task<List<ExpedienteViewModel>> GetExpedientesByEmpleadoIdAsync(int empleadoId)
        {
            using var context = _dbFactory.CreateDbContext();

            var empleado = await context.Empleados.AsNoTracking().FirstOrDefaultAsync(e => e.EmpId == empleadoId);
            if (empleado == null) return new List<ExpedienteViewModel>();

            int tipoEmpleadoId = empleado.TipId ?? 0;
            DateTime fechaIngresoEmp = (empleado.EmpFechaContratacion ?? empleado.EmpFechaRegistro ?? DateTime.MinValue).Date;

            var catalogoDocs = await context.DetalleExpediente.ToListAsync();
            var docsUnicos = await context.DetalleExpedienteUnico.Where(u => u.EmpId == empleadoId).ToListAsync();
            var expedientesSubidos = await context.Expediente.Where(e => e.EmpId == empleadoId).ToListAsync();

            // NUEVO: Cargar exclusiones de este empleado
            var exclusiones = await context.ExpedienteExclusiones
                .Where(x => x.EmpId == empleadoId)
                .Select(x => x.DexId)
                .ToListAsync();

            var resultado = new List<ExpedienteViewModel>();

            int[] docsBecarios = { 4, 5, 6, 9, 13, 15, 17, 19 };
            int[] docsExcluidosEmpleado = { 19 };

            foreach (var doc in catalogoDocs)
            {
                // Solo ocultamos si existe una exclusión explícita para este empleado y este documento
                bool mostrar = !exclusiones.Contains(doc.DexId);

                if (mostrar)
                {
                    var subido = expedientesSubidos.FirstOrDefault(e => e.DexId == doc.DexId);
                    resultado.Add(new ExpedienteViewModel
                    {
                        DexId = doc.DexId,
                        DeuId = null,
                        EmpleadoId = empleadoId,
                        Nomenclatura = doc.DexNomenclatura,
                        Nombre = doc.DexNombre,
                        ExpId = subido?.ExpId ?? 0,
                        NombreArchivo = subido?.ExpRuta,
                        FechaSubida = subido?.ExpFechasubido,
                        FechaCaducidad = subido?.ExpFechacaducidad
                    });
                }
            }

            // Documentos únicos (sin cambios)
            foreach (var unico in docsUnicos)
            {
                var subido = expedientesSubidos.FirstOrDefault(e => e.DeuId == unico.DeuId);
                resultado.Add(new ExpedienteViewModel
                {
                    DexId = 0,
                    DeuId = unico.DeuId,
                    EmpleadoId = empleadoId,
                    Nomenclatura = unico.DeuNomenclatura,
                    Nombre = unico.DeuNombre,
                    ExpId = subido?.ExpId ?? 0,
                    NombreArchivo = subido?.ExpRuta,
                    FechaSubida = subido?.ExpFechasubido,
                    FechaCaducidad = subido?.ExpFechacaducidad
                });
            }

            return resultado;
        }

        public async Task UpdateExpedienteConArchivoAsync(ExpedienteViewModel vm, System.IO.Stream fileStream)
        {
            using var context = _dbFactory.CreateDbContext();

            // 1. Convertir el archivo a Bytes
            byte[] fileBytes;
            using (var memoryStream = new System.IO.MemoryStream())
            {
                await fileStream.CopyToAsync(memoryStream);
                fileBytes = memoryStream.ToArray();
            }

            // 2. BUSQUEDA INTELIGENTE (ADAPTADA)
            Expediente? expedienteExistente = null;

            // Decidimos cómo buscar basándonos en si trae ID de documento único
            if (vm.DeuId.HasValue && vm.DeuId.Value > 0)
            {
                // CAMINO A: Buscamos por ID Único (DeuId)
                expedienteExistente = await context.Expediente
                    .FirstOrDefaultAsync(x => x.EmpId == vm.EmpleadoId && x.DeuId == vm.DeuId);
            }
            else
            {
                // CAMINO B: Buscamos por Catálogo Estándar (DexId)
                expedienteExistente = await context.Expediente
                    .FirstOrDefaultAsync(x => x.EmpId == vm.EmpleadoId && x.DexId == vm.DexId);
            }

            if (expedienteExistente == null)
            {
                // --- ESCENARIO: NO EXISTE EN BD -> CREAR NUEVO ---
                var nuevoExpediente = new Expediente
                {
                    EmpId = vm.EmpleadoId,
                    ExpRuta = vm.NombreArchivo,
                    ExpArchivo = fileBytes,
                    ExpFechasubido = DateTime.Now,
                    ExpFechacaducidad = vm.FechaCaducidad
                };

                // ASIGNAMOS LA RELACIÓN CORRECTA AL CREAR
                if (vm.DeuId.HasValue && vm.DeuId.Value > 0)
                {
                    nuevoExpediente.DeuId = vm.DeuId.Value; // Conectamos a tabla única
                    nuevoExpediente.DexId = null;           // Desconectamos catálogo
                }
                else
                {
                    nuevoExpediente.DexId = vm.DexId;       // Conectamos catálogo
                    nuevoExpediente.DeuId = null;           // Desconectamos tabla única
                }

                context.Expediente.Add(nuevoExpediente);
            }
            else
            {
                // --- ESCENARIO: YA EXISTE -> ACTUALIZAR ---
                expedienteExistente.ExpRuta = vm.NombreArchivo;
                expedienteExistente.ExpArchivo = fileBytes;
                expedienteExistente.ExpFechacaducidad = vm.FechaCaducidad;
                expedienteExistente.ExpFechasubido = DateTime.Now;

                context.Expediente.Update(expedienteExistente);
            }

            await context.SaveChangesAsync();
        }

        public async Task DeleteArchivoAsync(int expId)
        {
            using var context = _dbFactory.CreateDbContext();

            var expediente = await context.Expediente.FindAsync(expId);
            if (expediente != null)
            {
                expediente.ExpArchivo = null; expediente.ExpRuta = null; expediente.ExpFechasubido = null; expediente.ExpFechacaducidad = null;
                await context.SaveChangesAsync();
            }
        }

        // =================================================================
        // 12. UTILIDADES IDENTITY
        // =================================================================
        private async Task<string> GenerarCorreoCorporativoAsync(string nombre, string apellido)
        {
            string n = nombre.Trim().ToLower(); string a = apellido.Trim().ToLower();
            string baseUser = $"{n[0]}{a}"; string dominio = "@argoses.com";
            string emailGenerado = baseUser + dominio;
            int contador = 1;
            while (await _userManager.FindByEmailAsync(emailGenerado) != null)
            {
                emailGenerado = $"{baseUser}{contador}{dominio}"; contador++;
            }
            return emailGenerado;
        }


        // --- ÁREAS ---
        public async Task AddAreaAsync(string nombreArea)
        {
            using var context = _dbFactory.CreateDbContext();
            // 1. Calcular ID manual (Max + 1)
            int nextId = 1;
            if (await context.Area.AnyAsync())
            {
                nextId = await context.Area.MaxAsync(a => a.AreId) + 1;
            }
            // 2. CREAR AUTOMÁTICAMENTE EN MOODLE
            int moodleCohortId = 0;
            try
            {
                // Usamos "AREA_XX" como ID único para Moodle
                string idNumber = $"AREA_{nextId}";
                moodleCohortId = await _moodleService.CrearCohorteMoodleAsync(nombreArea, idNumber);
            }
            catch { /* Log error pero no detener */ }

            var nuevaArea = new Area
            {
                AreId = nextId,
                AreNombre = nombreArea.Trim(),
                MoodleCohortId = moodleCohortId > 0 ? moodleCohortId : null // <---22Dic GUARDAMOS EL VÍNCULO
            };

            context.Area.Add(nuevaArea);
            await context.SaveChangesAsync();
        }

        public async Task DeleteAreaAsync(int areaId)
        {
            using var context = _dbFactory.CreateDbContext();
            // Validar que no tenga puestos asociados para no romper integridad
            bool tienePuestos = await context.PerfilPuesto.AnyAsync(p => p.AreId == areaId);
            if (tienePuestos) throw new Exception("No se puede eliminar el área porque tiene puestos asociados.");

            var area = await context.Area.FindAsync(areaId);
            if (area != null)
            {
                context.Area.Remove(area);
                await context.SaveChangesAsync();
            }
        }

        public async Task UpdateAreaAsync(Area area)
        {
            using var context = _dbFactory.CreateDbContext();

            var existente = await context.Area.FindAsync(area.AreId);
            if (existente != null)
            {
                existente.AreNombre = area.AreNombre;
                context.Area.Update(existente);
                await context.SaveChangesAsync();
            }
        }

        // --- PERFILES DE PUESTO ---
        public async Task<List<PerfilPuesto>> GetPuestosPorAreaAsync(int areaId)
        {
            using var context = _dbFactory.CreateDbContext();

            return await context.PerfilPuesto
                .Where(p => p.AreId == areaId)
                .OrderBy(p => p.PepNombre)
                .ToListAsync();
        }

        //16DIC
        public async Task<PerfilPuesto> GetPuestoByIdAsync(int id)
        {
            using var context = _dbFactory.CreateDbContext();

            return await context.PerfilPuesto.FindAsync(id);
        }

        //16DIC
        public async Task UpdatePuestoAsync(PerfilPuesto puesto, Stream? fileStream = null, string? fileName = null)
        {
            using var context = _dbFactory.CreateDbContext();

            var existente = await context.PerfilPuesto.FindAsync(puesto.PepId);
            if (existente != null)
            {
                existente.PepNombre = puesto.PepNombre;
                existente.PepDescripcion = puesto.PepDescripcion; // Nuevo campo

                // Si viene archivo nuevo, guardamos los bytes
                if (fileStream != null)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await fileStream.CopyToAsync(memoryStream);
                        existente.PepArchivo = memoryStream.ToArray(); // Nuevo campo byte[]
                    }
                }

                context.PerfilPuesto.Update(existente);
                await context.SaveChangesAsync();
            }
        }

        public async Task AddPuestoAsync(int areaId, string nombrePuesto)
        {
            using var context = _dbFactory.CreateDbContext();
            // 1. Calcular ID manual (Max + 1)
            int nextId = 1;
            if (await context.PerfilPuesto.AnyAsync())
            {
                nextId = await context.PerfilPuesto.MaxAsync(p => p.PepId) + 1;
            }

            var nuevoPuesto = new PerfilPuesto
            {
                PepId = nextId,
                AreId = areaId,
                PepNombre = nombrePuesto.Trim()
            };

            context.PerfilPuesto.Add(nuevoPuesto);
            await context.SaveChangesAsync();
        }

        public async Task DeletePuestoAsync(int puestoId)
        {
            using var context = _dbFactory.CreateDbContext();
            // Validar si hay empleados con este puesto (Opcional, recomendado)
            // bool tieneEmpleados = await context.Personas.AnyAsync(p => p.PepId == puestoId);
            // if (tieneEmpleados) throw new Exception("No se puede eliminar, hay empleados con este cargo.");

            var puesto = await context.PerfilPuesto.FindAsync(puestoId);
            if (puesto != null)
            {
                context.PerfilPuesto.Remove(puesto);
                await context.SaveChangesAsync();
            }
        }


        // =================================================================
        // 13. ASIGNAR SUPERVISORES A EMPLEADOS
        // =================================================================


        // 1. Obtener Empleados filtrados por ROL (Identity) y por ÁREA (Base de Datos)
        public async Task<List<EmpleadoViewModel>> GetEmpleadosPorRolYAreaAsync(string nombreRol, string nombreArea)
        {
            using var context = _dbFactory.CreateDbContext();
            // A. Obtener todos los usuarios que tienen ese ROL en Identity
            var usersInRole = await _userManager.GetUsersInRoleAsync(nombreRol);
            var userIds = usersInRole.Select(u => u.Id).ToList();

            // B. Consultar Empleados que coincidan con esos IDs y con el Área solicitada
            var query = from e in context.Empleados
                        join p in context.Personas on e.PerId equals p.PerId
                        join puesto in context.PerfilPuesto on p.PepId equals puesto.PepId
                        join area in context.Area on puesto.AreId equals area.AreId

                        // Filtro 1: Que el empleado esté vinculado a un usuario con el Rol buscado
                        where userIds.Contains(e.AspNetUserId)

                        // Filtro 2: Que pertenezca al Área seleccionada
                        && area.AreNombre == nombreArea

                        select new EmpleadoViewModel
                        {
                            Id = e.EmpId,
                            Nombre = p.PerNombre,
                            ApellidoPaterno = p.PerApellidoPaterno,
                            ApellidoMaterno = p.PerApellidoMaterno,
                            Empleo = puesto.PepNombre,
                            Area = area.AreNombre
                        };

            return await query.ToListAsync();
        }

        // 2. Método para obtener subordinados que YA tienen supervisor (opcional, para ver la lista)
        public async Task<List<EmpleadoViewModel>> GetSubordinadosPorSupervisorAsync(int supervisorEmpId)
        {
            using var context = _dbFactory.CreateDbContext();
            // Asumiendo que SupId en Empleado apunta al EmpId del supervisor
            // Si SupId apunta a una tabla 'Supervisor', habría que hacer un join extra.
            // Aquí asumo la relación recursiva simple: Empleado -> Jefe (Empleado)

            var query = from e in context.Empleados
                        join p in context.Personas on e.PerId equals p.PerId
                        join puesto in context.PerfilPuesto on p.PepId equals puesto.PepId

                        // Si tu SupId es directo al ID de empleado:
                        where e.SupId == supervisorEmpId // O e.Sup.EmpId si usas la tabla intermedia

                        select new EmpleadoViewModel
                        {
                            Id = e.EmpId,
                            Nombre = p.PerNombre,
                            ApellidoPaterno = p.PerApellidoPaterno,
                            Empleo = puesto.PepNombre
                        };

            return await query.ToListAsync();
        }

        // 3. Guardar la Asignación
        public async Task AsignarSupervisorAsync(int supervisorEmpId, List<int> empleadosIds)
        {
            using var context = _dbFactory.CreateDbContext();
            // Buscamos a los empleados seleccionados
            var empleados = await context.Empleados
                .Where(e => empleadosIds.Contains(e.EmpId))
                .ToListAsync();

            foreach (var emp in empleados)
            {
                // Aquí asignamos el ID del supervisor.
                // NOTA: Si tienes una tabla intermedia 'Supervisors', primero debes buscar/crear el registro ahí
                // y asignar ese ID. Aquí asumo que SupId es el ID del Empleado Jefe.

                emp.SupId = supervisorEmpId;
                // Si tu lógica usa una tabla Supervisors:
                // var supervisorRecord = await context.Supervisors.FirstOrDefaultAsync(s => s.EmpId == supervisorEmpId);
                // emp.SupId = supervisorRecord.SupId;
            }

            await context.SaveChangesAsync();
        }

        public async Task<List<EmpleadoViewModel>> GetSupervisoresAsync()
        {
            using var context = _dbFactory.CreateDbContext();
            // A. Obtener IDs de usuarios con Rol "Supervisor"
            var usersInRole = await _userManager.GetUsersInRoleAsync("Supervisor");
            var userIds = usersInRole.Select(u => u.Id).ToList();

            // B. Buscar empleados que coincidan con esos usuarios
            var query = from e in context.Empleados
                        join p in context.Personas on e.PerId equals p.PerId
                        join puesto in context.PerfilPuesto on p.PepId equals puesto.PepId

                        // Filtro: Solo los que tienen usuario vinculado y ese usuario es Supervisor
                        where e.AspNetUserId != null && userIds.Contains(e.AspNetUserId)

                        orderby p.PerNombre // Ordenar alfabéticamente

                        select new EmpleadoViewModel
                        {
                            Id = e.EmpId,
                            Nombre = p.PerNombre,
                            ApellidoPaterno = p.PerApellidoPaterno,
                            ApellidoMaterno = p.PerApellidoMaterno,
                            Empleo = puesto.PepNombre
                        };

            return await query.ToListAsync();
        }




        // =================================================================
        // GESTION DE USUARIOS (CAMBIAR ROL Y REINICIAR CONTRASEÑAS)
        // =================================================================



        // 1. OBTENER LISTA DE USUARIOS (ADMINISTRADOR)
        public async Task<List<UsuarioSistemaViewModel>> GetUsuariosSistemaAsync()
        {
            using var context = _dbFactory.CreateDbContext();
            // Obtenemos todos los roles para mapeo rápido
            var roles = await _roleManager.Roles.ToListAsync();

            // 1. Consulta base: Join Users -> Empleados -> Personas
            // Usamos _userManager.Users o context.Users (que heredaste de IdentityDbContext)
            var query = from u in context.Users
                        join e in context.Empleados on u.Id equals e.AspNetUserId into empGroup
                        from emp in empGroup.DefaultIfEmpty() // Left Join por si hay admins sin ficha de empleado
                        join p in context.Personas on emp.PerId equals p.PerId into perGroup
                        from per in perGroup.DefaultIfEmpty()
                        select new
                        {
                            User = u,
                            Nombre = per != null ? per.PerNombre : "Usuario",
                            Paterno = per != null ? per.PerApellidoPaterno : "Sistema"
                        };

            var rawData = await query.ToListAsync();
            var listaFinal = new List<UsuarioSistemaViewModel>();

            // 2. Iteramos para obtener el ROL de cada uno (Identity maneja los roles en tablas separadas)
            foreach (var item in rawData)
            {
                // Obtener roles del usuario (devuelve lista de strings)
                var userRoles = await _userManager.GetRolesAsync(item.User);
                var rolPrincipal = userRoles.FirstOrDefault() ?? "Sin Rol";

                listaFinal.Add(new UsuarioSistemaViewModel
                {
                    UserId = item.User.Id,
                    EmpleadoNombreCompleto = $"{item.Nombre} {item.Paterno}",
                    Email = item.User.Email,
                    CurrentRole = rolPrincipal,
                    SelectedRoleDraft = rolPrincipal, // Inicializamos el draft igual al actual
                    EsPrimerIngreso = item.User.EsPrimerIngreso // true = default, false = cambiada
                });
            }

            return listaFinal;
        }

        // 2. OBTENER LISTA DE ROLES (PARA EL COMBOBOX)
        public async Task<List<string>> GetAllRolesAsync()
        {
            return await _roleManager.Roles.Select(r => r.Name).ToListAsync();
        }

        // 3. CAMBIAR ROL (POST)
        public async Task UpdateUserRoleAsync(string userId, string newRole)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) throw new Exception("Usuario no encontrado");

            // Obtener roles actuales
            var currentRoles = await _userManager.GetRolesAsync(user);

            // Remover todos los roles actuales
            if (currentRoles.Any())
            {
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
            }

            // Agregar el nuevo rol
            if (!string.IsNullOrEmpty(newRole))
            {
                await _userManager.AddToRoleAsync(user, newRole);
            }
        }

        // 4. REINICIAR CONTRASEÑA (RESET)
        public async Task ResetPasswordToDefaultAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) throw new Exception("Usuario no encontrado");

            // Generar token de reset (necesario para cambiar pass sin saber el anterior)
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            // Contraseña por defecto: "Argos" + AÑO + "." (Misma lógica que en SaveEmpleado)
            string defaultPassword = "Argos" + DateTime.Now.Year + ".";

            var result = await _userManager.ResetPasswordAsync(user, token, defaultPassword);

            if (result.Succeeded)
            {
                // IMPORTANTE: Volvemos a marcarlo como Primer Ingreso para que el sistema sepa
                // que tiene la contraseña por defecto.
                user.EsPrimerIngreso = true;
                await _userManager.UpdateAsync(user);
            }
            else
            {
                throw new Exception($"Error al reiniciar: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }


        // =================================================================
        // EMPLEADOS EXPEDIENTES
        // =================================================================

        public async Task<EmpleadoViewModel> GetPerfilEmpleadoLogueadoAsync(string aspNetUserId)
        {
            using var context = _dbFactory.CreateDbContext();
            // Buscamos el empleado que tenga ese AspNetUserId
            var empleado = await context.Empleados
                .Include(e => e.Usuario) // Incluimos para validar
                .FirstOrDefaultAsync(e => e.AspNetUserId == aspNetUserId);

            if (empleado == null) return null; // El usuario existe pero no tiene ficha de empleado

            // Reutilizamos tu método existente para obtener el ViewModel completo
            // Esto nos ahorra reescribir todo el mapeo de Personas, Puestos, Áreas, etc.
            return await GetEmpleadoByIdAsync(empleado.EmpId);
        }


        // =================================================================
        // EPP
        // =================================================================

        // --- GESTIÓN DE EPPs (ENTREGAS) ---

        // 1. OBTENER LISTA PARA LA TABLA (JOIN CONCATENADO)
        public async Task<List<EntregaEppViewModel>> GetEntregasEppAsync()
        {
            using var context = _dbFactory.CreateDbContext();
            // Usamos .Select directamente para que EF Core construya un SQL 
            // que NO incluya la columna ENT_EVIDENCIA (bytes), solo verifique si es nula.

            var query = context.EntregaEpp
                .AsNoTracking() // Importante para solo lectura, mejora rendimiento
                .OrderByDescending(e => e.EntFecha)
                .Select(e => new EntregaEppViewModel
                {
                    EntId = e.EntId,
                    Fecha = e.EntFecha ?? DateTime.Now,
                    // MODIFICANDO DIC 17
                    NombreEmpleado = e.Empleado.Per.PerNombre + " " + e.Empleado.Per.PerApellidoPaterno + " " + (e.Empleado.Per.PerApellidoMaterno ?? ""),

                    // Subconsulta para concatenar los Nombres de EPP (EF Core lo traduce)
                    ListaEpps = string.Join(", ", e.Detalles.Select(d => d.TipoEpp.TepTipo)),

                    // Subconsulta para concatenar Cantidades
                    ListaCantidades = e.Detalles.Sum(d => d.DetCantidad).ToString(), //MODIFICADO DIC 17

                    // LA CLAVE: Verificamos si es null en SQL, NO traemos los bytes
                    TieneEvidencia = e.EntEvidencia != null
                });

            return await query.ToListAsync();
        }

        // 2. OBTENER TIPOS DE EPP PARA EL FORMULARIO (CHECKBOXES)
        public async Task<List<SeleccionEppItem>> GetCatalogoEppParaSeleccionAsync()
        {
            using var context = _dbFactory.CreateDbContext();

            return await context.TipoEpps
                .OrderBy(t => t.TepTipo)
                .Select(t => new SeleccionEppItem
                {
                    TepId = t.TepId,
                    Nombre = t.TepTipo ?? "Desconocido",
                    IsSelected = false,
                    Cantidad = 1
                }).ToListAsync();
        }

        // 3. OBTENER EMPLEADOS POR ÁREA (PARA EL COMBOBOX EN CASCADA)
        public async Task<List<EmpleadoViewModel>> GetEmpleadosPorAreaIdAsync(int areaId)
        {
            using var context = _dbFactory.CreateDbContext();
            // Reutilizamos lógica de joins
            var query = from e in context.Empleados
                        join p in context.Personas on e.PerId equals p.PerId
                        join puesto in context.PerfilPuesto on p.PepId equals puesto.PepId
                        where puesto.AreId == areaId && (e.StaId == 1) // Solo activos
                        orderby p.PerNombre
                        select new EmpleadoViewModel
                        {
                            Id = e.EmpId,
                            Nombre = p.PerNombre,
                            ApellidoPaterno = p.PerApellidoPaterno,
                            ApellidoMaterno = p.PerApellidoMaterno// MODIFICADO DIC 17
                        };
            return await query.ToListAsync();
        }


        // 4. GUARDAR ENTREGA (MAESTRO - DETALLE) - VERSIÓN CORREGIDA
        // 4. GUARDAR ENTREGA (VERSIÓN SIMPLIFICADA - SIN STREAM) MODIFICADO DIC 19
        public async Task SaveEntregaEppAsync(EntregaEppViewModel vm)
        {
            using var context = _dbFactory.CreateDbContext();
            // Validaciones
            if (vm.EmpleadoId <= 0)
                throw new Exception("Debe seleccionar un empleado válido.");

            if (!vm.EppsSeleccionados.Any(x => x.IsSelected))
                throw new Exception("Debe seleccionar al menos un EPP.");

            if (vm.EntId == 0)
            {
                // CREACIÓN
                var nuevaEntrega = new EntregaEpp
                {
                    EmpId = vm.EmpleadoId,
                    EntFecha = vm.Fecha,
                    EntEvidencia = vm.EvidenciaBytes
                };

                context.EntregaEpp.Add(nuevaEntrega);
                await context.SaveChangesAsync(); // Genera EntId

                // Crear detalles
                var detalles = vm.EppsSeleccionados
                    .Where(x => x.IsSelected)
                    .Select(item => new DetalleEntregaEpp
                    {
                        EntId = (vm.EntId == 0) ? nuevaEntrega.EntId : vm.EntId,
                        TepId = item.TepId,
                        DetCantidad = item.Cantidad,
                        // NO asignar TipoEpp ni Entrega, solo los IDs
                        DetTallaModelo = item.TallaModelo,
                        DetEsNuevo = item.EsNuevo,
                        DetDevolvioAnterior = item.DevolvioAnterior
                    })
                    .ToList();

                context.DetalleEntregaEpp.AddRange(detalles);
                await context.SaveChangesAsync();
            }
            else
            {
                // EDICIÓN (similar)
                var entregaExistente = await context.EntregaEpp
                    .Include(e => e.Detalles)
                    .FirstOrDefaultAsync(e => e.EntId == vm.EntId);

                if (entregaExistente == null)
                    throw new Exception("No se encontró la entrega.");

                entregaExistente.EmpId = vm.EmpleadoId;
                entregaExistente.EntFecha = vm.Fecha;

                if (vm.EvidenciaBytes != null && vm.EvidenciaBytes.Length > 0)
                {
                    entregaExistente.EntEvidencia = vm.EvidenciaBytes;
                }

                context.DetalleEntregaEpp.RemoveRange(entregaExistente.Detalles);

                var detallesNuevos = vm.EppsSeleccionados
                    .Where(x => x.IsSelected)
                    .Select(item => new DetalleEntregaEpp
                    {
                        EntId = entregaExistente.EntId,
                        TepId = item.TepId,
                        DetCantidad = item.Cantidad,

                        DetTallaModelo = item.TallaModelo,
                        DetEsNuevo = item.EsNuevo,
                        DetDevolvioAnterior = item.DevolvioAnterior
                    })
                    .ToList();

                context.DetalleEntregaEpp.AddRange(detallesNuevos);
                await context.SaveChangesAsync();
            }
        }

        // 5. ELIMINAR ENTREGA
        public async Task DeleteEntregaEppAsync(int entId)
        {
            using var context = _dbFactory.CreateDbContext();

            var entrega = await context.EntregaEpp.FindAsync(entId);
            if (entrega != null)
            {
                context.EntregaEpp.Remove(entrega);
                await context.SaveChangesAsync();
            }
        }

        // 6. OBTENER UNA ENTREGA POR ID (PARA VER/EDITAR)
        public async Task<EntregaEppViewModel> GetEntregaEppByIdAsync(int entId)
        {
            using var context = _dbFactory.CreateDbContext();

            var entrega = await context.EntregaEpp
                .Include(e => e.Empleado)
                    .ThenInclude(emp => emp.Per)
                        .ThenInclude(per => per.Pep)
                            .ThenInclude(pep => pep.Are)
                .Include(e => e.Detalles)
                    .ThenInclude(d => d.TipoEpp)
                .FirstOrDefaultAsync(e => e.EntId == entId);

            if (entrega == null)
                throw new Exception("No se encontró la entrega.");

            // Obtener todos los tipos de EPP disponibles
            var todosLosEpps = await GetCatalogoEppParaSeleccionAsync();

            // Marcar los que están en esta entrega
            foreach (var detalle in entrega.Detalles)
            {
                var eppItem = todosLosEpps.FirstOrDefault(e => e.TepId == detalle.TepId);
                if (eppItem != null)
                {
                    eppItem.IsSelected = true;
                    eppItem.Cantidad = detalle.DetCantidad;

                    eppItem.TallaModelo = detalle.DetTallaModelo;
                    eppItem.EsNuevo = detalle.DetEsNuevo;
                    eppItem.DevolvioAnterior = detalle.DetDevolvioAnterior;
                }
            }

            // Obtener el área del empleado
            int? areaId = entrega.Empleado?.Per?.Pep?.AreId;

            var viewModel = new EntregaEppViewModel
            {
                EntId = entrega.EntId,
                Fecha = entrega.EntFecha ?? DateTime.Now,
                EmpleadoId = entrega.EmpId,
                NombreEmpleado = entrega.Empleado?.Per != null
                    ? $"{entrega.Empleado.Per.PerNombre} {entrega.Empleado.Per.PerApellidoPaterno}"
                    : "N/A",
                AreaSeleccionadaId = areaId ?? 0,
                EppsSeleccionados = todosLosEpps,
                TieneEvidencia = entrega.EntEvidencia != null
            };

            return viewModel;
        }


        // =================================================================
        //// =================PERFIL DE PUESTO===============================
        // =================================================================


        // =================================================================
        // 14. OBTENER PDF DEL PUESTO ACTUAL (Para visualización)
        // =================================================================
        public async Task<byte[]?> GetPdfPuestoPorUsuarioAsync(string aspNetUserId)
        {
            using var context = _dbFactory.CreateDbContext();
            // Navegamos: Empleado -> Persona -> PerfilPuesto
            var empleado = await context.Empleados
                .AsNoTracking() // Solo lectura, más rápido
                .Include(e => e.Per)
                    .ThenInclude(p => p.Pep)
                .FirstOrDefaultAsync(e => e.AspNetUserId == aspNetUserId);

            // Retornamos null si no tiene puesto o si el puesto no tiene archivo
            return empleado?.Per?.Pep?.PepArchivo;
        }

        // Opcional: Para mostrar el nombre del puesto en el título
        public async Task<string> GetNombrePuestoPorUsuarioAsync(string aspNetUserId)
        {
            using var context = _dbFactory.CreateDbContext();

            var empleado = await context.Empleados
                .AsNoTracking()
                .Include(e => e.Per)
                    .ThenInclude(p => p.Pep)
                .FirstOrDefaultAsync(e => e.AspNetUserId == aspNetUserId);

            return empleado?.Per?.Pep?.PepNombre ?? "Puesto no asignado";
        }

        // Epp Personal MODIFICADO DIC 18

        public async Task<List<EntregaEppViewModel>> GetEntregasEppPorUsuarioAsync(string aspNetUserId)
        {
            using var context = _dbFactory.CreateDbContext();

            var query = context.EntregaEpp
                .AsNoTracking()
                // FILTRO CLAVE: Solo registros donde el AspNetUserId coincida
                .Where(e => e.Empleado.AspNetUserId == aspNetUserId)
                .OrderByDescending(e => e.EntFecha)
                .Select(e => new EntregaEppViewModel
                {
                    EntId = e.EntId,
                    Fecha = e.EntFecha ?? DateTime.Now,
                    // No necesitamos mapear el nombre del empleado aquí porque es para él mismo
                    ListaEpps = string.Join(", ", e.Detalles.Select(d => d.TipoEpp.TepTipo)),
                    // Usamos la suma como pediste anteriormente
                    ListaCantidades = e.Detalles.Sum(d => d.DetCantidad).ToString(),
                    TieneEvidencia = e.EntEvidencia != null
                });

            return await query.ToListAsync();
        }


        // =================================================================
        // 15. FUNCIONES PARA MODULO SUBORDINADOS (SUPERVISOR)
        // =================================================================

        // A. Obtener el ID de Empleado dado el usuario de Identity (Para saber quién es el Supervisor conectado)
        public async Task<int> GetEmpleadoIdByUserIdAsync(string userId)
        {
            using var context = _dbFactory.CreateDbContext();

            var emp = await context.Empleados
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.AspNetUserId == userId);
            return emp?.EmpId ?? 0;
        }

        // B. Mejorar el método de obtener subordinados para traer más datos (Área y Apellido Materno)
        public async Task<List<EmpleadoViewModel>> GetSubordinadosCompletosAsync(int supervisorEmpId)
        {
            using var context = _dbFactory.CreateDbContext();

            var query = from e in context.Empleados
                        join p in context.Personas on e.PerId equals p.PerId
                        join puesto in context.PerfilPuesto on p.PepId equals puesto.PepId
                        join area in context.Area on puesto.AreId equals area.AreId
                        // Condición: Que el campo SupId coincida con el ID del supervisor
                        where e.SupId == supervisorEmpId && e.StaId == 1 // Solo activos
                        select new EmpleadoViewModel
                        {
                            Id = e.EmpId,
                            CodigoEmpleado = e.EmpCodigo,
                            Nombre = p.PerNombre,
                            ApellidoPaterno = p.PerApellidoPaterno,
                            ApellidoMaterno = p.PerApellidoMaterno,
                            Empleo = puesto.PepNombre,
                            Area = area.AreNombre,
                            Email = p.PerEmail,
                            Telefono = p.PerTelefono
                        };

            return await query.ToListAsync();
        }

        // C. Obtener Historial de EPP por ID de Empleado (Para la nueva pestaña en Editar)
        public async Task<List<EntregaEppViewModel>> GetHistorialEppByEmpleadoIdAsync(int empleadoId)
        {
            using var context = _dbFactory.CreateDbContext();

            var query = context.EntregaEpp
                .AsNoTracking()
                .Where(e => e.EmpId == empleadoId)
                .OrderByDescending(e => e.EntFecha)
                .Select(e => new EntregaEppViewModel
                {
                    EntId = e.EntId,
                    Fecha = e.EntFecha ?? DateTime.Now,
                    // Lista concatenada de lo que se le dio
                    ListaEpps = string.Join(", ", e.Detalles.Select(d => d.TipoEpp.TepTipo)),
                    ListaCantidades = e.Detalles.Sum(d => d.DetCantidad).ToString(),
                    TieneEvidencia = e.EntEvidencia != null
                });

            return await query.ToListAsync();
        }

        public async Task AsignarCursoAAreaAsync(int mcId, int areId)
        {
            using var context = _dbFactory.CreateDbContext();
            // 1. Validar si ya existe la relación localmente para no duplicar
            bool existe = await context.MoodleCursoAreas
                .AnyAsync(x => x.McId == mcId && x.AreId == areId);

            if (existe)
            {
                // Opcional: Si ya existe en BD local pero no en Moodle, podrías dejar pasar para re-intentar en Moodle.
                // Por ahora, asumimos que si existe en BD está bien.
                throw new Exception("Este curso ya está asignado a esa área en el sistema.");
            }

            // 2. Obtener los IDs reales
            var curso = await context.MoodleCursos.FindAsync(mcId);
            var area = await context.Area.FindAsync(areId);

            if (curso == null || area == null) throw new Exception("Curso o Área no encontrados.");
            if (area.MoodleCohortId == null) throw new Exception("Esta área no tiene un Cohorte vinculado (MOODLE_COHORT_ID es nulo).");

            // 3. LLAMAR A LA API DE MOODLE (La magia)
            // Esto agregará el método "Sincronización de cohorte" en el curso de Moodle
            bool exitoMoodle = await _moodleService.AsignarCohorteACursoMoodleAsync(
                area.MoodleCohortId.Value,
                curso.MoodleCursoId
            );

            if (exitoMoodle)
            {
                // 4. Guardar en tu tabla intermedia
                var nuevaRelacion = new MoodleCursoArea
                {
                    McId = mcId,
                    AreId = areId
                };
                context.MoodleCursoAreas.Add(nuevaRelacion);
                await context.SaveChangesAsync();
            }
            else
            {
                throw new Exception("Moodle rechazó la vinculación. Revisa si el método 'enrol_cohort_enrol_cohort' está permitido en el token.");
            }
        }


        // Método auxiliar para limpiar caracteres
        private string NormalizarApellido(string texto)
        {
            if (string.IsNullOrEmpty(texto)) return texto;
            return texto.Replace("ñ", "n").Replace("Ñ", "N");
        }

        public async Task<List<DetalleExpediente>> GetCatalogoDocumentosAsync()
        {
            using var context = _dbFactory.CreateDbContext();
            // Traemos todos ordenados por ID
            return await context.DetalleExpediente.OrderBy(d => d.DexId).ToListAsync();
        }

        public async Task SaveTipoDocumentoAsync(DetalleExpediente doc)
        {
            using var context = _dbFactory.CreateDbContext();

            if (doc.DexId == 0)
            {
                // NUEVO REGISTRO:
                // 1. Dejamos que SQL asigne el ID (Identity).
                // 2. Guardamos la fecha de HOY en tu nueva columna.
                doc.DexFecha = DateTime.Now;

                context.DetalleExpediente.Add(doc);
            }
            else
            {
                // EDICIÓN:
                var existente = await context.DetalleExpediente.FindAsync(doc.DexId);
                if (existente != null)
                {
                    existente.DexNombre = doc.DexNombre;
                    existente.DexNomenclatura = doc.DexNomenclatura;
                    // IMPORTANTE: NO tocamos la fecha al editar para mantener la regla histórica.
                    context.DetalleExpediente.Update(existente);
                }
            }
            await context.SaveChangesAsync();
        }

        // =================================================================
        // 15.1. ESPEDIENTE UNICO
        // =================================================================


        public async Task AddExpedienteUnicoAsync(int empId, string nombre, string nomenclatura)
        {
            using var context = _dbFactory.CreateDbContext();
            var nuevo = new DetalleExpedienteUnico
            {
                EmpId = empId,
                DeuNombre = nombre,
                DeuNomenclatura = nomenclatura,
            };
            context.DetalleExpedienteUnico.Add(nuevo);
            await context.SaveChangesAsync();
        }

        public async Task DeleteRequisitoUnicoAsync(int deuId)
        {
            using var context = _dbFactory.CreateDbContext();
            // Primero borramos el archivo físico si existe (la tabla Expediente tiene FK cascade? si no, borrar manual)
            var expedientesAsociados = await context.Expediente.Where(e => e.DeuId == deuId).ToListAsync();
            context.Expediente.RemoveRange(expedientesAsociados);

            // Borramos el requisito
            var requisito = await context.DetalleExpedienteUnico.FindAsync(deuId);
            if (requisito != null)
            {
                context.DetalleExpedienteUnico.Remove(requisito);
                await context.SaveChangesAsync();
            }
        }

        // =================================================================
        // 16. MÓDULO QHSE (STOP CARD)
        // =================================================================

        // A. Guardar Tarjeta (Create)
        public async Task SaveQhseCardAsync(int empleadoId, QhseFormModel model)
        {
            using var context = _dbFactory.CreateDbContext();

            // 1. Crear la Cabecera
            var nuevaCard = new QhseCard
            {
                EmpId = empleadoId,
                ObservadorNombre = model.Observador,
                Fecha = model.Fecha,
                Lugar = model.Lugar,
                EsInseguro = model.EsInseguro,
                TodoSeguro = model.TodoSeguro,

                DescripcionObservacion = model.DescripcionObservacion,
                DescripcionCausa = model.DescripcionCausa,
                AccionCorrectiva = model.AccionCorrectiva,
                ResponsableCierre = model.Responsable,
                FechaCompromiso = model.FechaCompromiso,
                FechaRegistro = DateTime.Now
            };

            context.QhseCards.Add(nuevaCard);
            await context.SaveChangesAsync(); // Se genera el QhcId

            // 2. Crear los Detalles (Checklists)
            var detalles = new List<QhseCardDetail>();

            void AgregarDetalles(Dictionary<string, bool> dict, string categoria)
            {
                foreach (var kvp in dict)
                {
                    detalles.Add(new QhseCardDetail
                    {
                        QhcId = nuevaCard.QhcId,
                        Categoria = categoria,
                        ItemNombre = kvp.Key,
                        EstaMarcado = kvp.Value
                    });
                }
            }

            AgregarDetalles(model.Posiciones, "Posiciones");
            AgregarDetalles(model.Epp, "EPP");
            AgregarDetalles(model.Procedimientos, "Procedimientos");
            AgregarDetalles(model.Herramientas, "Herramientas");

            context.QhseCardDetails.AddRange(detalles);
            await context.SaveChangesAsync();

            // =================================================================
            // 3. AUTOMATIZACIÓN DE KPI (NUEVO - VERSIÓN HISTÓRICA)
            // =================================================================

            int anioRegistro = nuevaCard.FechaRegistro.Year;

            var prioridadTarjetas = await context.PdPriorities
                .FirstOrDefaultAsync(p => p.EmpId == empleadoId
                                       && p.Year == anioRegistro
                                       && p.UnitOfMeasure == "Tarjetas");

            if (prioridadTarjetas != null)
            {
                // RECUENTO TOTAL: Contamos exactamente cuántas tarjetas tiene en ese año
                int totalTarjetasAnio = await context.QhseCards
                    .CountAsync(c => c.EmpId == empleadoId && c.FechaRegistro.Year == anioRegistro);

                // Asignamos el valor real en lugar de sumar +1
                prioridadTarjetas.ActualValue = totalTarjetasAnio;

                if (prioridadTarjetas.ActualValue >= prioridadTarjetas.TargetValue && prioridadTarjetas.TrackingType == "MHB")
                {
                    prioridadTarjetas.FinalStatus = "Cumplió";
                }

                context.PdPriorities.Update(prioridadTarjetas);
                await context.SaveChangesAsync();
            }
        }

        // B. Obtener historial de tarjetas de un empleado (Para la tabla resumen)
        public async Task<List<QhseCard>> GetQhseCardsByEmpleadoIdAsync(int empleadoId)
        {
            using var context = _dbFactory.CreateDbContext();

            return await context.QhseCards
                .AsNoTracking()
                .Where(c => c.EmpId == empleadoId)
                .OrderByDescending(c => c.Fecha)
                .ToListAsync();
        }

        // C. Obtener una tarjeta completa para verla (Reconstruir el FormModel)
        // Este método es opcional si quieres volver a mostrar el formulario lleno en modo lectura
        public async Task<QhseFormModel> GetQhseCardForViewAsync(int cardId)
        {
            using var context = _dbFactory.CreateDbContext();

            // IMPORTANTE: El .Include(c => c.Detalles) es obligatorio para ver los checkboxes
            var card = await context.QhseCards
                .AsNoTracking()
                .Include(c => c.Detalles) // <--- ¡ESTA LÍNEA ES CRÍTICA!
                .FirstOrDefaultAsync(c => c.QhcId == cardId);

            if (card == null) return new QhseFormModel();

            var model = new QhseFormModel
            {
                Observador = card.ObservadorNombre,
                Fecha = card.Fecha,
                Lugar = card.Lugar,
                EsInseguro = card.EsInseguro,
                TodoSeguro = card.TodoSeguro,
                DescripcionObservacion = card.DescripcionObservacion,
                DescripcionCausa = card.DescripcionCausa,
                AccionCorrectiva = card.AccionCorrectiva,
                Responsable = card.ResponsableCierre,
                FechaCompromiso = card.FechaCompromiso
            };

            // Helper para llenar los diccionarios (checkboxes)
            void LlenarDiccionario(Dictionary<string, bool> dict, string categoria)
            {
                // Filtramos los detalles de la BD que pertenecen a esta categoría
                var itemsDb = card.Detalles.Where(d => d.Categoria == categoria).ToList();

                foreach (var itemDb in itemsDb)
                {
                    // Si el nombre del item en BD coincide con una llave del diccionario, actualizamos el valor
                    if (dict.ContainsKey(itemDb.ItemNombre))
                    {
                        dict[itemDb.ItemNombre] = itemDb.EstaMarcado;
                    }
                }
            }

            // Ejecutamos el llenado
            LlenarDiccionario(model.Posiciones, "Posiciones");
            LlenarDiccionario(model.Epp, "EPP");
            LlenarDiccionario(model.Procedimientos, "Procedimientos");
            LlenarDiccionario(model.Herramientas, "Herramientas");

            return model;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        //18Marzo
        // 1. Obtener lista para el ComboBox (Supervisores primero)
        public async Task<List<EmpleadoDashboardItem>> GetEmpleadosParaAsignacionAsync()
        {
            // Creamos un contexto completamente nuevo y aislado para evitar choques de hilos
            using var context = await _dbFactory.CreateDbContextAsync();

            // 1. Buscamos el ID del rol "Supervisor" directamente en la tabla de Roles de Identity
            var supervisorRole = await context.Roles.FirstOrDefaultAsync(r => r.NormalizedName == "SUPERVISOR");
            string roleId = supervisorRole != null ? supervisorRole.Id : "";

            // 2. Obtenemos los IDs de los usuarios que tienen ese rol
            var supervisorUserIds = await context.UserRoles
                .Where(ur => ur.RoleId == roleId)
                .Select(ur => ur.UserId)
                .ToListAsync();

            // 3. Traemos a los empleados activos de la BD
            var query = await (from e in context.Empleados
                               join p in context.Personas on e.PerId equals p.PerId
                               join puesto in context.PerfilPuesto on p.PepId equals puesto.PepId into puestoGroup
                               from puestoJoin in puestoGroup.DefaultIfEmpty()
                               where e.StaId == 1 // Solo activos
                               select new
                               {
                                   Id = e.EmpId,
                                   AspNetUserId = e.AspNetUserId,
                                   NombreCompleto = p.PerNombre + " " + p.PerApellidoPaterno,
                                   Puesto = puestoJoin != null ? puestoJoin.PepNombre : "Sin Asignar"
                               }).ToListAsync();

            // 4. Cruzamos los datos (Usamos TieneTarjetasSinRevisar como bandera temporal)
            var lista = query.Select(e => new EmpleadoDashboardItem
            {
                Id = e.Id,
                Nombre = e.NombreCompleto,
                Area = e.Puesto,
                TieneTarjetasSinRevisar = e.AspNetUserId != null && supervisorUserIds.Contains(e.AspNetUserId)
            }).ToList();

            // 5. Ordenamos: Supervisores arriba, luego alfabético
            return lista
                .OrderByDescending(e => e.TieneTarjetasSinRevisar)
                .ThenBy(e => e.Nombre)
                .ToList();
        }

        // 2. Asignar la tarjeta y cambiar a "En Proceso"
        public async Task AsignarTarjetaAsync(int cardId, int asignadoEmpId)
        {
            using var context = _dbFactory.CreateDbContext();
            var card = await context.QhseCards.FindAsync(cardId);

            if (card != null)
            {
                card.AsignadoEmpId = asignadoEmpId;
                card.Estatus = 1; // 1 = En Proceso
                await context.SaveChangesAsync();
            }
        }

        // 3. Obtener las tarjetas que me han asignado a MÍ
        public async Task<List<QhseCard>> GetTarjetasAsignadasAsync(int asignadoEmpId)
        {
            using var context = _dbFactory.CreateDbContext();
            return await context.QhseCards
                .AsNoTracking()
                .Where(c => c.AsignadoEmpId == asignadoEmpId)
                .OrderByDescending(c => c.Fecha)
                .ToListAsync();
        }

        // 4. Subir Evidencia y Cerrar Tarjeta a "Resuelto"
        public async Task ResolverTarjetaAsync(int cardId, string evidenciaTexto, string evidenciaFotoRuta)
        {
            using var context = _dbFactory.CreateDbContext();
            var card = await context.QhseCards.FindAsync(cardId);

            if (card != null)
            {
                card.EvidenciaTexto = evidenciaTexto;
                card.EvidenciaFoto = evidenciaFotoRuta;
                card.Estatus = 2; // 2 = Resuelto
                await context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// /////////////
        /// </summary>
        /// <returns></returns>

        // En EmpleadoService.cs

        public async Task<List<EmpleadoViewModel>> GetAllEmpleadosAsync()
        {
            using var context = await _dbFactory.CreateDbContextAsync();

            // Consultamos la tabla Empleados e incluimos las relaciones
            // NOTA: Basado en tu DbContext, Empleado tiene EmpId y PerId.
            // Persona tiene PepId (Perfil) y SubId (Subsidiaria).
            // PerfilPuesto tiene AreId (Area).

            var query = context.Empleados
                // 1. Unimos con Persona (Datos demográficos)
                .Include(e => e.Per)
                // 2. Desde Persona, unimos con Perfil Puesto (Empleo)
                .Include(e => e.Per.Pep)
                .Include(e => e.Per.Pep2)
                // 3. Desde Perfil Puesto, unimos con Area
                .Include(e => e.Per.Pep.Are)

                // (Opcional) Filtrar solo activos si tienes un campo de estatus
                .Where(e => e.StaId == 1)

                // 5. Proyectamos al ViewModel para la UI
                .Select(e => new EmpleadoViewModel
                {
                    Id = e.EmpId,
                    Nombre = e.Per.PerNombre,
                    ApellidoPaterno = e.Per.PerApellidoPaterno,
                    ApellidoMaterno = e.Per.PerApellidoMaterno ?? "", // Manejo de nulos

                    // Navegamos: Empleado -> Persona -> PerfilPuesto -> Nombre
                    //Empleo = e.Per.Pep != null ? e.Per.Pep.PepNombre : "Sin Asignar",
                    Empleo = (e.Per.Pep != null ? e.Per.Pep.PepNombre : "Sin Asignar") +
                 (e.Per.Pep2 != null ? " / " + e.Per.Pep2.PepNombre : ""),
                    // Navegamos: Empleado -> Persona -> PerfilPuesto -> Area -> Nombre
                    Area = (e.Per.Pep != null && e.Per.Pep.Are != null) ? e.Per.Pep.Are.AreNombre : "Sin Area",

                    // Navegamos: Empleado -> Persona -> Subsidiaria -> Nombre
                    Subsidiaria = e.Per.Sub != null ? e.Per.Sub.SubNombre : "Sin Subsidiaria",
                    SaeSdi = e.SAE_SDI
                });

            return await query.ToListAsync();
        }


        // En EmpleadoService.cs

        // 1. Obtener empleados con flag de pendientes
        public async Task<List<EmpleadoDashboardItem>> GetEmpleadosConStatusTarjetasAsync()
        {
            using var context = _dbFactory.CreateDbContext();

            var query = from e in context.Empleados
                        join p in context.Personas on e.PerId equals p.PerId
                        join puesto in context.PerfilPuesto on p.PepId equals puesto.PepId into puestoGroup
                        from puestoJoin in puestoGroup.DefaultIfEmpty()
                        join a in context.Area on (puestoJoin == null ? -1 : puestoJoin.AreId) equals a.AreId into areaGroup
                        from areaJoin in areaGroup.DefaultIfEmpty()

                            // LÓGICA NUEVA:
                            // Buscamos si tiene alguna tarjeta donde Estatus sea 0 (Nuevo)
                        let tieneNuevas = context.QhseCards.Any(c => c.EmpId == e.EmpId && c.Estatus == 0)

                        // Ordenamos: Primero los que tienen alertas, luego alfabéticamente
                        orderby tieneNuevas descending, p.PerApellidoPaterno

                        select new EmpleadoDashboardItem
                        {
                            Id = e.EmpId,
                            Nombre = p.PerNombre,
                            ApellidoPaterno = p.PerApellidoPaterno,
                            Area = areaJoin != null ? areaJoin.AreNombre : "Sin Área",
                            TieneTarjetasSinRevisar = tieneNuevas
                        };

            return await query.ToListAsync();
        }

        // 2. Marcar como revisada
        //public async Task MarcarTarjetaComoRevisadaAsync(int cardId)
        //{
        // using var context = _dbFactory.CreateDbContext();
        // var card = await context.QhseCards.FindAsync(cardId);
        //     if (card != null)
        //   {
        //card.RevisadoPorQHSE = true; // Asumiendo que agregaste este campo bool a la tabla
        //await context.SaveChangesAsync();
        //}
        //}

        // 1. Obtener TODAS las tarjetas para el Dashboard Global
        public async Task<List<QhseCard>> GetAllQhseCardsAsync()
        {
            using var context = _dbFactory.CreateDbContext();

            return await context.QhseCards
                .AsNoTracking()
                .Include(c => c.Empleado)
                    .ThenInclude(e => e.Per)
                // ---> AGREGA ESTAS DOS LÍNEAS <---
                .Include(c => c.EmpleadoAsignado)
                    .ThenInclude(e => e.Per)
                .OrderByDescending(c => c.FechaRegistro)
                .ToListAsync();
        }

        // 2. Actualizar Estado (Desde el Toggle)
        public async Task UpdateCardStatusAsync(int cardId, int newStatus)
        {
            using var context = _dbFactory.CreateDbContext();
            var card = await context.QhseCards.FindAsync(cardId);

            if (card != null)
            {
                card.Estatus = newStatus;

                // Opcional: Si pasa a Terminado (3), guardamos fecha de cierre
                // if (newStatus == 3) card.FechaCierre = DateTime.Now;

                await context.SaveChangesAsync();
            }
        }

        //SEGUNDO PUESTO

        // 1. Crea este pequeño DTO para transportar los datos de ambos puestos


        // 2. Agrega este método a tu servicio
        public async Task<PuestosUsuarioDTO> GetInfoPuestosPorUsuarioAsync(string aspNetUserId)
        {
            using var context = _dbFactory.CreateDbContext();
            var empleado = await context.Empleados
                .AsNoTracking()
                .Include(e => e.Per)
                    .ThenInclude(p => p.Pep)  // Trae Puesto 1
                .Include(e => e.Per)
                    .ThenInclude(p => p.Pep2) // Trae Puesto 2
                .FirstOrDefaultAsync(e => e.AspNetUserId == aspNetUserId);

            if (empleado?.Per == null) return new PuestosUsuarioDTO();

            return new PuestosUsuarioDTO
            {
                NombrePuesto1 = empleado.Per.Pep?.PepNombre ?? "Puesto no asignado",
                PdfPuesto1 = empleado.Per.Pep?.PepArchivo,

                NombrePuesto2 = empleado.Per.Pep2?.PepNombre,
                PdfPuesto2 = empleado.Per.Pep2?.PepArchivo
            };
        }

        ////Mostrar tipos de incidencias en el combobox

        public async Task<List<TipoIncidencia>> GetTiposIncidenciaActivosAsync()
        {
            using var context = _dbFactory.CreateDbContext();
            return await context.Set<TipoIncidencia>()
                .Where(t => t.TinVisibleUi == true)
                .OrderBy(t => t.TinNombre)
                .ToListAsync();
        }


        // =================================================================
        // EXCLUSIONES DE EXPEDIENTE (OCULTAR/MOSTRAR DOCS ESTÁNDAR)
        // =================================================================
        public async Task OcultarDocumentoEstandarAsync(int empId, int dexId)
        {
            using var context = _dbFactory.CreateDbContext();

            // Verificar que no exista ya la exclusión
            bool existe = await context.ExpedienteExclusiones
                .AnyAsync(x => x.EmpId == empId && x.DexId == dexId);

            if (!existe)
            {
                context.ExpedienteExclusiones.Add(new ExpedienteExclusion
                {
                    EmpId = empId,
                    DexId = dexId
                });
                await context.SaveChangesAsync();
            }
        }

        public async Task MostrarDocumentoEstandarAsync(int empId, int dexId)
        {
            using var context = _dbFactory.CreateDbContext();

            var exclusion = await context.ExpedienteExclusiones
                .FirstOrDefaultAsync(x => x.EmpId == empId && x.DexId == dexId);

            if (exclusion != null)
            {
                context.ExpedienteExclusiones.Remove(exclusion);
                await context.SaveChangesAsync();
            }
        }

        public async Task<List<DetalleExpediente>> GetExpedientesOcultosAsync(int empId)
        {
            using var context = _dbFactory.CreateDbContext();

            var dexIdsOcultos = await context.ExpedienteExclusiones
                .Where(x => x.EmpId == empId)
                .Select(x => x.DexId)
                .ToListAsync();

            if (!dexIdsOcultos.Any()) return new List<DetalleExpediente>();

            return await context.DetalleExpediente
                .Where(d => dexIdsOcultos.Contains(d.DexId))
                .ToListAsync();
        }

        // =================================================================
        // GENERAR REPORTE EXCEL DE EMPLEADOS ACTIVOS
        // =================================================================
        public async Task<byte[]> GenerarReporteExcelEmpleadosActivosAsync()
        {
            // 1. Obtenemos los empleados y filtramos solo los activos
            var todos = await GetEmpleadosViewModelAsync();
            var activos = todos.Where(e => e.StaId == 1 || e.Status != "Inactivo").ToList();

            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Empleados Activos");

            // 2. Cabecera Institucional (Azul Marino con letras blancas)
            var headerRow = ws.Row(1);
            headerRow.Style.Font.Bold = true;
            headerRow.Style.Font.FontColor = XLColor.White;
            headerRow.Style.Fill.BackgroundColor = XLColor.FromArgb(37, 60, 96);

            // Nombres de Columnas
            ws.Cell(1, 1).Value = "CÓDIGO";
            ws.Cell(1, 2).Value = "NOMBRE COMPLETO";
            ws.Cell(1, 3).Value = "ÁREA";
            ws.Cell(1, 4).Value = "PUESTO(S)";
            ws.Cell(1, 5).Value = "TELÉFONO";
            ws.Cell(1, 6).Value = "ESTATUS";
            ws.Cell(1, 7).Value = "SUPERVISOR ASIGNADO";
            ws.Cell(1, 8).Value = "EXPEDIENTES ENTREGADOS";
            ws.Cell(1, 9).Value = "AVANCE EXPEDIENTE";

            // 3. Llenar los datos
            int row = 2;
            foreach (var emp in activos)
            {
                ws.Cell(row, 1).Value = emp.CodigoEmpleado;
                ws.Cell(row, 2).Value = $"{emp.Apellidos} {emp.Nombre}".Trim();
                ws.Cell(row, 3).Value = emp.Area;
                ws.Cell(row, 4).Value = emp.EmpleosCombinados;
                ws.Cell(row, 5).Value = emp.Telefono;
                ws.Cell(row, 6).Value = emp.Status;
                ws.Cell(row, 7).Value = (emp.SupId != null && emp.SupId > 0) ? "SÍ" : "NO";

                // Formato de texto para que se vea como "10 / 15"
                ws.Cell(row, 8).Value = $"'{emp.ExpedientesCompletos} / {emp.ExpedientesTotales}";

                // Formato de porcentaje real en Excel
                ws.Cell(row, 9).Value = emp.ExpedientesPorcentaje / 100.0m;
                ws.Cell(row, 9).Style.NumberFormat.Format = "0.00%";

                row++;
            }

            // Autoajustar las columnas a lo ancho del texto
            ws.Columns().AdjustToContents();

            // Guardar y retornar
            using var ms = new System.IO.MemoryStream();
            wb.SaveAs(ms);
            return ms.ToArray();
        }
    }
}