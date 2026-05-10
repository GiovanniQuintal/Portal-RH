using Microsoft.EntityFrameworkCore;
using RRHH_ARG.Data;
using RRHH_ARG.Models;

namespace RRHH_ARG.Services
{
    public class PdService
    {
        // Usamos Factory para evitar el error de "Second operation started..."
        private readonly IDbContextFactory<ApplicationDbContext> _factory;

        public PdService(IDbContextFactory<ApplicationDbContext> factory)
        {
            _factory = factory;
        }

        // =================================================================
        // SECCIÓN 1: CONFIGURACIÓN Y OBJETIVOS GLOBALES (ADMIN RH)
        // =================================================================

        

        // Para la pantalla de ADMIN (Trae todo)
        public async Task<List<PdGlobalGoal>> GetAllGlobalGoalsAsync(int year)
        {
            using var context = await _factory.CreateDbContextAsync();
            return await context.PdGlobalGoals
                .Where(g => g.Year == year)
                .OrderByDescending(g => g.Id)
                .ToListAsync();
        }

        // Para el DROPDOWN del empleado (Trae solo activos) -> CORRIGE ERROR "GetGlobalGoalsAsync"
        public async Task<List<PdGlobalGoal>> GetGlobalGoalsAsync(int year)
        {
            using var context = await _factory.CreateDbContextAsync();
            return await context.PdGlobalGoals
                .Where(g => g.Year == year && g.IsActive)
                .ToListAsync();
        }

        public async Task SaveGlobalGoalAsync(PdGlobalGoal goal)
        {
            using var context = await _factory.CreateDbContextAsync();
            if (goal.Id == 0)
                context.PdGlobalGoals.Add(goal);
            else
                context.PdGlobalGoals.Update(goal);

            await context.SaveChangesAsync();
        }

        public async Task ToggleGlobalGoalActiveAsync(int id)
        {
            using var context = await _factory.CreateDbContextAsync();
            var g = await context.PdGlobalGoals.FindAsync(id);
            if (g != null)
            {
                g.IsActive = !g.IsActive;
                await context.SaveChangesAsync();
            }
        }

        // Cambiamos el Toggle por un Delete físico
        public async Task DeleteGlobalGoalAsync(int id)
        {
            using var context = await _factory.CreateDbContextAsync();
            var goal = await context.PdGlobalGoals.FindAsync(id);
            if (goal != null)
            {
                context.PdGlobalGoals.Remove(goal);
                await context.SaveChangesAsync();
            }
        }
        // =================================================================
        // SECCIÓN 2: PRIORIDADES / KPIS (EMPLEADO)
        // =================================================================

        // -> CORRIGE ERROR "GetPrioritiesAsync"
        // -> CORRIGE ERROR "GetPrioritiesAsync"
        public async Task<List<PdPriority>> GetPrioritiesAsync(int empId, int year, bool showArchived)
        {
            using var context = await _factory.CreateDbContextAsync();

            var query = context.PdPriorities
                .Include(p => p.Indicator)
                    .ThenInclude(i => i.GlobalGoal)
                .Where(p => p.EmpId == empId && p.Year == year);

            if (!showArchived)
                query = query.Where(p => !p.IsArchived);

            // Traemos los datos de la BD
            var resultados = await query.ToListAsync();

            // ORDENAMOS EN MEMORIA: Primero alfabéticamente por Indicador, luego por Fecha de Creación
            // Los que no tengan indicador ("ZZZ") se irán hasta el final de la lista.
            return resultados
                .OrderBy(p => p.Indicator?.Title ?? "ZZZ")
                .ThenByDescending(p => p.CreatedAt)
                .ToList();
        }

