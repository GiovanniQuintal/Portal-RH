using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RRHH_ARG.Models;

public partial class Entrenamiento
{
    [Key]
    public int EntId { get; set; }

    public int? CurId { get; set; }

    public DateTime? EntFechaasignacion { get; set; }

    public DateTime? EntFechacompletacion { get; set; }

    public DateTime? EntFechaexpiracion { get; set; }

    public int? EnsId { get; set; }

    public virtual Curso? Cur { get; set; }

    public virtual EntrenamientoStatus? Ens { get; set; }

    public virtual ICollection<HistorialEntrenamiento> HistorialEntrenamientos { get; set; } = new List<HistorialEntrenamiento>();
}
