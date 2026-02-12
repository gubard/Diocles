using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Media;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Diocles.Models;
using Diocles.Services;
using Gaia.Helpers;
using Gaia.Models;
using Gaia.Services;
using Hestia.Contract.Helpers;
using Hestia.Contract.Models;
using Hestia.Contract.Services;
using IconPacks.Avalonia.MaterialDesign;
using Inanna.Generator;
using Inanna.Helpers;
using Inanna.Models;
using Inanna.Services;
using Inanna.Ui;
using Neotoma.Contract.Models;
using Weber.Helpers;
using Weber.Models;
using Weber.Services;

namespace Diocles.Ui;

[EditNotify]
public sealed partial class ToDoParametersViewModel
    : ParametersViewModelBase,
        IToDo,
        IInitUi,
        ISaveUi
{
    public static IEnumerable<PackIconMaterialDesignKind> Icons => IconsList;

    static ToDoParametersViewModel()
    {
        IconsList = [PackIconMaterialDesignKind.None, PackIconMaterialDesignKind.FoodBank];
    }

    public ToDoParametersViewModel(
        ToDoParametersSettings item,
        ValidationMode validationMode,
        bool isShowEdit,
        IToDoValidator toDoValidator,
        IDioclesViewModelFactory factory,
        IFileStorageUiService fileStorageUiService,
        Application app,
        IAppResourceService appResourceService,
        IStringFormater stringFormater,
        IDialogService dialogService
    )
        : base(validationMode, isShowEdit)
    {
        _filesDir = string.Empty;
        _files = new();
        _toDoValidator = toDoValidator;
        _fileStorageUiService = fileStorageUiService;
        _app = app;
        _appResourceService = appResourceService;
        _stringFormater = stringFormater;
        _dialogService = dialogService;
        _factory = factory;
        InitValidation();
        Tree = factory.CreateToDoTree();
        Name = item.Name;
        Description = item.Description;
        Type = item.Type;
        DueDate = item.DueDate;
        TypeOfPeriodicity = item.TypeOfPeriodicity;
        _annuallyDays = new(item.AnnuallyDays.ToArray());
        _monthlyDays = new(item.MonthlyDays.ToArray());
        _weeklyDays = new(item.WeeklyDays.ToArray());
        DaysOffset = item.DaysOffset;
        MonthsOffset = item.MonthsOffset;
        WeeksOffset = item.WeeksOffset;
        YearsOffset = item.YearsOffset;
        ChildrenCompletionType = item.ChildrenCompletionType;
        Link = item.Link;
        IsRequiredCompleteInDueDate = item.IsRequiredCompleteInDueDate;
        DescriptionType = item.DescriptionType;
        Color = Color.TryParse(item.Color, out var color) ? color : Colors.Transparent;
        RemindDaysBefore = item.RemindDaysBefore;
        IsBookmark = item.IsBookmark;
        IsFavorite = item.IsFavorite;
        Icon = item.Icon;
        _isTypeHasDueDate = item.Type.HasDueDate();
        ResetEdit();
    }

    public ToDoParametersViewModel(
        ToDoNotify item,
        ValidationMode validationMode,
        bool isShowEdit,
        IToDoValidator toDoValidator,
        IDioclesViewModelFactory factory,
        IFileStorageUiService fileStorageUiService,
        IFileStorageUiCache fileStorageUiCache,
        Application app,
        IAppResourceService appResourceService,
        IStringFormater stringFormater,
        IDialogService dialogService
    )
        : base(validationMode, isShowEdit)
    {
        _filesDir = $"{item.Id}/ToDo";
        _files = fileStorageUiCache.GetFiles(_filesDir);
        _toDoValidator = toDoValidator;
        _fileStorageUiService = fileStorageUiService;
        _app = app;
        _appResourceService = appResourceService;
        _stringFormater = stringFormater;
        _dialogService = dialogService;
        _factory = factory;
        InitValidation();
        Tree = factory.CreateToDoTree();
        Name = item.Name;
        Description = item.Description;
        Type = item.Type;
        DueDate = item.DueDate;
        TypeOfPeriodicity = item.TypeOfPeriodicity;
        _annuallyDays = new(item.AnnuallyDays.ToArray());
        _monthlyDays = new(item.MonthlyDays.ToArray());
        _weeklyDays = new(item.WeeklyDays.ToArray());
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
        _isTypeHasDueDate = item.Type.HasDueDate();
        ResetEdit();
    }

    public IEnumerable<FileObjectNotify> Files => _files;
    public IEnumerable<DayOfYear> AnnuallyDays => _annuallyDays;
    public IEnumerable<int> MonthlyDays => _monthlyDays;
    public IEnumerable<DayOfWeek> WeeklyDays => _weeklyDays;
    public ToDoTreeViewModel Tree { get; }

    [ObservableProperty]
    public partial bool IsBookmark { get; set; }

    [ObservableProperty]
    public partial bool IsFavorite { get; set; }

    [ObservableProperty]
    public partial string Name { get; set; }

    [ObservableProperty]
    public partial string Description { get; set; }

    [ObservableProperty]
    public partial ToDoType Type { get; set; }

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
    public partial string Link { get; set; }

    [ObservableProperty]
    public partial bool IsRequiredCompleteInDueDate { get; set; }

    [ObservableProperty]
    public partial DescriptionType DescriptionType { get; set; }

    [ObservableProperty]
    public partial PackIconMaterialDesignKind Icon { get; set; }

    [ObservableProperty]
    public partial Color Color { get; set; }

    [ObservableProperty]
    public partial ToDoNotify? Reference { get; set; }

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

    Guid? IToDo.ReferenceId => Reference?.Id;

    public ConfiguredValueTaskAwaitable InitUiAsync(CancellationToken ct)
    {
        Tree.PropertyChanged += TreePropertyChanged;
        _annuallyDays.CollectionChanged += AnnuallyDaysCollectionChanged;
        _monthlyDays.CollectionChanged += MonthlyDaysCollectionChanged;
        _weeklyDays.CollectionChanged += WeeklyDaysCollectionChanged;

        if (_filesDir.IsNullOrWhiteSpace())
        {
            return TaskHelper.ConfiguredCompletedTask;
        }

        return WrapCommandAsync(
            () => _fileStorageUiService.GetAsync(new() { GetFiles = [_filesDir] }, ct),
            ct
        );
    }

    public ConfiguredValueTaskAwaitable SaveUiAsync(CancellationToken ct)
    {
        Tree.PropertyChanged -= TreePropertyChanged;
        _annuallyDays.CollectionChanged -= AnnuallyDaysCollectionChanged;
        _monthlyDays.CollectionChanged -= MonthlyDaysCollectionChanged;
        _weeklyDays.CollectionChanged -= WeeklyDaysCollectionChanged;

        return TaskHelper.ConfiguredCompletedTask;
    }

    public NeotomaPostRequest CreateNeotomaPostRequest(params string[] dirs)
    {
        var request = new NeotomaPostRequest();

        foreach (var dir in dirs)
        {
            request.Creates.Add(
                dir,
                _files
                    .Where(x => x.Status == FileObjectNotifyStatus.Added)
                    .Select(x => x.ToFileData())
                    .ToArray()
            );
        }

        request.Deletes = _files
            .Where(x => x.Status == FileObjectNotifyStatus.Deleted)
            .Select(x => x.Id)
            .ToArray();

        return request;
    }

    public ShortToDo CreateShortToDo(Guid id, Guid? parentId)
    {
        return new()
        {
            Id = id,
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
            ParentId = parentId,
        };
    }

    public ToDoParametersSettings CreateSettings()
    {
        return new()
        {
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
            Icon = Icon,
            Color = Color.ToString(),
            RemindDaysBefore = RemindDaysBefore,
            IsBookmark = IsBookmark,
            IsFavorite = IsFavorite,
        };
    }

    public EditToDos CreateEditToDos(params Guid[] ids)
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

    protected override void OnPropertyChanging(PropertyChangingEventArgs e)
    {
        base.OnPropertyChanging(e);

        if (e.PropertyName == nameof(Type) && !IsEditDueDate && !_isTypeHasDueDate)
        {
            DueDate = DateTime.Now.ToDateOnly();
        }
    }

    private static readonly AvaloniaList<PackIconMaterialDesignKind> IconsList;

    private readonly AvaloniaList<FileObjectNotify> _files;
    private readonly AvaloniaList<DayOfYear> _annuallyDays;
    private readonly AvaloniaList<int> _monthlyDays;
    private readonly AvaloniaList<DayOfWeek> _weeklyDays;
    private readonly IToDoValidator _toDoValidator;
    private readonly bool _isTypeHasDueDate;
    private readonly string _filesDir;
    private readonly IFileStorageUiService _fileStorageUiService;
    private readonly Application _app;
    private readonly IAppResourceService _appResourceService;
    private readonly IStringFormater _stringFormater;
    private readonly IDialogService _dialogService;
    private readonly IDioclesViewModelFactory _factory;

    [RelayCommand]
    private void SetCurrentDateToName()
    {
        WrapCommand(() => Dispatcher.UIThread.Post(() => Name = $"{DateTime.Now}"));
    }

    [RelayCommand]
    private async Task ShowGenerateLinearBarcodeAsync(CancellationToken ct)
    {
        await WrapCommandAsync(
            () =>
            {
                var addBarcodeFile = _factory.CreateAddBarcodeFile();

                return _dialogService.ShowMessageBoxAsync(
                    new(
                        _appResourceService
                            .GetResource<string>("Lang.GenerateLinearBarcode")
                            .DispatchToDialogHeader(),
                        addBarcodeFile,
                        new(
                            _appResourceService.GetResource<string>("Lang.AddBarcode"),
                            UiHelper.CreateCommand(async c =>
                            {
                                await using var stream =
                                    addBarcodeFile.LinearBarcodeGenerator.Barcode.GetPngStream();

                                await _dialogService.CloseMessageBoxAsync(c);

                                var file = new FileObjectNotify(Guid.NewGuid())
                                {
                                    Name = $"{addBarcodeFile.FileName}.png",
                                    Data = stream.ToByteSpan().ToArray(),
                                    Description = string.Empty,
                                    Dir = _filesDir,
                                    Status = FileObjectNotifyStatus.Added,
                                };

                                Dispatcher.UIThread.Post(() => _files.Add(file));
                            }),
                            null,
                            DialogButtonType.Primary
                        ),
                        UiHelper.CancelButton
                    ),
                    ct
                );
            },
            ct
        );
    }

    [RelayCommand]
    private void DeleteFile(FileObjectNotify file)
    {
        WrapCommand(() =>
        {
            if (file.Status == FileObjectNotifyStatus.Added)
            {
                Dispatcher.UIThread.Post(() => _files.Remove(file));
            }
            else
            {
                Dispatcher.UIThread.Post(() => file.Status = FileObjectNotifyStatus.Deleted);
            }
        });
    }

    [RelayCommand]
    private async Task AddFilesAsync(CancellationToken ct)
    {
        await WrapCommandAsync(
            async () =>
            {
                var files = await _app.GetTopLevel()
                    .ThrowIfNull()
                    .StorageProvider.OpenFilePickerAsync(
                        new()
                        {
                            AllowMultiple = true,
                            Title = _stringFormater.Format(
                                _appResourceService.GetResource<string>("Lang.AddFilesItem"),
                                Name
                            ),
                        }
                    );

                if (files.Count == 0)
                {
                    return;
                }

                var addFiles = new FileObjectNotify[files.Count];

                for (var index = 0; index < files.Count; index++)
                {
                    var file = files[index];

                    addFiles[index] = new(Guid.NewGuid())
                    {
                        Name = file.Name,
                        Data = await file.GetDataAsync(ct),
                        Description = string.Empty,
                        Dir = _filesDir,
                        Status = FileObjectNotifyStatus.Added,
                    };
                }

                Dispatcher.UIThread.Post(() => _files.AddRange(addFiles));
            },
            ct
        );
    }

    private void InitValidation()
    {
        SetValidation(nameof(Name), () => _toDoValidator.Validate(Name, nameof(Name)));
        SetValidation(nameof(DueDate), () => _toDoValidator.Validate(this, nameof(DueDate)));
        SetValidation(nameof(Link), () => _toDoValidator.Validate(this, nameof(Link)));
        SetValidation(nameof(WeeklyDays), () => _toDoValidator.Validate(this, nameof(WeeklyDays)));
        SetValidation(nameof(Reference), () => _toDoValidator.Validate(this, nameof(Reference)));

        SetValidation(
            nameof(Description),
            () => _toDoValidator.Validate(Description, nameof(Description))
        );

        SetValidation(
            nameof(AnnuallyDays),
            () => _toDoValidator.Validate(this, nameof(AnnuallyDays))
        );

        SetValidation(
            nameof(MonthlyDays),
            () => _toDoValidator.Validate(this, nameof(MonthlyDays))
        );
    }

    private void AnnuallyDaysCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        IsEditAnnuallyDays = true;
    }

    private void MonthlyDaysCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        IsEditMonthlyDays = true;
    }

    private void WeeklyDaysCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        IsEditWeeklyDays = true;
    }

    private void TreePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Tree.Selected))
        {
            Reference = Tree.Selected;
        }
    }
}
