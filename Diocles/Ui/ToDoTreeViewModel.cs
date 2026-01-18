using System.ComponentModel;
using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.ComponentModel;
using Diocles.Models;
using Diocles.Services;
using Inanna.Models;
using Inanna.Services;

namespace Diocles.Ui;

public partial class ToDoTreeViewModel : ViewModelBase, IInitUi
{
    private readonly IUiToDoService _uiToDoService;

    [ObservableProperty]
    private ToDoNotify _selected;

    public ToDoTreeViewModel(IToDoMemoryCache toDoMemoryCache, IUiToDoService uiToDoService)
    {
        _uiToDoService = uiToDoService;
        Roots = toDoMemoryCache.Roots;
        _selected = Roots.First();
    }

    public IEnumerable<ToDoNotify> Roots { get; }

    public ConfiguredValueTaskAwaitable InitUiAsync(CancellationToken ct)
    {
        return WrapCommandAsync(
            () => _uiToDoService.GetAsync(new() { IsSelectors = true }, ct),
            ct
        );
    }
}
