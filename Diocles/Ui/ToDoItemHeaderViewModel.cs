using CommunityToolkit.Mvvm.ComponentModel;
using Diocles.Models;
using Diocles.Services;
using Inanna.Models;
using Inanna.Services;
using Inanna.Ui;

namespace Diocles.Ui;

public sealed partial class ToDoItemHeaderViewModel : ViewModelBase
{
    public ToDoItemHeaderViewModel(
        ToDoNotify item,
        IInannaViewModelFactory factory,
        ISafeExecuteWrapper safeExecuteWrapper,
        DioclesCommands dioclesCommands
    )
        : base(safeExecuteWrapper)
    {
        Commands = factory.CreateAdaptiveButtons(item.Commands);
        MultiCommands = factory.CreateAdaptiveButtons(item.MultiCommands);
        Item = item;
        DioclesCommands = dioclesCommands;
    }

    public AdaptiveButtonsViewModel Commands { get; }
    public AdaptiveButtonsViewModel MultiCommands { get; }
    public ToDoNotify Item { get; }
    public DioclesCommands DioclesCommands { get; }

    [ObservableProperty]
    private bool _isMulti;
}
