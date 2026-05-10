using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RRHH_ARG.Models;

[Table("DETALLE_EXPEDIENTE")]
public partial class DetalleExpediente
{
    [Key] // <-- 2. AÑADE ESTA LÍNEA
    [Column("DEX_ID")]
    public int DexId { get; set; }
    [Column("DEX_NOMENCLATURA")]
    public string? DexNomenclatura { get; set; }
    [Column("DEX_NOMBRE")]
    public string? DexNombre { get; set; }
    [Column("DEX_FECHA")]
    public DateTime? DexFecha { get; set; }

    public virtual ICollection<Expediente> Expedientes { get; set; } = new List<Expediente>();
}