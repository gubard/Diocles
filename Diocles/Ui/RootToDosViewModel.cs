using CommunityToolkit.Mvvm.Input;
using Diocles.Services;
using Gaia.Services;
using Inanna.Helpers;
using Inanna.Models;
using Inanna.Services;

namespace Diocles.Ui;

public partial class RootToDosViewModel : ViewModelBase, IHeader
{
    private readonly IUiToDoService _uiToDoService;
    private readonly IStringFormater _stringFormater;
    private readonly IDialogService _dialogService;
    private readonly IAppResourceService _appResourceService;

    public RootToDosViewModel(IUiToDoService uiToDoService, IToDoCache toDoCache, IStringFormater stringFormater,
        IDialogService dialogService, IAppResourceService appResourceService)
    {
        List = new(toDoCache.Roots);
        _uiToDoService = uiToDoService;
        _stringFormater = stringFormater;
        _dialogService = dialogService;
        _appResourceService = appResourceService;
        Header = new([]);
    }

    public ToDoListViewModel List { get; }
    public RootToDosHeaderViewModel Header { get; }
    object IHeader.Header => Header;

    [RelayCommand]
    private async Task InitializedAsync(CancellationToken ct)
    {
        await WrapCommand(() => _uiToDoService.GetAsync(new(), ct));
    }

    [RelayCommand]
    private async Task ShowCreateViewAsync(CancellationToken ct)
    {
        var credential = new ToDoParametersViewModel(ValidationMode.ValidateAll, false);

        await WrapCommand(() => _dialogService.ShowMessageBoxAsync(new(
            _stringFormater.Format(
                _appResourceService.GetResource<string>("Lang.CreatingNewItem"),
                _appResourceService.GetResource<string>("Lang.ToDo")),
            credential,
            new DialogButton(
                _appResourceService.GetResource<string>("Lang.Create"),
                CreateCommand,
                credential, DialogButtonType.Primary), UiHelper.CancelButton)));
    }

    [RelayCommand]
    private async Task CreateAsync(
        ToDoParametersViewModel parameters,
        CancellationToken ct)
    {
        await WrapCommand(async () =>
        {
            parameters.StartExecute();

            if (parameters.HasErrors)
            {
                return (IValidationErrors)EmptyValidationErrors.Instance;
            }

            var response = await _uiToDoService.PostAsync(new()
            {
                Creates =
                [
                    parameters.CreateToDo(),
                ],
            }, ct);

            _dialogService.CloseMessageBox();

            return response;
        });
    }
}