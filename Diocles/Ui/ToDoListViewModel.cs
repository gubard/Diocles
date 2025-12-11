using Diocles.Models;
using Inanna.Models;

namespace Diocles.Ui;

public class ToDoListViewModel : ViewModelBase
{
    public ToDoListViewModel(IEnumerable<ToDoNotify> items)
    {
        Items = items;
    }

    public IEnumerable<ToDoNotify> Items { get; }
}