using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RRHH_ARG.Models;

[Table("INCIDENCIA")]
public partial class Incidencium
{

    [Key]
    [Column("INC_ID")]
    public int IncId { get; set; }

    public int? DinId { get; set; }

    public DateTime? IncFecha { get; set; }

    public virtual DetalleIncidencium? Din { get; set; }

    public virtual ICollection<HistorialIncidencium> HistorialIncidencia { get; set; } = new List<HistorialIncidencium>();
}
