using Avalonia.Controls;
using Diocles.Models;

namespace Diocles.Ui;

public partial class ToDosView : UserControl, IToDosView
{
    public ToDosView()
    {
        InitializeComponent();
    }

    public IToDosViewModel ViewModel =>
        DataContext as IToDosViewModel ?? throw new InvalidOperationException();
}
