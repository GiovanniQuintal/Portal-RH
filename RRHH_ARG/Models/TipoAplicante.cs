using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RRHH_ARG.Models;

public partial class TipoAplicante
{
    [Key]
    [Column("TIP_ID")]
    public int TipId { get; set; }

    [Column("TIP_NOMBRE")]
    public string? TipNombre { get; set; }

    // Relación: Un tipo puede tener muchos aplicantes
    public virtual ICollection<Aplicante> Aplicantes { get; set; } = new List<Aplicante>();
}
