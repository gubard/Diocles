using Avalonia.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Diocles.Services;
using Inanna.Models;

namespace Diocles.Ui;

public sealed partial class RootToDosHeaderViewModel : ViewModelBase
{
    public RootToDosHeaderViewModel(
        IEnumerable<InannaCommand> commands,
        AppState appState,
        IUiToDoService uiToDoService
    )
    {
        _appState = appState;
        _uiToDoService = uiToDoService;
        _commands = new(commands);
    }

    public IEnumerable<InannaCommand> Commands => _commands;
    public bool IsOffline => _appState.GetServiceMode(nameof(UiToDoService)) == ServiceMode.Offline;

    private readonly AppState _appState;
    private readonly IUiToDoService _uiToDoService;
    private readonly AvaloniaList<InannaCommand> _commands;

    [ObservableProperty]
    private bool _isMulti;

    [RelayCommand]
    private async Task SwitchToOnlineAsync(CancellationToken ct)
    {
        await SwitchToOnlineCore(ct).ConfigureAwait(false);
    }

    private async ValueTask SwitchToOnlineCore(CancellationToken ct)
    {
        await _uiToDoService.HealthCheckAsync(ct);
        OnPropertyChanged(nameof(IsOffline));
    }
}
