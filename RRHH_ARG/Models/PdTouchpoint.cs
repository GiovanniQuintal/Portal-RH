using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RRHH_ARG.Models
{
    [Table("PD_TOUCHPOINTS")]
    public class PdTouchpoint
    {
        [Key] public int Id { get; set; }
        public int EmpId { get; set; }
        public int SupervisorId { get; set; }

        public int RevisionNumber { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;

        // Notas específicas (Opcionales, por si quieres usarlas en el futuro)
        public string? EmployeeNotes { get; set; }
        public string? ManagerNotes { get; set; }

        // --- ESTA ES LA QUE FALTABA ---
        public string? GeneralNotes { get; set; }

        public bool IsApproved { get; set; }
        public DateTime? ApprovalDate { get; set; }

        public virtual ICollection<PdTouchpointItem> Items { get; set; } = new List<PdTouchpointItem>();

        [NotMapped]
        public string Type => $"Revisión R{RevisionNumber}";
    }
}