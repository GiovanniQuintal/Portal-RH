using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RRHH_ARG.Models;


public partial class HistorialExpediente
{
    [Key]
    public int HexId { get; set; }

    public int? ExpId { get; set; }

    

    
}
