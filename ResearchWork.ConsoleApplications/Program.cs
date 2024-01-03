using AutoBogus;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using ResearchWork.Entities;
using ResearchWork.SqlTypes;

namespace ResearchWork.ConsoleApplications;

public static class Program
{
    public static async Task Main()
    {
        await using var dbContext = new SchoolDbContext();
        var data = await dbContext.Teachers.ToArrayAsync();
        await dbContext.SaveChangesAsync();
    }
}