        public async Task SavePriorityAsync(PdPriority priority)
        {
            using var context = await _factory.CreateDbContextAsync();

            // Lógica para sincronizar "Tarjetas"
            if (priority.UnitOfMeasure == "Tarjetas")
            {
                int totalTarjetasAnio = await context.QhseCards
                    .CountAsync(c => c.EmpId == priority.EmpId && c.FechaRegistro.Year == priority.Year);

                priority.ActualValue = totalTarjetasAnio;

                if (priority.ActualValue >= priority.TargetValue && priority.TrackingType == "MHB")
                {
                    priority.FinalStatus = "Cumplió";
                }
            }

            // LÓGICA NUEVA PARA SINCRONIZAR "CURSOS"
            if (priority.UnitOfMeasure == "Cursos")
            {
                var cursosDelAnio = await context.MoodleCursoEmpleados
                    .Where(c => c.EmpId == priority.EmpId && c.MceFechaAsignacion.Year == priority.Year)
                    .ToListAsync();

                priority.TargetValue = cursosDelAnio.Count;

                // Contamos como logrado solo los que están al 100% o Estado 2 (Completado)
                priority.ActualValue = cursosDelAnio.Count(c => c.MceEstado == 2 || c.McePorcentajeAvance >= 100);

                if (priority.TargetValue > 0 && priority.ActualValue >= priority.TargetValue && priority.TrackingType == "MHB")
                {
                    priority.FinalStatus = "Cumplió";
                }
            }

            if (priority.Id == 0)
            {
                priority.CreatedAt = DateTime.Now;
                context.PdPriorities.Add(priority);
            }
            else
            {
                context.PdPriorities.Update(priority);
            }

            await context.SaveChangesAsync();
        }

        public async Task DeletePriorityAsync(int id)
        {
            using var context = await _factory.CreateDbContextAsync();
            var p = await context.PdPriorities.FindAsync(id);
            if (p != null)
            {
                context.PdPriorities.Remove(p);
                await context.SaveChangesAsync();
            }
        }
        public async Task ToggleArchivePriorityAsync(int id)
        {
            using var context = await _factory.CreateDbContextAsync();
            var p = await context.PdPriorities.FindAsync(id);
            if (p != null)
            {
                p.IsArchived = !p.IsArchived;
                await context.SaveChangesAsync();
            }
        }

        public async Task SubmitPriorityAsync(int id)
        {
            using var context = await _factory.CreateDbContextAsync();
            var p = await context.PdPriorities.FindAsync(id);
            if (p != null)
            {
                p.IsSubmitted = true;
                await context.SaveChangesAsync();
            }
        }

        // -> CORRIGE ERROR "ApprovePriorityAsync"
        public async Task ApprovePriorityAsync(int id, bool approve)
        {
            using var context = await _factory.CreateDbContextAsync();
            var p = await context.PdPriorities.FindAsync(id);
            if (p != null)
            {
                p.IsApproved = approve;
                if (approve) p.IsSubmitted = true; // Si aprueba, forzamos submit
                await context.SaveChangesAsync();
            }
        }

        // =================================================================
        // SECCIÓN 3: TOUCHPOINTS / REVISIONES
        // =================================================================

        public async Task<List<PdTouchpoint>> GetTouchpointsAsync(int empId)
        {
            using var context = await _factory.CreateDbContextAsync();
            return await context.PdTouchpoints
                .Include(t => t.Items) // <--- ¡AGREGA ESTA LÍNEA!
                .Where(t => t.EmpId == empId)
                .OrderByDescending(t => t.Date)
                .ToListAsync();
        }

        public async Task SaveTouchpointAsync(PdTouchpoint touchpoint)
        {
            if (touchpoint.IsApproved)
                throw new Exception("No se puede editar un Touchpoint aprobado.");

            using var context = await _factory.CreateDbContextAsync();

            if (touchpoint.Id == 0)
                context.PdTouchpoints.Add(touchpoint);
            else
                context.PdTouchpoints.Update(touchpoint);

            await context.SaveChangesAsync();
        }

