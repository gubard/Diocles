using System.Windows.Input;
using Avalonia.Collections;
using Avalonia.Controls;
using Inanna.Models;
using Inanna.Services;

namespace Diocles.Ui;

public class RootToDosViewModel : ViewModelBase, IHeader
{
    private readonly IAppResourceService _appResourceService;

    public RootToDosViewModel(IAppResourceService appResourceService, ToDoSubItemsViewModel toDoSubItemsViewModel)
    {
        _appResourceService = appResourceService;
        ToDoSubItemsViewModel = toDoSubItemsViewModel;
        Commands = new();
    }

    public AvaloniaList<SpravaCommand> Commands { get; }
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
}