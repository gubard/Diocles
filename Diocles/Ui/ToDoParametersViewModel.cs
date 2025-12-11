using Avalonia.Collections;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using Gaia.Helpers;
using Gaia.Models;
using Hestia.Contract.Models;
using IconPacks.Avalonia.MaterialDesign;
using Inanna.Generator;
using Inanna.Models;
using Inanna.Ui;

namespace Diocles.Ui;

[EditNotify]
public partial class ToDoParametersViewModel : ParametersViewModelBase
{
    private static readonly AvaloniaList<PackIconMaterialDesignKind> _icons;

    public static IEnumerable<PackIconMaterialDesignKind> Icons => _icons;

    private readonly AvaloniaList<DayOfYear> _annuallyDays;
    private readonly AvaloniaList<int> _monthlyDays;
    private readonly AvaloniaList<DayOfWeek> _weeklyDays;

    static ToDoParametersViewModel()
    {
        _icons = [PackIconMaterialDesignKind.None, PackIconMaterialDesignKind.FoodBank,];
    }

    public ToDoParametersViewModel(ValidationMode validationMode, bool isShowEdit) : base(validationMode, isShowEdit)
    {
        _annuallyDays = [new(1, Month.January),];
        _annuallyDays.CollectionChanged += (_, _) => IsEditAnnuallyDays = true;
        _monthlyDays = [1,];
        _monthlyDays.CollectionChanged += (_, _) => IsEditMonthlyDays = true;
        _weeklyDays = [DayOfWeek.Monday,];
        _weeklyDays.CollectionChanged += (_, _) => IsEditWeeklyDays = true;
    }

    public IEnumerable<DayOfYear> AnnuallyDays => _annuallyDays;
    public IEnumerable<int> MonthlyDays => _monthlyDays;
    public IEnumerable<DayOfWeek> WeeklyDays => _weeklyDays;

    [ObservableProperty]
    public partial bool IsBookmark { get; set; }

    [ObservableProperty]
    public partial bool IsFavorite { get; set; }

    [ObservableProperty]
    public partial string Name { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Description { get; set; } = string.Empty;

    [ObservableProperty]
    public partial ToDoType Type { get; set; }

    [ObservableProperty]
    public partial DateOnly DueDate { get; set; } = DateTime.Now.ToDateOnly();

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
    public partial bool IsRequiredCompleteInDueDate { get; set; } = true;

    [ObservableProperty]
    public partial DescriptionType DescriptionType { get; set; }

    [ObservableProperty]
    public partial PackIconMaterialDesignKind Icon { get; set; }

    [ObservableProperty]
    public partial Color Color { get; set; } = Colors.Transparent;

    [ObservableProperty]
    public partial Guid? ReferenceId { get; set; }

    [ObservableProperty]
    public partial uint RemindDaysBefore { get; set; }

    [ObservableProperty]
    public partial bool IsEditIsBookmark { get; set; }

    [ObservableProperty]
    public partial bool IsEditIsFavorite { get; set; }

    [ObservableProperty]
    public partial bool IsEditName { get; set; }

    [ObservableProperty]
    public partial bool IsEditDescription { get; set; }

    [ObservableProperty]
    public partial bool IsEditType { get; set; }

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
    public partial bool IsEditChildrenCompletionType { get; set; }

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
    public partial bool IsEditRemindDaysBefore { get; set; }

    public CreateToDo CreateToDo()
    {
        return new()
        {
            Id = Guid.NewGuid(),
            Name = Name,
            Description = Description,
            Type = Type,
            DueDate = DueDate,
            TypeOfPeriodicity = TypeOfPeriodicity,
            AnnuallyDays = AnnuallyDaysToString(),
            MonthlyDays = MonthlyDaysToString(),
            WeeklyDays = WeeklyDaysToString(),
            DaysOffset = DaysOffset,
            MonthsOffset = MonthsOffset,
            WeeksOffset = WeeksOffset,
            YearsOffset = YearsOffset,
            ChildrenCompletionType = ChildrenCompletionType,
            Link = Link,
            IsRequiredCompleteInDueDate = IsRequiredCompleteInDueDate,
            DescriptionType = DescriptionType,
            Icon = Icon.ToString(),
            Color = Color.ToString(),
            ReferenceId = ReferenceId,
            RemindDaysBefore = RemindDaysBefore,
            IsBookmark = IsBookmark,
            IsFavorite = IsFavorite,
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