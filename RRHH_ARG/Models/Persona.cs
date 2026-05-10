using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RRHH_ARG.Models;

public partial class Persona
{
    [Key] // <-- 2. AÑADE ESTA LÍNEA
    [Column("PER_ID")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int PerId { get; set; }
    [Column("PER_NOMBRE")]
    public string? PerNombre { get; set; }
    [Column("PER_APELLIDO_MATERNO")]
    public string? PerApellidoMaterno { get; set; }
    [Column("PER_APELLIDO_PATERNO")]
    public string? PerApellidoPaterno { get; set; }
    [Column("PER_GENERO")]
    public string? PerGenero { get; set; }
    [Column("PER_EMAIL")]
    public string? PerEmail { get; set; }
    [Column("PER_TELEFONO")]
    public string? PerTelefono { get; set; }
    [Column("PER_FECHA_NACIMIENTO", TypeName = "date")]
    public DateTime? PerFechaNacimiento { get; set; }
    [Column("PER_FECHA_REGISTRO")]
    public DateTime? PerFechaRegistro { get; set; }

    [Column("PEP_ID")]
    public int? PepId { get; set; }
    [Column("PEP_ID2")]
    public int? PepId2 { get; set; }
    [Column("SUB_ID")]
    public int? SubId { get; set; }

   // public virtual PerfilPuesto? Pep { get; set; }
   // public virtual PerfilPuesto? Pep2 { get; set; }
    public virtual Subsidiarium? Sub { get; set; }

    public virtual InformacionMedica? InformacionMedica { get; set; }

    public virtual ICollection<ContactoEmergencia> ContactosEmergencia { get; set; } = new List<ContactoEmergencia>();
    
    [ForeignKey("PepId")]
    [InverseProperty("Personas")]
    public virtual PerfilPuesto? Pep { get; set; }

    // --- PUESTO SECUNDARIO ---
    // Simplemente lo conectamos a su ID, pero NO le ponemos InverseProperty.
    [ForeignKey("PepId2")]
    public virtual PerfilPuesto? Pep2 { get; set; }
}
