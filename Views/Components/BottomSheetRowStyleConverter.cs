using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace PAGELY.Views.Components;

public class BottomSheetRowStyleConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo? culture)
    {
        if (value is bool isSelected && isSelected)
            return SelectedStyle;
        return DefaultStyle;
    }

    public object ConvertBack(object? value, Type? targetType, object? parameter, CultureInfo? culture)
        => throw new NotImplementedException();

    public static readonly Style DefaultStyle = new Style(typeof(Button))
    {
        Setters =
        {
            new Setter { Property = Button.BackgroundColorProperty, Value = Colors.Transparent },
            new Setter { Property = Button.TextColorProperty, Value = Color.FromArgb("#3A2615") },
            new Setter { Property = Button.FontSizeProperty, Value = 16.0 },
            new Setter { Property = Button.HeightRequestProperty, Value = 52.0 },
            new Setter { Property = Button.PaddingProperty, Value = new Thickness(20, 0, 20, 0) },
            new Setter { Property = Button.HorizontalOptionsProperty, Value = LayoutOptions.FillAndExpand },
            new Setter { Property = Button.VerticalOptionsProperty, Value = LayoutOptions.Center },
        }
    };

    public static readonly Style SelectedStyle = new Style(typeof(Button))
    {
        Setters =
        {
            new Setter { Property = Button.BackgroundColorProperty, Value = Color.FromArgb("#EDE3D2") },
            new Setter { Property = Button.TextColorProperty, Value = Color.FromArgb("#6F4E2C") },
            new Setter { Property = Button.FontSizeProperty, Value = 16.0 },
            new Setter { Property = Button.HeightRequestProperty, Value = 52.0 },
            new Setter { Property = Button.PaddingProperty, Value = new Thickness(20, 0, 20, 0) },
            new Setter { Property = Button.HorizontalOptionsProperty, Value = LayoutOptions.FillAndExpand },
            new Setter { Property = Button.VerticalOptionsProperty, Value = LayoutOptions.Center },
        }
    };
}
