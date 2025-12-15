using System.Text.Json;
using Diocles.Models;
using Diocles.Ui;
using Gaia.Helpers;
using Gaia.Models;
using Gaia.Services;
using Hestia.Contract.Models;
using Hestia.Contract.Services;
using Inanna.Models;
using Inanna.Services;
using Jab;
using Nestor.Db.Sqlite.Helpers;

namespace Diocles.Services;

[ServiceProviderModule]
[Transient(typeof(RootToDosViewModel))]
[Transient(typeof(ToDoParametersFillerService))]
[Singleton(typeof(IToDoCache), typeof(ToDoCache))]
[Transient(typeof(IToDoValidator), typeof(ToDoValidator))]
[Transient(typeof(IDioclesViewModelFactory), typeof(DioclesViewModelFactory))]
[Transient(typeof(IUiToDoService), Factory = nameof(GetUiCredentialService))]
public interface IDioclesServiceProvider
{
    public static IUiToDoService GetUiCredentialService(
        ToDoServiceOptions options,
        ITryPolicyService tryPolicyService,
        IFactory<Memory<HttpHeader>> headersFactory,
        AppState appState,
        ToDoParametersFillerService toDoParametersFillerService,
        IToDoCache toDoCache,
        INavigator navigator,
        IStorageService storageService,
        IToDoValidator toDoValidator
    )
    {
        var user = appState.User.ThrowIfNull();

        return new UiToDoService(
            new HttpToDoService(
                new() { BaseAddress = new(options.Url) },
                new()
                {
                    TypeInfoResolver = HestiaJsonContext.Resolver,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                },
                tryPolicyService,
                headersFactory
            ),
            new EfToDoService(
                new FileInfo(
                    $"{storageService.GetAppDirectory()}/Diocles/{user.Id}.db"
                ).InitDbContext(),
                new(DateTimeOffset.UtcNow.Offset, user.Id),
                toDoParametersFillerService,
                toDoValidator
            ),
            appState,
            toDoCache,
            navigator
        );
    }
}
