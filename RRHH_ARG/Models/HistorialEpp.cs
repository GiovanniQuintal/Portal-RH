using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RRHH_ARG.Models;

public partial class HistorialEpp
{
    [Key]
    public int HepId { get; set; }

    public int? EppId { get; set; }

    public virtual ICollection<Empleado> Empleados { get; set; } = new List<Empleado>();

    public virtual Epp? Epp { get; set; }
}
