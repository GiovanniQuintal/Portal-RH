using RRHH_ARG.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("PD_INDICATORS")]
public class PdIndicator
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int GlobalGoalId { get; set; }

    [ForeignKey("GlobalGoalId")]
    public virtual PdGlobalGoal? GlobalGoal { get; set; }

    [Required]
    public string Title { get; set; } = "";

    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    public virtual ICollection<PdPriority> Priorities { get; set; } = new List<PdPriority>();
}