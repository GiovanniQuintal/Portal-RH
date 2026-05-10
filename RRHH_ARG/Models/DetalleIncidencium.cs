using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RRHH_ARG.Models;

[Table("DETALLE_INCIDENCIA")]
public partial class DetalleIncidencium
{
    [Key]
    [Column("DIN_ID")]
    public int DinId { get; set; }

    //public string? DinTipo { get; set; }

    [Column("DIN_ARCHIVO")]
    public string? DinArchivo { get; set; } // Solo guarda el nombre (ej. "foto.jpg")

    // --- NUEVO CAMPO MAPEADO ---
    [Column("DIN_CONTENIDO")]
    public byte[]? DinContenido { get; set; }

    public string? DinDescripcion { get; set; }

    [Column("TIN_ID")]
    public int? TinId { get; set; }

    [Column("DIN_CANTIDAD")]
    public decimal DinCantidad { get; set; } = 1;


    [ForeignKey("TinId")]
    public virtual TipoIncidencia? TipoIncidenciaRel { get; set; }

    public virtual ICollection<Incidencium> Incidencia { get; set; } = new List<Incidencium>();
}
