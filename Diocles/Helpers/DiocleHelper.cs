using Diocles.Models;
using IconPacks.Avalonia.MaterialDesign;
using Inanna.Models;

namespace Diocles.Helpers;

public static class DiocleHelper
{
    public static readonly StringCutParameters DesktopDescriptionStringParameters = new(5, 100);
    public static readonly StringCutParameters AndroidDescriptionStringParameters = new(1, 20);

    public static IEnumerable<InannaCommand> CreateMultiCommands(IEnumerable<ToDoNotify> parameter)
    {
        return
        [
            new(
                DioclesCommands.DeleteToDosCommand,
                parameter,
                PackIconMaterialDesignKind.Delete,
                ButtonType.Danger
            ),
            new(DioclesCommands.EditToDosCommand, parameter, PackIconMaterialDesignKind.Edit),
        ];
    }
}
