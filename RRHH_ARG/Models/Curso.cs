using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RRHH_ARG.Models;

public partial class Curso
{
    [Key]
    public int CurId { get; set; }

    public string? CurDescripcion { get; set; }

    public DateTime? CurVigencia { get; set; }

    public int? TicId { get; set; }

    public bool CurObligatorio { get; set; }

    public virtual ICollection<Entrenamiento> Entrenamientos { get; set; } = new List<Entrenamiento>();

    public virtual TipoCurso? Tic { get; set; }
}
