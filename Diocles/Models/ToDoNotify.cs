using Avalonia.Collections;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using Gaia.Helpers;
using Gaia.Models;
using Gaia.Services;
using Hestia.Contract.Models;
using Hestia.Contract.Services;
using IconPacks.Avalonia.MaterialDesign;
using Inanna.Helpers;
using Inanna.Models;

namespace Diocles.Models;

public partial class ToDoNotify
    : ObservableObject,
        IToDo,
        IStaticFactory<Guid, ToDoNotify>,
        IOrderedItem
{
    public ToDoNotify(Guid id)
    {
        Id = id;
        _children = [];
        _parents = [];
        _weeklyDays = [];
        _monthlyDays = [];
        _annuallyDays = [];
    }

    public Guid Id { get; }
    public IAvaloniaReadOnlyList<ToDoNotify> Children => _children;
    public IEnumerable<object> Parents => _parents;
    public IEnumerable<DayOfWeek> WeeklyDays => _weeklyDays;
    public IEnumerable<int> MonthlyDays => _monthlyDays;
    public IEnumerable<DayOfYear> AnnuallyDays => _annuallyDays;
    public ToDoNotify ActualItem => Type == ToDoType.Reference ? Reference ?? this : this;

    [ObservableProperty]
    public partial string Name { get; set; } = string.Empty;

    [ObservableProperty]
    public partial uint OrderIndex { get; set; }

    [ObservableProperty]
    public partial string Description { get; set; } = string.Empty;

    [ObservableProperty]
    public partial ToDoType Type { get; set; }

    [ObservableProperty]
    public partial bool IsBookmark { get; set; }

    [ObservableProperty]
    public partial bool IsFavorite { get; set; }

    [ObservableProperty]
    public partial DateOnly DueDate { get; set; }

    [ObservableProperty]
    public partial TypeOfPeriodicity TypeOfPeriodicity { get; set; }

    [ObservableProperty]
    public partial ushort DaysOffset { get; set; }

    [ObservableProperty]
    public partial ushort MonthsOffset { get; set; }

    [ObservableProperty]
    public partial ushort WeeksOffset { get; set; }

    [ObservableProperty]
    public partial ushort YearsOffset { get; set; }

    [ObservableProperty]
    public partial ChildrenCompletionType ChildrenCompletionType { get; set; }

    [ObservableProperty]
    public partial string Link { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsRequiredCompleteInDueDate { get; set; }

    [ObservableProperty]
    public partial DescriptionType DescriptionType { get; set; }

    [ObservableProperty]
    public partial PackIconMaterialDesignKind Icon { get; set; } = PackIconMaterialDesignKind.None;

    [ObservableProperty]
    public partial Color Color { get; set; } = Colors.Transparent;

    [ObservableProperty]
    public partial uint RemindDaysBefore { get; set; }

    [ObservableProperty]
    public partial ToDoStatus Status { get; set; }

    [ObservableProperty]
    public partial ToDoIsCanDo IsCanDo { get; set; }

    [ObservableProperty]
    public partial ToDoNotify? Active { get; set; }

    [ObservableProperty]
    public partial ToDoNotify? Reference { get; set; }

    [ObservableProperty]
    public partial ToDoNotify? Parent { get; set; }

    public void UpdateChildren(ToDoNotify[] children)
    {
        _children.UpdateOrder(children);
    }

    public void UpdateParents(ToDoNotify[] parents)
    {
        var allParents = HomeMark.Instance.Cast<object>().ToEnumerable().Concat(parents).ToArray();
        _parents.UpdateOrder(allParents);
    }

    public void UpdateWeeklyDays(DayOfWeek[] weeklyDays)
    {
        _weeklyDays.UpdateOrder(weeklyDays);
    }

    public void UpdateMonthlyDays(int[] monthlyDays)
    {
        _monthlyDays.UpdateOrder(monthlyDays);
    }

    public void UpdateAnnualDays(DayOfYear[] annualDays)
    {
        _annuallyDays.UpdateOrder(annualDays);
    }

    public void AddChild(ToDoNotify child)
    {
        _children.Add(child);
    }

    public void RemoveChild(ToDoNotify child)
    {
        _children.Remove(child);
    }

    public static ToDoNotify Create(Guid input)
    {
        return new(input) { Status = ToDoStatus.ReadyForComplete };
    }

    Guid? IToDo.ReferenceId => Reference?.Id;

    private readonly AvaloniaList<ToDoNotify> _children;
    private readonly AvaloniaList<object> _parents;
    private readonly AvaloniaList<DayOfWeek> _weeklyDays;
    private readonly AvaloniaList<int> _monthlyDays;
    private readonly AvaloniaList<DayOfYear> _annuallyDays;

    [ObservableProperty]
    private bool _isSelected;

    [ObservableProperty]
    private bool _isHideOnTree;

    [ObservableProperty]
    private bool _isChangingOrder;
}
