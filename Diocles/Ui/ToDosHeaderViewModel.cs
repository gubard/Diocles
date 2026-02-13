using Avalonia.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using Inanna.Models;
using Inanna.Services;
using Inanna.Ui;

namespace Diocles.Ui;

public partial class ToDosHeaderViewModel : ViewModelBase
{
    public ToDosHeaderViewModel(
        string title,
        IAvaloniaReadOnlyList<InannaCommand> commands,
        IAvaloniaReadOnlyList<InannaCommand> multiCommands,
        IInannaViewModelFactory factory
    )
    {
        _title = title;
        Commands = factory.CreateAdaptiveButtons(commands);
        MultiCommands = factory.CreateAdaptiveButtons(multiCommands);
    }

    public AdaptiveButtonsViewModel Commands { get; }
    public AdaptiveButtonsViewModel MultiCommands { get; }

    [ObservableProperty]
    private bool _isMulti;

    [ObservableProperty]
    private string _title;
}
