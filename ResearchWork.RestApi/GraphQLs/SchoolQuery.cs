using ResearchWork.Entities;
using ResearchWork.SqlTypes;

namespace ResearchWork.RestApi.GraphQLs;

public class SchoolQuery
{
    public IQueryable<Student> GetStudents([Service] SchoolDbContext dbContext)
    {
        return dbContext.Students;
    }

    public IQueryable<Teacher> GetTeachers([Service] SchoolDbContext dbContext)
    {
        return dbContext.Teachers;
    }

    public IQueryable<Group> GetGroups([Service] SchoolDbContext dbContext)
    {
        return dbContext.Groups;
    }

    public IQueryable<Evaluation> GetEvaluations([Service] SchoolDbContext dbContext)
    {
        return dbContext.Evaluations;
    }

    public IQueryable<Lesson> GetLessons([Service] SchoolDbContext dbContext)
    {
        return dbContext.Lessons;
    }
}