using CommunityToolkit.Mvvm.Input;

namespace Diocles.Models;

public interface IToDosView
{
    IToDosViewModel ViewModel { get; }
}

public interface IToDosViewModel
{
    bool IsMulti { get; }
    IAsyncRelayCommand<ToDoNotify> ShowEditCommand { get; }
}
