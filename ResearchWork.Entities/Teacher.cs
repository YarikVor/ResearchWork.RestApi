using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ResearchWork.Entities;

[Table("teachers")]
public class Teacher
{
    [Key] public int Id { get; set; }

    [StringLength(50, MinimumLength = 2)] public string Name { get; set; }
}