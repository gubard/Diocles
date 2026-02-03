using Avalonia.Controls;
using Inanna.Helpers;

namespace Diocles.Ui;

public partial class ToDoParametersView : UserControl
{
    public ToDoParametersView()
    {
        InitializeComponent();
        Loaded += (_, _) => NameTextBox.FocusCaretIndex();
    }
}
