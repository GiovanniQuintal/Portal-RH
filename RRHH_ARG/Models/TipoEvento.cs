using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RRHH_ARG.Models;

public partial class TipoEvento
{
    [Key]
    public int TipoEventoId { get; set; }

    public string Name { get; set; } = null!;

    public string Module { get; set; } = null!;

    public string Datasource { get; set; } = null!;

    public string Datecolumn { get; set; } = null!;

    public virtual ICollection<Evento> Eventos { get; set; } = new List<Evento>();
}
