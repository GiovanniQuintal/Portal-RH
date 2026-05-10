using System;
using RRHH_ARG.Model;

namespace RRHH_ARG.Model
{
    // Este modelo se usa SÓLO para mostrar datos en la tabla global
    public class IncidenciaViewModel
    {
        public int IncidenciaId { get; set; }
        public int EmpleadoId { get; set; }

        // Datos del Empleado
        public string NombreEmpleado { get; set; } = string.Empty;
        public string AreaEmpleado { get; set; } = string.Empty;

        // Datos de la Incidencia
        public string Tipo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public DateTime FechaIncidencia { get; set; }
    }
}