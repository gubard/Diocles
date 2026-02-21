using Hestia.Contract.Models;
using Hestia.Contract.Services;
using Inanna.Services;

namespace Diocles.Services;

public interface IToDoUiService
    : IUiService<HestiaGetRequest, HestiaPostRequest, HestiaGetResponse, HestiaPostResponse>;

public sealed class ToDoUiService(
    IToDoHttpService toDoHttpService,
    IToDoDbService toDoDbService,
    IToDoUiCache uiCache,
    INavigator navigator,
    string serviceName
)
    : UiService<
        HestiaGetRequest,
        HestiaPostRequest,
        HestiaGetResponse,
        HestiaPostResponse,
        IToDoHttpService,
        IToDoDbService,
        IToDoUiCache
    >(toDoHttpService, toDoDbService, uiCache, navigator, serviceName),
        IToDoUiService
{
    protected override HestiaGetRequest CreateGetRequestRefresh()
    {
        return new()
        {
            IsGetSelectors = true,
            IsRoots = true,
            IsCurrentActive = true,
            IsBookmarks = true,
            IsFavorites = true,
        };
    }
}
