using Avalonia.Media;
using Gaia.Helpers;
using Gaia.Models;
using Hestia.Contract.Models;
using IconPacks.Avalonia.MaterialDesign;

namespace Diocles.Models;

public class ToDoParametersSettings
{
    public bool IsBookmark { get; set; }
    public bool IsFavorite { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ToDoType Type { get; set; }
    public DateOnly DueDate { get; set; } = DateTime.Now.ToDateOnly();
    public TypeOfPeriodicity TypeOfPeriodicity { get; set; }
    public ushort DaysOffset { get; set; } = 1;
    public ushort MonthsOffset { get; set; }
    public ushort WeeksOffset { get; set; }
    public ushort YearsOffset { get; set; }
    public ChildrenCompletionType ChildrenCompletionType { get; set; }
    public string Link { get; set; } = string.Empty;
    public bool IsRequiredCompleteInDueDate { get; set; } = true;
    public DescriptionType DescriptionType { get; set; }
    public PackIconMaterialDesignKind Icon { get; set; }
    public string Color { get; set; } = "Transparent";
    public uint RemindDaysBefore { get; set; }
    public DayOfYear[] AnnuallyDays { get; set; } = [new() { Day = 1, Month = Month.January }];
    public int[] MonthlyDays { get; set; } = [1];
    public DayOfWeek[] WeeklyDays { get; set; } = [DayOfWeek.Monday];
}
