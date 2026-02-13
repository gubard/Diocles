using Avalonia.Controls;
using Inanna.Helpers;

namespace Diocles.Ui;

public sealed partial class ToDoParametersView : UserControl
{
    public ToDoParametersView()
    {
        InitializeComponent();
        Loaded += (_, _) => NameTextBox.FocusCaretIndex();
    }

    public ToDoParametersViewModel ViewModel =>
        DataContext as ToDoParametersViewModel ?? throw new InvalidOperationException();
}
