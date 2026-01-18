using System.Runtime.CompilerServices;
using Avalonia.Collections;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Diocles.Models;
using Diocles.Services;
using Gaia.Services;
using Hestia.Contract.Models;
using Inanna.Helpers;
using Inanna.Models;
using Inanna.Services;

namespace Diocles.Ui;

public abstract class ToDosMainViewModelBase : ToDosViewModelBase, IRefresh, IRefreshUi
{
    public ConfiguredValueTaskAwaitable RefreshAsync(CancellationToken ct)
    {
        return WrapCommandAsync(() => ToDoUiService.GetAsync(CreateRefreshRequest(), ct), ct);
    }

    public virtual void RefreshUi()
    {
        List.Refresh();
    }

    protected ToDosMainViewModelBase(
        IDialogService dialogService,
        IAppResourceService appResourceService,
        IStringFormater stringFormater,
        IDioclesViewModelFactory factory,
        IToDoUiService toDoUiService,
        IToDoUiCache toDoUiCache,
        IAvaloniaReadOnlyList<ToDoNotify> items
    )
        : base(
            dialogService,
            appResourceService,
            stringFormater,
            factory,
            toDoUiService,
            toDoUiCache,
            items
        ) { }

    protected abstract HestiaGetRequest CreateRefreshRequest();
}

public abstract partial class ToDosViewModelBase : ViewModelBase, IToDosViewModel
{
    public ToDoListViewModel List { get; }

    protected readonly IDialogService DialogService;
    protected readonly IAppResourceService AppResourceService;
    protected readonly IStringFormater StringFormater;
    protected readonly IDioclesViewModelFactory Factory;
    protected readonly IToDoUiService ToDoUiService;
    protected readonly IToDoUiCache ToDoUiCache;

    protected ToDosViewModelBase(
        IDialogService dialogService,
        IAppResourceService appResourceService,
        IStringFormater stringFormater,
        IDioclesViewModelFactory factory,
        IToDoUiService toDoUiService,
        IToDoUiCache toDoUiCache,
        IAvaloniaReadOnlyList<ToDoNotify> items
    )
    {
        DialogService = dialogService;
        AppResourceService = appResourceService;
        StringFormater = stringFormater;
        Factory = factory;
        ToDoUiService = toDoUiService;
        ToDoUiCache = toDoUiCache;
        List = factory.CreateToDoList(items);
    }

    private ToDoNotify? _editItem;

    [ObservableProperty]
    private bool _isMulti;

    [RelayCommand]
    private async Task ShowEditAsync(ToDoNotify item, CancellationToken ct)
    {
        await WrapCommandAsync(
            () =>
            {
                Dispatcher.UIThread.Post(() =>
                {
                    ToDoUiCache.ResetItems();
                    item.IsHideOnTree = true;
                });

                var edit = Factory.CreateToDoParameters(
                    item,
                    ValidationMode.ValidateOnlyEdited,
                    false
                );

                _editItem = item;

                return DialogService.ShowMessageBoxAsync(
                    new(
                        StringFormater
                            .Format(
                                AppResourceService.GetResource<string>("Lang.EditItem"),
                                item.Name
                            )
                            .DispatchToDialogHeader(),
                        edit,
                        new(
                            AppResourceService.GetResource<string>("Lang.Edit"),
                            EditCommand,
                            edit,
                            DialogButtonType.Primary
                        ),
                        UiHelper.CancelButton
                    ),
                    ct
                );
            },
            ct
        );
    }

    [RelayCommand]
    private async Task EditAsync(ToDoParametersViewModel viewModel, CancellationToken ct)
    {
        await WrapCommandAsync(() => EditCore(viewModel, ct).ConfigureAwait(false), ct);
    }

    private async ValueTask<IValidationErrors> EditCore(
        ToDoParametersViewModel viewModel,
        CancellationToken ct
    )
    {
        if (_editItem is null)
        {
            DialogService.DispatchCloseMessageBox();

            return new EmptyValidationErrors();
        }

        var edit = viewModel.CreateEditToDos(_editItem.Id);
        DialogService.DispatchCloseMessageBox();

        return await ToDoUiService.PostAsync(Guid.NewGuid(), new() { Edits = [edit] }, ct);
    }
}
