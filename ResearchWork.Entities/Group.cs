using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ResearchWork.Entities;

[Table("groups")]
public class Group
{
    [Key] public int Id { get; set; }

    [StringLength(50, MinimumLength = 2)] public string Name { get; set; }

    public DateTime Created { get; set; }

    public int CuratorId { get; set; }

    [ForeignKey(nameof(CuratorId))] public virtual Teacher Curator { get; set; }

    public string? Description { get; set; }
}