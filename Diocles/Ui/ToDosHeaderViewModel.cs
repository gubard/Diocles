using Avalonia.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using Diocles.Models;
using Inanna.Models;

namespace Diocles.Ui;

public partial class ToDosHeaderViewModel : ViewModelBase
{
    private readonly AvaloniaList<InannaCommand> _commands;

    [ObservableProperty]
    private bool _isMulti;

    public ToDosHeaderViewModel(ToDoNotify item, AvaloniaList<InannaCommand> commands)
    {
        Item = item;
        _commands = commands;
    }

    public ToDoNotify Item { get; }
    public IEnumerable<InannaCommand> Commands => _commands;
}
