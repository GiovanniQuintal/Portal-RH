using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RRHH_ARG.Models
{
    [Table("NominaEmpleado")]
    public class NominaEmpleado
    {
        [Key]
        public int NominaEmpleadoId { get; set; }

        public int NominaId { get; set; }

        // Mantuvimos el nombre original de la BD según tu instrucción
        [Column("EMP_ID")]
        public int EMP_ID { get; set; }

        public decimal Salario { get; set; }

        public decimal DiasPagados { get; set; } = 15;

        public decimal Percepciones { get; set; }

        public decimal Deducciones { get; set; }

        public decimal Pagado { get; set; }

        [Required]
        [StringLength(50)]
        public string Estado { get; set; } = "EN_PROCESO";

        // Navegación
        [ForeignKey("NominaId")]
        public virtual Nomina Nomina { get; set; }

        [ForeignKey("EMP_ID")]
        public virtual Empleado Empleado { get; set; }

        public virtual ICollection<NominaEmpleadoDetalle> NominaEmpleadoDetalles { get; set; } = new List<NominaEmpleadoDetalle>();
    }
}