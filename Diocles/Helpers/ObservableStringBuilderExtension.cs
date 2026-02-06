using LiveMarkdown.Avalonia;

namespace Diocles.Helpers;

public static class ObservableStringBuilderExtension
{
    public static ObservableStringBuilder AppendStr(
        this ObservableStringBuilder builder,
        string value
    )
    {
        builder.Append(value);

        return builder;
    }
}
