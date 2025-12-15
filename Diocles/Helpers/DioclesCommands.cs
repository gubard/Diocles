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
    }

    public static readonly ICommand OpenToDosCommand;
    public static readonly ICommand DeleteToDoCommand;
    public static readonly ICommand OpenEditCommand;
    public static readonly ICommand SwitchToDoCommand;
}
