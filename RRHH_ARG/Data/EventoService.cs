using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RRHH_ARG.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims; // <--- NECESARIO PARA CLAIMS
using System.Threading.Tasks;

namespace RRHH_ARG.Data
{
    public class EventoService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public EventoService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        // AHORA RECIBIMOS EL USUARIO (ClaimsPrincipal)
        public async Task<List<CalendarEventDTO>> GetDashboardEventsAsync(ClaimsPrincipal user)
        {
            using var scope = _scopeFactory.CreateScope();
            var _context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var listaEventos = new List<CalendarEventDTO>();

            DateTime fechaCorte = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            int currentYear = DateTime.Now.Year;

            // ==========================================================
            // 1. CUMPLEAÑOS (VISIBLE PARA TODOS: Empleados, RH, Admin)
            // ==========================================================
            // Esta lógica se ejecuta siempre, sin importar el rol.
            var cumpleañeros = await _context.Personas
                .Where(p => p.PerFechaNacimiento != null)
                .Select(p => new {
                    Nombre = p.PerNombre + " " + p.PerApellidoPaterno,
                    Dia = p.PerFechaNacimiento.Value.Day,
                    Mes = p.PerFechaNacimiento.Value.Month
                })
                .ToListAsync();

            foreach (var persona in cumpleañeros)
            {
                for (int year = currentYear; year <= currentYear + 1; year++)
                {
                    try
                    {
                        var fechaCumple = new DateTime(year, persona.Mes, persona.Dia);
                        if (fechaCumple >= fechaCorte)
                        {
                            listaEventos.Add(new CalendarEventDTO
                            {
                                Title = $"Cumpleaños: {persona.Nombre}",
                                Start = fechaCumple,
                                Type = "CUMPLEAÑOS",
                                IconPath = "images/birthday_icon.png"
                            });
                        }
                    }
                    catch { }
                }
            }

            // ==========================================================
            // LOGICA DE SEGURIDAD:
            // Solo si es RH o ADMIN buscamos información sensible
            // ==========================================================

            bool esPersonalRH = user.IsInRole("RH") || user.IsInRole("Admin");

            if (esPersonalRH)
            {
                // ==========================================================
                // 2. VENCIMIENTOS (SOLO RH/ADMIN)
                // ==========================================================
                var vencimientos = await (from exp in _context.Expediente
                                          join emp in _context.Empleados on exp.EmpId equals emp.EmpId
                                          join per in _context.Personas on emp.PerId equals per.PerId
                                          join dex in _context.DetalleExpediente on exp.DexId equals dex.DexId
                                          where exp.ExpFechacaducidad != null && exp.ExpFechacaducidad >= fechaCorte
                                          select new
                                          {
                                              EmpId = emp.EmpId,
                                              Nombre = per.PerNombre + " " + per.PerApellidoPaterno,
                                              Documento = dex.DexNombre ?? "Documento",
                                              Fecha = exp.ExpFechacaducidad
                                          }).ToListAsync();

                listaEventos.AddRange(vencimientos.Select(v => new CalendarEventDTO
                {
                    Title = $"Vence {v.Documento}: {v.Nombre}",
                    Start = v.Fecha.Value,
                    Type = "EXPEDIENTE_VENCIMIENTO",
                    IconPath = "images/document_icon.png",
                    ReferenceId = v.EmpId
                }));

                // ==========================================================
                // 3. ENTREVISTAS (SOLO RH/ADMIN)
                // ==========================================================
                var entrevistas = await (from apl in _context.Aplicante
                                         join per in _context.Personas on apl.PerId equals per.PerId
                                         where apl.AplFechaInicioEntrevista != null && apl.AplFechaInicioEntrevista >= fechaCorte
                                         && apl.AplIspromoted == false
                                         select new
                                         {
                                             AplId = apl.AplId,
                                             Nombre = per.PerNombre + " " + per.PerApellidoPaterno,
                                             Fecha = apl.AplFechaInicioEntrevista
                                         }).ToListAsync();

                listaEventos.AddRange(entrevistas.Select(e => new CalendarEventDTO
                {
                    Title = $"Entrevista: {e.Nombre}",
                    Start = e.Fecha.Value,
                    Type = "ENTREVISTA",
                    IconPath = "images/interview_icon.png",
                    ReferenceId = e.AplId
                }));
            }

            return listaEventos.OrderBy(e => e.Start).ToList();
        }
    }
}