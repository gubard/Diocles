using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.ComponentModel;
using Diocles.Models;
using Diocles.Services;
using Inanna.Models;
using Inanna.Services;

namespace Diocles.Ui;

public sealed partial class ToDoTreeViewModel : ViewModelBase, IInit
{
    public ToDoTreeViewModel(IToDoUiCache toDoUiCache, IToDoUiService toDoUiService)
    {
        _toDoUiService = toDoUiService;
        Roots = toDoUiCache.Roots;
        _selected = Roots.FirstOrDefault();
    }

    public IEnumerable<ToDoNotify> Roots { get; }

    public ConfiguredValueTaskAwaitable InitAsync(CancellationToken ct)
    {
        return WrapCommandAsync(
            () => _toDoUiService.GetAsync(new() { IsGetSelectors = true }, ct),
            ct
        );
    }

    private readonly IToDoUiService _toDoUiService;

    [ObservableProperty]
    private ToDoNotify? _selected;
}
