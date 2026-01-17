using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Diocles.Models;

namespace Diocles.Ui;

public partial class SearchToDoView : UserControl, IToDosView
{
    public SearchToDoView()
    {
        InitializeComponent();
    }

    public IToDosViewModel ViewModel =>
        DataContext as IToDosViewModel ?? throw new InvalidOperationException();
}
