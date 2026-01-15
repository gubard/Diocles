using Avalonia.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Diocles.Services;
using Gaia.Services;
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

    public void RefreshUi()
    {
        OnPropertyChanged(nameof(IsOffline));
    }

    private readonly AppState _appState;
    private readonly IUiToDoService _uiToDoService;
    private readonly AvaloniaList<InannaCommand> _commands;

    [ObservableProperty]
    private bool _isMulti;

    [RelayCommand]
    private async Task SwitchToOnlineAsync(CancellationToken ct)
    {
        await WrapCommandAsync(() => SwitchToOnlineCore(ct).ConfigureAwait(false), ct);
    }

    private async ValueTask<IValidationErrors> SwitchToOnlineCore(CancellationToken ct)
    {
        var errors = await _uiToDoService.HealthCheckAsync(ct);

        if (errors.ValidationErrors.Count > 0)
        {
            return errors;
        }

        errors = await _uiToDoService.UpdateEventsAsync(ct);

        RefreshUi();

        return errors;
    }
}
