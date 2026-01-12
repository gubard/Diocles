using Avalonia.Controls;
using Diocles.Models;
using Diocles.Services;

namespace Diocles.Ui;

public partial class ToDosView : UserControl, IToDosView
{
    public ToDosView()
    {
        InitializeComponent();
    }

    public ToDosViewModelBase ViewModel =>
        DataContext as ToDosViewModelBase ?? throw new InvalidOperationException();
}
