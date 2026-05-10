using System;
using System.Collections.Generic;

namespace RRHH_ARG.Models
{
    public class MoodleCurso
    {
        public int McId { get; set; }
        public int MoodleCursoId { get; set; } // ID original de Moodle
        public string McNombre { get; set; } = null!;
        public string? McDescripcion { get; set; }
        public string McCodigo { get; set; } = null!; // MC_CODIGO
        public bool McEsPublico { get; set; }        // MC_ES_PUBLICO
        public DateTime? McFechaInicio { get; set; }
        public DateTime? McFechaFin { get; set; }
        public byte McEstado { get; set; } // 1: Activo, 0: Inactivo, etc.
        public int TicId { get; set; } // FK Tipo Curso
        public DateTime McFechaSync { get; set; }

        // Relaciones
        public virtual TipoCurso TipoCurso { get; set; } = null!;
        public virtual ICollection<MoodleCursoArea> MoodleCursoAreas { get; set; } = new List<MoodleCursoArea>();
        public virtual ICollection<MoodleCursoEmpleado> MoodleCursoEmpleados { get; set; } = new List<MoodleCursoEmpleado>();
    }
}