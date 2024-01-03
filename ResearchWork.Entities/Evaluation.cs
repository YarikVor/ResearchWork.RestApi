using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ResearchWork.Entities;

[Table("evaluations")]
public class Evaluation
{
    [Key] public int Id { get; set; }

    public int StudentId { get; set; }

    [ForeignKey(nameof(StudentId))] public virtual Student Student { get; set; }

    public int LessonId { get; set; }

    [ForeignKey(nameof(LessonId))] public virtual Lesson Lesson { get; set; }

    public int TeacherId { get; set; }

    [ForeignKey(nameof(TeacherId))] public virtual Teacher Teacher { get; set; }

    public float? Value { get; set; }

    public string? Description { get; set; }
}