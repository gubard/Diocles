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
    ToDoItemViewModel CreateToDos(ToDoNotify item);
    ChangeParentToDoViewModel CreateChangeParentToDo();
    RootToDosViewModel CreateRootToDos();
    ToDoTreeViewModel CreateToDoTree();
    ToDoParametersViewModel CreateToDoParameters(ValidationMode validationMode, bool isShowEdit);
    ToDoListViewModel CreateToDoList(IAvaloniaReadOnlyList<ToDoNotify> input);

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
        AppState appState
    )
    {
        _toDoValidator = toDoValidator;
        _toDoUiCache = toDoUiCache;
        _toDoUiService = toDoUiService;
        _stringFormater = stringFormater;
        _dialogService = dialogService;
        _appResourceService = appResourceService;
        _objectStorage = objectStorage;
        _appState = appState;
    }

    public ToDosHeaderViewModel CreateToDosHeader(
        string title,
        IAvaloniaReadOnlyList<InannaCommand> commands,
        IAvaloniaReadOnlyList<InannaCommand> multiCommands
    )
    {
        return new(title, commands, multiCommands, _appState, _toDoUiService);
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
            _objectStorage
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
            _objectStorage
        );
    }

    public ToDoTreeViewModel CreateToDoTree()
    {
        return new(_toDoUiCache, _toDoUiService);
    }

    public ToDoParametersViewModel CreateToDoParameters(
        ValidationMode validationMode,
        bool isShowEdit
    )
    {
        return new(validationMode, isShowEdit, _toDoValidator, this);
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
        return new(input, _toDoUiCache);
    }

    private readonly IToDoValidator _toDoValidator;
    private readonly IToDoUiCache _toDoUiCache;
    private readonly IToDoUiService _toDoUiService;
    private readonly IStringFormater _stringFormater;
    private readonly IDialogService _dialogService;
    private readonly IAppResourceService _appResourceService;
    private readonly IObjectStorage _objectStorage;
    private readonly AppState _appState;
}
