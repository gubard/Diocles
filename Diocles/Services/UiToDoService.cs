using Hestia.Contract.Models;
using Hestia.Contract.Services;
using Inanna.Models;
using Inanna.Services;

namespace Diocles.Services;

public interface IUiToDoService
    : IUiService<HestiaGetRequest, HestiaPostRequest, HestiaGetResponse, HestiaPostResponse>;

public sealed class UiToDoService(
    IHttpToDoService service,
    IEfToDoService efService,
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
        IEfToDoService,
        IToDoCache
    >(service, efService, appState, cache, navigator),
        IUiToDoService;
