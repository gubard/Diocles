using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Diocles.Helpers;
using Diocles.Services;
using Gaia.Helpers;
using Gaia.Services;
using Hestia.Contract.Models;
using Inanna.Models;
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
        IToDoUiService toDoUiService,
        IToDoUiCache toDoUiCache,
        IDialogService dialogService,
        IAppResourceService appResourceService,
        IStringFormater stringFormater
    )
        : base(
            dialogService,
            appResourceService,
            stringFormater,
            factory,
            toDoUiService,
            toDoUiCache,
            toDoUiCache.Search
        )
    {
        _header = factory.CreateToDosHeader(
            appResourceService.GetResource<string>("Lang.SearchToDos"),
            new AvaloniaList<InannaCommand>(),
            DiocleHelper.CreateMultiCommands(toDoUiCache.Search)
        );

        _toDoUiService = toDoUiService;
        _types = new();
    }

    public object Header => _header;
    public IEnumerable<ToDoType> Types => _types;

    public ConfiguredValueTaskAwaitable RefreshAsync(CancellationToken ct)
    {
        return WrapCommandAsync(() => SearchCore(ct).ConfigureAwait(false), ct);
    }

    public ConfiguredValueTaskAwaitable SaveUiAsync(CancellationToken ct)
    {
        _header.PropertyChanged -= HeaderPropertyChanged;

        return List.SaveUiAsync(ct);
    }

    public ConfiguredValueTaskAwaitable InitUiAsync(CancellationToken ct)
    {
        _header.PropertyChanged += HeaderPropertyChanged;

        return List.InitUiAsync(ct);
    }

    private readonly IToDoUiService _toDoUiService;
    private readonly ToDosHeaderViewModel _header;
    private readonly AvaloniaList<ToDoType> _types;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [RelayCommand]
    private async Task SearchAsync(CancellationToken ct)
    {
        await WrapCommandAsync(() => SearchCore(ct).ConfigureAwait(false), ct);
    }

    private void HeaderPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ToDosHeaderViewModel.IsMulti))
        {
            IsMulti = _header.IsMulti;
        }
    }

    private async ValueTask<HestiaGetResponse> SearchCore(CancellationToken ct)
    {
        var response = await _toDoUiService.GetAsync(
            new()
            {
                Search = new() { SearchText = SearchText, Types = _types.ToArray() },
            },
            ct
        );

        return response;
    }
}
