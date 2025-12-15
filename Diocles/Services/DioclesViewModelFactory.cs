using Diocles.Models;
using Diocles.Ui;
using Gaia.Services;
using Hestia.Contract.Services;
using Inanna.Models;
using Inanna.Services;

namespace Diocles.Services;

public interface
    IDioclesViewModelFactory : IFactory<(ValidationMode validationMode, bool isShowEdit), ToDoParametersViewModel>,
    IFactory<ToDoTreeViewModel>, IFactory<ToDoNotify, ToDosViewModel>;

public class DioclesViewModelFactory : IDioclesViewModelFactory
{
    private readonly IToDoValidator _toDoValidator;
    private readonly IToDoCache _toDoCache;
    private readonly IUiToDoService _uiToDoService;
    private readonly IStringFormater _stringFormater;
    private readonly IDialogService _dialogService;
    private readonly IAppResourceService _appResourceService;

    public DioclesViewModelFactory(IToDoValidator toDoValidator, IToDoCache toDoCache, IUiToDoService uiToDoService,
        IStringFormater stringFormater, IDialogService dialogService, IAppResourceService appResourceService)
    {
        _toDoValidator = toDoValidator;
        _toDoCache = toDoCache;
        _uiToDoService = uiToDoService;
        _stringFormater = stringFormater;
        _dialogService = dialogService;
        _appResourceService = appResourceService;
    }

    public ToDoParametersViewModel Create((ValidationMode validationMode, bool isShowEdit ) value)
    {
        return new(_toDoValidator, this, value.validationMode, value.isShowEdit);
    }

    public ToDoTreeViewModel Create()
    {
        return new(_toDoCache, _uiToDoService);
    }

    public ToDosViewModel Create(ToDoNotify input)
    {
        return new(input, _uiToDoService, _toDoCache, _stringFormater, _dialogService, _appResourceService, this);
    }
}