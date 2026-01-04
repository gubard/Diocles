using CommunityToolkit.Mvvm.Input;
using Diocles.Models;
using Diocles.Services;
using Gaia.Services;
using Hestia.Contract.Models;
using Inanna.Helpers;
using Inanna.Models;
using Inanna.Services;
using Inanna.Ui;

namespace Diocles.Ui;

public partial class ToDosViewModel : ToDosViewModelBase, IHeader
{
    public ToDosViewModel(
        ToDoNotify item,
        IUiToDoService uiToDoService,
        IStringFormater stringFormater,
        IDialogService dialogService,
        IAppResourceService appResourceService,
        IDioclesViewModelFactory factory
    )
        : base(
            dialogService,
            appResourceService,
            stringFormater,
            factory,
            uiToDoService,
            item.Children
        )
    {
        Header = new(item, []);
    }

    public ToDosHeaderViewModel Header { get; }
    object IHeader.Header => Header;

    [RelayCommand]
    private async Task InitializedAsync(CancellationToken ct)
    {
        await RefreshAsync(ct);
    }

    [RelayCommand]
    private async Task ShowCreateViewAsync(CancellationToken ct)
    {
        var credential = Factory.Create((ValidationMode.ValidateAll, false));

        await WrapCommandAsync(
            () =>
            {
                var header = StringFormater.Format(
                    AppResourceService.GetResource<string>("Lang.CreatingNewItem"),
                    AppResourceService.GetResource<string>("Lang.ToDo")
                );

                var button = new DialogButton(
                    AppResourceService.GetResource<string>("Lang.Create"),
                    CreateCommand,
                    credential,
                    DialogButtonType.Primary
                );

                var dialog = new DialogViewModel(header, credential, button, UiHelper.CancelButton);

                return DialogService.ShowMessageBoxAsync(dialog, ct);
            },
            ct
        );
    }

    [RelayCommand]
    private async Task CreateAsync(ToDoParametersViewModel parameters, CancellationToken ct)
    {
        await WrapCommandAsync(() => CreateCore(parameters, ct).ConfigureAwait(false), ct);
    }

    private async ValueTask<IValidationErrors> CreateCore(
        ToDoParametersViewModel parameters,
        CancellationToken ct
    )
    {
        parameters.StartExecute();

        if (parameters.HasErrors)
        {
            return new EmptyValidationErrors();
        }

        var create = parameters.CreateShortToDo();
        create.ParentId = Header.Item.Id;
        var response = await UiToDoService.PostAsync(new() { Creates = [create] }, ct);
        DialogService.CloseMessageBox();

        return response;
    }

    protected override HestiaGetRequest CreateRefreshRequest()
    {
        return new() { ChildrenIds = [Header.Item.Id], ParentIds = [Header.Item.Id] };
    }
}
