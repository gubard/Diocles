using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Threading;
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
        var appResourceService = DiHelper.ServiceProvider.GetService<IAppResourceService>();
        var stringFormater = DiHelper.ServiceProvider.GetService<IStringFormater>();
        var navigator = DiHelper.ServiceProvider.GetService<INavigator>();
        var uiToDoService = DiHelper.ServiceProvider.GetService<IToDoUiService>();
        var toDoCache = DiHelper.ServiceProvider.GetService<IToDoMemoryCache>();
        var factory = DiHelper.ServiceProvider.GetService<IDioclesViewModelFactory>();
        var dialogService = DiHelper.ServiceProvider.GetService<IDialogService>();

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
                    factory.CreateToDos(toDoCache.CurrentActive.Parent.ActualItem),
                    ct
                );
            }

            return response;
        }

        async ValueTask<IValidationErrors> ChangeOrderAsync(ToDoNotify item, CancellationToken ct)
        {
            var items = item.Parent is null ? toDoCache.Roots : item.Parent.Children;
            var changeOrder = await UiHelper.ShowChangeOrderAsync(items.ToArray(), [item], ct);

            if (changeOrder is null)
            {
                return new EmptyValidationErrors();
            }

            return await uiToDoService.PostAsync(
                Guid.NewGuid(),
                new()
                {
                    ChangeOrder =
                    [
                        new()
                        {
                            IsAfter = changeOrder.IsAfter,
                            StartId = changeOrder.Item.Id,
                            InsertIds = [item.Id],
                        },
                    ],
                },
                ct
            );
        }

        OpenToDosCommand = UiHelper.CreateCommand<ToDoNotify>(
            (item, ct) => navigator.NavigateToAsync(factory.CreateToDos(item.ActualItem), ct)
        );

        ShowDeleteToDoCommand = UiHelper.CreateCommand<ToDoNotify>(
            (item, ct) =>
            {
                var header = Dispatcher.UIThread.Invoke(() =>
                    new TextBlock
                    {
                        Text = stringFormater.Format(
                            appResourceService.GetResource<string>("Lang.Delete")
                        ),
                    }
                );

                return dialogService.ShowMessageBoxAsync(
                    new(
                        header,
                        stringFormater.Format(
                            appResourceService.GetResource<string>("Lang.AskDelete"),
                            item.Name
                        ),
                        new DialogButton(
                            appResourceService.GetResource<string>("Lang.Delete"),
                            UiHelper.CreateCommand(ct =>
                            {
                                dialogService.DispatchCloseMessageBox();

                                return uiToDoService.PostAsync(
                                    Guid.NewGuid(),
                                    new() { DeleteIds = [item.Id] },
                                    ct
                                );
                            }),
                            null,
                            DialogButtonType.Primary
                        ),
                        UiHelper.CancelButton
                    ),
                    ct
                );
            }
        );

        ShowEditToDosCommand = UiHelper.CreateCommand<IEnumerable<ToDoNotify>>(
            (items, ct) =>
            {
                var selected = items.Where(x => x.IsSelected).ToArray();

                Dispatcher.UIThread.Post(() =>
                {
                    toDoCache.ResetItems();

                    foreach (var s in selected)
                    {
                        s.IsHideOnTree = true;
                    }
                });

                var header = stringFormater
                    .Format(appResourceService.GetResource<string>("Lang.Edit"))
                    .DispatchToDialogHeader();

                var viewModel = factory.CreateToDoParameters(
                    ValidationMode.ValidateOnlyEdited,
                    true
                );

                return dialogService.ShowMessageBoxAsync(
                    new(
                        header,
                        viewModel,
                        new DialogButton(
                            appResourceService.GetResource<string>("Lang.Edit"),
                            UiHelper.CreateCommand(ct =>
                            {
                                var edit = viewModel.CreateEditToDos(
                                    selected.Select(x => x.Id).ToArray()
                                );

                                dialogService.DispatchCloseMessageBox();

                                return uiToDoService.PostAsync(
                                    Guid.NewGuid(),
                                    new() { Edits = [edit] },
                                    ct
                                );
                            }),
                            null,
                            DialogButtonType.Primary
                        ),
                        UiHelper.CancelButton
                    ),
                    ct
                );
            }
        );

        ShowDeleteToDosCommand = UiHelper.CreateCommand<IEnumerable<ToDoNotify>>(
            (items, ct) =>
            {
                var selected = items.Where(x => x.IsSelected).ToArray();

                var header = Dispatcher.UIThread.Invoke(() =>
                    new TextBlock
                    {
                        Text = stringFormater.Format(
                            appResourceService.GetResource<string>("Lang.Delete")
                        ),
                    }
                );

                return dialogService.ShowMessageBoxAsync(
                    new(
                        header,
                        stringFormater.Format(
                            appResourceService.GetResource<string>("Lang.AskDelete"),
                            selected.Select(x => x.Name).JoinString(", ")
                        ),
                        new DialogButton(
                            appResourceService.GetResource<string>("Lang.Delete"),
                            UiHelper.CreateCommand(ct =>
                            {
                                dialogService.DispatchCloseMessageBox();

                                return uiToDoService.PostAsync(
                                    Guid.NewGuid(),
                                    new() { DeleteIds = selected.Select(x => x.Id).ToArray() },
                                    ct
                                );
                            }),
                            null,
                            DialogButtonType.Primary
                        ),
                        UiHelper.CancelButton
                    ),
                    ct
                );
            }
        );

        SwitchToDoCommand = UiHelper.CreateCommand<ToDoNotify, HestiaPostResponse>(
            (item, ct) =>
                uiToDoService.PostAsync(Guid.NewGuid(), new() { SwitchCompleteIds = [item.Id] }, ct)
        );

        OpenCurrentToDoCommand = UiHelper.CreateCommand(ct =>
            OpenCurrentToDoAsync(ct).ConfigureAwait(false)
        );

        OpenParentCommand = UiHelper.CreateCommand<ToDoNotify>(
            (item, ct) =>
                item.Parent is null
                    ? navigator.NavigateToAsync(factory.CreateRootToDos(), ct)
                    : navigator.NavigateToAsync(factory.CreateToDos(item.Parent.ActualItem), ct)
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

        ChangeOrderCommand = UiHelper.CreateCommand<ToDoNotify, IValidationErrors>(
            (item, ct) => ChangeOrderAsync(item, ct).ConfigureAwait(false)
        );

        ShowChangeParentCommand = UiHelper.CreateCommand<ToDoNotify>(
            (item, ct) =>
            {
                var viewModel = Dispatcher.UIThread.Invoke(() => factory.CreateChangeParentToDo());
                Dispatcher.UIThread.Post(() =>
                {
                    toDoCache.ResetItems();
                    item.IsHideOnTree = true;
                });

                return dialogService.ShowMessageBoxAsync(
                    new(
                        stringFormater.Format(
                            appResourceService.GetResource<string>("Lang.ChangeParentItem"),
                            item.Name
                        ),
                        viewModel,
                        new(
                            appResourceService.GetResource<string>("Lang.ChangeParent"),
                            UiHelper.CreateCommand(ct =>
                            {
                                var parentId = viewModel.IsRoot
                                    ? (Guid?)null
                                    : viewModel.Tree.Selected.Id;

                                dialogService.DispatchCloseMessageBox();

                                return uiToDoService.PostAsync(
                                    Guid.NewGuid(),
                                    new()
                                    {
                                        Edits =
                                        [
                                            new()
                                            {
                                                Ids = [item.Id],
                                                ParentId = parentId,
                                                IsEditParentId = true,
                                            },
                                        ],
                                    },
                                    ct
                                );
                            }),
                            null,
                            DialogButtonType.Primary
                        ),
                        UiHelper.CancelButton
                    ),
                    ct
                );
            }
        );

        ShowChangesParentCommand = UiHelper.CreateCommand<IEnumerable<ToDoNotify>>(
            (items, ct) =>
            {
                var selected = items.Where(x => x.IsSelected).ToArray();
                var viewModel = Dispatcher.UIThread.Invoke(() => factory.CreateChangeParentToDo());

                Dispatcher.UIThread.Post(() =>
                {
                    toDoCache.ResetItems();

                    foreach (var item in selected)
                    {
                        item.IsHideOnTree = true;
                    }
                });

                return dialogService.ShowMessageBoxAsync(
                    new(
                        appResourceService.GetResource<string>("Lang.ChangeParent"),
                        viewModel,
                        new(
                            appResourceService.GetResource<string>("Lang.ChangeParent"),
                            UiHelper.CreateCommand(ct =>
                            {
                                var parentId = viewModel.IsRoot
                                    ? (Guid?)null
                                    : viewModel.Tree.Selected.Id;

                                dialogService.CloseMessageBox();

                                return uiToDoService.PostAsync(
                                    Guid.NewGuid(),
                                    new()
                                    {
                                        Edits =
                                        [
                                            new()
                                            {
                                                Ids = selected.Select(x => x.Id).ToArray(),
                                                ParentId = parentId,
                                                IsEditParentId = true,
                                            },
                                        ],
                                    },
                                    ct
                                );
                            }),
                            null,
                            DialogButtonType.Primary
                        ),
                        UiHelper.CancelButton
                    ),
                    ct
                );
            }
        );
    }

    public static readonly ICommand OpenToDosCommand;
    public static readonly ICommand OpenParentCommand;
    public static readonly ICommand ShowDeleteToDoCommand;
    public static readonly ICommand ShowDeleteToDosCommand;
    public static readonly ICommand ShowEditToDosCommand;
    public static readonly ICommand SwitchToDoCommand;
    public static readonly ICommand OpenCurrentToDoCommand;
    public static readonly ICommand SwitchFavoriteCommand;
    public static readonly ICommand ChangeOrderCommand;
    public static readonly ICommand ShowChangeParentCommand;
    public static readonly ICommand ShowChangesParentCommand;
}
