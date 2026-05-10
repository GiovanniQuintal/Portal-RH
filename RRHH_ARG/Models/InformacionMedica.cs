using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RRHH_ARG.Models;

[Table("INFORMACION_MEDICA")] // Nombre sugerido para la tabla
public partial class InformacionMedica
{
    [Key]
    [Column("IME_ID")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ImeId { get; set; }

    [Column("IME_TIPO_SANGRE")]
    [StringLength(10)]
    public string? ImeTipoSangre { get; set; }

    [Column("IME_ALERGIAS")]
    public string? ImeAlergias { get; set; }

    [Column("IME_PADECIMIENTOS")]
    public string? ImePadecimientos { get; set; }

    [Column("IME_OBSERVACIONES")]
    public string? ImeObservaciones { get; set; }

    // --- Llave Foránea hacia Persona ---
    [Column("PER_ID")]
    public int PerId { get; set; }

    [ForeignKey("PerId")]
    public virtual Persona? Persona { get; set; }
}
