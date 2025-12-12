using Diocles.Ui;
using Gaia.Services;
using Hestia.Contract.Services;
using Inanna.Models;

namespace Diocles.Services;

public interface
    IDioclesViewModelFactory : IFactory<(ValidationMode validationMode, bool isShowEdit), ToDoParametersViewModel>;

public class DioclesViewModelFactory : IDioclesViewModelFactory
{
    private readonly IToDoValidator _toDoValidator;

    public DioclesViewModelFactory(IToDoValidator toDoValidator)
    {
        _toDoValidator = toDoValidator;
    }

    public ToDoParametersViewModel Create((ValidationMode validationMode, bool isShowEdit ) value)
    {
        return new(_toDoValidator, value.validationMode, value.isShowEdit);
    }
}