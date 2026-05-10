using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RRHH_ARG.Models
{
    [Table("parametro_anual")]
    public class ParametroAnual
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("anio_vigencia")]
        public int AnioVigencia { get; set; }

        [Column("salario_minimo")]
        public decimal SalarioMinimo { get; set; }

        [Column("valor_uma")]
        public decimal ValorUma { get; set; }

        [Column("dias_aguinaldo")]
        public int DiasAguinaldo { get; set; } = 15;

        [Column("porcentaje_prima_vacacional")]
        public decimal PorcentajePrimaVacacional { get; set; } = 0.25m;

        [Column("is_activo")]
        public bool IsActivo  { get; set; } = true;
    }
}