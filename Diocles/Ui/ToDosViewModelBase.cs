using Avalonia.Collections;
using CommunityToolkit.Mvvm.Input;
using Diocles.Models;
using Diocles.Services;
using Gaia.Services;
using Hestia.Contract.Models;
using Inanna.Helpers;
using Inanna.Models;
using Inanna.Services;

namespace Diocles.Ui;

public abstract partial class ToDosViewModelBase : ViewModelBase, IRefresh
{
    private ToDoNotify? _editItem;
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
        List = factory.Create(items);
    }

    public ToDoListViewModel List { get; }

    protected abstract HestiaGetRequest CreateRefreshRequest();

    [RelayCommand]
    private async Task ShowEditAsync(ToDoNotify item, CancellationToken ct)
    {
        await WrapCommandAsync(
            () =>
            {
                var edit = Factory.Create((item, ValidationMode.ValidateOnlyEdited, false));
                _editItem = item;

                return DialogService.ShowMessageBoxAsync(
                    new(
                        StringFormater.Format(
                            AppResourceService.GetResource<string>("Lang.EditItem"),
                            item.Name
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
        await WrapCommandAsync(
            async () =>
            {
                if (_editItem is null)
                {
                    return;
                }

                var edit = viewModel.CreateEditToDos();
                edit.Ids = [_editItem.Id];
                await UiToDoService.PostAsync(new() { Edits = [edit] }, ct);
                DialogService.CloseMessageBox();
            },
            ct
        );
    }

    public ValueTask RefreshAsync(CancellationToken ct)
    {
        return WrapCommandAsync(
            async () =>
            {
                var response = await UiToDoService.GetAsync(CreateRefreshRequest(), ct);
                List.Refresh();

                return response;
            },
            ct
        );
    }

    public void Refresh()
    {
        WrapCommand(() =>
        {
            var response = UiToDoService.Get(CreateRefreshRequest());
            List.Refresh();

            return response;
        });
    }
}
