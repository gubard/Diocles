using Hestia.Contract.Models;
using Hestia.Contract.Services;
using Inanna.Models;
using Inanna.Services;

namespace Diocles.Services;

public interface IToDoUiService
    : IUiService<HestiaGetRequest, HestiaPostRequest, HestiaGetResponse, HestiaPostResponse>;

public sealed class ToDoUiService(
    IHttpToDoService httpService,
    IDbToDoService dbService,
    AppState appState,
    IToDoUiCache uiCache,
    INavigator navigator,
    string serviceName,
    IResponseHandler responseHandler
)
    : UiService<
        HestiaGetRequest,
        HestiaPostRequest,
        HestiaGetResponse,
        HestiaPostResponse,
        IHttpToDoService,
        IDbToDoService,
        IToDoUiCache
    >(httpService, dbService, appState, uiCache, navigator, serviceName, responseHandler),
        IToDoUiService;
