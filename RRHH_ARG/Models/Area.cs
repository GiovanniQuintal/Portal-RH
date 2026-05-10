using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations; // <-- 1. AÑADE ESTA LÍNEA
using System.ComponentModel.DataAnnotations.Schema;

namespace RRHH_ARG.Models;

public partial class Area
{
    [Key] // <-- 2. AÑADE ESTA LÍNEA
    [Column("ARE_ID")]
    public int AreId { get; set; }
    [Column("ARE_NOMBRE")]
    public string? AreNombre { get; set; }

    [Column("MOODLE_COHORT_ID")]
    public int? MoodleCohortId { get; set; }

    public virtual ICollection<PerfilPuesto> PerfilesPuesto { get; set; } = new List<PerfilPuesto>();
}
