using Avalonia.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using Gaia.Models;
using Hestia.Contract.Models;
using Hestia.Contract.Services;
using Inanna.Helpers;

namespace Diocles.Models;

public partial class ToDoNotify : ObservableObject, IToDo
{
    private readonly AvaloniaList<ToDoNotify> _children;
    private readonly AvaloniaList<ToDoNotify> _parents;
    private readonly AvaloniaList<DayOfWeek> _weeklyDays;
    private readonly AvaloniaList<int> _monthlyDays;
    private readonly AvaloniaList<DayOfYear> _annuallyDays;

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
    public IEnumerable<ToDoNotify> Children => _children;
    public IEnumerable<ToDoNotify> Parents => _parents;
    public IEnumerable<DayOfWeek> WeeklyDays => _weeklyDays;
    public IEnumerable<int> MonthlyDays => _monthlyDays;
    public IEnumerable<DayOfYear> AnnuallyDays => _annuallyDays;
    Guid? IToDo.ReferenceId => Reference?.Id;

    [ObservableProperty]
    public partial string Name { get; set; } = string.Empty;

    [ObservableProperty]
    public partial uint OrderIndex { get; set; }

    [ObservableProperty]
    public partial string Description { get; set; } = string.Empty;

    [ObservableProperty]
    public partial DateTimeOffset CreatedDateTime { get; set; }

    [ObservableProperty]
    public partial ToDoType Type { get; set; }

    [ObservableProperty]
    public partial bool IsBookmark { get; set; }

    [ObservableProperty]
    public partial bool IsFavorite { get; set; }

    [ObservableProperty]
    public partial DateOnly DueDate { get; set; }

    [ObservableProperty]
    public partial bool IsCompleted { get; set; }

    [ObservableProperty]
    public partial TypeOfPeriodicity TypeOfPeriodicity { get; set; }

    [ObservableProperty]
    public partial DateTimeOffset? LastCompleted { get; set; }

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
    public partial uint CurrentCircleOrderIndex { get; set; }

    [ObservableProperty]
    public partial string Link { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsRequiredCompleteInDueDate { get; set; }

    [ObservableProperty]
    public partial DescriptionType DescriptionType { get; set; }

    [ObservableProperty]
    public partial string Icon { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Color { get; set; } = string.Empty;

    [ObservableProperty]
    public partial uint RemindDaysBefore { get; set; }

    [ObservableProperty]
    public partial ToDoItemStatus Status { get; set; }

    [ObservableProperty]
    public partial ToDoItemIsCan IsCan { get; set; }

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
        _parents.UpdateOrder(parents);
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
}