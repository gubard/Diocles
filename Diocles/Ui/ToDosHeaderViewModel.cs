using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Diocles.Services;
using Gaia.Services;
using Inanna.Models;

namespace Diocles.Ui;

public partial class ToDosHeaderViewModel : ViewModelBase
{
    public ToDosHeaderViewModel(
        string title,
        IEnumerable<InannaCommand> commands,
        IEnumerable<InannaCommand> multiCommands,
        AppState appState,
        IUiToDoService uiToDoService
    )
    {
        _title = title;
        Commands = commands;
        _appState = appState;
        _uiToDoService = uiToDoService;
        MultiCommands = multiCommands;
    }

    public IEnumerable<InannaCommand> Commands { get; }
    public IEnumerable<InannaCommand> MultiCommands { get; }
    public bool IsOffline => _appState.GetServiceMode(nameof(UiToDoService)) == ServiceMode.Offline;

    public void RefreshUi()
    {
        OnPropertyChanged(nameof(IsOffline));
    }

    [ObservableProperty]
    private bool _isMulti;

    [ObservableProperty]
    private string _title;

    private readonly AppState _appState;
    private readonly IUiToDoService _uiToDoService;

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
