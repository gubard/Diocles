using Avalonia.Controls;
using Diocles.Models;
using Inanna.Helpers;

namespace Diocles.Ui;

public sealed partial class SearchToDoView : UserControl, IToDosView
{
    public SearchToDoView()
    {
        InitializeComponent();
        Loaded += (_, _) => SearchTextTextBox.FocusCaretIndex();
    }

    public IToDosViewModel ViewModel =>
        DataContext as IToDosViewModel ?? throw new InvalidOperationException();
}
