using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RRHH_ARG.Models;

public partial class DetalleEntregaEpp
{
    [Key]
    [Column("DET_ID")]
    public int DetId { get; set; }

    [Column("ENT_ID")]
    public int EntId { get; set; }

    [Column("TEP_ID")]
    public int TepId { get; set; }

    [Column("DET_CANTIDAD")]
    public int DetCantidad { get; set; }

    [Column("DET_TALLA_MODELO")]//MODIFICADO DIC 19
    public string? DetTallaModelo { get; set; }

    [Column("DET_ES_NUEVO")]
    public bool DetEsNuevo { get; set; } = true; // Por defecto Nuevo

    [Column("DET_DEVOLVIO_ANTERIOR")]
    public bool DetDevolvioAnterior { get; set; } = false; // Por defecto No

    // Relaciones
    [ForeignKey("EntId")]
    public virtual EntregaEpp? Entrega { get; set; }

    [ForeignKey("TepId")]
    public virtual TipoEpp? TipoEpp { get; set; }
}