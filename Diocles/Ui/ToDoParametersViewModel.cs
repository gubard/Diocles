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
using Inanna.Helpers;
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
        _icons = [PackIconMaterialDesignKind.None, PackIconMaterialDesignKind.FoodBank];
    }

    public ToDoParametersViewModel(
        ToDoNotify item,
        ValidationMode validationMode,
        bool isShowEdit,
        IToDoValidator toDoValidator,
        IDioclesViewModelFactory factory
    )
        : this(validationMode, isShowEdit, toDoValidator, factory)
    {
        Name = item.Name;
        Description = item.Description;
        Type = item.Type;
        DueDate = item.DueDate;
        TypeOfPeriodicity = item.TypeOfPeriodicity;
        _annuallyDays.UpdateOrder(item.AnnuallyDays.ToArray());
        _monthlyDays.UpdateOrder(item.MonthlyDays.ToArray());
        _weeklyDays.UpdateOrder(item.WeeklyDays.ToArray());
        DaysOffset = item.DaysOffset;
        MonthsOffset = item.MonthsOffset;
        WeeksOffset = item.WeeksOffset;
        YearsOffset = item.YearsOffset;
        ChildrenCompletionType = item.ChildrenCompletionType;
        Link = item.Link;
        IsRequiredCompleteInDueDate = item.IsRequiredCompleteInDueDate;
        DescriptionType = item.DescriptionType;
        Color = item.Color;
        Reference = item.Reference;
        RemindDaysBefore = item.RemindDaysBefore;
        IsBookmark = item.IsBookmark;
        IsFavorite = item.IsFavorite;
        Icon = item.Icon;
        ResetEdit();
    }

    public ToDoParametersViewModel(
        ValidationMode validationMode,
        bool isShowEdit,
        IToDoValidator toDoValidator,
        IDioclesViewModelFactory factory
    )
        : base(validationMode, isShowEdit)
    {
        _toDoValidator = toDoValidator;
        InitValidation();
        _annuallyDays = [new() { Day = 1, Month = Month.January }];
        _annuallyDays.CollectionChanged += (_, _) => IsEditAnnuallyDays = true;
        _monthlyDays = [1];
        _monthlyDays.CollectionChanged += (_, _) => IsEditMonthlyDays = true;
        _weeklyDays = [DayOfWeek.Monday];
        _weeklyDays.CollectionChanged += (_, _) => IsEditWeeklyDays = true;
        Tree = factory.CreateToDoTree();

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

    public ShortToDo CreateShortToDo()
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

    public EditToDos CreateEditToDos(Guid id)
    {
        return CreateEditToDos([id]);
    }

    public EditToDos CreateEditToDos(Guid[] ids)
    {
        return new()
        {
            Ids = ids,
            AnnuallyDays = AnnuallyDays.ToArray(),
            IsEditAnnuallyDays = IsEditAnnuallyDays,
            ChildrenCompletionType = ChildrenCompletionType,
            IsEditChildrenCompletionType = IsEditChildrenCompletionType,
            Color = Color.ToString(),
            IsEditColor = IsEditColor,
            DaysOffset = DaysOffset,
            IsEditDaysOffset = IsEditDaysOffset,
            Description = Description.Trim(),
            IsEditDescription = IsEditDescription,
            DescriptionType = DescriptionType,
            IsEditDescriptionType = IsEditDescriptionType,
            DueDate = DueDate,
            IsEditDueDate = IsEditDueDate,
            Icon = Icon.ToString(),
            IsEditIcon = IsEditIcon,
            IsBookmark = IsBookmark,
            IsEditIsBookmark = IsEditIsBookmark,
            IsFavorite = IsFavorite,
            IsEditIsFavorite = IsEditIsFavorite,
            IsRequiredCompleteInDueDate = IsRequiredCompleteInDueDate,
            IsEditIsRequiredCompleteInDueDate = IsEditIsRequiredCompleteInDueDate,
            Link = Link.Trim(),
            IsEditLink = IsEditLink,
            MonthlyDays = MonthlyDays.Select(x => (byte)x).ToArray(),
            IsEditMonthlyDays = IsEditMonthlyDays,
            MonthsOffset = MonthsOffset,
            IsEditMonthsOffset = IsEditMonthsOffset,
            Name = Name.Trim(),
            IsEditName = IsEditName,
            ReferenceId = Reference?.Id,
            IsEditReferenceId = IsEditReference,
            RemindDaysBefore = RemindDaysBefore,
            IsEditRemindDaysBefore = IsEditRemindDaysBefore,
            Type = Type,
            IsEditType = IsEditType,
            TypeOfPeriodicity = TypeOfPeriodicity,
            IsEditTypeOfPeriodicity = IsEditTypeOfPeriodicity,
            WeeklyDays = WeeklyDays.ToArray(),
            IsEditWeeklyDays = IsEditWeeklyDays,
            WeeksOffset = WeeksOffset,
            IsEditWeeksOffset = IsEditWeeksOffset,
            YearsOffset = YearsOffset,
            IsEditYearsOffset = IsEditYearsOffset,
        };
    }

    private void InitValidation()
    {
        SetValidation(nameof(Name), () => _toDoValidator.Validate(Name, nameof(Name)));
        SetValidation(
            nameof(Description),
            () => _toDoValidator.Validate(Description, nameof(Description))
        );
        SetValidation(nameof(DueDate), () => _toDoValidator.Validate(this, nameof(DueDate)));
        SetValidation(nameof(Link), () => _toDoValidator.Validate(this, nameof(Link)));
        SetValidation(
            nameof(AnnuallyDays),
            () => _toDoValidator.Validate(this, nameof(AnnuallyDays))
        );
        SetValidation(
            nameof(MonthlyDays),
            () => _toDoValidator.Validate(this, nameof(MonthlyDays))
        );
        SetValidation(nameof(WeeklyDays), () => _toDoValidator.Validate(this, nameof(WeeklyDays)));
        SetValidation(nameof(Reference), () => _toDoValidator.Validate(this, nameof(Reference)));
    }
}
