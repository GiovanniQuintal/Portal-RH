using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RRHH_ARG.Models;

[Table("CONTACTO_EMERGENCIA")] // Nombre sugerido para la tabla
public partial class ContactoEmergencia
{
    [Key]
    [Column("CEM_ID")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int CemId { get; set; }

    [Column("CEM_NOMBRE_COMPLETO")]
    public string? CemNombreCompleto { get; set; }

    [Column("CEM_PARENTESCO")]
    public string? CemParentesco { get; set; } // Ej: Padre, Hermano

    [Column("CEM_TELEFONO_1")]
    public string? CemTelefono1 { get; set; }

    [Column("CEM_TELEFONO_2")]
    public string? CemTelefono2 { get; set; }

    // --- Llave Foránea hacia Persona ---
    [Column("PER_ID")]
    public int PerId { get; set; }

    [ForeignKey("PerId")]
    public virtual Persona? Persona { get; set; }
}