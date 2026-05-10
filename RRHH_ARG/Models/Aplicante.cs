using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations; // <-- 1. AÑADE ESTA LÍNEA
using System.ComponentModel.DataAnnotations.Schema;

namespace RRHH_ARG.Models;

public partial class Aplicante
{
    [Key] // <-- 2. AÑADE ESTA LÍNEA
    [Column("APL_ID")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int AplId { get; set; }
    [Column("PER_ID")]
    public int? PerId { get; set; }
    [Column("APL_STATUS")]
    public bool AplStatus { get; set; }
    [Column("APL_FECHA_INICIO_ENTREVISTA")]
    public DateTime? AplFechaInicioEntrevista { get; set; }
    [Column("APL_FECHA_TERMINO_ENTREVISTA")]
    public DateTime? AplFechaTerminoEntrevista { get; set; }
    [Column("APL_ISPROMOTED")]
    public bool AplIspromoted { get; set; }
    [Column("APL_CV")]
    public string? AplCv { get; set; }
    [Column("APL_CV_ARCHIVO")]
    public byte[]? AplCvArchivo { get; set; }
    [Column("TIP_ID")]
    public int? TipId { get; set; }
    [ForeignKey("TipId")]
    public virtual TipoAplicante? TipoAplicante { get; set; }
}