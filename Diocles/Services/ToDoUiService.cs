using Gaia.Services;
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
    string serviceName,
    IStatusBarService statusBarService,
    IInannaViewModelFactory factory
)
    : UiService<
        HestiaGetRequest,
        HestiaPostRequest,
        HestiaGetResponse,
        HestiaPostResponse,
        IToDoHttpService,
        IToDoDbService,
        IToDoUiCache
    >(toDoHttpService, toDoDbService, uiCache, navigator, serviceName, statusBarService, factory),
        IToDoUiService
{
    protected override async ValueTask<IValidationErrors> RefreshServiceCore(CancellationToken ct)
    {
        var request = new HestiaGetRequest
        {
            IsFull = true,
            IsBookmarks = true,
            IsFavorites = true,
        };

        var response = await DbService.GetAsync(request, ct);
        await UiCache.UpdateAsync(response, ct);

        return response;
    }
}
