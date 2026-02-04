using Diocles.Controls;
using Diocles.Models;

namespace Diocles.Ui;

public partial class ToDoItemView : ToDoDropUserControl, IToDosView
{
    public ToDoItemView()
    {
        InitializeComponent();
    }

    public IToDosViewModel ViewModel =>
        DataContext as IToDosViewModel ?? throw new InvalidOperationException();

    public ToDoItemViewModel VM =>
        DataContext as ToDoItemViewModel ?? throw new InvalidOperationException();
}
