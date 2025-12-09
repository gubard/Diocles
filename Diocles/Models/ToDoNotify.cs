using Avalonia.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using Hestia.Contract.Models;

namespace Diocles.Models;

public partial class ToDoNotify : ObservableObject
{
    public ToDoNotify(Guid id)
    {
        Id = id;
        Children = [];
        Parents = [];
    }

    public Guid Id { get; }
    public AvaloniaList<ToDoNotify> Children { get; }
    public AvaloniaList<ToDoNotify> Parents { get; }

    [ObservableProperty]
    public partial string Name { get; set; } = string.Empty;

    [ObservableProperty]
    public partial uint OrderIndex { get; set; }

    [ObservableProperty]
    public partial string Description { get; set; } = string.Empty;

    [ObservableProperty]
    public partial DateTimeOffset CreatedDateTime { get; set; }

    [ObservableProperty]
    public partial ToDoItemType Type { get; set; }

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
    public partial string WeeklyDays { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string MonthlyDays { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string AnnuallyDays { get; set; } = string.Empty;

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
    public partial ToDoItemChildrenType ChildrenType { get; set; }

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
    public partial ToDoNotify? ReferenceId { get; set; }

    [ObservableProperty]
    public partial ToDoNotify? ParentId { get; set; }
}