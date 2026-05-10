using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RRHH_ARG.Models
{
    [Table("vacacion_tomada")]
    public class VacacionTomada
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("kardex_vacacion_id")]
        public int KardexVacacionId { get; set; }

        [Column("empleado_id")]
        public int EmpleadoId { get; set; }

        [Column("fecha_inicial")]
        public DateTime FechaInicial { get; set; }

        [Column("fecha_final")]
        public DateTime FechaFinal { get; set; }

        [Column("fecha_pago")]
        public DateTime? FechaPago { get; set; }

        [Column("dias_descanso")]
        public decimal DiasDescanso { get; set; }

        [Column("dias_vacaciones")]
        public decimal DiasVacaciones { get; set; }

        [Column("prima_vacacional_dias")]
        public decimal PrimaVacacionalDias { get; set; }

        [Column("fecha_registro")]
        public DateTime FechaRegistro { get; set; } = DateTime.Now;

        [Column("incidencia_id")]
        public int? IncidenciaId { get; set; }
    }
}
