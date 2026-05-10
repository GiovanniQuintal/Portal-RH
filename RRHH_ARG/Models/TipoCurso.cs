using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RRHH_ARG.Models;

public partial class TipoCurso
{
    [Key]
    public int TicId { get; set; }

    public string? TicNombre { get; set; }

    public virtual ICollection<Curso> Cursos { get; set; } = new List<Curso>();
}
