using Gaia.Services;
using Hestia.Contract.Models;

namespace Diocles.Models;

public sealed class SearchToDoSettings : ObjectStorageValue<SearchToDoSettings>
{
    public string SearchText { get; set; } = string.Empty;
    public ToDoType[] Types { get; set; } = [];
}
