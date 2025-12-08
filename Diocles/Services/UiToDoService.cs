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
    AppState appState)
    :
        UiService<HestiaGetRequest, HestiaPostRequest, HestiaGetResponse,
            HestiaPostResponse, HttpToDoService, EfToDoService>(service,
            efService, appState), IUiToDoService;