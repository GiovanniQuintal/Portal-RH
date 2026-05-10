using System;

namespace RRHH_ARG.Model
{
    public class Expediente
    {
        // --- Campos Recomendados ---

        // Buena práctica: un Id único para la base de datos
        public int Id { get; set; }

        // Necesario: para saber a qué empleado pertenece este expediente
        public int EmpleadoId { get; set; }

        // --- Tus Campos (Modificados/Mantenidos) ---

        // Mantenido: Ya lo tenías.
        public int Numero { get; set; }

        // Mantenido
        public string Nomenclatura { get; set; } = string.Empty;

        // Mantenido (Ej: "Acta de Nacimiento")
        public string Nombre { get; set; } = string.Empty;

        // --- Campos Faltantes (Agregados) ---

        // Agregado: Para guardar el nombre del archivo (Ej: "acta_juan.pdf")
        // El '?' (nullable) permite que esté vacío si no se ha subido nada.
        public string? NombreArchivo { get; set; }

        // Agregado: Para guardar cuándo se subió el archivo
        // También es nullable.
        public DateTime? FechaSubida { get; set; }

        // Modificado: Tu campo "Vencimiento" se renombró y se hizo Nullable
        // El '?' es clave. Permite que el valor sea 'null', 
        // lo que usaremos para representar "No Aplica".
        public DateTime? FechaCaducidad { get; set; }
    }
}