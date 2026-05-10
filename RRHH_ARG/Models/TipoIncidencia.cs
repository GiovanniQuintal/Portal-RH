using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RRHH_ARG.Models
{
    [Table("TIPO_INCIDENCIA")]
    public class TipoIncidencia
    {
        [Key]
        [Column("TIN_ID")]
        public int TinId { get; set; }

        [Column("TIN_NOMBRE")]
        public string TinNombre { get; set; } = string.Empty;

        [Column("TIN_VISIBLE_UI")]
        public bool TinVisibleUi { get; set; }

        [Column("CON_ID")]
        public int? ConId { get; set; }

        [ForeignKey("ConId")]
        public virtual NominaConceptos? NominaConcepto { get; set; }
    }
}