        public async Task ApproveTouchpointAsync(int id)
        {
            using var context = await _factory.CreateDbContextAsync();
            var t = await context.PdTouchpoints.FindAsync(id);
            if (t != null)
            {
                t.IsApproved = true;
                t.ApprovalDate = DateTime.Now;
                await context.SaveChangesAsync();
            }
        }

        public async Task<PdYearConfig?> GetCurrentYearConfigAsync(int year)
        {
            using var context = await _factory.CreateDbContextAsync();
            return await context.PdYearConfigs
                .Include(c => c.RevisionDates) // Traemos las fechas R1..R4
                .FirstOrDefaultAsync(c => c.Year == year);
        }

        // CREAR O ACTUALIZAR CONFIGURACIÓN (RH)
        public async Task SaveYearConfigAsync(PdYearConfig config)
        {
            using var context = await _factory.CreateDbContextAsync();

            if (config.Id == 0)
            {
                // Lógica para crear las fechas default si es nuevo
                context.PdYearConfigs.Add(config);
            }
            else
            {
                context.PdYearConfigs.Update(config);
                // Actualizar fechas hijas
                foreach (var date in config.RevisionDates)
                {
                    context.Entry(date).State = EntityState.Modified;
                }
            }
            await context.SaveChangesAsync();
        }

        // VERIFICAR QUÉ REVISIÓN ESTÁ ACTIVA HOY
        public async Task<PdRevisionDate?> GetActiveRevisionAsync()
        {
            using var context = await _factory.CreateDbContextAsync();

            // IMPORTANTE: Aseguramos fecha pura sin horas
            var today = DateTime.Today;

            return await context.PdRevisionDates
                .Include(r => r.YearConfig)
                .Where(r => r.YearConfig.IsActive
                         && !r.IsLockedByRH
                         && today >= r.StartDate
                         && today <= r.EndDate) // Rango inclusivo
                .FirstOrDefaultAsync();
        }

        // CAMBIAR TOGGLE DEL EMPLEADO (ReadyForReview)
        public async Task TogglePriorityReadyAsync(int id, bool ready)
        {
            using var context = await _factory.CreateDbContextAsync();
            var p = await context.PdPriorities.FindAsync(id);
            if (p != null)
            {
                p.ReadyForReview = ready;
                await context.SaveChangesAsync();
            }
        }

        // --- GESTIÓN DE PRIORIDADES Y BLOQUEOS ---

        public async Task SavePriorityAsSupervisorAsync(PdPriority priority)
        {
            using var context = await _factory.CreateDbContextAsync();

            // Regla: Si lo crea/edita el supervisor, ya está aprobado y enviado.
            priority.IsSubmitted = true;
            priority.IsApproved = true;

            if (priority.Id == 0)
            {
                priority.CreatedAt = DateTime.Now;
                context.PdPriorities.Add(priority);
            }
            else
            {
                context.PdPriorities.Update(priority);
            }
            await context.SaveChangesAsync();
        }

        public async Task ApprovePriorityAsync(int id)
        {
            using var context = await _factory.CreateDbContextAsync();
            var p = await context.PdPriorities.FindAsync(id);
            if (p != null)
            {
                p.IsApproved = true; // Activar
                await context.SaveChangesAsync();
            }
        }

        // --- GESTIÓN DE TOUCHPOINTS CON DETALLES ---

        // Obtener Touchpoint con sus Items (Include)
        public async Task<PdTouchpoint?> GetTouchpointWithDetailsAsync(int empId, int revisionNumber)
        {
            using var context = await _factory.CreateDbContextAsync();
            return await context.PdTouchpoints
                .Include(t => t.Items) // Traer los detalles guardados
                .FirstOrDefaultAsync(t => t.EmpId == empId && t.RevisionNumber == revisionNumber);
        }

