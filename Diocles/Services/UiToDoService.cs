using Hestia.Contract.Models;
using Hestia.Contract.Services;
using Inanna.Models;
using Inanna.Services;

namespace Diocles.Services;

public interface IUiToDoService
    : IUiService<HestiaGetRequest, HestiaPostRequest, HestiaGetResponse, HestiaPostResponse>;

public sealed class UiToDoService(
    IHttpToDoService httpService,
    IDbToDoService dbService,
    AppState appState,
    IToDoMemoryCache memoryCache,
    INavigator navigator,
    string serviceName
)
    : UiService<
        HestiaGetRequest,
        HestiaPostRequest,
        HestiaGetResponse,
        HestiaPostResponse,
        IHttpToDoService,
        IDbToDoService,
        IToDoMemoryCache
    >(httpService, dbService, appState, memoryCache, navigator, serviceName),
        IUiToDoService;
