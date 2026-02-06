using System.Globalization;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Diocles.Helpers;
using Diocles.Models;
using Gaia.Helpers;
using Hestia.Contract.Models;
using Inanna.Models;
using LiveMarkdown.Avalonia;

namespace Diocles.Services;

public sealed class DescriptionToolTipConverter : IValueConverter
{
    public static readonly DescriptionToolTipConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not ToDoNotify item)
        {
            return value;
        }

        if (item.Description.IsNullOrWhiteSpace())
        {
            return null;
        }

        if (parameter is not StringCutParameters p)
        {
            return item;
        }

        return item.DescriptionType switch
        {
            DescriptionType.PlainText => new TextBlock
            {
                Classes = { "h6" },
                Text = p.Cut(item.Description),
            },
            DescriptionType.Markdown => new MarkdownRenderer
            {
                MarkdownBuilder = new ObservableStringBuilder().AppendStr(p.Cut(item.Description)),
            },
            _ => item,
        };
    }

    public object? ConvertBack(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture
    )
    {
        throw new NotSupportedException();
    }
}
