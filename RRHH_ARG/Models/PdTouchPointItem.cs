using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RRHH_ARG.Models
{
    [Table("PD_TOUCHPOINT_ITEMS")]
    public class PdTouchpointItem
    {
        [Key] public int Id { get; set; }
        public int TouchpointId { get; set; }

        public int PriorityId { get; set; } // Link a la prioridad original

        // Guardamos el valor que tenía en ese momento (Snapshot)
        public decimal RegisteredValue { get; set; }

        // Retroalimentación específica para este KPI
        public string? Feedback { get; set; }

        [ForeignKey("TouchpointId")]
        public virtual PdTouchpoint? Touchpoint { get; set; }

        [ForeignKey("PriorityId")]
        public virtual PdPriority? Priority { get; set; }

        public bool GoalMet { get; set; }
    }
}