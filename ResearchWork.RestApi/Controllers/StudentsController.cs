using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ResearchWork.RestApi.Dtos;
using ResearchWork.SqlTypes;
using IConfigurationProvider = AutoMapper.IConfigurationProvider;

namespace ResearchWork.RestApi.Controllers;

using IConfigurationProvider = IConfigurationProvider;

[ApiController]
[Route("api/[controller]")]
public class StudentsController : Controller
{
    private readonly IConfigurationProvider _configuration;
    private readonly SchoolDbContext _dbContext;

    public StudentsController(SchoolDbContext dbContext, IConfigurationProvider configuration)
    {
        _dbContext = dbContext;
        _configuration = configuration;
    }

    [HttpGet("list/short")]
    public async Task<IActionResult> GetShortItems()
    {
        var models = await _dbContext.Students
            .AsNoTracking()
            .ProjectTo<StudentShortItem>(_configuration)
            .ToArrayAsync();
        return Json(models);
    }

    [HttpGet("list")]
    public async Task<IActionResult> GetItems()
    {
        var models = await _dbContext.Students
            .AsNoTracking()
            .ProjectTo<StudentItem>(_configuration)
            .ToArrayAsync();
        return Json(models);
    }

    [HttpGet("list/long")]
    public async Task<IActionResult> GetLongItems()
    {
        var models = await _dbContext.Students
            .AsNoTracking()
            .ProjectTo<StudentInfo>(_configuration)
            .ToArrayAsync();
        return Json(models);
    }
}