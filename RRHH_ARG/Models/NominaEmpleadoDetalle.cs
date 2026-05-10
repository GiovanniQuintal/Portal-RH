using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RRHH_ARG.Models
{
    [Table("NominaEmpleadoDetalle")]
    public class NominaEmpleadoDetalle
    {
        [Key]
        public int Id { get; set; }

        public int NominaEmpleadoId { get; set; }

        public int ConceptoId { get; set; }

        public decimal? Cantidad { get; set; }

        public decimal Importe { get; set; }

        public decimal Total { get; set; }

        [StringLength(255)]
        public string? Comentarios { get; set; }

        // Navegación
        [ForeignKey("NominaEmpleadoId")]
        public virtual NominaEmpleado NominaEmpleado { get; set; }

        [ForeignKey("ConceptoId")]
        public virtual NominaConceptos NominaConcepto { get; set; }
    }
}