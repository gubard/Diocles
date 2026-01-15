using Avalonia.Controls;
using Diocles.Models;

namespace Diocles.Ui;

public partial class RootToDosView : UserControl, IToDosView
{
    public RootToDosView()
    {
        InitializeComponent();
    }

    public ToDosViewModelBase ViewModel =>
        DataContext as ToDosViewModelBase ?? throw new InvalidOperationException();
}
