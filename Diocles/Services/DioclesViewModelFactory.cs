using Diocles.Ui;
using Gaia.Services;
using Hestia.Contract.Services;
using Inanna.Models;

namespace Diocles.Services;

public interface
    IDioclesViewModelFactory : IFactory<(ValidationMode validationMode, bool isShowEdit), ToDoParametersViewModel>,
    IFactory<ToDoTreeViewModel>;

public class DioclesViewModelFactory : IDioclesViewModelFactory
{
    private readonly IToDoValidator _toDoValidator;
    private readonly IToDoCache _toDoCache;
    private readonly IUiToDoService _uiToDoService;

    public DioclesViewModelFactory(IToDoValidator toDoValidator, IToDoCache toDoCache, IUiToDoService uiToDoService)
    {
        _toDoValidator = toDoValidator;
        _toDoCache = toDoCache;
        _uiToDoService = uiToDoService;
    }

    public ToDoParametersViewModel Create((ValidationMode validationMode, bool isShowEdit ) value)
    {
        return new(_toDoValidator, this, value.validationMode, value.isShowEdit);
    }

    public ToDoTreeViewModel Create()
    {
        return new(_toDoCache, _uiToDoService);
    }
}