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

public sealed class DioclesCommands
{
    public DioclesCommands(IServiceProvider serviceProvider, IAppResourceService appResourceService)
    {
        _appResourceService = appResourceService;
        _serviceProvider = serviceProvider;

        async ValueTask<HestiaGetResponse> OpenCurrentToDoAsync(CancellationToken ct)
        {
            var response = await _serviceProvider
                .GetService<IToDoUiService>()
                .GetAsync(new() { IsCurrentActive = true }, ct);
            var currentActive = _serviceProvider.GetService<IToDoUiCache>().CurrentActive;

            if (currentActive?.Parent is null)
            {
                await _serviceProvider
                    .GetService<INavigator>()
                    .NavigateToAsync(
                        _serviceProvider.GetService<IDioclesViewModelFactory>().CreateRootToDos(),
                        ct
                    );
            }
            else
            {
                await _serviceProvider
                    .GetService<INavigator>()
                    .NavigateToAsync(
                        _serviceProvider
                            .GetService<IDioclesViewModelFactory>()
                            .CreateToDos(currentActive.Parent.ActualItem),
                        ct
                    );
            }

            return response;
        }

        async ValueTask<IValidationErrors> ChangeOrderAsync(ToDoNotify item, CancellationToken ct)
        {
            var items = item.Parent is null
                ? _serviceProvider.GetService<IToDoUiCache>().Roots
                : item.Parent.Children;

            var changeOrder = await _serviceProvider
                .GetService<IItemMutationService>()
                .ShowChangeOrderAsync(items.ToArray(), [item], ct);

            if (changeOrder is null)
            {
                return new DefaultValidationErrors();
            }

            return await _serviceProvider
                .GetService<IToDoUiService>()
                .PostAsync(
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

        async ValueTask EditToDosCommandAsync(IEnumerable<ToDoNotify> items, CancellationToken ct)
        {
            var selected = items.Where(x => x.IsSelected).ToArray();

            Dispatcher.UIThread.Post(() =>
            {
                _serviceProvider.GetService<IToDoUiCache>().ResetItems();

                foreach (var s in selected)
                {
                    s.IsHideOnTree = true;
                }
            });

            var header = appResourceService
                .GetResource<string>("Lang.Edit")
                .DispatchToDialogHeader();

            var settings = await _serviceProvider
                .GetService<IObjectStorage>()
                .LoadAsync<ToDoParametersSettings>(Guid.Empty, ct);

            var viewModel = _serviceProvider
                .GetService<IDioclesViewModelFactory>()
                .CreateToDoParameters(settings, ValidationMode.ValidateOnlyEdited, true);

            async ValueTask<IValidationErrors> EditToDosAsync(CancellationToken c)
            {
                var edit = viewModel.CreateEditToDos(selected.Select(x => x.Id).ToArray());

                var files = viewModel.CreateNeotomaPostRequest(
                    selected.Select(x => $"{x.Id}/ToDo").ToArray()
                );

                var newSettings = viewModel.CreateSettings();
                await _serviceProvider.GetService<IDialogService>().CloseMessageBoxAsync(c);
                await _serviceProvider
                    .GetService<IObjectStorage>()
                    .SaveAsync(newSettings, Guid.Empty, c);

                var errors = await TaskHelper.WhenAllAsync(
                    [
                        _serviceProvider
                            .GetService<IToDoUiService>()
                            .PostAsync(Guid.NewGuid(), new() { Edits = [edit] }, c)
                            .ToValidationErrors(),
                        _serviceProvider
                            .GetService<IFileStorageUiService>()
                            .PostAsync(Guid.NewGuid(), files, ct)
                            .ToValidationErrors(),
                    ],
                    c
                );

                return errors.Combine();
            }

            await _serviceProvider
                .GetService<IDialogService>()
                .ShowMessageBoxAsync(
                    new(
                        header,
                        viewModel,
                        _serviceProvider.GetService<ISafeExecuteWrapper>(),
                        new DialogButton(
                            appResourceService.GetResource<string>("Lang.Edit"),
                            _serviceProvider
                                .GetService<ICommandFactory>()
                                .CreateCommand(c => EditToDosAsync(c).ConfigureAwait(false)),
                            null,
                            DialogButtonType.Primary
                        ),
                        _serviceProvider.GetService<IDialogService>().CancelButton
                    ),
                    ct
                );
        }

        _openToDosCommand = new Lazy<ICommand>(() =>
            _serviceProvider
                .GetService<ICommandFactory>()
                .CreateCommand<ToDoNotify>(
                    (item, ct) =>
                        _serviceProvider
                            .GetService<INavigator>()
                            .NavigateToAsync(
                                _serviceProvider
                                    .GetService<IDioclesViewModelFactory>()
                                    .CreateToDos(item.ActualItem),
                                ct
                            ),
                    true
                )
        );

        _showDeleteToDoCommand = new Lazy<ICommand>(() =>
            _serviceProvider
                .GetService<ICommandFactory>()
                .CreateCommand<ToDoNotify>(
                    (item, ct) =>
                    {
                        var header = appResourceService
                            .GetResource<string>("Lang.Delete")
                            .DispatchToDialogHeader();

                        async ValueTask<IValidationErrors> DeleteToDoAsync(CancellationToken c)
                        {
                            await _serviceProvider
                                .GetService<IDialogService>()
                                .CloseMessageBoxAsync(c);

                            var errors = await TaskHelper.WhenAllAsync(
                                [
                                    _serviceProvider
                                        .GetService<IToDoUiService>()
                                        .PostAsync(
                                            Guid.NewGuid(),
                                            new() { DeleteIds = [item.Id] },
                                            c
                                        )
                                        .ToValidationErrors(),
                                    _serviceProvider
                                        .GetService<IFileStorageUiService>()
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

                        async ValueTask<IValidationErrors> OkAsync(CancellationToken c)
                        {
                            if (item.Parent is null)
                            {
                                await _serviceProvider
                                    .GetService<INavigator>()
                                    .NavigateToAsync(
                                        _serviceProvider
                                            .GetService<IDioclesViewModelFactory>()
                                            .CreateRootToDos(),
                                        c
                                    );
                            }
                            else
                            {
                                await _serviceProvider
                                    .GetService<INavigator>()
                                    .NavigateToAsync(
                                        _serviceProvider
                                            .GetService<IDioclesViewModelFactory>()
                                            .CreateToDos(item.Parent),
                                        c
                                    );
                            }

                            return await DeleteToDoAsync(c);
                        }

                        return _serviceProvider
                            .GetService<IDialogService>()
                            .ShowMessageBoxAsync(
                                new(
                                    header,
                                    Dispatcher.UIThread.Invoke(() =>
                                        new TextBlock
                                        {
                                            Text = _serviceProvider
                                                .GetService<IStringFormater>()
                                                .Format(
                                                    appResourceService.GetResource<string>(
                                                        "Lang.AskDelete"
                                                    ),
                                                    item.Name
                                                ),
                                            Classes = { "text-wrap" },
                                        }
                                    ),
                                    _serviceProvider.GetService<ISafeExecuteWrapper>(),
                                    new DialogButton(
                                        appResourceService.GetResource<string>("Lang.Delete"),
                                        _serviceProvider
                                            .GetService<ICommandFactory>()
                                            .CreateCommand(c => OkAsync(c).ConfigureAwait(false)),
                                        null,
                                        DialogButtonType.Primary
                                    ),
                                    _serviceProvider.GetService<IDialogService>().CancelButton
                                ),
                                ct
                            );
                    }
                )
        );

        _showEditToDosCommand = new Lazy<ICommand>(() =>
            _serviceProvider
                .GetService<ICommandFactory>()
                .CreateCommand<IEnumerable<ToDoNotify>>(
                    (items, ct) => EditToDosCommandAsync(items, ct).ConfigureAwait(false)
                )
        );

        _showDeleteToDosCommand = new Lazy<ICommand>(() =>
            _serviceProvider
                .GetService<ICommandFactory>()
                .CreateCommand<IEnumerable<ToDoNotify>>(
                    (items, ct) =>
                    {
                        var selected = items.Where(x => x.IsSelected).ToArray();

                        var header = appResourceService
                            .GetResource<string>("Lang.Delete")
                            .DispatchToDialogHeader();

                        async ValueTask<IValidationErrors> DeleteToDosAsync(CancellationToken c)
                        {
                            await _serviceProvider
                                .GetService<IDialogService>()
                                .CloseMessageBoxAsync(c);

                            var errors = await TaskHelper.WhenAllAsync(
                                [
                                    _serviceProvider
                                        .GetService<IToDoUiService>()
                                        .PostAsync(
                                            Guid.NewGuid(),
                                            new()
                                            {
                                                DeleteIds = selected.Select(x => x.Id).ToArray(),
                                            },
                                            c
                                        )
                                        .ToValidationErrors(),
                                    _serviceProvider
                                        .GetService<IFileStorageUiService>()
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
                        }

                        return _serviceProvider
                            .GetService<IDialogService>()
                            .ShowMessageBoxAsync(
                                new(
                                    header,
                                    Dispatcher.UIThread.Invoke(() =>
                                        new TextBlock
                                        {
                                            Text = _serviceProvider
                                                .GetService<IStringFormater>()
                                                .Format(
                                                    appResourceService.GetResource<string>(
                                                        "Lang.AskDelete"
                                                    ),
                                                    selected.Select(x => x.Name).JoinString(", ")
                                                ),
                                            Classes = { "text-wrap" },
                                        }
                                    ),
                                    _serviceProvider.GetService<ISafeExecuteWrapper>(),
                                    new DialogButton(
                                        appResourceService.GetResource<string>("Lang.Delete"),
                                        _serviceProvider
                                            .GetService<ICommandFactory>()
                                            .CreateCommand(ct =>
                                                DeleteToDosAsync(ct).ConfigureAwait(false)
                                            ),
                                        null,
                                        DialogButtonType.Primary
                                    ),
                                    _serviceProvider.GetService<IDialogService>().CancelButton
                                ),
                                ct
                            );
                    }
                )
        );

        _switchToDoCommand = new Lazy<ICommand>(() =>
            _serviceProvider
                .GetService<ICommandFactory>()
                .CreateCommand<ToDoNotify, HestiaPostResponse>(
                    (item, ct) =>
                        _serviceProvider
                            .GetService<IToDoUiService>()
                            .PostAsync(Guid.NewGuid(), new() { SwitchCompleteIds = [item.Id] }, ct),
                    true,
                    false
                )
        );

        _openCurrentToDoCommand = new Lazy<ICommand>(() =>
            _serviceProvider
                .GetService<ICommandFactory>()
                .CreateCommand(ct => OpenCurrentToDoAsync(ct).ConfigureAwait(false))
        );

        _openParentCommand = new Lazy<ICommand>(() =>
            _serviceProvider
                .GetService<ICommandFactory>()
                .CreateCommand<ToDoNotify>(
                    (item, ct) =>
                        item.Parent is null
                            ? _serviceProvider
                                .GetService<INavigator>()
                                .NavigateToAsync(
                                    _serviceProvider
                                        .GetService<IDioclesViewModelFactory>()
                                        .CreateRootToDos(),
                                    ct
                                )
                            : _serviceProvider
                                .GetService<INavigator>()
                                .NavigateToAsync(
                                    _serviceProvider
                                        .GetService<IDioclesViewModelFactory>()
                                        .CreateToDos(item.Parent.ActualItem),
                                    ct
                                )
                )
        );

        _switchFavoriteCommand = new Lazy<ICommand>(() =>
            _serviceProvider
                .GetService<ICommandFactory>()
                .CreateCommand<ToDoNotify, HestiaPostResponse>(
                    (item, ct) =>
                        _serviceProvider
                            .GetService<IToDoUiService>()
                            .PostAsync(
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
                )
        );

        _changeOrderCommand = new Lazy<ICommand>(() =>
            _serviceProvider
                .GetService<ICommandFactory>()
                .CreateCommand<ToDoNotify, IValidationErrors>(
                    (item, ct) => ChangeOrderAsync(item, ct).ConfigureAwait(false)
                )
        );

        _showChangeParentCommand = new Lazy<ICommand>(() =>
            _serviceProvider
                .GetService<ICommandFactory>()
                .CreateCommand<ToDoNotify>(
                    (item, ct) =>
                    {
                        var viewModel = _serviceProvider
                            .GetService<IDioclesViewModelFactory>()
                            .CreateChangeParentToDo();

                        Dispatcher.UIThread.Post(() =>
                        {
                            _serviceProvider.GetService<IToDoUiCache>().ResetItems();
                            item.IsHideOnTree = true;
                        });

                        async ValueTask<HestiaPostResponse> ChangeParentAsync(CancellationToken ct)
                        {
                            var parentId = viewModel.IsRoot ? null : viewModel.Tree.Selected?.Id;

                            await _serviceProvider
                                .GetService<IDialogService>()
                                .CloseMessageBoxAsync(ct);

                            return await _serviceProvider
                                .GetService<IToDoUiService>()
                                .PostAsync(
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

                        return _serviceProvider
                            .GetService<IDialogService>()
                            .ShowMessageBoxAsync(
                                new(
                                    _serviceProvider
                                        .GetService<IStringFormater>()
                                        .Format(
                                            appResourceService.GetResource<string>(
                                                "Lang.ChangeParentItem"
                                            ),
                                            item.Name
                                        )
                                        .DispatchToDialogHeader(),
                                    viewModel,
                                    _serviceProvider.GetService<ISafeExecuteWrapper>(),
                                    new(
                                        appResourceService.GetResource<string>("Lang.ChangeParent"),
                                        _serviceProvider
                                            .GetService<ICommandFactory>()
                                            .CreateCommand(c =>
                                                ChangeParentAsync(c).ConfigureAwait(false)
                                            ),
                                        null,
                                        DialogButtonType.Primary
                                    ),
                                    _serviceProvider.GetService<IDialogService>().CancelButton
                                ),
                                ct
                            );
                    }
                )
        );

        _showChangesParentCommand = new Lazy<ICommand>(() =>
            _serviceProvider
                .GetService<ICommandFactory>()
                .CreateCommand<IEnumerable<ToDoNotify>>(
                    (items, ct) =>
                    {
                        var selected = items.Where(x => x.IsSelected).ToArray();
                        var viewModel = _serviceProvider
                            .GetService<IDioclesViewModelFactory>()
                            .CreateChangeParentToDo();

                        Dispatcher.UIThread.Post(() =>
                        {
                            _serviceProvider.GetService<IToDoUiCache>().ResetItems();

                            foreach (var item in selected)
                            {
                                item.IsHideOnTree = true;
                            }
                        });

                        async ValueTask<HestiaPostResponse> ChangesParentAsync(CancellationToken ct)
                        {
                            var parentId = viewModel.IsRoot ? null : viewModel.Tree.Selected?.Id;

                            await _serviceProvider
                                .GetService<IDialogService>()
                                .CloseMessageBoxAsync(ct);

                            return await _serviceProvider
                                .GetService<IToDoUiService>()
                                .PostAsync(
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

                        return _serviceProvider
                            .GetService<IDialogService>()
                            .ShowMessageBoxAsync(
                                new(
                                    appResourceService
                                        .GetResource<string>("Lang.ChangeParent")
                                        .DispatchToDialogHeader(),
                                    viewModel,
                                    _serviceProvider.GetService<ISafeExecuteWrapper>(),
                                    new(
                                        appResourceService.GetResource<string>("Lang.ChangeParent"),
                                        _serviceProvider
                                            .GetService<ICommandFactory>()
                                            .CreateCommand(ct =>
                                                ChangesParentAsync(ct).ConfigureAwait(false)
                                            ),
                                        null,
                                        DialogButtonType.Primary
                                    ),
                                    _serviceProvider.GetService<IDialogService>().CancelButton
                                ),
                                ct
                            );
                    }
                )
        );

        _showCloneCommand = new Lazy<ICommand>(() =>
            _serviceProvider
                .GetService<ICommandFactory>()
                .CreateCommand<ToDoNotify>(
                    (item, ct) =>
                    {
                        var viewModel = _serviceProvider
                            .GetService<IDioclesViewModelFactory>()
                            .CreateChangeParentToDo();
                        Dispatcher.UIThread.Post(() =>
                            _serviceProvider.GetService<IToDoUiCache>().ResetItems()
                        );

                        async ValueTask<HestiaPostResponse> CloneAsync(CancellationToken ct)
                        {
                            var parentId = viewModel.IsRoot ? null : viewModel.Tree.Selected?.Id;

                            await _serviceProvider
                                .GetService<IDialogService>()
                                .CloseMessageBoxAsync(ct);

                            return await _serviceProvider
                                .GetService<IToDoUiService>()
                                .PostAsync(
                                    Guid.NewGuid(),
                                    new()
                                    {
                                        Clones =
                                        [
                                            new() { ParentId = parentId, CloneIds = [item.Id] },
                                        ],
                                    },
                                    ct
                                );
                        }

                        return _serviceProvider
                            .GetService<IDialogService>()
                            .ShowMessageBoxAsync(
                                new(
                                    _serviceProvider
                                        .GetService<IStringFormater>()
                                        .Format(
                                            appResourceService.GetResource<string>(
                                                "Lang.CloneItem"
                                            ),
                                            item.Name
                                        )
                                        .DispatchToDialogHeader(),
                                    viewModel,
                                    _serviceProvider.GetService<ISafeExecuteWrapper>(),
                                    new(
                                        appResourceService.GetResource<string>("Lang.Clone"),
                                        _serviceProvider
                                            .GetService<ICommandFactory>()
                                            .CreateCommand(ct =>
                                                CloneAsync(ct).ConfigureAwait(false)
                                            ),
                                        null,
                                        DialogButtonType.Primary
                                    ),
                                    _serviceProvider.GetService<IDialogService>().CancelButton
                                ),
                                ct
                            );
                    }
                )
        );

        _showClonesCommand = new Lazy<ICommand>(() =>
            _serviceProvider
                .GetService<ICommandFactory>()
                .CreateCommand<IEnumerable<ToDoNotify>>(
                    (items, ct) =>
                    {
                        var selected = items.Where(x => x.IsSelected).ToArray();
                        var viewModel = _serviceProvider
                            .GetService<IDioclesViewModelFactory>()
                            .CreateChangeParentToDo();
                        Dispatcher.UIThread.Post(() =>
                            _serviceProvider.GetService<IToDoUiCache>().ResetItems()
                        );

                        async ValueTask<HestiaPostResponse> CloneAsync(CancellationToken ct)
                        {
                            var parentId = viewModel.IsRoot ? null : viewModel.Tree.Selected?.Id;

                            await _serviceProvider
                                .GetService<IDialogService>()
                                .CloseMessageBoxAsync(ct);

                            return await _serviceProvider
                                .GetService<IToDoUiService>()
                                .PostAsync(
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

                        return _serviceProvider
                            .GetService<IDialogService>()
                            .ShowMessageBoxAsync(
                                new(
                                    appResourceService
                                        .GetResource<string>("Lang.Clone")
                                        .DispatchToDialogHeader(),
                                    viewModel,
                                    _serviceProvider.GetService<ISafeExecuteWrapper>(),
                                    new(
                                        appResourceService.GetResource<string>("Lang.Clone"),
                                        _serviceProvider
                                            .GetService<ICommandFactory>()
                                            .CreateCommand(c =>
                                                CloneAsync(c).ConfigureAwait(false)
                                            ),
                                        null,
                                        DialogButtonType.Primary
                                    ),
                                    _serviceProvider.GetService<IDialogService>().CancelButton
                                ),
                                ct
                            );
                    }
                )
        );

        _showEditToDoCommand = new Lazy<ICommand>(() =>
            _serviceProvider
                .GetService<ICommandFactory>()
                .CreateCommand<ToDoNotify>(
                    (item, ct) =>
                    {
                        Dispatcher.UIThread.Post(() =>
                        {
                            _serviceProvider.GetService<IToDoUiCache>().ResetItems();
                            item.IsHideOnTree = true;
                        });

                        var edit = _serviceProvider
                            .GetService<IDioclesViewModelFactory>()
                            .CreateToDoParameters(item, ValidationMode.ValidateOnlyEdited, false);

                        return _serviceProvider
                            .GetService<IDialogService>()
                            .ShowMessageBoxAsync(
                                new(
                                    _serviceProvider
                                        .GetService<IStringFormater>()
                                        .Format(
                                            appResourceService.GetResource<string>("Lang.EditItem"),
                                            item.Name
                                        )
                                        .DispatchToDialogHeader(),
                                    edit,
                                    _serviceProvider.GetService<ISafeExecuteWrapper>(),
                                    new(
                                        appResourceService.GetResource<string>("Lang.Edit"),
                                        edit.EditItemCommand,
                                        item,
                                        DialogButtonType.Primary
                                    ),
                                    _serviceProvider.GetService<IDialogService>().CancelButton
                                ),
                                ct
                            );
                    }
                )
        );
    }

    public ICommand ShowCloneCommand => _showCloneCommand.Value;
    public ICommand ShowClonesCommand => _showClonesCommand.Value;
    public ICommand OpenToDosCommand => _openToDosCommand.Value;
    public ICommand OpenParentCommand => _openParentCommand.Value;
    public ICommand ShowDeleteToDoCommand => _showDeleteToDoCommand.Value;
    public ICommand ShowDeleteToDosCommand => _showDeleteToDosCommand.Value;
    public ICommand ShowEditToDosCommand => _showEditToDosCommand.Value;
    public ICommand ShowEditToDoCommand => _showEditToDoCommand.Value;
    public ICommand SwitchToDoCommand => _switchToDoCommand.Value;
    public ICommand OpenCurrentToDoCommand => _openCurrentToDoCommand.Value;
    public ICommand SwitchFavoriteCommand => _switchFavoriteCommand.Value;
    public ICommand ChangeOrderCommand => _changeOrderCommand.Value;
    public ICommand ShowChangeParentCommand => _showChangeParentCommand.Value;
    public ICommand ShowChangesParentCommand => _showChangesParentCommand.Value;

    public IAvaloniaReadOnlyList<InannaCommand> CreateMultiCommands(
        IEnumerable<ToDoNotify> parameter
    )
    {
        return new AvaloniaList<InannaCommand>
        {
            new(
                ShowDeleteToDosCommand,
                parameter,
                _appResourceService.GetResource<string>("Lang.Delete"),
                PackIconMaterialDesignKind.Delete,
                ButtonType.Danger
            ),
            new(
                ShowEditToDosCommand,
                parameter,
                _appResourceService.GetResource<string>("Lang.Edit"),
                PackIconMaterialDesignKind.Edit
            ),
            new(
                ShowChangesParentCommand,
                parameter,
                _appResourceService.GetResource<string>("Lang.ChangeParent"),
                PackIconMaterialDesignKind.AccountTree
            ),
            new(
                ShowClonesCommand,
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
                ShowDeleteToDoCommand,
                parameter,
                _appResourceService.GetResource<string>("Lang.Delete"),
                PackIconMaterialDesignKind.Delete,
                ButtonType.Danger
            ),
        };
    }

    private readonly IAppResourceService _appResourceService;
    private readonly IServiceProvider _serviceProvider;
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
