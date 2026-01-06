using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using Diocles.Services;
using Gaia.Services;
using Hestia.Contract.Models;
using Inanna.Helpers;
using Inanna.Models;
using Inanna.Services;

namespace Diocles.Ui;

public partial class RootToDosViewModel : ToDosViewModelBase, IHeader
{
    public RootToDosViewModel(
        IUiToDoService uiToDoService,
        IToDoCache toDoCache,
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
            toDoCache.Roots
        )
    {
        Header = new([]);
    }

    public RootToDosHeaderViewModel Header { get; }
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
                DialogService.ShowMessageBoxAsync(
                    new(
                        StringFormater.Format(
                            AppResourceService.GetResource<string>("Lang.CreatingNewItem"),
                            AppResourceService.GetResource<string>("Lang.ToDo")
                        ),
                        credential,
                        new DialogButton(
                            AppResourceService.GetResource<string>("Lang.Create"),
                            CreateCommand,
                            credential,
                            DialogButtonType.Primary
                        ),
                        UiHelper.CancelButton
                    ),
                    ct
                ),
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

        var request = new HestiaPostRequest { Creates = [parameters.CreateShortToDo()] };
        var response = await UiToDoService.PostAsync(Guid.NewGuid(), request, ct);
        Dispatcher.UIThread.Post(() => DialogService.CloseMessageBox());

        return response;
    }

    protected override HestiaGetRequest CreateRefreshRequest()
    {
        return new() { IsRoots = true };
    }
}
