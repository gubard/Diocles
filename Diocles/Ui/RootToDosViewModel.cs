using Avalonia.Collections;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using Diocles.Services;
using Inanna.Helpers;
using Inanna.Models;
using Inanna.Services;

namespace Diocles.Ui;

public partial class RootToDosViewModel : ViewModelBase, IHeader
{
    private readonly IAppResourceService _appResourceService;
    private readonly IUiToDoService _uiToDoService;

    public RootToDosViewModel(IAppResourceService appResourceService,
        ToDoListViewModel toDoListViewModel,
        IUiToDoService uiToDoService)
    {
        _appResourceService = appResourceService;
        ToDoListViewModel = toDoListViewModel;
        _uiToDoService = uiToDoService;
        Commands = new();
    }

    public AvaloniaList<InannaCommand> Commands { get; }
    public ToDoListViewModel ToDoListViewModel { get; }

    public object Header
    {
        get => new TextBlock
        {
            Text = _appResourceService.GetResource<string>("Lang.ToDos"),
            Classes =
            {
                "h2",
            },
        };
    }

    [RelayCommand]
    private async Task InitializedAsync(CancellationToken ct)
    {
        await WrapCommand(async () =>
        {
            var response = await _uiToDoService.GetAsync(new(), ct);

            if (!await UiHelper.CheckValidationErrorsAsync(response))
            {
                return;
            }
            
            ToDoListViewModel.UpdateItems();
        });
    }
}