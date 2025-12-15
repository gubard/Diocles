using Diocles.Models;
using Inanna.Models;

namespace Diocles.Ui;

public class EditToDoHeaderViewModel : ViewModelBase
{
    public EditToDoHeaderViewModel(ToDoNotify item)
    {
        Item = item;
    }

    public ToDoNotify Item { get; }
}
