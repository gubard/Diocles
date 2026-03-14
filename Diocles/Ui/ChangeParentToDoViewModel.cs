using CommunityToolkit.Mvvm.ComponentModel;
using Diocles.Services;
using Inanna.Models;
using Inanna.Services;

namespace Diocles.Ui;

public sealed partial class ChangeParentToDoViewModel : ViewModelBase
{
    public ChangeParentToDoViewModel(
        IDioclesViewModelFactory factory,
        ISafeExecuteWrapper safeExecuteWrapper
    )
        : base(safeExecuteWrapper)
    {
        Tree = factory.CreateToDoTree();
    }

    public ToDoTreeViewModel Tree { get; }

    [ObservableProperty]
    private bool _isRoot;
}
