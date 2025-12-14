using Avalonia.Collections;
using Diocles.Models;
using Inanna.Models;

namespace Diocles.Ui;

public class ToDoListViewModel : ViewModelBase
{
    public ToDoListViewModel(IAvaloniaReadOnlyList<ToDoNotify> items)
    {
        Items = items;
    }

    public IAvaloniaReadOnlyList<ToDoNotify> Items { get; }
}