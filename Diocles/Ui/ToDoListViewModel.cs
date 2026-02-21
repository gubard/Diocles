using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using Diocles.Models;
using Diocles.Services;
using Gaia.Helpers;
using Hestia.Contract.Models;
using Inanna.Helpers;
using Inanna.Models;
using Inanna.Services;

namespace Diocles.Ui;

public sealed partial class ToDoListViewModel : ViewModelBase, IInit, ISave
{
    public ToDoListViewModel(
        IAvaloniaReadOnlyList<ToDoNotify> itemsSource,
        IToDoUiCache toDoUiCache
    )
    {
        _favorites = toDoUiCache.Favorites;
        _groupBy = ToDoGroupBy.Status;
        _itemsSource = itemsSource;
        _items = new();
        _circle = new();
        _group = new();
        _periodicity = new();
        _periodicityOffset = new();
        _fixedDate = new();
        _reference = new();
        _step = new();
        _value = new();
        _comingSoon = new();
        _planned = new();
        _completed = new();
        _miss = new();
        _readyForComplete = new();
        Refresh();
    }

    public IAvaloniaReadOnlyList<ToDoNotify> Items => _items;
    public IAvaloniaReadOnlyList<ToDoNotify> Favorites => _favorites;
    public IAvaloniaReadOnlyList<ToDoNotify> Circle => _circle;
    public IAvaloniaReadOnlyList<ToDoNotify> Group => _group;
    public IAvaloniaReadOnlyList<ToDoNotify> Periodicity => _periodicity;
    public IAvaloniaReadOnlyList<ToDoNotify> PeriodicityOffset => _periodicityOffset;
    public IAvaloniaReadOnlyList<ToDoNotify> FixedDate => _fixedDate;
    public IAvaloniaReadOnlyList<ToDoNotify> Reference => _reference;
    public IAvaloniaReadOnlyList<ToDoNotify> Step => _step;
    public IAvaloniaReadOnlyList<ToDoNotify> Value => _value;
    public IAvaloniaReadOnlyList<ToDoNotify> ComingSoon => _comingSoon;
    public IAvaloniaReadOnlyList<ToDoNotify> Planned => _planned;
    public IAvaloniaReadOnlyList<ToDoNotify> Completed => _completed;
    public IAvaloniaReadOnlyList<ToDoNotify> Miss => _miss;
    public IAvaloniaReadOnlyList<ToDoNotify> ReadyForComplete => _readyForComplete;

    public ConfiguredValueTaskAwaitable InitAsync(CancellationToken ct)
    {
        _itemsSource.CollectionChanged += ItemsSourceCollectionChanged;

        return TaskHelper.ConfiguredCompletedTask;
    }

    public ConfiguredValueTaskAwaitable SaveAsync(CancellationToken ct)
    {
        _itemsSource.CollectionChanged -= ItemsSourceCollectionChanged;

        return TaskHelper.ConfiguredCompletedTask;
    }

    public void Refresh()
    {
        _items.UpdateOrder(
            OrderBy switch
            {
                ToDoOrderBy.OrderIndex => _itemsSource.OrderBy(x => x.OrderIndex).ToArray(),
                ToDoOrderBy.Name => _itemsSource
                    .OrderBy(x => x.Name)
                    .ThenBy(x => x.OrderIndex)
                    .ToArray(),
                ToDoOrderBy.DueDate => _itemsSource
                    .OrderBy(x => x.DueDate)
                    .ThenBy(x => x.OrderIndex)
                    .ToArray(),
                _ => throw new ArgumentOutOfRangeException(nameof(OrderBy), OrderBy, null),
            }
        );

        _periodicityOffset.UpdateOrder(
            _items.Where(x => x.Type == ToDoType.PeriodicityOffset).ToArray()
        );

        _readyForComplete.UpdateOrder(
            _items.Where(x => x.Status == ToDoStatus.ReadyForComplete).ToArray()
        );

        _circle.UpdateOrder(_items.Where(x => x.Type == ToDoType.Circle).ToArray());
        _group.UpdateOrder(_items.Where(x => x.Type == ToDoType.Group).ToArray());
        _periodicity.UpdateOrder(_items.Where(x => x.Type == ToDoType.Periodicity).ToArray());
        _fixedDate.UpdateOrder(_items.Where(x => x.Type == ToDoType.FixedDate).ToArray());
        _reference.UpdateOrder(_items.Where(x => x.Type == ToDoType.Reference).ToArray());
        _step.UpdateOrder(_items.Where(x => x.Type == ToDoType.Step).ToArray());
        _value.UpdateOrder(_items.Where(x => x.Type == ToDoType.Value).ToArray());
        _comingSoon.UpdateOrder(_items.Where(x => x.Status == ToDoStatus.ComingSoon).ToArray());
        _planned.UpdateOrder(_items.Where(x => x.Status == ToDoStatus.Planned).ToArray());
        _completed.UpdateOrder(_items.Where(x => x.Status == ToDoStatus.Completed).ToArray());
        _miss.UpdateOrder(_items.Where(x => x.Status == ToDoStatus.Miss).ToArray());
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.PropertyName == nameof(OrderBy))
        {
            Refresh();
        }
    }

    [ObservableProperty]
    private ToDoGroupBy _groupBy;

    [ObservableProperty]
    private ToDoOrderBy _orderBy;

    private readonly IAvaloniaReadOnlyList<ToDoNotify> _itemsSource;
    private readonly IAvaloniaReadOnlyList<ToDoNotify> _favorites;
    private readonly AvaloniaList<ToDoNotify> _items;
    private readonly AvaloniaList<ToDoNotify> _circle;
    private readonly AvaloniaList<ToDoNotify> _group;
    private readonly AvaloniaList<ToDoNotify> _periodicity;
    private readonly AvaloniaList<ToDoNotify> _periodicityOffset;
    private readonly AvaloniaList<ToDoNotify> _fixedDate;
    private readonly AvaloniaList<ToDoNotify> _reference;
    private readonly AvaloniaList<ToDoNotify> _step;
    private readonly AvaloniaList<ToDoNotify> _value;
    private readonly AvaloniaList<ToDoNotify> _comingSoon;
    private readonly AvaloniaList<ToDoNotify> _planned;
    private readonly AvaloniaList<ToDoNotify> _completed;
    private readonly AvaloniaList<ToDoNotify> _miss;
    private readonly AvaloniaList<ToDoNotify> _readyForComplete;

    private void ItemsSourceCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        Refresh();
    }
}
