using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RRHH_ARG.Models;

public partial class EntrenamientoStatus
{
    [Key]
    public int EnsId { get; set; }

    public string? EnsStatus { get; set; }

    public virtual ICollection<Entrenamiento> Entrenamientos { get; set; } = new List<Entrenamiento>();
}
