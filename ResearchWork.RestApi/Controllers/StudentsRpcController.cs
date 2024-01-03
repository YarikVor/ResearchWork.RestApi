using AutoMapper.QueryableExtensions;
using EdjCase.JsonRpc.Router;
using EdjCase.JsonRpc.Router.Abstractions;
using Microsoft.EntityFrameworkCore;
using ResearchWork.RestApi.Dtos;
using ResearchWork.SqlTypes;
using IConfigurationProvider = AutoMapper.IConfigurationProvider;

namespace ResearchWork.RestApi;

[RpcRoute("rpc/students")]
public class StudentsRpcController : RpcController
{
    private readonly IConfigurationProvider _configuration;
    private readonly SchoolDbContext _dbContext;

    public StudentsRpcController(SchoolDbContext dbContext, IConfigurationProvider configuration)
    {
        _dbContext = dbContext;
        _configuration = configuration;
    }

    [RpcRoute("list/short")]
    public async Task<IRpcMethodResult> GetShortItemsAsync()
    {
        var models = await _dbContext.Students
            .AsNoTracking()
            .ProjectTo<StudentShortItem>(_configuration)
            .ToArrayAsync();
        return Ok(models);
    }

    [RpcRoute("list")]
    public async Task<IRpcMethodResult> GetItemsAsync()
    {
        var models = await _dbContext.Students
            .AsNoTracking()
            .ProjectTo<StudentItem>(_configuration)
            .ToArrayAsync();
        return Ok(models);
    }

    [RpcRoute("list/long")]
    public async Task<IRpcMethodResult> GetLongItemsAsync()
    {
        var models = await _dbContext.Students
            .AsNoTracking()
            .ProjectTo<StudentInfo>(_configuration)
            .ToArrayAsync();
        return Ok(models);
    }
}