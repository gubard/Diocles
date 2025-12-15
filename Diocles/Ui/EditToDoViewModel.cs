using Diocles.Models;
using Diocles.Services;
using Inanna.Models;

namespace Diocles.Ui;

public class EditToDoViewModel : ViewModelBase
{
    public EditToDoViewModel(ToDoNotify item, IDioclesViewModelFactory factory)
    {
        Parameters = factory.Create((item, ValidationMode.ValidateOnlyEdited, false));
    }

    private ToDoParametersViewModel Parameters { get; }
}
