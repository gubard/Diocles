using System.Windows.Input;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Threading;
using Diocles.Models;
using Gaia.Helpers;
using Gaia.Services;
using Hestia.Contract.Models;
using IconPacks.Avalonia.MaterialDesign;
using Inanna.Helpers;
using Inanna.Models;
using Inanna.Services;
using Weber.Services;
using IServiceProvider = Gaia.Services.IServiceProvider;

namespace Diocles.Services;

public sealed class DioclesCommands : Commands
{
    public DioclesCommands(IServiceProvider serviceProvider, IAppResourceService appResourceService)
        : base(serviceProvider)
    {
        _appResourceService = appResourceService;

        _openToDosCommand = CreateLazyCommand<ToDoNotify>(
            (item, ct) =>
            {
                var navigator = ServiceProvider.GetService<INavigator>();
                var factory = ServiceProvider.GetService<IDioclesViewModelFactory>();

                return navigator.NavigateToAsync(factory.CreateToDos(item.ActualItem), ct);
            },
            true
        );

        _showDeleteToDoCommand = CreateLazyCommand<ToDoNotify>(
            (item, ct) =>
            {
                var dialogService = ServiceProvider.GetService<IDialogService>();
                var stringFormater = ServiceProvider.GetService<IStringFormater>();
                var safeExecuteWrapper = ServiceProvider.GetService<ISafeExecuteWrapper>();
                var commandFactory = ServiceProvider.GetService<ICommandFactory>();
                var navigator = ServiceProvider.GetService<INavigator>();
                var factory = ServiceProvider.GetService<IDioclesViewModelFactory>();
                var toDoUiService = ServiceProvider.GetService<IToDoUiService>();
                var fileStorageUiService = ServiceProvider.GetService<IFileStorageUiService>();

                var header = appResourceService
                    .GetResource<string>("Lang.Delete")
                    .DispatchToDialogHeader();

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
                        safeExecuteWrapper,
                        new DialogButton(
                            appResourceService.GetResource<string>("Lang.Delete"),
                            commandFactory.CreateCommand(async c =>
                            {
                                if (item.Parent is null)
                                {
                                    await navigator.NavigateToAsync(factory.CreateRootToDos(), c);
                                }
                                else
                                {
                                    await navigator.NavigateToAsync(
                                        factory.CreateToDos(item.Parent),
                                        c
                                    );
                                }

                                await dialogService.CloseMessageBoxAsync(c);

                                var errors = await TaskHelper.WhenAllAsync(
                                    [
                                        toDoUiService
                                            .PostAsync(
                                                Guid.NewGuid(),
                                                new() { DeleteIds = [item.Id] },
                                                c
                                            )
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
                            }),
                            null,
                            DialogButtonType.Primary
                        ),
                        dialogService.CancelButton
                    ),
                    ct
                );
            }
        );

        _showEditToDosCommand = CreateLazyCommand<IEnumerable<ToDoNotify>>(
            async (items, ct) =>
            {
                var toDoUiCache = ServiceProvider.GetService<IToDoUiCache>();
                var objectStorage = ServiceProvider.GetService<IObjectStorage>();
                var factory = ServiceProvider.GetService<IDioclesViewModelFactory>();
                var dialogService = ServiceProvider.GetService<IDialogService>();
                var safeExecuteWrapper = ServiceProvider.GetService<ISafeExecuteWrapper>();
                var commandFactory = ServiceProvider.GetService<ICommandFactory>();
                var toDoUiService = ServiceProvider.GetService<IToDoUiService>();
                var fileStorageUiService = ServiceProvider.GetService<IFileStorageUiService>();
                var selected = items.Where(x => x.IsSelected).ToArray();

                Dispatcher.UIThread.Post(() =>
                {
                    toDoUiCache.ResetItems();

                    foreach (var s in selected)
                    {
                        s.IsHideOnTree = true;
                    }
                });

                var header = appResourceService
                    .GetResource<string>("Lang.Edit")
                    .DispatchToDialogHeader();

                var settings = await objectStorage.LoadAsync<ToDoParametersSettings>(
                    Guid.Empty,
                    ct
                );

                var viewModel = factory.CreateToDoParameters(
                    settings,
                    ValidationMode.ValidateOnlyEdited,
                    true
                );

                await dialogService.ShowMessageBoxAsync(
                    new(
                        header,
                        viewModel,
                        safeExecuteWrapper,
                        new DialogButton(
                            appResourceService.GetResource<string>("Lang.Edit"),
                            commandFactory.CreateCommand(async c =>
                            {
                                var edit = viewModel.CreateEditToDos(
                                    selected.Select(x => x.Id).ToArray()
                                );

                                var files = viewModel.CreateNeotomaPostRequest(
                                    selected.Select(x => $"{x.Id}/ToDo").ToArray()
                                );

                                var newSettings = viewModel.CreateSettings();
                                await dialogService.CloseMessageBoxAsync(c);
                                await objectStorage.SaveAsync(newSettings, Guid.Empty, c);

                                var errors = await TaskHelper.WhenAllAsync(
                                    [
                                        toDoUiService
                                            .PostAsync(Guid.NewGuid(), new() { Edits = [edit] }, c)
                                            .ToValidationErrors(),
                                        fileStorageUiService
                                            .PostAsync(Guid.NewGuid(), files, ct)
                                            .ToValidationErrors(),
                                    ],
                                    c
                                );

                                return errors.Combine();
                            }),
                            null,
                            DialogButtonType.Primary
                        ),
                        dialogService.CancelButton
                    ),
                    ct
                );
            }
        );

        _showDeleteToDosCommand = CreateLazyCommand<IEnumerable<ToDoNotify>>(
            (items, ct) =>
            {
                var dialogService = ServiceProvider.GetService<IDialogService>();
                var stringFormater = ServiceProvider.GetService<IStringFormater>();
                var safeExecuteWrapper = ServiceProvider.GetService<ISafeExecuteWrapper>();
                var commandFactory = ServiceProvider.GetService<ICommandFactory>();
                var toDoUiService = ServiceProvider.GetService<IToDoUiService>();
                var fileStorageUiService = ServiceProvider.GetService<IFileStorageUiService>();
                var selected = items.Where(x => x.IsSelected).ToArray();

                var header = appResourceService
                    .GetResource<string>("Lang.Delete")
                    .DispatchToDialogHeader();

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
                        safeExecuteWrapper,
                        new DialogButton(
                            appResourceService.GetResource<string>("Lang.Delete"),
                            commandFactory.CreateCommand(async c =>
                            {
                                await dialogService.CloseMessageBoxAsync(c);

                                var errors = await TaskHelper.WhenAllAsync(
                                    [
                                        toDoUiService
                                            .PostAsync(
                                                Guid.NewGuid(),
                                                new()
                                                {
                                                    DeleteIds = selected
                                                        .Select(x => x.Id)
                                                        .ToArray(),
                                                },
                                                c
                                            )
                                            .ToValidationErrors(),
                                        fileStorageUiService
                                            .PostAsync(
                                                Guid.NewGuid(),
                                                new()
                                                {
                                                    DeleteDirs = selected
                                                        .Select(x => $"{x.Id}/ToDo")
                                                        .ToArray(),
                                                },
                                                c
                                            )
                                            .ToValidationErrors(),
                                    ],
                                    c
                                );

                                return errors.Combine();
                            }),
                            null,
                            DialogButtonType.Primary
                        ),
                        dialogService.CancelButton
                    ),
                    ct
                );
            }
        );

        _switchToDoCommand = CreateLazyCommand<ToDoNotify, HestiaPostResponse>(
            (item, ct) =>
            {
                var toDoUiService = ServiceProvider.GetService<IToDoUiService>();

                return toDoUiService.PostAsync(
                    Guid.NewGuid(),
                    new() { SwitchCompleteIds = [item.Id] },
                    ct
                );
            },
            true,
            false
        );

        _openCurrentToDoCommand = CreateLazyCommand(async ct =>
        {
            var toDoUiService = ServiceProvider.GetService<IToDoUiService>();
            var navigator = ServiceProvider.GetService<INavigator>();
            var factory = ServiceProvider.GetService<IDioclesViewModelFactory>();
            var response = await toDoUiService.GetAsync(new() { IsCurrentActive = true }, ct);
            var currentActive = ServiceProvider.GetService<IToDoUiCache>().CurrentActive;

            if (currentActive?.Parent is null)
            {
                await navigator.NavigateToAsync(factory.CreateRootToDos(), ct);
            }
            else
            {
                await navigator.NavigateToAsync(
                    factory.CreateToDos(currentActive.Parent.ActualItem),
                    ct
                );
            }

            return response;
        });

        _openParentCommand = CreateLazyCommand<ToDoNotify>(
            (item, ct) =>
            {
                var navigator = ServiceProvider.GetService<INavigator>();
                var factory = ServiceProvider.GetService<IDioclesViewModelFactory>();

                return item.Parent is null
                    ? navigator.NavigateToAsync(factory.CreateRootToDos(), ct)
                    : navigator.NavigateToAsync(factory.CreateToDos(item.Parent.ActualItem), ct);
            }
        );

        _switchFavoriteCommand = CreateLazyCommand<ToDoNotify, HestiaPostResponse>(
            (item, ct) =>
            {
                var toDoUiService = ServiceProvider.GetService<IToDoUiService>();

                return toDoUiService.PostAsync(
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
                );
            }
        );

        _changeOrderCommand = CreateLazyCommand<ToDoNotify, IValidationErrors>(
            async (item, ct) =>
            {
                var toDoUiCache = ServiceProvider.GetService<IToDoUiCache>();
                var toDoUiService = ServiceProvider.GetService<IToDoUiService>();
                var itemMutationService = ServiceProvider.GetService<IItemMutationService>();
                var items = item.Parent is null ? toDoUiCache.Roots : item.Parent.Children;

                var changeOrder = await itemMutationService.ShowChangeOrderAsync(
                    items.ToArray(),
                    [item],
                    ct
                );

                if (changeOrder is null)
                {
                    return new DefaultValidationErrors();
                }

                return await toDoUiService.PostAsync(
                    Guid.NewGuid(),
                    new()
                    {
                        ChangeOrders =
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
        );

        _showChangeParentCommand = CreateLazyCommand<ToDoNotify>(
            (item, ct) =>
            {
                var factory = ServiceProvider.GetService<IDioclesViewModelFactory>();
                var toDoUiCache = ServiceProvider.GetService<IToDoUiCache>();
                var toDoUiService = ServiceProvider.GetService<IToDoUiService>();
                var dialogService = ServiceProvider.GetService<IDialogService>();
                var stringFormater = ServiceProvider.GetService<IStringFormater>();
                var safeExecuteWrapper = ServiceProvider.GetService<ISafeExecuteWrapper>();
                var commandFactory = ServiceProvider.GetService<ICommandFactory>();
                var viewModel = factory.CreateChangeParentToDo();

                Dispatcher.UIThread.Post(() =>
                {
                    toDoUiCache.ResetItems();
                    item.IsHideOnTree = true;
                });

                return dialogService.ShowMessageBoxAsync(
                    new(
                        stringFormater
                            .Format(
                                appResourceService.GetResource<string>("Lang.ChangeParentItem"),
                                item.Name
                            )
                            .DispatchToDialogHeader(),
                        viewModel,
                        safeExecuteWrapper,
                        new(
                            appResourceService.GetResource<string>("Lang.ChangeParent"),
                            commandFactory.CreateCommand(async c =>
                            {
                                var parentId = viewModel.IsRoot
                                    ? null
                                    : viewModel.Tree.Selected?.Id;

                                await dialogService.CloseMessageBoxAsync(c);

                                return await toDoUiService.PostAsync(
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
                                    c
                                );
                            }),
                            null,
                            DialogButtonType.Primary
                        ),
                        dialogService.CancelButton
                    ),
                    ct
                );
            }
        );

        _showChangesParentCommand = CreateLazyCommand<IEnumerable<ToDoNotify>>(
            (items, ct) =>
            {
                var factory = ServiceProvider.GetService<IDioclesViewModelFactory>();
                var toDoUiCache = ServiceProvider.GetService<IToDoUiCache>();
                var dialogService = ServiceProvider.GetService<IDialogService>();
                var safeExecuteWrapper = ServiceProvider.GetService<ISafeExecuteWrapper>();
                var commandFactory = ServiceProvider.GetService<ICommandFactory>();
                var toDoUiService = ServiceProvider.GetService<IToDoUiService>();
                var selected = items.Where(x => x.IsSelected).ToArray();
                var viewModel = factory.CreateChangeParentToDo();

                Dispatcher.UIThread.Post(() =>
                {
                    toDoUiCache.ResetItems();

                    foreach (var item in selected)
                    {
                        item.IsHideOnTree = true;
                    }
                });

                return dialogService.ShowMessageBoxAsync(
                    new(
                        appResourceService
                            .GetResource<string>("Lang.ChangeParent")
                            .DispatchToDialogHeader(),
                        viewModel,
                        safeExecuteWrapper,
                        new(
                            appResourceService.GetResource<string>("Lang.ChangeParent"),
                            commandFactory.CreateCommand(async c =>
                            {
                                var parentId = viewModel.IsRoot
                                    ? null
                                    : viewModel.Tree.Selected?.Id;

                                await dialogService.CloseMessageBoxAsync(c);

                                return await toDoUiService.PostAsync(
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
                                    c
                                );
                            }),
                            null,
                            DialogButtonType.Primary
                        ),
                        dialogService.CancelButton
                    ),
                    ct
                );
            }
        );

        _showCloneCommand = CreateLazyCommand<ToDoNotify>(
            (item, ct) =>
            {
                var factory = ServiceProvider.GetService<IDioclesViewModelFactory>();
                var toDoUiCache = ServiceProvider.GetService<IToDoUiCache>();
                var stringFormater = ServiceProvider.GetService<IStringFormater>();
                var dialogService = ServiceProvider.GetService<IDialogService>();
                var safeExecuteWrapper = ServiceProvider.GetService<ISafeExecuteWrapper>();
                var commandFactory = ServiceProvider.GetService<ICommandFactory>();
                var toDoUiService = ServiceProvider.GetService<IToDoUiService>();
                var viewModel = factory.CreateChangeParentToDo();
                Dispatcher.UIThread.Post(() => toDoUiCache.ResetItems());

                return dialogService.ShowMessageBoxAsync(
                    new(
                        stringFormater
                            .Format(
                                appResourceService.GetResource<string>("Lang.CloneItem"),
                                item.Name
                            )
                            .DispatchToDialogHeader(),
                        viewModel,
                        safeExecuteWrapper,
                        new(
                            appResourceService.GetResource<string>("Lang.Clone"),
                            commandFactory.CreateCommand(async c =>
                            {
                                var parentId = viewModel.IsRoot
                                    ? null
                                    : viewModel.Tree.Selected?.Id;

                                await dialogService.CloseMessageBoxAsync(c);

                                return await toDoUiService.PostAsync(
                                    Guid.NewGuid(),
                                    new()
                                    {
                                        Clones =
                                        [
                                            new() { ParentId = parentId, CloneIds = [item.Id] },
                                        ],
                                    },
                                    c
                                );
                            }),
                            null,
                            DialogButtonType.Primary
                        ),
                        dialogService.CancelButton
                    ),
                    ct
                );
            }
        );

        _showClonesCommand = CreateLazyCommand<IEnumerable<ToDoNotify>>(
            (items, ct) =>
            {
                var factory = ServiceProvider.GetService<IDioclesViewModelFactory>();
                var toDoUiCache = ServiceProvider.GetService<IToDoUiCache>();
                var dialogService = ServiceProvider.GetService<IDialogService>();
                var safeExecuteWrapper = ServiceProvider.GetService<ISafeExecuteWrapper>();
                var commandFactory = ServiceProvider.GetService<ICommandFactory>();
                var toDoUiService = ServiceProvider.GetService<IToDoUiService>();
                var selected = items.Where(x => x.IsSelected).ToArray();
                var viewModel = factory.CreateChangeParentToDo();
                Dispatcher.UIThread.Post(() => toDoUiCache.ResetItems());

                return dialogService.ShowMessageBoxAsync(
                    new(
                        appResourceService
                            .GetResource<string>("Lang.Clone")
                            .DispatchToDialogHeader(),
                        viewModel,
                        safeExecuteWrapper,
                        new(
                            appResourceService.GetResource<string>("Lang.Clone"),
                            commandFactory.CreateCommand(async c =>
                            {
                                var parentId = viewModel.IsRoot
                                    ? null
                                    : viewModel.Tree.Selected?.Id;

                                await dialogService.CloseMessageBoxAsync(c);

                                return await toDoUiService.PostAsync(
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
                                    c
                                );
                            }),
                            null,
                            DialogButtonType.Primary
                        ),
                        dialogService.CancelButton
                    ),
                    ct
                );
            }
        );

        _showEditToDoCommand = CreateLazyCommand<ToDoNotify>(
            (item, ct) =>
            {
                var factory = ServiceProvider.GetService<IDioclesViewModelFactory>();
                var toDoUiCache = ServiceProvider.GetService<IToDoUiCache>();
                var dialogService = ServiceProvider.GetService<IDialogService>();
                var stringFormater = ServiceProvider.GetService<IStringFormater>();
                var safeExecuteWrapper = ServiceProvider.GetService<ISafeExecuteWrapper>();

                Dispatcher.UIThread.Post(() =>
                {
                    toDoUiCache.ResetItems();
                    item.IsHideOnTree = true;
                });

                var edit = factory.CreateToDoParameters(
                    item,
                    ValidationMode.ValidateOnlyEdited,
                    false
                );

                return dialogService.ShowMessageBoxAsync(
                    new(
                        stringFormater
                            .Format(
                                appResourceService.GetResource<string>("Lang.EditItem"),
                                item.Name
                            )
                            .DispatchToDialogHeader(),
                        edit,
                        safeExecuteWrapper,
                        new(
                            appResourceService.GetResource<string>("Lang.Edit"),
                            edit.EditItemCommand,
                            item,
                            DialogButtonType.Primary
                        ),
                        dialogService.CancelButton
                    ),
                    ct
                );
            }
        );
    }

    public ICommand ShowCloneCommand => _showCloneCommand.Value;
    public ICommand OpenToDosCommand => _openToDosCommand.Value;
    public ICommand OpenParentCommand => _openParentCommand.Value;
    public ICommand ShowDeleteToDoCommand => _showDeleteToDoCommand.Value;
    public ICommand ShowEditToDoCommand => _showEditToDoCommand.Value;
    public ICommand SwitchToDoCommand => _switchToDoCommand.Value;
    public ICommand OpenCurrentToDoCommand => _openCurrentToDoCommand.Value;
    public ICommand SwitchFavoriteCommand => _switchFavoriteCommand.Value;
    public ICommand ChangeOrderCommand => _changeOrderCommand.Value;
    public ICommand ShowChangeParentCommand => _showChangeParentCommand.Value;

    public IAvaloniaReadOnlyList<InannaCommand> CreateMultiCommands(
        IEnumerable<ToDoNotify> parameter
    )
    {
        return new AvaloniaList<InannaCommand>
        {
            new(
                _showDeleteToDosCommand.Value,
                parameter,
                _appResourceService.GetResource<string>("Lang.Delete"),
                PackIconMaterialDesignKind.Delete,
                ButtonType.Danger
            ),
            new(
                _showEditToDosCommand.Value,
                parameter,
                _appResourceService.GetResource<string>("Lang.Edit"),
                PackIconMaterialDesignKind.Edit
            ),
            new(
                _showChangesParentCommand.Value,
                parameter,
                _appResourceService.GetResource<string>("Lang.ChangeParent"),
                PackIconMaterialDesignKind.AccountTree
            ),
            new(
                _showClonesCommand.Value,
                parameter,
                _appResourceService.GetResource<string>("Lang.Clone"),
                PackIconMaterialDesignKind.CopyrightOutline
            ),
        };
    }

    public IAvaloniaReadOnlyList<InannaCommand> CreateCommands(ToDoNotify parameter)
    {
        return new AvaloniaList<InannaCommand>
        {
            new(
                ShowEditToDoCommand,
                parameter,
                _appResourceService.GetResource<string>("Lang.Edit"),
                PackIconMaterialDesignKind.Edit
            ),
            new(
                SwitchFavoriteCommand,
                parameter,
                _appResourceService.GetResource<string>("Lang.IsFavorite"),
                PackIconMaterialDesignKind.Favorite
            ),
            new(
                ShowChangeParentCommand,
                parameter,
                _appResourceService.GetResource<string>("Lang.ChangeParent"),
                PackIconMaterialDesignKind.AccountTree
            ),
            new(
                ShowCloneCommand,
                parameter,
                _appResourceService.GetResource<string>("Lang.Clone"),
                PackIconMaterialDesignKind.CopyrightOutline
            ),
            new(
                ShowDeleteToDoCommand,
                parameter,
                _appResourceService.GetResource<string>("Lang.Delete"),
                PackIconMaterialDesignKind.Delete,
                ButtonType.Danger
            ),
        };
    }

    private readonly IAppResourceService _appResourceService;
    private readonly Lazy<ICommand> _showCloneCommand;
    private readonly Lazy<ICommand> _showClonesCommand;
    private readonly Lazy<ICommand> _openToDosCommand;
    private readonly Lazy<ICommand> _openParentCommand;
    private readonly Lazy<ICommand> _showDeleteToDoCommand;
    private readonly Lazy<ICommand> _showDeleteToDosCommand;
    private readonly Lazy<ICommand> _showEditToDosCommand;
    private readonly Lazy<ICommand> _showEditToDoCommand;
    private readonly Lazy<ICommand> _switchToDoCommand;
    private readonly Lazy<ICommand> _openCurrentToDoCommand;
    private readonly Lazy<ICommand> _switchFavoriteCommand;
    private readonly Lazy<ICommand> _changeOrderCommand;
    private readonly Lazy<ICommand> _showChangeParentCommand;
    private readonly Lazy<ICommand> _showChangesParentCommand;
}
