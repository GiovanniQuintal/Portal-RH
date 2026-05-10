using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RRHH_ARG.Models;

public partial class Expediente
{
    [Key]
    [Column("EXP_ID")]
    public int ExpId { get; set; }

    [Column("DEX_ID")]
    public int? DexId { get; set; } // Relación con el Catálogo (Qué documento es)

    [Column("EMP_ID")]
    public int? EmpId { get; set; } // <--- AÑADE ESTO: ¿De qué empleado es?

    [Column("EXP_FECHACADUCIDAD", TypeName = "date")]
    public DateTime? ExpFechacaducidad { get; set; }

    [Column("EXP_FECHASUBIDO", TypeName = "date")]
    public DateTime? ExpFechasubido { get; set; }

    [Column("EXP_RUTA")]
    public string? ExpRuta { get; set; } // <--- AÑADE ESTO: El nombre del archivo (ej: acta.pdf)

    [Column("EXP_ARCHIVO")]
    public byte[]? ExpArchivo { get; set; }
    [Column("DEU_ID")]
    public int? DeuId { get; set; }

    // Relaciones
    [ForeignKey("DeuId")]
    public virtual DetalleExpedienteUnico? Deu { get; set; }
    [ForeignKey("DexId")]
    public virtual DetalleExpediente? Dex { get; set; }
    [ForeignKey("EmpId")]
    public virtual Empleado? Empleado { get; set; }




    public virtual ICollection<HistorialExpediente> HistorialExpedientes { get; set; } = new List<HistorialExpediente>();
}

