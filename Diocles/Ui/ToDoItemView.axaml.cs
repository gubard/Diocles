using Avalonia.Controls;
using Diocles.Models;

namespace Diocles.Ui;

public partial class ToDoItemView : UserControl, IToDosView
{
    public ToDoItemView()
    {
        InitializeComponent();
    }

    public IToDosViewModel ViewModel =>
        DataContext as IToDosViewModel ?? throw new InvalidOperationException();
}
