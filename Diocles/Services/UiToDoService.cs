using Hestia.Contract.Models;
using Hestia.Contract.Services;
using Inanna.Models;
using Inanna.Services;

namespace Diocles.Services;

public interface IUiToDoService
    : IUiService<HestiaGetRequest, HestiaPostRequest, HestiaGetResponse, HestiaPostResponse>;

public sealed class UiToDoService(
    IHttpToDoService service,
    IDbToDoService dbService,
    AppState appState,
    IToDoCache cache,
    INavigator navigator
)
    : UiService<
        HestiaGetRequest,
        HestiaPostRequest,
        HestiaGetResponse,
        HestiaPostResponse,
        IHttpToDoService,
        IDbToDoService,
        IToDoCache
    >(service, dbService, appState, cache, navigator),
        IUiToDoService;
