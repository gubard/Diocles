using Avalonia.Collections;
using Diocles.Models;
using Diocles.Services;
using Inanna.Models;

namespace Diocles.Ui;

public sealed class SearchToDoViewModel : ViewModelBase
{
    private readonly AvaloniaList<ToDoNotify> _todos;

    public SearchToDoViewModel(IDioclesViewModelFactory factory)
    {
        _todos = new();
        ToDoList = factory.CreateToDoList(_todos);
    }

    public ToDoListViewModel ToDoList { get; }
}
