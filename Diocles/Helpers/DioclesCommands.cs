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
            (item, ct) => uiToDoService.PostAsync(new() { DeleteIds = [item.Id] }, ct)
        );

        OpenEditCommand = UiHelper.CreateCommand<ToDoNotify>(
            (item, ct) => navigator.NavigateToAsync(factory.CreateEditToDo(item), ct)
        );

        SwitchToDoCommand = UiHelper.CreateCommand<ToDoNotify, HestiaPostResponse>(
            (item, ct) => uiToDoService.PostAsync(new() { SwitchCompleteIds = [item.Id] }, ct)
        );

        OpenCurrentToDoCommand = UiHelper.CreateCommand(async ct =>
        {
            var response = await uiToDoService.GetAsync(new() { IsCurrentActive = true }, ct);

            if (toDoCache.CurrentActive is null)
            {
                await navigator.NavigateToAsync(factory.CreateRootToDos(), ct);
            }
            else
            {
                await navigator.NavigateToAsync(factory.CreateToDos(toDoCache.CurrentActive), ct);
            }

            return response;
        });
    }

    public static readonly ICommand OpenToDosCommand;
    public static readonly ICommand DeleteToDoCommand;
    public static readonly ICommand OpenEditCommand;
    public static readonly ICommand SwitchToDoCommand;
    public static readonly ICommand OpenCurrentToDoCommand;
}
