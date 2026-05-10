using RRHH_ARG.Models;

namespace RRHH_ARG.Models
{
    public class EntregaEppViewModel
    {
        public int EntId { get; set; }
        public DateTime Fecha { get; set; } = DateTime.Now;
        public int EmpleadoId { get; set; }
        public string NombreEmpleado { get; set; } = "";
        
        // Para mostrar en la tabla
        public string ListaEpps { get; set; } = "";
        public string ListaCantidades { get; set; } = "";

        // Para el formulario
        public int AreaSeleccionadaId { get; set; }
        public List<SeleccionEppItem> EppsSeleccionados { get; set; } = new List<SeleccionEppItem>();
        
        // CAMBIO: Almacenamos los bytes directamente en el ViewModel
        public byte[]? EvidenciaBytes { get; set; }
        public string? NombreArchivo { get; set; } // Opcional: para logging
        public bool TieneEvidencia { get; set; }
    }

    public class SeleccionEppItem
    {
        public int TepId { get; set; }
        public string Nombre { get; set; } = "";
        public bool IsSelected { get; set; }
        public int Cantidad { get; set; } = 1;
        // Nuevos campos extras MODIFICADO DIC 19
        public string? TallaModelo { get; set; }
        public bool EsNuevo { get; set; } = true;
        public bool DevolvioAnterior { get; set; } = false;
    }
}