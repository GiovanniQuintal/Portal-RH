// Models/ExpedienteExclusion.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RRHH_ARG.Models;

[Table("EXPEDIENTE_EXCLUSION")]
public partial class ExpedienteExclusion
{
    [Key]
    [Column("EXC_ID")]
    public int ExcId { get; set; }

    [Column("EMP_ID")]
    public int EmpId { get; set; }

    [Column("DEX_ID")]
    public int? DexId { get; set; }

    [ForeignKey("EmpId")]
    public virtual Empleado? Empleado { get; set; }

    [ForeignKey("DexId")]
    public virtual DetalleExpediente? Dex { get; set; }
}