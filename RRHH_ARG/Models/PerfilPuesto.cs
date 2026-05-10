using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RRHH_ARG.Models;

public partial class PerfilPuesto
{
    [Key] // <-- 2. AÑADE ESTA LÍNEA
    [Column("PEP_ID")]
    public int PepId { get; set; }
    [Column("PEP_NOMBRE")]
    public string? PepNombre { get; set; }
    [Column("ARE_ID")]
    public int? AreId { get; set; } // La llave foránea
    [Column("PEP_DESCRIPCION")]
    public string? PepDescripcion { get; set; } // El '?' permite nulos
    [Column("PEP_ARCHIVO")]
    public byte[]? PepArchivo { get; set; }
    public virtual Area? Are { get; set; }

    public virtual ICollection<Persona> Personas { get; set; } = new List<Persona>();
}