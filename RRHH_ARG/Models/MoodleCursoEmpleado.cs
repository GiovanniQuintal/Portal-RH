using System;

namespace RRHH_ARG.Models
{
    public class MoodleCursoEmpleado
    {
        public int EmpId { get; set; }
        public int McId { get; set; }
        public DateTime MceFechaAsignacion { get; set; }
        public DateTime? MceFechaInicio { get; set; }
        public DateTime? MceFechaFin { get; set; }
        public decimal McePorcentajeAvance { get; set; }
        public byte MceEstado { get; set; } // Estado: Activo/Completado/Expirado
        public DateTime? MceFechaUltimoAcceso { get; set; }
        public DateTime MceFechaSync { get; set; }

        // Relaciones
        public virtual Empleado Empleado { get; set; } = null!;
        public virtual MoodleCurso Curso { get; set; } = null!;
    }
}