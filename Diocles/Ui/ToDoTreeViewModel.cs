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
    private ToDoNotify? _selected;

    public ToDoTreeViewModel(IToDoCache toDoCache, IUiToDoService uiToDoService)
    {
        _uiToDoService = uiToDoService;
        Roots = toDoCache.Roots;
    }

    public IEnumerable<ToDoNotify> Roots { get; }

    public ConfiguredValueTaskAwaitable InitAsync(CancellationToken ct)
    {
        return WrapCommandAsync(
            () => _uiToDoService.GetAsync(new() { IsSelectors = true }, ct),
            ct
        );
    }
}
