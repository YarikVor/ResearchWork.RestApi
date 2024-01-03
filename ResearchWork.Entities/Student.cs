using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ResearchWork.Entities;

[Table("students")]
public class Student
{
    [Key] public int Id { get; set; }

    [StringLength(50, MinimumLength = 2)] public string Name { get; set; }

    [ForeignKey(nameof(Group))] public int GroupId { get; set; }

    public virtual Group Group { get; set; }

    public DateTime Birthday { get; set; }
}