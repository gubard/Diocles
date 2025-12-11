using CommunityToolkit.Mvvm.ComponentModel;
using Gaia.Helpers;
using Hestia.Contract.Models;
using Inanna.Generator;
using Inanna.Models;
using Inanna.Ui;

namespace Diocles.Ui;

[EditNotify]
public partial class ToDoParametersViewModel : ParametersViewModelBase
{
    public ToDoParametersViewModel(ValidationMode validationMode, bool isShowEdit) : base(validationMode, isShowEdit)
    {
    }

    [ObservableProperty]
    public partial string Name { get; set; } = string.Empty;

    [ObservableProperty]
    public partial uint OrderIndex { get; set; }

    [ObservableProperty]
    public partial string Description { get; set; } = string.Empty;

    [ObservableProperty]
    public partial ToDoItemType Type { get; set; }

    [ObservableProperty]
    public partial bool IsBookmark { get; set; }

    [ObservableProperty]
    public partial bool IsFavorite { get; set; }

    [ObservableProperty]
    public partial DateOnly DueDate { get; set; }

    [ObservableProperty]
    public partial TypeOfPeriodicity TypeOfPeriodicity { get; set; }

    [ObservableProperty]
    public partial DayOfYear[] AnnuallyDays { get; set; } = [];

    [ObservableProperty]
    public partial byte[] MonthlyDays { get; set; } = [];

    [ObservableProperty]
    public partial DayOfWeek[] WeeklyDays { get; set; } = [];

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
    public partial Guid? ReferenceId { get; set; }

    [ObservableProperty]
    public partial Guid? ParentId { get; set; }

    [ObservableProperty]
    public partial uint RemindDaysBefore { get; set; }

    [ObservableProperty]
    public partial bool IsEditName { get; set; }

    [ObservableProperty]
    public partial bool IsEditOrderIndex { get; set; }

    [ObservableProperty]
    public partial bool IsEditDescription { get; set; }

    [ObservableProperty]
    public partial bool IsEditType { get; set; }

    [ObservableProperty]
    public partial bool IsEditIsBookmark { get; set; }

    [ObservableProperty]
    public partial bool IsEditIsFavorite { get; set; }

    [ObservableProperty]
    public partial bool IsEditDueDate { get; set; }

    [ObservableProperty]
    public partial bool IsEditTypeOfPeriodicity { get; set; }

    [ObservableProperty]
    public partial bool IsEditAnnuallyDays { get; set; }

    [ObservableProperty]
    public partial bool IsEditMonthlyDays { get; set; }

    [ObservableProperty]
    public partial bool IsEditWeeklyDays { get; set; }

    [ObservableProperty]
    public partial bool IsEditDaysOffset { get; set; }

    [ObservableProperty]
    public partial bool IsEditMonthsOffset { get; set; }

    [ObservableProperty]
    public partial bool IsEditWeeksOffset { get; set; }

    [ObservableProperty]
    public partial bool IsEditYearsOffset { get; set; }

    [ObservableProperty]
    public partial bool IsEditChildrenType { get; set; }

    [ObservableProperty]
    public partial bool IsEditLink { get; set; }

    [ObservableProperty]
    public partial bool IsEditIsRequiredCompleteInDueDate { get; set; }

    [ObservableProperty]
    public partial bool IsEditDescriptionType { get; set; }

    [ObservableProperty]
    public partial bool IsEditIcon { get; set; }

    [ObservableProperty]
    public partial bool IsEditColor { get; set; }

    [ObservableProperty]
    public partial bool IsEditReferenceId { get; set; }

    [ObservableProperty]
    public partial bool IsEditParentId { get; set; }

    [ObservableProperty]
    public partial bool IsEditRemindDaysBefore { get; set; }

    public CreateToDo CreateToDo()
    {
        return new()
        {
            Id = Guid.NewGuid(),
            Name = Name,
            Description = Description,
            Type = Type,
            IsBookmark = IsBookmark,
            IsFavorite = IsFavorite,
            DueDate = DueDate,
            TypeOfPeriodicity = TypeOfPeriodicity,
            AnnuallyDays = AnnuallyDaysToString(),
            MonthlyDays = MonthlyDaysToString(),
            WeeklyDays = WeeklyDaysToString(),
            DaysOffset = DaysOffset,
            MonthsOffset = MonthsOffset,
            WeeksOffset = WeeksOffset,
            YearsOffset = YearsOffset,
            ChildrenType = ChildrenType,
            Link = Link ?? string.Empty,
            IsRequiredCompleteInDueDate = IsRequiredCompleteInDueDate,
            DescriptionType = DescriptionType,
            Icon = Icon,
            Color = Color,
            ReferenceId = ReferenceId,
            ParentId = ParentId,
            RemindDaysBefore = RemindDaysBefore,
        };
    }

    private string AnnuallyDaysToString()
    {
        return AnnuallyDays.Select(x => $"{x.Day}.{x.Month}").JoinString(";");
    }

    private string MonthlyDaysToString()
    {
        return MonthlyDays.Select(x => $"{x}").JoinString(";");
    }

    private string WeeklyDaysToString()
    {
        return WeeklyDays.Select(x => $"{(int)x}").JoinString(";");
    }
}