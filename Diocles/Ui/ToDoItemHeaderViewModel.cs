using Avalonia.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using Diocles.Models;
using Inanna.Models;

namespace Diocles.Ui;

public sealed partial class ToDoItemHeaderViewModel : ViewModelBase
{
    public ToDoItemHeaderViewModel(
        IAvaloniaReadOnlyList<InannaCommand> commands,
        IAvaloniaReadOnlyList<InannaCommand> multiCommands,
        ToDoNotify item
    )
    {
        Commands = commands;
        MultiCommands = multiCommands;
        Item = item;
    }

    public IAvaloniaReadOnlyList<InannaCommand> Commands { get; }
    public IAvaloniaReadOnlyList<InannaCommand> MultiCommands { get; }
    public ToDoNotify Item { get; }

    [ObservableProperty]
    private bool _isMulti;
}
