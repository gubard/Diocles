using Avalonia.Collections;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using Diocles.Models;
using Diocles.Services;
using Gaia.Helpers;
using Gaia.Models;
using Hestia.Contract.Models;
using Hestia.Contract.Services;
using IconPacks.Avalonia.MaterialDesign;
using Inanna.Generator;
using Inanna.Models;
using Inanna.Ui;

namespace Diocles.Ui;

[EditNotify]
public partial class ToDoParametersViewModel : ParametersViewModelBase, IToDo
{
    private static readonly AvaloniaList<PackIconMaterialDesignKind> _icons;

    public static IEnumerable<PackIconMaterialDesignKind> Icons => _icons;

    private readonly AvaloniaList<DayOfYear> _annuallyDays;
    private readonly AvaloniaList<int> _monthlyDays;
    private readonly AvaloniaList<DayOfWeek> _weeklyDays;
    private readonly IToDoValidator _toDoValidator;

    static ToDoParametersViewModel()
    {
        _icons = [PackIconMaterialDesignKind.None, PackIconMaterialDesignKind.FoodBank,];
    }

    public ToDoParametersViewModel(IToDoValidator toDoValidator, IDioclesViewModelFactory factory,
        ValidationMode validationMode, bool isShowEdit) : base(validationMode, isShowEdit)
    {
        _toDoValidator = toDoValidator;
        InitValidation();
        _annuallyDays = [new(1, Month.January),];
        _annuallyDays.CollectionChanged += (_, _) => IsEditAnnuallyDays = true;
        _monthlyDays = [1,];
        _monthlyDays.CollectionChanged += (_, _) => IsEditMonthlyDays = true;
        _weeklyDays = [DayOfWeek.Monday,];
        _weeklyDays.CollectionChanged += (_, _) => IsEditWeeklyDays = true;
        Tree = factory.Create();

        Tree.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(Tree.Selected))
            {
                Reference = Tree.Selected;
            }
        };
    }

    public IEnumerable<DayOfYear> AnnuallyDays => _annuallyDays;
    public IEnumerable<int> MonthlyDays => _monthlyDays;
    public IEnumerable<DayOfWeek> WeeklyDays => _weeklyDays;
    public ToDoTreeViewModel Tree { get; }

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
    public partial ushort DaysOffset { get; set; } = 1;

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
    public partial bool IsEditReference { get; set; }

    [ObservableProperty]
    public partial bool IsEditRemindDaysBefore { get; set; }

    [ObservableProperty]
    public partial ToDoNotify? Reference { get; set; }

    Guid? IToDo.ReferenceId => Reference?.Id;

    public CreateToDo CreateToDo()
    {
        return new()
        {
            Id = Guid.NewGuid(),
            Name = Name.Trim(),
            Description = Description.Trim(),
            Type = Type,
            DueDate = DueDate,
            TypeOfPeriodicity = TypeOfPeriodicity,
            AnnuallyDays = AnnuallyDays.ToArray(),
            MonthlyDays = MonthlyDays.ToArray(),
            WeeklyDays = WeeklyDays.ToArray(),
            DaysOffset = DaysOffset,
            MonthsOffset = MonthsOffset,
            WeeksOffset = WeeksOffset,
            YearsOffset = YearsOffset,
            ChildrenCompletionType = ChildrenCompletionType,
            Link = Link.Trim(),
            IsRequiredCompleteInDueDate = IsRequiredCompleteInDueDate,
            DescriptionType = DescriptionType,
            Icon = Icon.ToString(),
            Color = Color.ToString(),
            ReferenceId = Reference?.Id,
            RemindDaysBefore = RemindDaysBefore,
            IsBookmark = IsBookmark,
            IsFavorite = IsFavorite,
        };
    }

    private void InitValidation()
    {
        SetValidation(nameof(Name), () => _toDoValidator.Validate(Name, nameof(Name)));
        SetValidation(nameof(Description), () => _toDoValidator.Validate(Description, nameof(Description)));
        SetValidation(nameof(DueDate), () => _toDoValidator.Validate(this, nameof(DueDate)));
        SetValidation(nameof(Link), () => _toDoValidator.Validate(this, nameof(Link)));
        SetValidation(nameof(AnnuallyDays), () => _toDoValidator.Validate(this, nameof(AnnuallyDays)));
        SetValidation(nameof(MonthlyDays), () => _toDoValidator.Validate(this, nameof(MonthlyDays)));
        SetValidation(nameof(WeeklyDays), () => _toDoValidator.Validate(this, nameof(WeeklyDays)));
        SetValidation(nameof(Reference), () => _toDoValidator.Validate(this, nameof(Reference)));
    }
}