using System;

namespace RRHH_ARG.Models
{
    public class CalendarEventDTO
    {
        public string Title { get; set; } = string.Empty;
        public DateTime Start { get; set; }
        public string Type { get; set; } = string.Empty; // "CUMPLEAÑOS", "ENTREVISTA", etc.
        public string IconPath { get; set; } = string.Empty; // Para tu lógica de icono
        public int ReferenceId { get; set; } // <--- NUEVO CAMPO
    }
}
