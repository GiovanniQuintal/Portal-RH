using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RRHH_ARG.Models;

public partial class HistorialEntrenamiento
{
    [Key]
    public int HenId { get; set; }

    public int? EntId { get; set; }

    public virtual ICollection<Empleado> Empleados { get; set; } = new List<Empleado>();

    public virtual Entrenamiento? Ent { get; set; }
}
