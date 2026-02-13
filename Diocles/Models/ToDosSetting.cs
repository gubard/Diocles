using Gaia.Services;

namespace Diocles.Models;

public sealed class ToDosSetting : ObjectStorageValue<ToDosSetting>
{
    public ToDoGroupBy GroupBy { get; set; } = ToDoGroupBy.Status;
    public ToDoOrderBy OrderBy { get; set; }
}
