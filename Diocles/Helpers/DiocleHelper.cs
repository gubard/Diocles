using Inanna.Models;

namespace Diocles.Helpers;

public static class DiocleHelper
{
    public static readonly StringCutParameters DesktopDescriptionStringParameters = new(5, 100);
    public static readonly StringCutParameters AndroidDescriptionStringParameters = new(1, 20);
}
