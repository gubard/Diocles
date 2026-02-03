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
        var objectStorage = DiHelper.ServiceProvider.GetService<IObjectStorage>();
        var openerLink = DiHelper.ServiceProvider.GetService<IOpenerLink>();
        var fileStorageUiService = DiHelper.ServiceProvider.GetService<IFileStorageUiService>();

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
                return new DefaultValidationErrors();
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

        async ValueTask EditToDosCommandAsync(IEnumerable<ToDoNotify> items, CancellationToken ct)
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

            var header = appResourceService
                .GetResource<string>("Lang.Edit")
                .DispatchToDialogHeader();

            var settings = await objectStorage.LoadAsync<ToDoParametersSettings>(
                $"{typeof(ToDoParametersSettings).FullName}.multi",
                ct
            );

            var viewModel = factory.CreateToDoParameters(
                settings,
                ValidationMode.ValidateOnlyEdited,
                true
            );

            async ValueTask<IValidationErrors> EditToDosAsync(CancellationToken c)
            {
                var edit = viewModel.CreateEditToDos(selected.Select(x => x.Id).ToArray());

                var files = viewModel.CreateNeotomaPostRequest(
                    selected.Select(x => $"{x.Id}/ToDo").ToArray()
                );

                var newSettings = viewModel.CreateSettings();
                await dialogService.CloseMessageBoxAsync(c);

                await objectStorage.SaveAsync(
                    $"{typeof(ToDoParametersSettings).FullName}.multi",
                    newSettings,
                    c
                );

                var errors = await TaskHelper.WhenAllAsync(
                    [
                        uiToDoService
                            .PostAsync(Guid.NewGuid(), new() { Edits = [edit] }, c)
                            .ToValidationErrors(),
                        fileStorageUiService
                            .PostAsync(Guid.NewGuid(), files, ct)
                            .ToValidationErrors(),
                    ],
                    c
                );

                return errors.Combine();
            }

            await dialogService.ShowMessageBoxAsync(
                new(
                    header,
                    viewModel,
                    new DialogButton(
                        appResourceService.GetResource<string>("Lang.Edit"),
                        UiHelper.CreateCommand(ct => EditToDosAsync(ct).ConfigureAwait(false)),
                        null,
                        DialogButtonType.Primary
                    ),
                    UiHelper.CancelButton
                ),
                ct
            );
        }

        OpenToDosCommand = UiHelper.CreateCommand<ToDoNotify>(
            (item, ct) => navigator.NavigateToAsync(factory.CreateToDos(item.ActualItem), ct)
        );

        ShowDeleteToDoCommand = UiHelper.CreateCommand<ToDoNotify>(
            (item, ct) =>
            {
                var header = appResourceService
                    .GetResource<string>("Lang.Delete")
                    .DispatchToDialogHeader();

                async ValueTask<IValidationErrors> DeleteToDoAsync(CancellationToken c)
                {
                    await dialogService.CloseMessageBoxAsync(c);

                    var errors = await TaskHelper.WhenAllAsync(
                        [
                            uiToDoService
                                .PostAsync(Guid.NewGuid(), new() { DeleteIds = [item.Id] }, c)
                                .ToValidationErrors(),
                            fileStorageUiService
                                .PostAsync(
                                    Guid.NewGuid(),
                                    new() { DeleteDirs = [$"{item.Id}/ToDo"] },
                                    c
                                )
                                .ToValidationErrors(),
                        ],
                        c
                    );

                    return errors.Combine();
                }

                return dialogService.ShowMessageBoxAsync(
                    new(
                        header,
                        Dispatcher.UIThread.Invoke(() =>
                            new TextBlock
                            {
                                Text = stringFormater.Format(
                                    appResourceService.GetResource<string>("Lang.AskDelete"),
                                    item.Name
                                ),
                                Classes = { "text-wrap" },
                            }
                        ),
                        new DialogButton(
                            appResourceService.GetResource<string>("Lang.Delete"),
                            UiHelper.CreateCommand(ct => DeleteToDoAsync(ct).ConfigureAwait(false)),
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
            (items, ct) => EditToDosCommandAsync(items, ct).ConfigureAwait(false)
        );

        ShowDeleteToDosCommand = UiHelper.CreateCommand<IEnumerable<ToDoNotify>>(
            (items, ct) =>
            {
                var selected = items.Where(x => x.IsSelected).ToArray();

                var header = appResourceService
                    .GetResource<string>("Lang.Delete")
                    .DispatchToDialogHeader();

                async ValueTask<IValidationErrors> DeleteToDosAsync(CancellationToken c)
                {
                    await dialogService.CloseMessageBoxAsync(c);

                    var errors = await TaskHelper.WhenAllAsync(
                        [
                            uiToDoService
                                .PostAsync(
                                    Guid.NewGuid(),
                                    new() { DeleteIds = selected.Select(x => x.Id).ToArray() },
                                    c
                                )
                                .ToValidationErrors(),
                            fileStorageUiService
                                .PostAsync(
                                    Guid.NewGuid(),
                                    new()
                                    {
                                        DeleteDirs = selected.Select(x => $"{x.Id}/ToDo").ToArray(),
                                    },
                                    c
                                )
                                .ToValidationErrors(),
                        ],
                        c
                    );

                    return errors.Combine();
                }

                return dialogService.ShowMessageBoxAsync(
                    new(
                        header,
                        Dispatcher.UIThread.Invoke(() =>
                            new TextBlock
                            {
                                Text = stringFormater.Format(
                                    appResourceService.GetResource<string>("Lang.AskDelete"),
                                    selected.Select(x => x.Name).JoinString(", ")
                                ),
                                Classes = { "text-wrap" },
                            }
                        ),
                        new DialogButton(
                            appResourceService.GetResource<string>("Lang.Delete"),
                            UiHelper.CreateCommand(ct =>
                                DeleteToDosAsync(ct).ConfigureAwait(false)
                            ),
                            null,
                            DialogButtonType.Primary
                        ),
                        UiHelper.CancelButton
                    ),
                    ct
                );
            }
        );

        SwitchToDoCommand = UiHelper.CreateCommand<ToDoNotify>(
            (item, ct) =>
            {
                uiToDoService.PostAsync(
                    Guid.NewGuid(),
                    new() { SwitchCompleteIds = [item.Id] },
                    ct
                );

                return TaskHelper.ConfiguredCompletedTask;
            }
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
                var viewModel = factory.CreateChangeParentToDo();

                Dispatcher.UIThread.Post(() =>
                {
                    toDoCache.ResetItems();
                    item.IsHideOnTree = true;
                });

                async ValueTask<HestiaPostResponse> ChangeParentAsync(CancellationToken ct)
                {
                    var parentId = viewModel.IsRoot ? null : viewModel.Tree.Selected?.Id;

                    await dialogService.CloseMessageBoxAsync(ct);

                    return await uiToDoService.PostAsync(
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
                }

                return dialogService.ShowMessageBoxAsync(
                    new(
                        stringFormater
                            .Format(
                                appResourceService.GetResource<string>("Lang.ChangeParentItem"),
                                item.Name
                            )
                            .DispatchToDialogHeader(),
                        viewModel,
                        new(
                            appResourceService.GetResource<string>("Lang.ChangeParent"),
                            UiHelper.CreateCommand(ct =>
                                ChangeParentAsync(ct).ConfigureAwait(false)
                            ),
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
                var viewModel = factory.CreateChangeParentToDo();

                Dispatcher.UIThread.Post(() =>
                {
                    toDoCache.ResetItems();

                    foreach (var item in selected)
                    {
                        item.IsHideOnTree = true;
                    }
                });

                async ValueTask<HestiaPostResponse> ChangesParentAsync(CancellationToken ct)
                {
                    var parentId = viewModel.IsRoot ? null : viewModel.Tree.Selected?.Id;

                    await dialogService.CloseMessageBoxAsync(ct);

                    return await uiToDoService.PostAsync(
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
                }

                return dialogService.ShowMessageBoxAsync(
                    new(
                        appResourceService
                            .GetResource<string>("Lang.ChangeParent")
                            .DispatchToDialogHeader(),
                        viewModel,
                        new(
                            appResourceService.GetResource<string>("Lang.ChangeParent"),
                            UiHelper.CreateCommand(ct =>
                                ChangesParentAsync(ct).ConfigureAwait(false)
                            ),
                            null,
                            DialogButtonType.Primary
                        ),
                        UiHelper.CancelButton
                    ),
                    ct
                );
            }
        );

        ShowCloneCommand = UiHelper.CreateCommand<ToDoNotify>(
            (item, ct) =>
            {
                var viewModel = factory.CreateChangeParentToDo();
                Dispatcher.UIThread.Post(() => toDoCache.ResetItems());

                async ValueTask<HestiaPostResponse> CloneAsync(CancellationToken ct)
                {
                    var parentId = viewModel.IsRoot ? null : viewModel.Tree.Selected?.Id;

                    await dialogService.CloseMessageBoxAsync(ct);

                    return await uiToDoService.PostAsync(
                        Guid.NewGuid(),
                        new() { Clones = [new() { ParentId = parentId, CloneIds = [item.Id] }] },
                        ct
                    );
                }

                return dialogService.ShowMessageBoxAsync(
                    new(
                        stringFormater
                            .Format(
                                appResourceService.GetResource<string>("Lang.CloneItem"),
                                item.Name
                            )
                            .DispatchToDialogHeader(),
                        viewModel,
                        new(
                            appResourceService.GetResource<string>("Lang.Clone"),
                            UiHelper.CreateCommand(ct => CloneAsync(ct).ConfigureAwait(false)),
                            null,
                            DialogButtonType.Primary
                        ),
                        UiHelper.CancelButton
                    ),
                    ct
                );
            }
        );

        ShowClonesCommand = UiHelper.CreateCommand<IEnumerable<ToDoNotify>>(
            (items, ct) =>
            {
                var selected = items.Where(x => x.IsSelected).ToArray();
                var viewModel = factory.CreateChangeParentToDo();
                Dispatcher.UIThread.Post(() => toDoCache.ResetItems());

                async ValueTask<HestiaPostResponse> CloneAsync(CancellationToken ct)
                {
                    var parentId = viewModel.IsRoot ? null : viewModel.Tree.Selected?.Id;

                    await dialogService.CloseMessageBoxAsync(ct);

                    return await uiToDoService.PostAsync(
                        Guid.NewGuid(),
                        new()
                        {
                            Clones =
                            [
                                new()
                                {
                                    ParentId = parentId,
                                    CloneIds = selected.Select(x => x.Id).ToArray(),
                                },
                            ],
                        },
                        ct
                    );
                }

                return dialogService.ShowMessageBoxAsync(
                    new(
                        appResourceService
                            .GetResource<string>("Lang.Clone")
                            .DispatchToDialogHeader(),
                        viewModel,
                        new(
                            appResourceService.GetResource<string>("Lang.Clone"),
                            UiHelper.CreateCommand(ct => CloneAsync(ct).ConfigureAwait(false)),
                            null,
                            DialogButtonType.Primary
                        ),
                        UiHelper.CancelButton
                    ),
                    ct
                );
            }
        );

        OpenLinkCommand = UiHelper.CreateCommand<ToDoNotify>(
            (item, ct) => openerLink.OpenLinkAsync(item.Link.ToUri(), ct)
        );
    }

    public static readonly ICommand OpenLinkCommand;
    public static readonly ICommand ShowCloneCommand;
    public static readonly ICommand ShowClonesCommand;
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
