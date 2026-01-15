using Avalonia.Controls;

namespace Diocles.Ui;

public partial class ToDoParametersView : UserControl
{
    public ToDoParametersView()
    {
        InitializeComponent();
        Loaded += (_, _) => NameTextBox.Focus();
    }
}
