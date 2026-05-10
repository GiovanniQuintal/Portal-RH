using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RRHH_ARG.Models;

public partial class TipoEpp
{
    [Key]
    public int TepId { get; set; }

    public string? TepTipo { get; set; }

    // Relaciones
    public virtual ICollection<Epp> Epps { get; set; } = new List<Epp>();

    // ESTA ES LA QUE NECESITAMOS
    public virtual ICollection<DetalleEntregaEpp> DetallesEntrega { get; set; } = new List<DetalleEntregaEpp>();
}