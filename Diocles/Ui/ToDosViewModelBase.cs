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

public abstract partial class ToDosViewModelBase : ViewModelBase, IRefresh, IRefreshUi
{
    public ToDoListViewModel List { get; }

    public ConfiguredValueTaskAwaitable RefreshAsync(CancellationToken ct)
    {
        return WrapCommandAsync(() => UiToDoService.GetAsync(CreateRefreshRequest(), ct), ct);
    }

    public virtual void RefreshUi()
    {
        List.Refresh();
    }

    protected readonly IDialogService DialogService;
    protected readonly IAppResourceService AppResourceService;
    protected readonly IStringFormater StringFormater;
    protected readonly IDioclesViewModelFactory Factory;
    protected readonly IUiToDoService UiToDoService;

    protected ToDosViewModelBase(
        IDialogService dialogService,
        IAppResourceService appResourceService,
        IStringFormater stringFormater,
        IDioclesViewModelFactory factory,
        IUiToDoService uiToDoService,
        IAvaloniaReadOnlyList<ToDoNotify> items
    )
    {
        DialogService = dialogService;
        AppResourceService = appResourceService;
        StringFormater = stringFormater;
        Factory = factory;
        UiToDoService = uiToDoService;
        List = factory.CreateToDoList(items);
    }

    protected abstract HestiaGetRequest CreateRefreshRequest();

    private ToDoNotify? _editItem;

    [ObservableProperty]
    private bool _isMulti;

    [RelayCommand]
    private async Task ShowEditAsync(ToDoNotify item, CancellationToken ct)
    {
        await WrapCommandAsync(
            () =>
            {
                var edit = Factory.CreateToDoParameters(
                    item,
                    ValidationMode.ValidateOnlyEdited,
                    false
                );
                _editItem = item;

                return DialogService.ShowMessageBoxAsync(
                    new(
                        Dispatcher.UIThread.Invoke(() =>
                            StringFormater
                                .Format(
                                    AppResourceService.GetResource<string>("Lang.EditItem"),
                                    item.Name
                                )
                                .ToDialogHeader()
                        ),
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
            return new EmptyValidationErrors();
        }

        var edit = viewModel.CreateEditToDos();
        edit.Ids = [_editItem.Id];
        var response = await UiToDoService.PostAsync(Guid.NewGuid(), new() { Edits = [edit] }, ct);
        Dispatcher.UIThread.Post(() => DialogService.CloseMessageBox());

        return response;
    }
}
