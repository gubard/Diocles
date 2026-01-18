using Avalonia.Collections;
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
        IAvaloniaReadOnlyList<InannaCommand> commands,
        IAvaloniaReadOnlyList<InannaCommand> multiCommands,
        AppState appState,
        IToDoUiService toDoUiService
    )
    {
        _title = title;
        Commands = commands;
        _appState = appState;
        _toDoUiService = toDoUiService;
        MultiCommands = multiCommands;
    }

    public IAvaloniaReadOnlyList<InannaCommand> Commands { get; }
    public IAvaloniaReadOnlyList<InannaCommand> MultiCommands { get; }
    public bool IsOffline => _appState.GetServiceMode(nameof(ToDoUiService)) == ServiceMode.Offline;

    public void RefreshUi()
    {
        OnPropertyChanged(nameof(IsOffline));
    }

    [ObservableProperty]
    private bool _isMulti;

    [ObservableProperty]
    private string _title;

    private readonly AppState _appState;
    private readonly IToDoUiService _toDoUiService;

    [RelayCommand]
    private async Task SwitchToOnlineAsync(CancellationToken ct)
    {
        await WrapCommandAsync(() => SwitchToOnlineCore(ct).ConfigureAwait(false), ct);
    }

    private async ValueTask<IValidationErrors> SwitchToOnlineCore(CancellationToken ct)
    {
        var errors = await _toDoUiService.HealthCheckAsync(ct);

        if (errors.ValidationErrors.Count > 0)
        {
            return errors;
        }

        errors = await _toDoUiService.UpdateEventsAsync(ct);

        RefreshUi();

        return errors;
    }
}
