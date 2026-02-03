using Diocles.Models;
using Neotoma.Contract.Models;

namespace Diocles.Helpers;

public static class Mapper
{
    public static FileData ToFileData(this FileObjectNotify item)
    {
        return new()
        {
            Data = item.Data,
            Description = item.Description,
            Name = item.Name,
            Id = item.Id,
        };
    }
}
