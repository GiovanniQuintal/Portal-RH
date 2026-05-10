namespace RRHH_ARG.Models
{
    public class ExpedienteViewModel
    {
        // Datos para lógica (ocultos)
        public int ExpId { get; set; }      // ID del registro (0 si no se ha subido nada)
        public int DexId { get; set; }      // ID del tipo de documento (Catálogo)
        public int EmpleadoId { get; set; }
        // Datos para mostrar en la tabla
        public string Nomenclatura { get; set; } // Viene de DetalleExpediente
        public string Nombre { get; set; }       // Viene de DetalleExpediente

        // Datos del archivo subido
        public string? NombreArchivo { get; set; } // Viene de Expediente.ExpRuta
        public int? DeuId { get; set; }
        public DateTime? FechaSubida { get; set; }
        public DateTime? FechaCaducidad { get; set; }
    }
}