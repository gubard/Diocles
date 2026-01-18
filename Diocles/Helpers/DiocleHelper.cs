using Avalonia.Collections;
using Diocles.Models;
using Gaia.Helpers;
using IconPacks.Avalonia.MaterialDesign;
using Inanna.Models;
using Inanna.Services;

namespace Diocles.Helpers;

public static class DiocleHelper
{
    private static readonly IAppResourceService AppResourceService =
        DiHelper.ServiceProvider.GetService<IAppResourceService>();

    public static readonly StringCutParameters DesktopDescriptionStringParameters = new(5, 100);
    public static readonly StringCutParameters AndroidDescriptionStringParameters = new(1, 20);

    public static IAvaloniaReadOnlyList<InannaCommand> CreateMultiCommands(
        IEnumerable<ToDoNotify> parameter
    )
    {
        return new AvaloniaList<InannaCommand>()
        {
            new(
                DioclesCommands.ShowDeleteToDosCommand,
                parameter,
                AppResourceService.GetResource<string>("Lang.Delete"),
                PackIconMaterialDesignKind.Delete,
                ButtonType.Danger
            ),
            new(
                DioclesCommands.ShowEditToDosCommand,
                parameter,
                AppResourceService.GetResource<string>("Lang.Edit"),
                PackIconMaterialDesignKind.Edit
            ),
            new(
                DioclesCommands.ShowChangesParentCommand,
                parameter,
                AppResourceService.GetResource<string>("Lang.ChangeParent"),
                PackIconMaterialDesignKind.AccountTree
            ),
        };
    }
}
