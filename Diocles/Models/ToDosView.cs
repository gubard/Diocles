namespace Diocles.Models;

public interface IToDosView
{
    IToDosViewModel ViewModel { get; }
}

public interface IToDosViewModel
{
    bool IsMulti { get; }
}
