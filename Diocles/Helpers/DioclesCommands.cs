using System.Windows.Input;
using Diocles.Models;
using Diocles.Services;
using Gaia.Helpers;
using Inanna.Helpers;
using Inanna.Services;

namespace Diocles.Helpers;

public static class DioclesCommands
{
    static DioclesCommands()
    {
        var navigator = DiHelper.ServiceProvider.GetService<INavigator>();
        var dioclesViewModelFactory =
            DiHelper.ServiceProvider.GetService<IDioclesViewModelFactory>();

        OpenToDosCommand = UiHelper.CreateCommand<ToDoNotify>(
            (item, ct) => navigator.NavigateToAsync(dioclesViewModelFactory.Create(item), ct)
        );
    }

    public static readonly ICommand OpenToDosCommand;
}
