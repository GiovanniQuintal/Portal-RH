using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RRHH_ARG.Models
{
    [Table("NominaConceptos")]
    public class NominaConceptos
    {
        [Key]
        public int ConceptoId { get; set; }

        [Required]
        [StringLength(150)]
        public string Concepto { get; set; }

        [Required]
        [StringLength(2)]
        public string Tipo { get; set; } // 'P' o 'D'

        public bool CeldaEditable { get; set; }

        [StringLength(50)]
        public string? Clasificacion { get; set; } = "REGULAR";

        public bool MostrarEnSabana { get; set; }

        public bool EsMontoDirecto { get; set; }

        // Navegación
        public virtual ICollection<NominaEmpleadoDetalle> NominaEmpleadoDetalles { get; set; } = new List<NominaEmpleadoDetalle>();
    }
}