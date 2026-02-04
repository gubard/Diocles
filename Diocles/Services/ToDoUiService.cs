using Hestia.Contract.Models;
using Hestia.Contract.Services;
using Inanna.Models;
using Inanna.Services;
using Neotoma.Contract.Models;

namespace Diocles.Services;

public interface IToDoUiService
    : IUiService<HestiaGetRequest, HestiaPostRequest, HestiaGetResponse, HestiaPostResponse>;

public sealed class ToDoUiService(
    IToDoHttpService toDoHttpService,
    IToDoDbService toDoDbService,
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
        IToDoHttpService,
        IToDoDbService,
        IToDoUiCache
    >(toDoHttpService, toDoDbService, uiCache, navigator, serviceName, responseHandler),
        IToDoUiService;
