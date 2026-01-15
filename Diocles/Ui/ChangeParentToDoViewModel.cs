using CommunityToolkit.Mvvm.ComponentModel;
using Diocles.Services;
using Inanna.Models;

namespace Diocles.Ui;

public partial class ChangeParentToDoViewModel : ViewModelBase
{
    [ObservableProperty]
    private bool _isRoot;

    public ChangeParentToDoViewModel(IDioclesViewModelFactory factory)
    {
        Tree = factory.CreateToDoTree();
    }

    public ToDoTreeViewModel Tree { get; }
}