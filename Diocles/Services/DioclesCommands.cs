using System.Windows.Input;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Threading;
using Diocles.Models;
using Gaia.Helpers;
using Gaia.Models;
using Gaia.Services;
using Hestia.Contract.Models;
using IconPacks.Avalonia.MaterialDesign;
using Inanna.Helpers;
using Inanna.Models;
using Inanna.Services;
using Inanna.Ui;
using Neotoma.Contract.Models;
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

                var content = Dispatcher.UIThread.Invoke(() =>
                    new TextBlock
                    {
                        Text = stringFormater.Format(
                            appResourceService.GetResource<string>("Lang.AskDelete"),
                            item.Name
                        ),
                        Classes = { "text-wrap" },
                    }
                );

                var commandDelete = commandFactory.CreateCommand(async c =>
                {
                    if (item.Parent is null)
                    {
                        await navigator.NavigateToAsync(factory.CreateRootToDos(), c);
                    }
                    else
                    {
                        var itemView = factory.CreateToDos(item.Parent);
                        await navigator.NavigateToAsync(itemView, c);
                    }

                    await dialogService.CloseMessageBoxAsync(c);

                    var fileStorageRequest = new NeotomaPostRequest
                    {
                        DeleteDirs = [$"{item.Id}/ToDo"],
                    };

                    var errors = await TaskHelper.WhenAllAsync(
                        [
                            toDoUiService
                                .PostAsync(Guid.NewGuid(), new() { DeleteIds = [item.Id] }, c)
                                .ToValidationErrors(),
                            fileStorageUiService
                                .PostAsync(Guid.NewGuid(), fileStorageRequest, c)
                                .ToValidationErrors(),
                        ],
                        c
                    );

                    return errors.Combine();
                });

                var deleteButton = new DialogButton(
                    appResourceService.GetResource<string>("Lang.Delete"),
                    commandDelete,
                    null,
                    DialogButtonType.Primary
                );

                var dialog = new DialogViewModel(
                    header,
                    content,
                    safeExecuteWrapper,
                    deleteButton,
                    dialogService.CancelButton
                );

                return dialogService.ShowMessageBoxAsync(dialog, ct);
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

                var command = commandFactory.CreateCommand(async c =>
                {
                    var ids = selected.SelectAsSpan(x => x.Id).ToArray();
                    var edit = viewModel.CreateEditToDos(ids);
                    var dirs = selected.SelectAsSpan(x => $"{x.Id}/ToDo").ToArray();
                    var files = viewModel.CreateNeotomaPostRequest(dirs);
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
                });

                var button = new DialogButton(
                    appResourceService.GetResource<string>("Lang.Edit"),
                    command,
                    null,
                    DialogButtonType.Primary
                );

                var dialog = new DialogViewModel(
                    header,
                    viewModel,
                    safeExecuteWrapper,
                    button,
                    dialogService.CancelButton
                );

                await dialogService.ShowMessageBoxAsync(dialog, ct);
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

                var command = commandFactory.CreateCommand(async c =>
                {
                    await dialogService.CloseMessageBoxAsync(c);
                    var deleteIds = selected.SelectAsSpan(x => x.Id).ToArray();
                    var deleteDirs = selected.SelectAsSpan(x => $"{x.Id}/ToDo").ToArray();
                    var toDoRequest = new HestiaPostRequest { DeleteIds = deleteIds };
                    var fileStorageRequest = new NeotomaPostRequest { DeleteDirs = deleteDirs };

                    var errors = await TaskHelper.WhenAllAsync(
                        [
                            toDoUiService
                                .PostAsync(Guid.NewGuid(), toDoRequest, c)
                                .ToValidationErrors(),
                            fileStorageUiService
                                .PostAsync(Guid.NewGuid(), fileStorageRequest, c)
                                .ToValidationErrors(),
                        ],
                        c
                    );

                    return errors.Combine();
                });

                var content = Dispatcher.UIThread.Invoke(() =>
                    new TextBlock
                    {
                        Text = stringFormater.Format(
                            appResourceService.GetResource<string>("Lang.AskDelete"),
                            selected.SelectAsSpan(x => x.Name).JoinString(", ")
                        ),
                        Classes = { "text-wrap" },
                    }
                );

                var button = new DialogButton(
                    appResourceService.GetResource<string>("Lang.Delete"),
                    command,
                    null,
                    DialogButtonType.Primary
                );

                var dialog = new DialogViewModel(
                    header,
                    content,
                    safeExecuteWrapper,
                    button,
                    dialogService.CancelButton
                );

                return dialogService.ShowMessageBoxAsync(dialog, ct);
            }
        );

        _switchToDoCommand = CreateLazyCommand<ToDoNotify, HestiaPostResponse>(
            (item, ct) =>
            {
                var toDoUiService = ServiceProvider.GetService<IToDoUiService>();
                var request = new HestiaPostRequest { SwitchCompleteIds = [item.Id] };

                return toDoUiService.PostAsync(Guid.NewGuid(), request, ct);
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
                var itemView = factory.CreateToDos(currentActive.Parent.ActualItem);
                await navigator.NavigateToAsync(itemView, ct);
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

                var edit = new EditToDos
                {
                    Ids = [item.Id],
                    IsFavorite = !item.IsFavorite,
                    IsEditIsFavorite = true,
                };

                var request = new HestiaPostRequest { Edits = [edit] };

                return toDoUiService.PostAsync(Guid.NewGuid(), request, ct);
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

                var change = new ChangeOrder
                {
                    IsAfter = changeOrder.IsAfter,
                    StartId = changeOrder.Item.Id,
                    InsertIds = [item.Id],
                };

                var request = new HestiaPostRequest { ChangeOrders = [change] };

                return await toDoUiService.PostAsync(Guid.NewGuid(), request, ct);
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

                var header = stringFormater
                    .Format(
                        appResourceService.GetResource<string>("Lang.ChangeParentItem"),
                        item.Name
                    )
                    .DispatchToDialogHeader();

                var command = commandFactory.CreateCommand(async c =>
                {
                    var parentId = viewModel.IsRoot ? null : viewModel.Tree.Selected?.Id;
                    await dialogService.CloseMessageBoxAsync(c);

                    var edit = new EditToDos
                    {
                        Ids = [item.Id],
                        ParentId = parentId,
                        IsEditParentId = true,
                    };

                    var request = new HestiaPostRequest { Edits = [edit] };

                    return await toDoUiService.PostAsync(Guid.NewGuid(), request, c);
                });

                var button = new DialogButton(
                    appResourceService.GetResource<string>("Lang.ChangeParent"),
                    command,
                    null,
                    DialogButtonType.Primary
                );

                var dialog = new DialogViewModel(
                    header,
                    viewModel,
                    safeExecuteWrapper,
                    button,
                    dialogService.CancelButton
                );

                return dialogService.ShowMessageBoxAsync(dialog, ct);
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

                var header = appResourceService
                    .GetResource<string>("Lang.ChangeParent")
                    .DispatchToDialogHeader();

                var command = commandFactory.CreateCommand(async c =>
                {
                    var parentId = viewModel.IsRoot ? null : viewModel.Tree.Selected?.Id;
                    await dialogService.CloseMessageBoxAsync(c);

                    var edit = new EditToDos
                    {
                        Ids = selected.SelectAsSpan(x => x.Id).ToArray(),
                        ParentId = parentId,
                        IsEditParentId = true,
                    };

                    var request = new HestiaPostRequest { Edits = [edit] };

                    return await toDoUiService.PostAsync(Guid.NewGuid(), request, c);
                });

                var button = new DialogButton(
                    appResourceService.GetResource<string>("Lang.ChangeParent"),
                    command,
                    null,
                    DialogButtonType.Primary
                );

                var dialog = new DialogViewModel(
                    header,
                    viewModel,
                    safeExecuteWrapper,
                    button,
                    dialogService.CancelButton
                );

                return dialogService.ShowMessageBoxAsync(dialog, ct);
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
                Dispatcher.UIThread.Post(toDoUiCache.ResetItems);

                var header = stringFormater
                    .Format(appResourceService.GetResource<string>("Lang.CloneItem"), item.Name)
                    .DispatchToDialogHeader();

                var command = commandFactory.CreateCommand(async c =>
                {
                    var parentId = viewModel.IsRoot ? null : viewModel.Tree.Selected?.Id;
                    await dialogService.CloseMessageBoxAsync(c);
                    var clone = new CloneToDoItem { ParentId = parentId, CloneIds = [item.Id] };
                    var request = new HestiaPostRequest { Clones = [clone] };

                    return await toDoUiService.PostAsync(Guid.NewGuid(), request, c);
                });

                var button = new DialogButton(
                    appResourceService.GetResource<string>("Lang.Clone"),
                    command,
                    null,
                    DialogButtonType.Primary
                );

                var dialog = new DialogViewModel(
                    header,
                    viewModel,
                    safeExecuteWrapper,
                    button,
                    dialogService.CancelButton
                );

                return dialogService.ShowMessageBoxAsync(dialog, ct);
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
                Dispatcher.UIThread.Post(toDoUiCache.ResetItems);

                var header = appResourceService
                    .GetResource<string>("Lang.Clone")
                    .DispatchToDialogHeader();

                var command = commandFactory.CreateCommand(async c =>
                {
                    var parentId = viewModel.IsRoot ? null : viewModel.Tree.Selected?.Id;
                    await dialogService.CloseMessageBoxAsync(c);

                    var clone = new CloneToDoItem
                    {
                        ParentId = parentId,
                        CloneIds = selected.SelectAsSpan(x => x.Id).ToArray(),
                    };

                    var request = new HestiaPostRequest { Clones = [clone] };

                    return await toDoUiService.PostAsync(Guid.NewGuid(), request, c);
                });

                var button = new DialogButton(
                    appResourceService.GetResource<string>("Lang.Clone"),
                    command,
                    null,
                    DialogButtonType.Primary
                );

                var dialog = new DialogViewModel(
                    header,
                    viewModel,
                    safeExecuteWrapper,
                    button,
                    dialogService.CancelButton
                );

                return dialogService.ShowMessageBoxAsync(dialog, ct);
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

                var header = stringFormater
                    .Format(appResourceService.GetResource<string>("Lang.EditItem"), item.Name)
                    .DispatchToDialogHeader();

                var button = new DialogButton(
                    appResourceService.GetResource<string>("Lang.Edit"),
                    edit.EditItemCommand,
                    item,
                    DialogButtonType.Primary
                );

                var dialog = new DialogViewModel(
                    header,
                    edit,
                    safeExecuteWrapper,
                    button,
                    dialogService.CancelButton
                );

                return dialogService.ShowMessageBoxAsync(dialog, ct);
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
