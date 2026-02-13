using Avalonia.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using Diocles.Models;
using Inanna.Models;
using Inanna.Services;
using Inanna.Ui;

namespace Diocles.Ui;

public sealed partial class ToDoItemHeaderViewModel : ViewModelBase
{
    public ToDoItemHeaderViewModel(
        IAvaloniaReadOnlyList<InannaCommand> commands,
        IAvaloniaReadOnlyList<InannaCommand> multiCommands,
        ToDoNotify item,
        IInannaViewModelFactory factory
    )
    {
        Commands = factory.CreateAdaptiveButtons(commands);
        MultiCommands = factory.CreateAdaptiveButtons(multiCommands);
        Item = item;
    }

    public AdaptiveButtonsViewModel Commands { get; }
    public AdaptiveButtonsViewModel MultiCommands { get; }
    public ToDoNotify Item { get; }

    [ObservableProperty]
    private bool _isMulti;
}
