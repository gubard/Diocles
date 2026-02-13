using Diocles.Controls;
using Diocles.Models;

namespace Diocles.Ui;

public sealed partial class RootToDosView : ToDoDropUserControl, IToDosView
{
    public RootToDosView()
    {
        InitializeComponent();
    }

    public IToDosViewModel ViewModel =>
        DataContext as IToDosViewModel ?? throw new InvalidOperationException();
}
