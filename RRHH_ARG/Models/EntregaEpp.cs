using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RRHH_ARG.Models;

public partial class EntregaEpp
{
    [Key]
    [Column("ENT_ID")]
    public int EntId { get; set; }

    [Column("EMP_ID")]
    public int EmpId { get; set; }

    [Column("ENT_FECHA")]
    public DateTime? EntFecha { get; set; }

    [Column("ENT_EVIDENCIA")]
    public byte[]? EntEvidencia { get; set; } // Aquí se guarda el PDF

    // Relaciones
    [ForeignKey("EmpId")]
    public virtual Empleado? Empleado { get; set; }

    public virtual ICollection<DetalleEntregaEpp> Detalles { get; set; } = new List<DetalleEntregaEpp>();
}
