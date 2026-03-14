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
                ServiceProvider
                    .GetService<INavigator>()
                    .NavigateToAsync(
                        ServiceProvider
                            .GetService<IDioclesViewModelFactory>()
                            .CreateToDos(item.ActualItem),
                        ct
                    ),
            true
        );

        _showDeleteToDoCommand = CreateLazyCommand<ToDoNotify>(
            (item, ct) =>
            {
                var header = appResourceService
                    .GetResource<string>("Lang.Delete")
                    .DispatchToDialogHeader();

                async ValueTask<IValidationErrors> DeleteToDoAsync(CancellationToken c)
                {
                    await ServiceProvider.GetService<IDialogService>().CloseMessageBoxAsync(c);

                    var errors = await TaskHelper.WhenAllAsync(
                        [
                            ServiceProvider
                                .GetService<IToDoUiService>()
                                .PostAsync(Guid.NewGuid(), new() { DeleteIds = [item.Id] }, c)
                                .ToValidationErrors(),
                            ServiceProvider
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
                        await ServiceProvider
                            .GetService<INavigator>()
                            .NavigateToAsync(
                                ServiceProvider
                                    .GetService<IDioclesViewModelFactory>()
                                    .CreateRootToDos(),
                                c
                            );
                    }
                    else
                    {
                        await ServiceProvider
                            .GetService<INavigator>()
                            .NavigateToAsync(
                                ServiceProvider
                                    .GetService<IDioclesViewModelFactory>()
                                    .CreateToDos(item.Parent),
                                c
                            );
                    }

                    return await DeleteToDoAsync(c);
                }

                return ServiceProvider
                    .GetService<IDialogService>()
                    .ShowMessageBoxAsync(
                        new(
                            header,
                            Dispatcher.UIThread.Invoke(() =>
                                new TextBlock
                                {
                                    Text = ServiceProvider
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
                            ServiceProvider.GetService<ISafeExecuteWrapper>(),
                            new DialogButton(
                                appResourceService.GetResource<string>("Lang.Delete"),
                                ServiceProvider
                                    .GetService<ICommandFactory>()
                                    .CreateCommand(c => OkAsync(c).ConfigureAwait(false)),
                                null,
                                DialogButtonType.Primary
                            ),
                            ServiceProvider.GetService<IDialogService>().CancelButton
                        ),
                        ct
                    );
            }
        );

        _showEditToDosCommand = CreateLazyCommand<IEnumerable<ToDoNotify>>(
            async (items, ct) =>
            {
                var selected = items.Where(x => x.IsSelected).ToArray();

                Dispatcher.UIThread.Post(() =>
                {
                    ServiceProvider.GetService<IToDoUiCache>().ResetItems();

                    foreach (var s in selected)
                    {
                        s.IsHideOnTree = true;
                    }
                });

                var header = appResourceService
                    .GetResource<string>("Lang.Edit")
                    .DispatchToDialogHeader();

                var settings = await ServiceProvider
                    .GetService<IObjectStorage>()
                    .LoadAsync<ToDoParametersSettings>(Guid.Empty, ct);

                var viewModel = ServiceProvider
                    .GetService<IDioclesViewModelFactory>()
                    .CreateToDoParameters(settings, ValidationMode.ValidateOnlyEdited, true);

                async ValueTask<IValidationErrors> EditToDosAsync(CancellationToken c)
                {
                    var edit = viewModel.CreateEditToDos(selected.Select(x => x.Id).ToArray());

                    var files = viewModel.CreateNeotomaPostRequest(
                        selected.Select(x => $"{x.Id}/ToDo").ToArray()
                    );

                    var newSettings = viewModel.CreateSettings();
                    await ServiceProvider.GetService<IDialogService>().CloseMessageBoxAsync(c);
                    await ServiceProvider
                        .GetService<IObjectStorage>()
                        .SaveAsync(newSettings, Guid.Empty, c);

                    var errors = await TaskHelper.WhenAllAsync(
                        [
                            ServiceProvider
                                .GetService<IToDoUiService>()
                                .PostAsync(Guid.NewGuid(), new() { Edits = [edit] }, c)
                                .ToValidationErrors(),
                            ServiceProvider
                                .GetService<IFileStorageUiService>()
                                .PostAsync(Guid.NewGuid(), files, ct)
                                .ToValidationErrors(),
                        ],
                        c
                    );

                    return errors.Combine();
                }

                await ServiceProvider
                    .GetService<IDialogService>()
                    .ShowMessageBoxAsync(
                        new(
                            header,
                            viewModel,
                            ServiceProvider.GetService<ISafeExecuteWrapper>(),
                            new DialogButton(
                                appResourceService.GetResource<string>("Lang.Edit"),
                                ServiceProvider
                                    .GetService<ICommandFactory>()
                                    .CreateCommand(c => EditToDosAsync(c).ConfigureAwait(false)),
                                null,
                                DialogButtonType.Primary
                            ),
                            ServiceProvider.GetService<IDialogService>().CancelButton
                        ),
                        ct
                    );
            }
        );

        _showDeleteToDosCommand = CreateLazyCommand<IEnumerable<ToDoNotify>>(
            (items, ct) =>
            {
                var selected = items.Where(x => x.IsSelected).ToArray();

                var header = appResourceService
                    .GetResource<string>("Lang.Delete")
                    .DispatchToDialogHeader();

                async ValueTask<IValidationErrors> DeleteToDosAsync(CancellationToken c)
                {
                    await ServiceProvider.GetService<IDialogService>().CloseMessageBoxAsync(c);

                    var errors = await TaskHelper.WhenAllAsync(
                        [
                            ServiceProvider
                                .GetService<IToDoUiService>()
                                .PostAsync(
                                    Guid.NewGuid(),
                                    new() { DeleteIds = selected.Select(x => x.Id).ToArray() },
                                    c
                                )
                                .ToValidationErrors(),
                            ServiceProvider
                                .GetService<IFileStorageUiService>()
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

                return ServiceProvider
                    .GetService<IDialogService>()
                    .ShowMessageBoxAsync(
                        new(
                            header,
                            Dispatcher.UIThread.Invoke(() =>
                                new TextBlock
                                {
                                    Text = ServiceProvider
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
                            ServiceProvider.GetService<ISafeExecuteWrapper>(),
                            new DialogButton(
                                appResourceService.GetResource<string>("Lang.Delete"),
                                ServiceProvider
                                    .GetService<ICommandFactory>()
                                    .CreateCommand(ct =>
                                        DeleteToDosAsync(ct).ConfigureAwait(false)
                                    ),
                                null,
                                DialogButtonType.Primary
                            ),
                            ServiceProvider.GetService<IDialogService>().CancelButton
                        ),
                        ct
                    );
            }
        );

        _switchToDoCommand = CreateLazyCommand<ToDoNotify, HestiaPostResponse>(
            (item, ct) =>
                ServiceProvider
                    .GetService<IToDoUiService>()
                    .PostAsync(Guid.NewGuid(), new() { SwitchCompleteIds = [item.Id] }, ct),
            true,
            false
        );

        _openCurrentToDoCommand = CreateLazyCommand(async ct =>
        {
            var response = await ServiceProvider
                .GetService<IToDoUiService>()
                .GetAsync(new() { IsCurrentActive = true }, ct);
            var currentActive = ServiceProvider.GetService<IToDoUiCache>().CurrentActive;

            if (currentActive?.Parent is null)
            {
                await ServiceProvider
                    .GetService<INavigator>()
                    .NavigateToAsync(
                        ServiceProvider.GetService<IDioclesViewModelFactory>().CreateRootToDos(),
                        ct
                    );
            }
            else
            {
                await ServiceProvider
                    .GetService<INavigator>()
                    .NavigateToAsync(
                        ServiceProvider
                            .GetService<IDioclesViewModelFactory>()
                            .CreateToDos(currentActive.Parent.ActualItem),
                        ct
                    );
            }

            return response;
        });

        _openParentCommand = CreateLazyCommand<ToDoNotify>(
            (item, ct) =>
                item.Parent is null
                    ? ServiceProvider
                        .GetService<INavigator>()
                        .NavigateToAsync(
                            ServiceProvider
                                .GetService<IDioclesViewModelFactory>()
                                .CreateRootToDos(),
                            ct
                        )
                    : ServiceProvider
                        .GetService<INavigator>()
                        .NavigateToAsync(
                            ServiceProvider
                                .GetService<IDioclesViewModelFactory>()
                                .CreateToDos(item.Parent.ActualItem),
                            ct
                        )
        );

        _switchFavoriteCommand = CreateLazyCommand<ToDoNotify, HestiaPostResponse>(
            (item, ct) =>
                ServiceProvider
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
        );

        _changeOrderCommand = CreateLazyCommand<ToDoNotify, IValidationErrors>(
            async (item, ct) =>
            {
                var items = item.Parent is null
                    ? ServiceProvider.GetService<IToDoUiCache>().Roots
                    : item.Parent.Children;

                var changeOrder = await ServiceProvider
                    .GetService<IItemMutationService>()
                    .ShowChangeOrderAsync(items.ToArray(), [item], ct);

                if (changeOrder is null)
                {
                    return new DefaultValidationErrors();
                }

                return await ServiceProvider
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
        );

        _showChangeParentCommand = CreateLazyCommand<ToDoNotify>(
            (item, ct) =>
            {
                var viewModel = ServiceProvider
                    .GetService<IDioclesViewModelFactory>()
                    .CreateChangeParentToDo();

                Dispatcher.UIThread.Post(() =>
                {
                    ServiceProvider.GetService<IToDoUiCache>().ResetItems();
                    item.IsHideOnTree = true;
                });

                async ValueTask<HestiaPostResponse> ChangeParentAsync(CancellationToken ct)
                {
                    var parentId = viewModel.IsRoot ? null : viewModel.Tree.Selected?.Id;

                    await ServiceProvider.GetService<IDialogService>().CloseMessageBoxAsync(ct);

                    return await ServiceProvider
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

                return ServiceProvider
                    .GetService<IDialogService>()
                    .ShowMessageBoxAsync(
                        new(
                            ServiceProvider
                                .GetService<IStringFormater>()
                                .Format(
                                    appResourceService.GetResource<string>("Lang.ChangeParentItem"),
                                    item.Name
                                )
                                .DispatchToDialogHeader(),
                            viewModel,
                            ServiceProvider.GetService<ISafeExecuteWrapper>(),
                            new(
                                appResourceService.GetResource<string>("Lang.ChangeParent"),
                                ServiceProvider
                                    .GetService<ICommandFactory>()
                                    .CreateCommand(c => ChangeParentAsync(c).ConfigureAwait(false)),
                                null,
                                DialogButtonType.Primary
                            ),
                            ServiceProvider.GetService<IDialogService>().CancelButton
                        ),
                        ct
                    );
            }
        );

        _showChangesParentCommand = CreateLazyCommand<IEnumerable<ToDoNotify>>(
            (items, ct) =>
            {
                var selected = items.Where(x => x.IsSelected).ToArray();
                var viewModel = ServiceProvider
                    .GetService<IDioclesViewModelFactory>()
                    .CreateChangeParentToDo();

                Dispatcher.UIThread.Post(() =>
                {
                    ServiceProvider.GetService<IToDoUiCache>().ResetItems();

                    foreach (var item in selected)
                    {
                        item.IsHideOnTree = true;
                    }
                });

                async ValueTask<HestiaPostResponse> ChangesParentAsync(CancellationToken ct)
                {
                    var parentId = viewModel.IsRoot ? null : viewModel.Tree.Selected?.Id;

                    await ServiceProvider.GetService<IDialogService>().CloseMessageBoxAsync(ct);

                    return await ServiceProvider
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

                return ServiceProvider
                    .GetService<IDialogService>()
                    .ShowMessageBoxAsync(
                        new(
                            appResourceService
                                .GetResource<string>("Lang.ChangeParent")
                                .DispatchToDialogHeader(),
                            viewModel,
                            ServiceProvider.GetService<ISafeExecuteWrapper>(),
                            new(
                                appResourceService.GetResource<string>("Lang.ChangeParent"),
                                ServiceProvider
                                    .GetService<ICommandFactory>()
                                    .CreateCommand(ct =>
                                        ChangesParentAsync(ct).ConfigureAwait(false)
                                    ),
                                null,
                                DialogButtonType.Primary
                            ),
                            ServiceProvider.GetService<IDialogService>().CancelButton
                        ),
                        ct
                    );
            }
        );

        _showCloneCommand = CreateLazyCommand<ToDoNotify>(
            (item, ct) =>
            {
                var viewModel = ServiceProvider
                    .GetService<IDioclesViewModelFactory>()
                    .CreateChangeParentToDo();
                Dispatcher.UIThread.Post(() =>
                    ServiceProvider.GetService<IToDoUiCache>().ResetItems()
                );

                async ValueTask<HestiaPostResponse> CloneAsync(CancellationToken ct)
                {
                    var parentId = viewModel.IsRoot ? null : viewModel.Tree.Selected?.Id;

                    await ServiceProvider.GetService<IDialogService>().CloseMessageBoxAsync(ct);

                    return await ServiceProvider
                        .GetService<IToDoUiService>()
                        .PostAsync(
                            Guid.NewGuid(),
                            new()
                            {
                                Clones = [new() { ParentId = parentId, CloneIds = [item.Id] }],
                            },
                            ct
                        );
                }

                return ServiceProvider
                    .GetService<IDialogService>()
                    .ShowMessageBoxAsync(
                        new(
                            ServiceProvider
                                .GetService<IStringFormater>()
                                .Format(
                                    appResourceService.GetResource<string>("Lang.CloneItem"),
                                    item.Name
                                )
                                .DispatchToDialogHeader(),
                            viewModel,
                            ServiceProvider.GetService<ISafeExecuteWrapper>(),
                            new(
                                appResourceService.GetResource<string>("Lang.Clone"),
                                ServiceProvider
                                    .GetService<ICommandFactory>()
                                    .CreateCommand(ct => CloneAsync(ct).ConfigureAwait(false)),
                                null,
                                DialogButtonType.Primary
                            ),
                            ServiceProvider.GetService<IDialogService>().CancelButton
                        ),
                        ct
                    );
            }
        );

        _showClonesCommand = CreateLazyCommand<IEnumerable<ToDoNotify>>(
            (items, ct) =>
            {
                var selected = items.Where(x => x.IsSelected).ToArray();
                var viewModel = ServiceProvider
                    .GetService<IDioclesViewModelFactory>()
                    .CreateChangeParentToDo();
                Dispatcher.UIThread.Post(() =>
                    ServiceProvider.GetService<IToDoUiCache>().ResetItems()
                );

                async ValueTask<HestiaPostResponse> CloneAsync(CancellationToken ct)
                {
                    var parentId = viewModel.IsRoot ? null : viewModel.Tree.Selected?.Id;

                    await ServiceProvider.GetService<IDialogService>().CloseMessageBoxAsync(ct);

                    return await ServiceProvider
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

                return ServiceProvider
                    .GetService<IDialogService>()
                    .ShowMessageBoxAsync(
                        new(
                            appResourceService
                                .GetResource<string>("Lang.Clone")
                                .DispatchToDialogHeader(),
                            viewModel,
                            ServiceProvider.GetService<ISafeExecuteWrapper>(),
                            new(
                                appResourceService.GetResource<string>("Lang.Clone"),
                                ServiceProvider
                                    .GetService<ICommandFactory>()
                                    .CreateCommand(c => CloneAsync(c).ConfigureAwait(false)),
                                null,
                                DialogButtonType.Primary
                            ),
                            ServiceProvider.GetService<IDialogService>().CancelButton
                        ),
                        ct
                    );
            }
        );

        _showEditToDoCommand = CreateLazyCommand<ToDoNotify>(
            (item, ct) =>
            {
                Dispatcher.UIThread.Post(() =>
                {
                    ServiceProvider.GetService<IToDoUiCache>().ResetItems();
                    item.IsHideOnTree = true;
                });

                var edit = ServiceProvider
                    .GetService<IDioclesViewModelFactory>()
                    .CreateToDoParameters(item, ValidationMode.ValidateOnlyEdited, false);

                return ServiceProvider
                    .GetService<IDialogService>()
                    .ShowMessageBoxAsync(
                        new(
                            ServiceProvider
                                .GetService<IStringFormater>()
                                .Format(
                                    appResourceService.GetResource<string>("Lang.EditItem"),
                                    item.Name
                                )
                                .DispatchToDialogHeader(),
                            edit,
                            ServiceProvider.GetService<ISafeExecuteWrapper>(),
                            new(
                                appResourceService.GetResource<string>("Lang.Edit"),
                                edit.EditItemCommand,
                                item,
                                DialogButtonType.Primary
                            ),
                            ServiceProvider.GetService<IDialogService>().CancelButton
                        ),
                        ct
                    );
            }
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
