using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RRHH_ARG.Models
{
    [Table("QHSE_CARD_DETAIL")]
    public class QhseCardDetail
    {
        [Key]
        [Column("QCD_ID")]
        public int QcdId { get; set; }

        [Column("QHC_ID")]
        public int QhcId { get; set; }

        [ForeignKey("QhcId")]
        public virtual QhseCard Card { get; set; }

        [Column("QCD_CATEGORIA")]
        public string Categoria { get; set; } // Ej: "Posiciones", "EPP", "Herramientas"

        [Column("QCD_ITEM")]
        public string ItemNombre { get; set; } // Ej: "Cabeza", "Caídas"

        [Column("QCD_MARCADO")]
        public bool EstaMarcado { get; set; }
    }
}