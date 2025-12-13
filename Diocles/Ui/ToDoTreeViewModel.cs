using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Diocles.Models;
using Diocles.Services;
using Inanna.Models;

namespace Diocles.Ui;

public partial class ToDoTreeViewModel : ViewModelBase
{
    private readonly IUiToDoService _uiToDoService;

    [ObservableProperty] private ToDoNotify? _selected;

    public ToDoTreeViewModel(IToDoCache toDoCache, IUiToDoService uiToDoService)
    {
        _uiToDoService = uiToDoService;
        Roots = toDoCache.Roots;
    }

    public IEnumerable<ToDoNotify> Roots { get; }

    [RelayCommand]
    private async Task InitializedAsync(CancellationToken ct)
    {
        await WrapCommand(() => _uiToDoService.GetAsync(new()
        {
            IsSelectors = true,
        }, ct));
    }
}