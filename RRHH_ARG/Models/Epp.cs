using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RRHH_ARG.Models;

public partial class Epp
{
    [Key]
    public int EppId { get; set; }

    public int? TepId { get; set; }

    public int? EppCantidad { get; set; }

    public DateTime? EppFecha { get; set; }

    public virtual ICollection<HistorialEpp> HistorialEpps { get; set; } = new List<HistorialEpp>();

    [ForeignKey("TepId")]
    public virtual TipoEpp? Tep { get; set; }
}
