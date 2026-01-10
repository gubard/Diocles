using Avalonia.Collections;
using Diocles.Models;
using Diocles.Ui;
using Gaia.Services;
using Hestia.Contract.Services;
using Inanna.Models;
using Inanna.Services;

namespace Diocles.Services;

public interface IDioclesViewModelFactory
    : IFactory<(ValidationMode validationMode, bool isShowEdit), ToDoParametersViewModel>,
        IFactory<
            (ToDoNotify item, ValidationMode validationMode, bool isShowEdit),
            ToDoParametersViewModel
        >,
        IFactory<ToDoTreeViewModel>,
        IFactory<RootToDosViewModel>,
        IFactory<IAvaloniaReadOnlyList<ToDoNotify>, ToDoListViewModel>,
        IFactory<ToDoNotify, ToDosViewModel>,
        IFactory<ToDoNotify, EditToDoViewModel>,
        IFactory<ToDoNotify, EditToDoHeaderViewModel>
{
    ToDosViewModel CreateToDos(ToDoNotify item);
    EditToDoViewModel CreateEditToDo(ToDoNotify item);
    EditToDoHeaderViewModel CreateEditToDoHeader(ToDoNotify item);
    RootToDosViewModel CreateRootToDos();
    ToDoTreeViewModel CreateToDoTree();
}

public class DioclesViewModelFactory : IDioclesViewModelFactory
{
    public DioclesViewModelFactory(
        IToDoValidator toDoValidator,
        IToDoCache toDoCache,
        IUiToDoService uiToDoService,
        IStringFormater stringFormater,
        IDialogService dialogService,
        IAppResourceService appResourceService,
        INotificationService notificationService,
        IObjectStorage objectStorage
    )
    {
        _toDoValidator = toDoValidator;
        _toDoCache = toDoCache;
        _uiToDoService = uiToDoService;
        _stringFormater = stringFormater;
        _dialogService = dialogService;
        _appResourceService = appResourceService;
        _notificationService = notificationService;
        _objectStorage = objectStorage;
    }

    public ToDoParametersViewModel Create((ValidationMode validationMode, bool isShowEdit) input)
    {
        return new(input.validationMode, input.isShowEdit, _toDoValidator, this);
    }

    public ToDoTreeViewModel Create()
    {
        return CreateToDoTree();
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

    public EditToDoHeaderViewModel CreateEditToDoHeader(ToDoNotify item)
    {
        return new(item);
    }

    public RootToDosViewModel CreateRootToDos()
    {
        return new(
            _uiToDoService,
            _toDoCache,
            _stringFormater,
            _dialogService,
            _appResourceService,
            this,
            _objectStorage
        );
    }

    public ToDoTreeViewModel CreateToDoTree()
    {
        return new(_toDoCache, _uiToDoService);
    }

    public ToDoParametersViewModel Create(
        (ToDoNotify item, ValidationMode validationMode, bool isShowEdit) input
    )
    {
        return new(input.item, input.validationMode, input.isShowEdit, _toDoValidator, this);
    }

    public EditToDoHeaderViewModel Create(ToDoNotify input)
    {
        return CreateEditToDoHeader(input);
    }

    public ToDoListViewModel Create(IAvaloniaReadOnlyList<ToDoNotify> input)
    {
        return new(input, _toDoCache);
    }

    ToDosViewModel IFactory<ToDoNotify, ToDosViewModel>.Create(ToDoNotify input)
    {
        return CreateToDos(input);
    }

    EditToDoViewModel IFactory<ToDoNotify, EditToDoViewModel>.Create(ToDoNotify input)
    {
        return CreateEditToDo(input);
    }

    RootToDosViewModel IFactory<RootToDosViewModel>.Create()
    {
        return CreateRootToDos();
    }

    private readonly IToDoValidator _toDoValidator;
    private readonly IToDoCache _toDoCache;
    private readonly IUiToDoService _uiToDoService;
    private readonly IStringFormater _stringFormater;
    private readonly IDialogService _dialogService;
    private readonly IAppResourceService _appResourceService;
    private readonly INotificationService _notificationService;
    private readonly IObjectStorage _objectStorage;
}
