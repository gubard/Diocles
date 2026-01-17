using System.Runtime.CompilerServices;
using Avalonia.Collections;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Diocles.Helpers;
using Diocles.Models;
using Diocles.Services;
using Gaia.Services;
using Hestia.Contract.Models;
using IconPacks.Avalonia.MaterialDesign;
using Inanna.Helpers;
using Inanna.Models;
using Inanna.Services;

namespace Diocles.Ui;

public sealed partial class SearchToDoViewModel : ToDosViewModelBase, IHeader, IRefresh
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
        var header = factory.CreateSearchToDoHeder([
            new(
                DioclesCommands.DeleteToDosCommand,
                Todos,
                PackIconMaterialDesignKind.Delete,
                ButtonType.Danger
            ),
        ]);

        _uiToDoService = uiToDoService;
        _toDoUiCache = toDoUiCache;
        Dispatcher.UIThread.Post(() => Todos.Clear());
        Header = header;

        header.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(SearchToDoHeaderViewModel.IsMulti))
            {
                IsMulti = header.IsMulti;
            }
        };
    }

    public object Header { get; }

    private static readonly AvaloniaList<ToDoNotify> Todos = new();
    private readonly IUiToDoService _uiToDoService;
    private readonly IToDoUiCache _toDoUiCache;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [RelayCommand]
    private async Task SearchAsync(CancellationToken ct)
    {
        await WrapCommandAsync(() => SearchCore(ct).ConfigureAwait(false), ct);
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

    public ConfiguredValueTaskAwaitable RefreshAsync(CancellationToken ct)
    {
        return WrapCommandAsync(() => SearchCore(ct).ConfigureAwait(false), ct);
    }
}
