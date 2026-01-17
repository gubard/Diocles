using Avalonia.Controls;
using Diocles.Models;

namespace Diocles.Ui;

public partial class RootToDosView : UserControl, IToDosView
{
    public RootToDosView()
    {
        InitializeComponent();
    }

    public IToDosViewModel ViewModel =>
        DataContext as IToDosViewModel ?? throw new InvalidOperationException();
}
