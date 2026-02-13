using Avalonia;
using Avalonia.Collections;
using Diocles.Models;
using Diocles.Ui;
using Gaia.Services;
using Hestia.Contract.Services;
using Inanna.Models;
using Inanna.Services;
using Weber.Services;

namespace Diocles.Services;

public interface IDioclesViewModelFactory
{
    AddBarcodeFileViewModel CreateAddBarcodeFile();
    ToDoItemViewModel CreateToDos(ToDoNotify item);
    ChangeParentToDoViewModel CreateChangeParentToDo();
    RootToDosViewModel CreateRootToDos();
    ToDoTreeViewModel CreateToDoTree();
    ToDoListViewModel CreateToDoList(IAvaloniaReadOnlyList<ToDoNotify> input);

    ToDoItemHeaderViewModel CreateToDoItemHeader(
        ToDoNotify item,
        IAvaloniaReadOnlyList<InannaCommand> commands,
        IAvaloniaReadOnlyList<InannaCommand> multiCommands
    );

    ToDoParametersViewModel CreateToDoParameters(
        ToDoParametersSettings settings,
        ValidationMode validationMode,
        bool isShowEdit
    );

    ToDosHeaderViewModel CreateToDosHeader(
        string title,
        IAvaloniaReadOnlyList<InannaCommand> commands,
        IAvaloniaReadOnlyList<InannaCommand> multiCommands
    );

    ToDoParametersViewModel CreateToDoParameters(
        ToDoNotify item,
        ValidationMode validationMode,
        bool isShowEdit
    );
}

public sealed class DioclesViewModelFactory : IDioclesViewModelFactory
{
    public DioclesViewModelFactory(
        IToDoValidator toDoValidator,
        IToDoUiCache toDoUiCache,
        IToDoUiService toDoUiService,
        IStringFormater stringFormater,
        IDialogService dialogService,
        IAppResourceService appResourceService,
        IObjectStorage objectStorage,
        IFileStorageUiService fileStorageUiService,
        IFileStorageUiCache fileStorageUiCache,
        Application app,
        IWeberViewModelFactory weberFactory,
        IInannaViewModelFactory inannaViewModelFactor
    )
    {
        _toDoValidator = toDoValidator;
        _toDoUiCache = toDoUiCache;
        _toDoUiService = toDoUiService;
        _stringFormater = stringFormater;
        _dialogService = dialogService;
        _appResourceService = appResourceService;
        _objectStorage = objectStorage;
        _fileStorageUiService = fileStorageUiService;
        _fileStorageUiCache = fileStorageUiCache;
        _app = app;
        _weberFactory = weberFactory;
        _inannaViewModelFactor = inannaViewModelFactor;
    }

    public ToDosHeaderViewModel CreateToDosHeader(
        string title,
        IAvaloniaReadOnlyList<InannaCommand> commands,
        IAvaloniaReadOnlyList<InannaCommand> multiCommands
    )
    {
        return new(title, commands, multiCommands, _inannaViewModelFactor);
    }

    public ToDoItemHeaderViewModel CreateToDoItemHeader(
        ToDoNotify item,
        IAvaloniaReadOnlyList<InannaCommand> commands,
        IAvaloniaReadOnlyList<InannaCommand> multiCommands
    )
    {
        return new(commands, multiCommands, item, _inannaViewModelFactor);
    }

    public AddBarcodeFileViewModel CreateAddBarcodeFile()
    {
        return new(_inannaViewModelFactor);
    }

    public ToDoItemViewModel CreateToDos(ToDoNotify item)
    {
        return new(
            item,
            _toDoUiService,
            _toDoUiCache,
            _stringFormater,
            _dialogService,
            _appResourceService,
            this,
            _objectStorage,
            _fileStorageUiService,
            _fileStorageUiCache,
            _weberFactory
        );
    }

    public ChangeParentToDoViewModel CreateChangeParentToDo()
    {
        return new(this);
    }

    public RootToDosViewModel CreateRootToDos()
    {
        return new(
            _toDoUiService,
            _toDoUiCache,
            _stringFormater,
            _dialogService,
            _appResourceService,
            this,
            _objectStorage,
            _fileStorageUiService
        );
    }

    public ToDoTreeViewModel CreateToDoTree()
    {
        return new(_toDoUiCache, _toDoUiService);
    }

    public ToDoParametersViewModel CreateToDoParameters(
        ToDoParametersSettings settings,
        ValidationMode validationMode,
        bool isShowEdit
    )
    {
        return new(
            settings,
            validationMode,
            isShowEdit,
            _toDoValidator,
            this,
            _fileStorageUiService,
            _app,
            _appResourceService,
            _stringFormater,
            _dialogService
        );
    }

    public ToDoParametersViewModel CreateToDoParameters(
        ToDoNotify item,
        ValidationMode validationMode,
        bool isShowEdit
    )
    {
        return new(
            item,
            validationMode,
            isShowEdit,
            _toDoValidator,
            this,
            _fileStorageUiService,
            _fileStorageUiCache,
            _app,
            _appResourceService,
            _stringFormater,
            _dialogService
        );
    }

    public ToDoListViewModel CreateToDoList(IAvaloniaReadOnlyList<ToDoNotify> input)
    {
        return new(input, _toDoUiCache);
    }

    private readonly IToDoValidator _toDoValidator;
    private readonly IToDoUiCache _toDoUiCache;
    private readonly IToDoUiService _toDoUiService;
    private readonly IStringFormater _stringFormater;
    private readonly IDialogService _dialogService;
    private readonly IAppResourceService _appResourceService;
    private readonly IObjectStorage _objectStorage;
    private readonly IFileStorageUiService _fileStorageUiService;
    private readonly IFileStorageUiCache _fileStorageUiCache;
    private readonly Application _app;
    private readonly IWeberViewModelFactory _weberFactory;
    private readonly IInannaViewModelFactory _inannaViewModelFactor;
}
