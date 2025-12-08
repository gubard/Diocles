using System.Threading;
using System.Threading.Tasks;
using Avalonia.Collections;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using Inanna.Models;
using Inanna.Services;

namespace Diocles.Ui;

public partial class RootToDosViewModel : ViewModelBase, IHeader
{
    private readonly IAppResourceService _appResourceService;

    public RootToDosViewModel(IAppResourceService appResourceService,
        ToDoSubItemsViewModel toDoSubItemsViewModel)
    {
        _appResourceService = appResourceService;
        ToDoSubItemsViewModel = toDoSubItemsViewModel;
        Commands = new();
    }

    public AvaloniaList<InannaCommand> Commands { get; }
    public ToDoSubItemsViewModel ToDoSubItemsViewModel { get; }

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
    private async Task InitializedAsync(CancellationToken cancellationToken)
    {
    }
}