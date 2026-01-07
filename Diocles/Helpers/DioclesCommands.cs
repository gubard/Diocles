using System.Windows.Input;
using Diocles.Models;
using Diocles.Services;
using Gaia.Helpers;
using Hestia.Contract.Models;
using Inanna.Helpers;
using Inanna.Services;

namespace Diocles.Helpers;

public static class DioclesCommands
{
    static DioclesCommands()
    {
        var navigator = DiHelper.ServiceProvider.GetService<INavigator>();
        var uiToDoService = DiHelper.ServiceProvider.GetService<IUiToDoService>();
        var toDoCache = DiHelper.ServiceProvider.GetService<IToDoCache>();
        var factory = DiHelper.ServiceProvider.GetService<IDioclesViewModelFactory>();

        OpenToDosCommand = UiHelper.CreateCommand<ToDoNotify>(
            (item, ct) => navigator.NavigateToAsync(factory.CreateToDos(item), ct)
        );

        DeleteToDoCommand = UiHelper.CreateCommand<ToDoNotify, HestiaPostResponse>(
            (item, ct) =>
                uiToDoService.PostAsync(Guid.NewGuid(), new() { DeleteIds = [item.Id] }, ct)
        );

        SwitchToDoCommand = UiHelper.CreateCommand<ToDoNotify, HestiaPostResponse>(
            (item, ct) =>
                uiToDoService.PostAsync(Guid.NewGuid(), new() { SwitchCompleteIds = [item.Id] }, ct)
        );

        async ValueTask<HestiaGetResponse> OpenCurrentToDoAsync(CancellationToken ct)
        {
            var response = await uiToDoService.GetAsync(new() { IsCurrentActive = true }, ct);

            if (toDoCache.CurrentActive?.Parent is null)
            {
                await navigator.NavigateToAsync(factory.CreateRootToDos(), ct);
            }
            else
            {
                await navigator.NavigateToAsync(
                    factory.CreateToDos(toDoCache.CurrentActive.Parent),
                    ct
                );
            }

            return response;
        }

        OpenCurrentToDoCommand = UiHelper.CreateCommand(ct =>
            OpenCurrentToDoAsync(ct).ConfigureAwait(false)
        );

        OpenParentCommand = UiHelper.CreateCommand<ToDoNotify>(
            (item, ct) =>
                item.Parent is null
                    ? navigator.NavigateToAsync(factory.CreateRootToDos(), ct)
                    : navigator.NavigateToAsync(factory.CreateToDos(item.Parent), ct)
        );

        SwitchFavoriteCommand = UiHelper.CreateCommand<ToDoNotify, HestiaPostResponse>(
            (item, ct) =>
                uiToDoService.PostAsync(
                    Guid.NewGuid(),
                    new()
                    {
                        Edits =
                        [
                            new()
                            {
                                Ids = [item.Id],
                                IsFavorite = !item.IsFavorite,
                                IsEditIsFavorite = true,
                            },
                        ],
                    },
                    ct
                )
        );
    }

    public static readonly ICommand OpenToDosCommand;
    public static readonly ICommand OpenParentCommand;
    public static readonly ICommand DeleteToDoCommand;
    public static readonly ICommand SwitchToDoCommand;
    public static readonly ICommand OpenCurrentToDoCommand;
    public static readonly ICommand SwitchFavoriteCommand;
}
