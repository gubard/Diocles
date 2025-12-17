using System.Windows.Input;
using Diocles.Models;
using Diocles.Services;
using Gaia.Helpers;
using Gaia.Services;
using Hestia.Contract.Models;
using Inanna.Helpers;
using Inanna.Models;
using Inanna.Services;

namespace Diocles.Helpers;

public static class DioclesCommands
{
    static DioclesCommands()
    {
        var dialogService = DiHelper.ServiceProvider.GetService<IDialogService>();
        var stringFormater = DiHelper.ServiceProvider.GetService<IStringFormater>();
        var appResourceService = DiHelper.ServiceProvider.GetService<IAppResourceService>();
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
        });

        ShowEditCommand = UiHelper.CreateCommand<ToDoNotify>(
            (item, ct) =>
                dialogService.ShowMessageBoxAsync(
                    new(
                        stringFormater.Format(
                            appResourceService.GetResource<string>("Lang.EditItem"),
                            item.Name
                        ),
                        factory.CreateEditToDoHeader(item),
                        new DialogButton(
                            appResourceService.GetResource<string>("Lang.Edit"),
                            OpenEditCommand,
                            null,
                            DialogButtonType.Primary
                        ),
                        UiHelper.CancelButton
                    ),
                    ct
                )
        );
    }

    public static readonly ICommand OpenToDosCommand;
    public static readonly ICommand DeleteToDoCommand;
    public static readonly ICommand OpenEditCommand;
    public static readonly ICommand ShowEditCommand;
    public static readonly ICommand SwitchToDoCommand;
    public static readonly ICommand OpenCurrentToDoCommand;
}
