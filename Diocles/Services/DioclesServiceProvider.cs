using System.Text.Json;
using Diocles.Models;
using Diocles.Ui;
using Gaia.Helpers;
using Gaia.Models;
using Gaia.Services;
using Hestia.Contract.Models;
using Inanna.Models;
using Jab;
using Nestor.Db.Sqlite.Helpers;

namespace Diocles.Services;

[ServiceProviderModule]
[Transient(typeof(RootToDosViewModel))]
[Transient(typeof(ToDoListViewModel))]
[Transient(typeof(IUiToDoService), Factory = nameof(GetUiCredentialService))]
public interface IDioclesServiceProvider
{
    public static IUiToDoService GetUiCredentialService(
        ToDoServiceOptions options, ITryPolicyService tryPolicyService,
        IFactory<Memory<HttpHeader>> headersFactory, AppState appState)
    {
        return new UiToDoService(new(new()
            {
                BaseAddress = new(options.Url),
            }, new()
            {
                TypeInfoResolver = HestiaJsonContext.Resolver,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            }, tryPolicyService, headersFactory),
            new(new FileInfo(
                    $"./storage/Diocles/{appState.User.ThrowIfNull().Id}.db")
               .InitDbContext()), appState);
    }
}