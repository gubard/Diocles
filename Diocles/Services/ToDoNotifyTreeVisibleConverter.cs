using System.Globalization;
using Avalonia.Data.Converters;
using Diocles.Models;
using Hestia.Contract.Models;

namespace Diocles.Services;

public class ToDoNotifyTreeVisibleConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not ToDoNotify item)
        {
            return value;
        }

        if (item.IsHideOnTree)
        {
            return false;
        }

        if (item.Type == ToDoType.Reference)
        {
            return false;
        }

        return true;
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
