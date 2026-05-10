using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RRHH_ARG.Models
{
    [Table("KARDEX_VACACION")]
    public class KardexVacacion
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("empleado_id")]
        public int EmpleadoId { get; set; }

        [Column("numero_aniversario")]
        public int NumeroAniversario { get; set; }

        [Column("periodo")]
        public string Periodo { get; set; } = string.Empty;

        [Column("dias_derecho")]
        public decimal DiasDerecho { get; set; }

        [Column("dias_tomados")]
        public decimal DiasTomados { get; set; }

        [Column("fecha_inicio")]
        public DateTime FechaInicio { get; set; }

        [Column("fecha_caducidad")]
        public DateTime FechaCaducidad { get; set; }

        [Column("is_vigente")]
        public bool IsVigente { get; set; } = true;
    }
}