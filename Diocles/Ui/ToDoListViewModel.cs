using System.ComponentModel;
using Avalonia.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using Diocles.Models;
using Hestia.Contract.Models;
using Inanna.Models;

namespace Diocles.Ui;

public partial class ToDoListViewModel : ViewModelBase
{
    [ObservableProperty]
    private ToDoGroupBy _groupBy;

    [ObservableProperty]
    private ToDoOrderBy _orderBy;
    private readonly IAvaloniaReadOnlyList<ToDoNotify> _items;

    public ToDoListViewModel(IAvaloniaReadOnlyList<ToDoNotify> items)
    {
        _items = items;
        items.CollectionChanged += (_, _) => Refresh();
    }

    public IEnumerable<ToDoNotify> Items =>
        OrderBy switch
        {
            ToDoOrderBy.OrderIndex => _items,
            ToDoOrderBy.Name => _items.OrderBy(x => x.Name),
            ToDoOrderBy.DueDate => _items.OrderBy(x => x.DueDate),
            _ => throw new ArgumentOutOfRangeException(),
        };

    public int ItemsCount => _items.Count;
    public IEnumerable<ToDoNotify> Circle => Items.Where(x => x.Type == ToDoType.Circle);
    public int CircleCount => Circle.Count();
    public IEnumerable<ToDoNotify> Group => Items.Where(x => x.Type == ToDoType.Group);
    public int GroupCount => Group.Count();
    public IEnumerable<ToDoNotify> Periodicity => Items.Where(x => x.Type == ToDoType.Periodicity);
    public int PeriodicityCount => Periodicity.Count();
    public IEnumerable<ToDoNotify> PeriodicityOffset =>
        Items.Where(x => x.Type == ToDoType.PeriodicityOffset);
    public int PeriodicityOffsetCount => PeriodicityOffset.Count();
    public IEnumerable<ToDoNotify> FixedDate => Items.Where(x => x.Type == ToDoType.FixedDate);
    public int FixedDateCount => FixedDate.Count();
    public IEnumerable<ToDoNotify> Reference => Items.Where(x => x.Type == ToDoType.Reference);
    public int ReferenceCount => Reference.Count();
    public IEnumerable<ToDoNotify> Step => Items.Where(x => x.Type == ToDoType.Step);
    public int StepCount => Step.Count();
    public IEnumerable<ToDoNotify> Value => Items.Where(x => x.Type == ToDoType.Value);
    public int ValueCount => Value.Count();
    public IEnumerable<ToDoNotify> ComingSoon =>
        Items.Where(x => x.Status == ToDoStatus.ComingSoon);
    public int ComingSoonCount => ComingSoon.Count();
    public IEnumerable<ToDoNotify> Planned => Items.Where(x => x.Status == ToDoStatus.Planned);
    public int PlannedCount => Planned.Count();
    public IEnumerable<ToDoNotify> Completed => Items.Where(x => x.Status == ToDoStatus.Completed);
    public int CompletedCount => Completed.Count();
    public IEnumerable<ToDoNotify> Miss => Items.Where(x => x.Status == ToDoStatus.Miss);
    public int MissCount => Miss.Count();
    public IEnumerable<ToDoNotify> ReadyForComplete =>
        Items.Where(x => x.Status == ToDoStatus.ReadyForComplete);
    public int ReadyForCompleteCount => ReadyForComplete.Count();

    public void Refresh()
    {
        OnPropertyChanged(nameof(Items));
        OnPropertyChanged(nameof(ItemsCount));
        OnPropertyChanged(nameof(Circle));
        OnPropertyChanged(nameof(CircleCount));
        OnPropertyChanged(nameof(Group));
        OnPropertyChanged(nameof(GroupCount));
        OnPropertyChanged(nameof(Periodicity));
        OnPropertyChanged(nameof(PeriodicityCount));
        OnPropertyChanged(nameof(PeriodicityOffset));
        OnPropertyChanged(nameof(PeriodicityOffsetCount));
        OnPropertyChanged(nameof(FixedDate));
        OnPropertyChanged(nameof(FixedDateCount));
        OnPropertyChanged(nameof(Reference));
        OnPropertyChanged(nameof(ReferenceCount));
        OnPropertyChanged(nameof(Step));
        OnPropertyChanged(nameof(StepCount));
        OnPropertyChanged(nameof(Value));
        OnPropertyChanged(nameof(ValueCount));
        OnPropertyChanged(nameof(Miss));
        OnPropertyChanged(nameof(MissCount));
        OnPropertyChanged(nameof(ReadyForComplete));
        OnPropertyChanged(nameof(ReadyForCompleteCount));
        OnPropertyChanged(nameof(Planned));
        OnPropertyChanged(nameof(PlannedCount));
        OnPropertyChanged(nameof(Completed));
        OnPropertyChanged(nameof(CompletedCount));
        OnPropertyChanged(nameof(ComingSoon));
        OnPropertyChanged(nameof(ComingSoonCount));
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.PropertyName == nameof(OrderBy))
        {
            Refresh();
        }
    }
}
