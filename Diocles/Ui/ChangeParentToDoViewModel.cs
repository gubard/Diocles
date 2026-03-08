using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.ComponentModel;
using Diocles.Services;
using Inanna.Models;
using Inanna.Services;

namespace Diocles.Ui;

public sealed partial class ChangeParentToDoViewModel : ViewModelBase, IInit
{
    public ChangeParentToDoViewModel(IDioclesViewModelFactory factory)
    {
        Tree = factory.CreateToDoTree();
    }

    public ToDoTreeViewModel Tree { get; }

    [ObservableProperty]
    private bool _isRoot;

    public ConfiguredValueTaskAwaitable InitAsync(CancellationToken ct)
    {
        return Tree.InitAsync(ct);
    }
}
