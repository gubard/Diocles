using System.Diagnostics.CodeAnalysis;
using Avalonia.Controls;
using Diocles.Models;

namespace Diocles.Ui;

public partial class ToDoListView : UserControl
{
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(ToDoGroupBy))]
    public ToDoListView()
    {
        InitializeComponent();
    }
}
