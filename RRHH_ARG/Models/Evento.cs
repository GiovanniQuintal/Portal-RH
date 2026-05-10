using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RRHH_ARG.Models;

public partial class Evento
{
    [Key]
    public int EventId { get; set; }

    public string NameEvent { get; set; } = null!;

    public DateTime EventDate { get; set; }

    public int TipoEventoId { get; set; }

    public virtual TipoEvento TipoEvento { get; set; } = null!;
}
