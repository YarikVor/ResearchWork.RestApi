using Microsoft.EntityFrameworkCore;
using ResearchWork.Entities;

namespace ResearchWork.SqlTypes;

public class SchoolDbContext : DbContext
{
    public SchoolDbContext()
    {
        Database.EnsureCreated();
        SaveChanges();
    }

    public DbSet<Evaluation> Evaluations { get; set; }
    public DbSet<Group> Groups { get; set; }
    public DbSet<Lesson> Lessons { get; set; }
    public DbSet<Student> Students { get; set; }
    public DbSet<Teacher> Teachers { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=data.db");
        optionsBuilder.UseLazyLoadingProxies();
    }
}