using System.Globalization;
using Avalonia.Data.Converters;
using Gaia.Helpers;
using Inanna.Models;

namespace Diocles.Services;

public class DescriptionToolTipConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string str)
        {
            return value;
        }

        if (str.IsNullOrWhiteSpace())
        {
            return null;
        }

        if (parameter is not StringCutParameters p)
        {
            return str;
        }

        return p.Cut(str);
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
