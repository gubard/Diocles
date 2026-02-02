using Inanna.Models;
using Inanna.Services;
using Neotoma.Contract.Models;
using Neotoma.Contract.Services;

namespace Diocles.Services;

public interface IFileStorageUiService
    : IUiService<NeotomaGetRequest, NeotomaPostRequest, NeotomaGetResponse, NeotomaPostResponse>;

public sealed class FileStorageUiService(
    IFileStorageHttpService httpService,
    IFileStorageDbService dbService,
    AppState appState,
    IFileStorageUiCache uiCache,
    INavigator navigator,
    string serviceName,
    IResponseHandler responseHandler
)
    : UiService<
        NeotomaGetRequest,
        NeotomaPostRequest,
        NeotomaGetResponse,
        NeotomaPostResponse,
        IFileStorageHttpService,
        IFileStorageDbService,
        IFileStorageUiCache
    >(httpService, dbService, appState, uiCache, navigator, serviceName, responseHandler),
        IFileStorageUiService;
