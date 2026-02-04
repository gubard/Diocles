using Avalonia.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using Inanna.Models;

namespace Diocles.Ui;

public partial class ToDosHeaderViewModel : ViewModelBase
{
    public ToDosHeaderViewModel(
        string title,
        IAvaloniaReadOnlyList<InannaCommand> commands,
        IAvaloniaReadOnlyList<InannaCommand> multiCommands
    )
    {
        _title = title;
        Commands = commands;
        MultiCommands = multiCommands;
    }

    public IAvaloniaReadOnlyList<InannaCommand> Commands { get; }
    public IAvaloniaReadOnlyList<InannaCommand> MultiCommands { get; }

    [ObservableProperty]
    private bool _isMulti;

    [ObservableProperty]
    private string _title;
}
