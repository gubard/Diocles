using CommunityToolkit.Mvvm.Input;
using Diocles.Models;
using Diocles.Services;
using Gaia.Services;
using Inanna.Helpers;
using Inanna.Models;
using Inanna.Services;

namespace Diocles.Ui;

public partial class ToDosViewModel : ViewModelBase, IHeader, IRefresh
{
    private readonly IUiToDoService _uiToDoService;
    private readonly IStringFormater _stringFormater;
    private readonly IDialogService _dialogService;
    private readonly IAppResourceService _appResourceService;
    private readonly IDioclesViewModelFactory _dioclesViewModelFactory;

    public ToDosViewModel(
        ToDoNotify item,
        IUiToDoService uiToDoService,
        IStringFormater stringFormater,
        IDialogService dialogService,
        IAppResourceService appResourceService,
        IDioclesViewModelFactory dioclesViewModelFactory
    )
    {
        List = new(item.Children);
        _uiToDoService = uiToDoService;
        _stringFormater = stringFormater;
        _dialogService = dialogService;
        _appResourceService = appResourceService;
        _dioclesViewModelFactory = dioclesViewModelFactory;
        Header = new(item, []);
    }

    public ToDoListViewModel List { get; }
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
        var credential = _dioclesViewModelFactory.Create((ValidationMode.ValidateAll, false));

        await WrapCommandAsync(
            () =>
                _dialogService.ShowMessageBoxAsync(
                    new(
                        _stringFormater.Format(
                            _appResourceService.GetResource<string>("Lang.CreatingNewItem"),
                            _appResourceService.GetResource<string>("Lang.ToDo")
                        ),
                        credential,
                        new DialogButton(
                            _appResourceService.GetResource<string>("Lang.Create"),
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
        await WrapCommandAsync(
            async () =>
            {
                parameters.StartExecute();

                if (parameters.HasErrors)
                {
                    return (IValidationErrors)EmptyValidationErrors.Instance;
                }

                var create = parameters.CreateShortToDo();
                create.ParentId = Header.Item.Id;

                var response = await _uiToDoService.PostAsync(new() { Creates = [create] }, ct);

                _dialogService.CloseMessageBox();

                return response;
            },
            ct
        );
    }

    public ValueTask RefreshAsync(CancellationToken ct)
    {
        return WrapCommandAsync(
            async () =>
            {
                var response = await _uiToDoService.GetAsync(
                    new() { ChildrenIds = [Header.Item.Id], ParentIds = [Header.Item.Id] },
                    ct
                );

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
            var response = _uiToDoService.Get(
                new() { ChildrenIds = [Header.Item.Id], ParentIds = [Header.Item.Id] }
            );

            List.Refresh();

            return response;
        });
    }
}
