using Diocles.Controls;
using Diocles.Models;

namespace Diocles.Ui;

public partial class ToDoItemView : ToDoDropUserControl, IToDosView
{
    public ToDoItemView()
    {
        InitializeComponent();
    }

    public ToDoItemViewModel ViewModel =>
        DataContext as ToDoItemViewModel ?? throw new InvalidOperationException();

    IToDosViewModel IToDosView.ViewModel =>
        DataContext as IToDosViewModel ?? throw new InvalidOperationException();
}
