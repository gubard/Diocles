using Avalonia.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Diocles.Models;
using Diocles.Services;
using Gaia.Services;
using Inanna.Models;

namespace Diocles.Ui;

public partial class ToDosHeaderViewModel : ViewModelBase, IIsMulti
{
    public ToDosHeaderViewModel(
        ToDoNotify item,
        AvaloniaList<InannaCommand> commands,
        AppState appState,
        IUiToDoService uiToDoService
    )
    {
        Item = item;
        _commands = commands;
        _appState = appState;
        _uiToDoService = uiToDoService;
    }

    public ToDoNotify Item { get; }
    public IEnumerable<InannaCommand> Commands => _commands;
    public bool IsOffline => _appState.GetServiceMode(nameof(UiToDoService)) == ServiceMode.Offline;

    public void RefreshUi()
    {
        OnPropertyChanged(nameof(IsOffline));
    }

    [ObservableProperty]
    private bool _isMulti;

    private readonly AvaloniaList<InannaCommand> _commands;
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
