using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RRHH_ARG.Models;

public partial class Supervisor
{
    [Key] // <-- 2. AÑADE ESTA LÍNEA
    [Column("SUP_ID")]
    public int SupId { get; set; }

    public string? SupNombre { get; set; }

    //public virtual ICollection<Empleado> Empleados { get; set; } = new List<Empleado>();
}

