using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia.Collections;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Diocles.Models;
using Diocles.Services;
using Gaia.Helpers;
using Gaia.Services;
using Hestia.Contract.Models;
using Inanna.Models;
using Inanna.Services;
using Weber.Services;

namespace Diocles.Ui;

public sealed partial class SearchToDoViewModel
    : ToDosViewModelBase,
        IHeader,
        IRefresh,
        ISave,
        IInit,
        IRefreshUi
{
    public SearchToDoViewModel(
        IDioclesViewModelFactory factory,
        IToDoUiService toDoUiService,
        IToDoUiCache toDoUiCache,
        IDialogService dialogService,
        IAppResourceService appResourceService,
        IStringFormater stringFormater,
        IFileStorageUiService fileStorageUiService,
        IObjectStorage objectStorage,
        DioclesCommands dioclesCommands,
        ISafeExecuteWrapper safeExecuteWrapper
    )
        : base(
            dialogService,
            appResourceService,
            stringFormater,
            factory,
            toDoUiService,
            toDoUiCache.Search,
            fileStorageUiService,
            safeExecuteWrapper
        )
    {
        _header = factory.CreateToDosHeader(
            appResourceService.GetResource<string>("Lang.SearchToDos"),
            new AvaloniaList<InannaCommand>(),
            dioclesCommands.CreateMultiCommands(toDoUiCache.Search)
        );

        _toDoUiService = toDoUiService;
        _objectStorage = objectStorage;
        _types = new();
    }

    public object Header => _header;
    public IEnumerable<ToDoType> Types => _types;

    public ConfiguredValueTaskAwaitable RefreshAsync(CancellationToken ct)
    {
        return WrapCommandAsync(() => SearchCore(ct).ConfigureAwait(false), ct);
    }

    public ConfiguredValueTaskAwaitable SaveAsync(CancellationToken ct)
    {
        _header.PropertyChanged -= HeaderPropertyChanged;

        return WrapCommandAsync(
            async () =>
            {
                await List.SaveAsync(ct);

                await _objectStorage.SaveAsync(
                    new SearchToDoSettings { SearchText = SearchText, Types = _types.ToArray() },
                    ct
                );
            },
            ct
        );
    }

    public ConfiguredValueTaskAwaitable InitAsync(CancellationToken ct)
    {
        _header.PropertyChanged += HeaderPropertyChanged;

        return WrapCommandAsync(
            async () =>
            {
                var settings = await _objectStorage.LoadAsync<SearchToDoSettings>(ct);

                Dispatcher.UIThread.Post(() =>
                {
                    SearchText = settings.SearchText;
                    _types.AddRange(settings.Types);
                });
            },
            ct
        );
    }

    public void RefreshUi()
    {
        List.RefreshUi();
    }

    private readonly IObjectStorage _objectStorage;
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
