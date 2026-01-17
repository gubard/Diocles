using Avalonia.Collections;
using Diocles.Models;
using Diocles.Ui;
using Gaia.Services;
using Hestia.Contract.Services;
using Inanna.Models;
using Inanna.Services;

namespace Diocles.Services;

public interface IDioclesViewModelFactory
{
    ToDosHeaderViewModel CreateToDosHeader(ToDoNotify item, AvaloniaList<InannaCommand> commands);
    RootToDosHeaderViewModel CreateRootToDosHeader(AvaloniaList<InannaCommand> commands);
    ToDosViewModel CreateToDos(ToDoNotify item);
    EditToDoViewModel CreateEditToDo(ToDoNotify item);
    ChangeParentToDoViewModel CreateChangeParentToDo();
    EditToDoHeaderViewModel CreateEditToDoHeader(ToDoNotify item);
    RootToDosViewModel CreateRootToDos();
    ToDoTreeViewModel CreateToDoTree();
    ToDoParametersViewModel CreateToDoParameters(ValidationMode validationMode, bool isShowEdit);
    ToDoListViewModel CreateToDoList(IAvaloniaReadOnlyList<ToDoNotify> input);
    SearchToDoHeaderViewModel CreateSearchToDoHeder();

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
        IToDoMemoryCache toDoMemoryCache,
        IUiToDoService uiToDoService,
        IStringFormater stringFormater,
        IDialogService dialogService,
        IAppResourceService appResourceService,
        INotificationService notificationService,
        IObjectStorage objectStorage,
        AppState appState
    )
    {
        _toDoValidator = toDoValidator;
        _toDoMemoryCache = toDoMemoryCache;
        _uiToDoService = uiToDoService;
        _stringFormater = stringFormater;
        _dialogService = dialogService;
        _appResourceService = appResourceService;
        _notificationService = notificationService;
        _objectStorage = objectStorage;
        _appState = appState;
    }

    public ToDosHeaderViewModel CreateToDosHeader(
        ToDoNotify item,
        AvaloniaList<InannaCommand> commands
    )
    {
        return new(item, commands, _appState, _uiToDoService);
    }

    public RootToDosHeaderViewModel CreateRootToDosHeader(AvaloniaList<InannaCommand> commands)
    {
        return new(commands, _appState, _uiToDoService);
    }

    public ToDosViewModel CreateToDos(ToDoNotify item)
    {
        return new(
            item,
            _uiToDoService,
            _stringFormater,
            _dialogService,
            _appResourceService,
            this,
            _objectStorage
        );
    }

    public EditToDoViewModel CreateEditToDo(ToDoNotify item)
    {
        return new(item, this, _uiToDoService, _notificationService, _appResourceService);
    }

    public ChangeParentToDoViewModel CreateChangeParentToDo()
    {
        return new(this);
    }

    public EditToDoHeaderViewModel CreateEditToDoHeader(ToDoNotify item)
    {
        return new(item);
    }

    public RootToDosViewModel CreateRootToDos()
    {
        return new(
            _uiToDoService,
            _toDoMemoryCache,
            _stringFormater,
            _dialogService,
            _appResourceService,
            this,
            _objectStorage
        );
    }

    public ToDoTreeViewModel CreateToDoTree()
    {
        return new(_toDoMemoryCache, _uiToDoService);
    }

    public ToDoParametersViewModel CreateToDoParameters(
        ValidationMode validationMode,
        bool isShowEdit
    )
    {
        return new(validationMode, isShowEdit, _toDoValidator, this);
    }

    public SearchToDoHeaderViewModel CreateSearchToDoHeder()
    {
        return new();
    }

    public ToDoParametersViewModel CreateToDoParameters(
        ToDoNotify item,
        ValidationMode validationMode,
        bool isShowEdit
    )
    {
        return new(item, validationMode, isShowEdit, _toDoValidator, this);
    }

    public ToDoListViewModel CreateToDoList(IAvaloniaReadOnlyList<ToDoNotify> input)
    {
        return new(input, _toDoMemoryCache);
    }

    private readonly IToDoValidator _toDoValidator;
    private readonly IToDoMemoryCache _toDoMemoryCache;
    private readonly IUiToDoService _uiToDoService;
    private readonly IStringFormater _stringFormater;
    private readonly IDialogService _dialogService;
    private readonly IAppResourceService _appResourceService;
    private readonly INotificationService _notificationService;
    private readonly IObjectStorage _objectStorage;
    private readonly AppState _appState;
}
