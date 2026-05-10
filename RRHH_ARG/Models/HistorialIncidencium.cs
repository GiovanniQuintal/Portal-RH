using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RRHH_ARG.Models;

[Table("HISTORIAL_INCIDENCIA")]
public partial class HistorialIncidencium
{
    [Key]
    [Column("HIN_ID")]
    public int HinId { get; set; }

    [Column("EMP_ID")]
    public int? EmpId { get; set; }

    [ForeignKey("EmpId")]
    public virtual Empleado? Empleado { get; set; }

    public int? IncId { get; set; }

    //public virtual ICollection<Empleado> Empleados { get; set; } = new List<Empleado>();

    public virtual Incidencium? Inc { get; set; }
}
