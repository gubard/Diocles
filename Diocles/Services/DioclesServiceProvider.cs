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
[Transient(typeof(ToDoListViewModel))]
[Transient(typeof(ToDoParametersFillerService))]
[Singleton(typeof(IToDoCache), typeof(ToDoCache))]
[Transient(typeof(IUiToDoService), Factory = nameof(GetUiCredentialService))]
public interface IDioclesServiceProvider
{
    public static IUiToDoService GetUiCredentialService(
        ToDoServiceOptions options, ITryPolicyService tryPolicyService,
        IFactory<Memory<HttpHeader>> headersFactory, AppState appState,
        ToDoParametersFillerService toDoParametersFillerService,
        IToDoCache toDoCache,
        INavigator navigator)
    {
        return new UiToDoService(new HttpToDoService(new()
            {
                BaseAddress = new(options.Url),
            }, new()
            {
                TypeInfoResolver = HestiaJsonContext.Resolver,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            }, tryPolicyService, headersFactory),
            new EfToDoService(new FileInfo(
                        $"./storage/Diocles/{appState.User.ThrowIfNull().Id}.db")
                   .InitDbContext(), new(DateTimeOffset.UtcNow.Offset),
                toDoParametersFillerService), appState, toDoCache, navigator);
    }
}