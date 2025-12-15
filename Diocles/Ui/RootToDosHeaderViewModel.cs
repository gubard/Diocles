using Avalonia.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using Inanna.Models;

namespace Diocles.Ui;

public sealed partial class RootToDosHeaderViewModel : ViewModelBase
{
    private readonly AvaloniaList<InannaCommand> _commands;

    [ObservableProperty]
    private bool _isMulti;

    public RootToDosHeaderViewModel(IEnumerable<InannaCommand> commands)
    {
        _commands = new(commands);
    }

    public IEnumerable<InannaCommand> Commands => _commands;
}
