using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RRHH_ARG.Models
{
    [Table("EMPLEADO_BENEFICIO")] // Nombre exacto de la tabla en SQL
    public class EmpleadoBeneficio
    {
        [Key, Column(Order = 0)]
        public int EmpId { get; set; }

        [Key, Column(Order = 1)]
        public int ConId { get; set; }

        public decimal MontoQ1 { get; set; }
        public decimal MontoQ2 { get; set; }
        // --- RELACIÓN AL NUEVO CATÁLOGO ---
        [ForeignKey("ConId")]
        public virtual NominaConceptos? NominaConcepto { get; set; }

        [ForeignKey("EmpId")]
        public virtual Empleado? Empleado { get; set; }
    }
}