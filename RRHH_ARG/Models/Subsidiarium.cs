using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RRHH_ARG.Models;

public partial class Subsidiarium
{
    [Key]
    public int SubId { get; set; }

    public string? SubNombre { get; set; }

    public virtual ICollection<Persona> Personas { get; set; } = new List<Persona>();
}
