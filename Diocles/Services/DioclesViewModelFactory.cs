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
        IFactory<ToDoNotify, ToDosViewModel>,
        IFactory<ToDoNotify, EditToDoViewModel>
{
    ToDosViewModel CreateToDos(ToDoNotify item);
    EditToDoViewModel CreateEditToDo(ToDoNotify item);
}

public class DioclesViewModelFactory : IDioclesViewModelFactory
{
    private readonly IToDoValidator _toDoValidator;
    private readonly IToDoCache _toDoCache;
    private readonly IUiToDoService _uiToDoService;
    private readonly IStringFormater _stringFormater;
    private readonly IDialogService _dialogService;
    private readonly IAppResourceService _appResourceService;

    public DioclesViewModelFactory(
        IToDoValidator toDoValidator,
        IToDoCache toDoCache,
        IUiToDoService uiToDoService,
        IStringFormater stringFormater,
        IDialogService dialogService,
        IAppResourceService appResourceService
    )
    {
        _toDoValidator = toDoValidator;
        _toDoCache = toDoCache;
        _uiToDoService = uiToDoService;
        _stringFormater = stringFormater;
        _dialogService = dialogService;
        _appResourceService = appResourceService;
    }

    public ToDoParametersViewModel Create((ValidationMode validationMode, bool isShowEdit) input)
    {
        return new(input.validationMode, input.isShowEdit, _toDoValidator, this);
    }

    public ToDoTreeViewModel Create()
    {
        return new(_toDoCache, _uiToDoService);
    }

    ToDosViewModel IFactory<ToDoNotify, ToDosViewModel>.Create(ToDoNotify input)
    {
        return CreateToDos(input);
    }

    EditToDoViewModel IFactory<ToDoNotify, EditToDoViewModel>.Create(ToDoNotify input)
    {
        return CreateEditToDo(input);
    }

    public ToDosViewModel CreateToDos(ToDoNotify item)
    {
        return new(
            item,
            _uiToDoService,
            _toDoCache,
            _stringFormater,
            _dialogService,
            _appResourceService,
            this
        );
    }

    public EditToDoViewModel CreateEditToDo(ToDoNotify item)
    {
        return new(item, this);
    }

    public ToDoParametersViewModel Create(
        (ToDoNotify item, ValidationMode validationMode, bool isShowEdit) input
    )
    {
        return new(input.item, input.validationMode, input.isShowEdit, _toDoValidator, this);
    }
}
