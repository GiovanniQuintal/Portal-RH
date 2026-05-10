using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RRHH_ARG.Models;

public partial class Usuario
{
    [Key] // <-- 2. AÑADE ESTA LÍNEA
    [Column("USU_ID")]
    public int UsuId { get; set; }

    public string? UsuCodigo { get; set; }

    public string? UsuUsuario { get; set; }

    public string? UsuContrasena { get; set; }

    public int? RolId { get; set; }

    

    public virtual Rol? Rol { get; set; }
}
