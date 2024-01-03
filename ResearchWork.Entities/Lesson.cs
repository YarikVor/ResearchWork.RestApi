using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ResearchWork.Entities;

[Table("lessons")]
public class Lesson
{
    public int Id { get; set; }

    [StringLength(50, MinimumLength = 2)] public string Name { get; set; }

    public string? Description { get; set; }
}