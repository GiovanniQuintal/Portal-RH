using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace RRHH_ARG.Models
{

        public partial class Empleado
        {
            // NOTA: Quitamos la flecha '=>' y agregamos '{ get; set; }'
            // Esto permite que nosotros le metamos el dato manualmente.

            [NotMapped]
            public string Nombre { get; set; } = string.Empty;

            [NotMapped]
            public string Apellidos { get; set; } = string.Empty;

            [NotMapped]
            public string Subsidiaria { get; set; } = string.Empty;

            [NotMapped]
            public string Area { get; set; } = string.Empty;

            // Alias para el ID (para que tu modal no se rompa si usa .Id)
            [NotMapped]
            public int Id => EmpId;
        
    }
}