        // Modificamos la firma para recibir si es la revisión final
        public async Task SaveTouchpointFullAsync(PdTouchpoint touchpoint, List<PdTouchpointItem> items, bool isFinalRevision)
        {
            using var context = await _factory.CreateDbContextAsync();

            // 1. Guardar/Actualizar Cabecera
            if (touchpoint.Id == 0)
                context.PdTouchpoints.Add(touchpoint);
            else
                context.PdTouchpoints.Update(touchpoint);

            await context.SaveChangesAsync();

            // 2. Procesar Items (KPIs)
            foreach (var item in items)
            {
                item.TouchpointId = touchpoint.Id;

                var existingItem = await context.PdTouchpointItems
                    .FirstOrDefaultAsync(x => x.TouchpointId == touchpoint.Id && x.PriorityId == item.PriorityId);

                if (existingItem != null)
                {
                    existingItem.RegisteredValue = item.RegisteredValue;
                    existingItem.Feedback = item.Feedback;
                    if (isFinalRevision) existingItem.GoalMet = item.GoalMet; // Solo actualiza si es la final
                }
                else
                {
                    context.PdTouchpointItems.Add(item);
                }

                // B. ACTUALIZAR LA PRIORIDAD MAESTRA
                var priorityMaster = await context.PdPriorities.FindAsync(item.PriorityId);
                if (priorityMaster != null)
                {
                    priorityMaster.ActualValue = item.RegisteredValue;

                    // Si es la revisión final, dictamos sentencia
                    if (isFinalRevision)
                    {
                        priorityMaster.FinalStatus = item.GoalMet ? "Cumplió" : "No Cumplió";
                    }
                }
            }

            await context.SaveChangesAsync();
        }

        // Agrega estos métodos a tu clase PdService.cs

        // 1. Obtener TODOS los empleados (Para el buscador del SuperUsuario)
        // Necesitarás inyectar o usar el contexto para acceder a la tabla Empleados si no usas EmpleadoService
        public async Task<List<EmpleadoViewModel>> GetAllEmpleadosForAdminAsync(string searchTerm)
        {
            using var context = await _factory.CreateDbContextAsync();
            // Asumiendo que tienes una tabla Empleados o similar mapeada
            // Esto es un ejemplo genérico, adáptalo a tu modelo de Empleado
            /*
            var query = context.Empleados.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(e => e.Nombre.Contains(searchTerm) || e.Apellido.Contains(searchTerm));
            }

            return await query.Select(e => new EmpleadoViewModel { ... }).ToListAsync();
            */

            // NOTA: Si ya tienes EmpleadoService, úsalo directamente en el Razor AdminPrioridades
            return new List<EmpleadoViewModel>();
        }

        // 2. DESBLOQUEAR PRIORIDAD (Romper el candado)
        public async Task UnlockPriorityAsync(int id)
        {
            using var context = await _factory.CreateDbContextAsync();
            var p = await context.PdPriorities.FindAsync(id);
            if (p != null)
            {
                // Regresar a estado "Borrador" (Editable)
                p.IsApproved = false;
                p.IsSubmitted = false; // Lo regresamos a borrador para que puedan editar todo
                await context.SaveChangesAsync();
            }
        }

        // =================================================================
        // SECCIÓN 1.5: INDICADORES / KPIs (ADMIN RH)
        // =================================================================

        public async Task<List<PdIndicator>> GetIndicatorsByGoalAsync(int goalId)
        {
            using var context = await _factory.CreateDbContextAsync();
            return await context.PdIndicators
                .Where(i => i.GlobalGoalId == goalId)
                .ToListAsync();
        }

        public async Task SaveIndicatorAsync(PdIndicator indicator)
        {
            using var context = await _factory.CreateDbContextAsync();
            if (indicator.Id == 0)
                context.PdIndicators.Add(indicator);
            else
                context.PdIndicators.Update(indicator);

            await context.SaveChangesAsync();
        }

        public async Task DeleteIndicatorAsync(int id)
        {
            using var context = await _factory.CreateDbContextAsync();
            var ind = await context.PdIndicators.FindAsync(id);
            if (ind != null)
            {
                context.PdIndicators.Remove(ind);
                await context.SaveChangesAsync();
            }
        }
    }
}