using Avalonia.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using Diocles.Services;
using Inanna.Models;
using Inanna.Services;
using Inanna.Ui;

namespace Diocles.Ui;

public sealed partial class ToDosHeaderViewModel : ViewModelBase
{
    public ToDosHeaderViewModel(
        string title,
        IAvaloniaReadOnlyList<InannaCommand> commands,
        IAvaloniaReadOnlyList<InannaCommand> multiCommands,
        IInannaViewModelFactory factory,
        ISafeExecuteWrapper safeExecuteWrapper,
        DioclesCommands dioclesCommands
    )
        : base(safeExecuteWrapper)
    {
        _title = title;
        DioclesCommands = dioclesCommands;
        Commands = factory.CreateAdaptiveButtons(commands);
        MultiCommands = factory.CreateAdaptiveButtons(multiCommands);
    }

    public AdaptiveButtonsViewModel Commands { get; }
    public AdaptiveButtonsViewModel MultiCommands { get; }
    public DioclesCommands DioclesCommands { get; }

    [ObservableProperty]
    private bool _isMulti;

    [ObservableProperty]
    private string _title;
}
