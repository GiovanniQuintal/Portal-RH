using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RRHH_ARG.Models
{
    [Table("proyeccion_pasivo")]
    public class ProyeccionPasivo
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("empleado_id")]
        public int EmpleadoId { get; set; }

        [Column("fecha_calculo")]
        public DateTime FechaCalculo { get; set; }

        [Column("monto_finiquito")]
        public decimal MontoFiniquito { get; set; }

        [Column("monto_liquidacion")]
        public decimal MontoLiquidacion { get; set; }

        [Column("is_ultima_proyeccion")]
        public bool IsUltimaProyeccion { get; set; } = true;
    }
}
