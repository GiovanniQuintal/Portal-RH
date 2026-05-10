using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RRHH_ARG.Models
{
    [Table("PD_GLOBAL_GOALS")]
    public class PdGlobalGoal
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Title { get; set; }
        public string? Description { get; set; }
        public int Year { get; set; }
        public bool IsActive { get; set; } = true;
        
        public virtual ICollection<PdIndicator> Indicators { get; set; } = new List<PdIndicator>();
    }
}