using Hestia.Contract.Models;
using Hestia.Contract.Services;
using Inanna.Models;
using Inanna.Services;

namespace Diocles.Services;

public interface IUiToDoService : IUiService<HestiaGetRequest,
    HestiaPostRequest, HestiaGetResponse, HestiaPostResponse>;

public sealed class UiToDoService(
    HttpToDoService service,
    EfToDoService efService,
    AppState appState, 
    IToDoCache cache)
    :
        UiService<HestiaGetRequest, HestiaPostRequest, HestiaGetResponse,
            HestiaPostResponse, HttpToDoService, EfToDoService, IToDoCache>(service,
            efService, appState, cache), IUiToDoService;