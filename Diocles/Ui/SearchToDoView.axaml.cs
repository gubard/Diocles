using Avalonia.Controls;
using Diocles.Models;

namespace Diocles.Ui;

public sealed partial class SearchToDoView : UserControl, IToDosView
{
    public SearchToDoView()
    {
        InitializeComponent();
    }

    public IToDosViewModel ViewModel =>
        DataContext as IToDosViewModel ?? throw new InvalidOperationException();
}
