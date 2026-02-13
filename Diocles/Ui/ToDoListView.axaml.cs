using Avalonia.Controls;

namespace Diocles.Ui;

public sealed partial class ToDoListView : UserControl
{
    public ToDoListView()
    {
        InitializeComponent();
    }

    public ToDoListViewModel ViewModel =>
        DataContext as ToDoListViewModel ?? throw new InvalidOperationException();
}
