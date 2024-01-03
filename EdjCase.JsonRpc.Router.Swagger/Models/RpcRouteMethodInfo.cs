using EdjCase.JsonRpc.Router.Abstractions;

namespace EdjCase.JsonRpc.Router.Swagger.Models;

public class UniqueMethod
{
    public UniqueMethod(string uniqueUrl, IRpcMethodInfo info)
    {
        UniqueUrl = uniqueUrl;
        Info = info;
    }

    public string UniqueUrl { get; }
    public IRpcMethodInfo Info { get; }
}