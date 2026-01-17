using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia.Collections;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Diocles.Helpers;
using Diocles.Models;
using Diocles.Services;
using Gaia.Helpers;
using Gaia.Services;
using Hestia.Contract.Models;
using Inanna.Helpers;
using Inanna.Services;

namespace Diocles.Ui;

public sealed partial class SearchToDoViewModel
    : ToDosViewModelBase,
        IHeader,
        IRefresh,
        ISaveUi,
        IInitUi
{
    public SearchToDoViewModel(
        IDioclesViewModelFactory factory,
        IUiToDoService uiToDoService,
        IToDoUiCache toDoUiCache,
        IDialogService dialogService,
        IAppResourceService appResourceService,
        IStringFormater stringFormater
    )
        : base(dialogService, appResourceService, stringFormater, factory, uiToDoService, Todos)
    {
        _header = factory.CreateToDosHeader(
            appResourceService.GetResource<string>("Lang.SearchToDos"),
            [],
            DiocleHelper.CreateMultiCommands(Todos)
        );

        _uiToDoService = uiToDoService;
        _toDoUiCache = toDoUiCache;
        Dispatcher.UIThread.Post(() => Todos.Clear());
    }

    public object Header => _header;

    public ConfiguredValueTaskAwaitable RefreshAsync(CancellationToken ct)
    {
        return WrapCommandAsync(() => SearchCore(ct).ConfigureAwait(false), ct);
    }

    public ConfiguredValueTaskAwaitable SaveUiAsync(CancellationToken ct)
    {
        _header.PropertyChanged -= HeaderChanged;

        return TaskHelper.ConfiguredCompletedTask;
    }

    public ConfiguredValueTaskAwaitable InitUiAsync(CancellationToken ct)
    {
        _header.PropertyChanged += HeaderChanged;

        return TaskHelper.ConfiguredCompletedTask;
    }

    private static readonly AvaloniaList<ToDoNotify> Todos = new();
    private readonly IUiToDoService _uiToDoService;
    private readonly IToDoUiCache _toDoUiCache;
    private readonly ToDosHeaderViewModel _header;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [RelayCommand]
    private async Task SearchAsync(CancellationToken ct)
    {
        await WrapCommandAsync(() => SearchCore(ct).ConfigureAwait(false), ct);
    }

    private void HeaderChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ToDosHeaderViewModel.IsMulti))
        {
            IsMulti = _header.IsMulti;
        }
    }

    private async ValueTask<HestiaGetResponse> SearchCore(CancellationToken ct)
    {
        var response = await _uiToDoService.GetAsync(
            new() { Search = new() { SearchText = SearchText } },
            ct
        );

        Dispatcher.UIThread.Post(() =>
            Todos.UpdateOrder(
                response.Search.Select(x => _toDoUiCache.Get(x.Parameters.Id)).ToArray()
            )
        );

        return response;
    }
}
