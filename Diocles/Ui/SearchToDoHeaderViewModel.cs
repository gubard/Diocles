using CommunityToolkit.Mvvm.ComponentModel;
using Inanna.Models;

namespace Diocles.Ui;

public sealed partial class SearchToDoHeaderViewModel : ViewModelBase
{
    public SearchToDoHeaderViewModel(IEnumerable<InannaCommand> multiCommands)
    {
        MultiCommands = multiCommands;
    }

    public IEnumerable<InannaCommand> MultiCommands { get; }

    [ObservableProperty]
    private bool _isMulti;
}
