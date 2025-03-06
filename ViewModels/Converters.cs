using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace TimeTracker.ViewModels
{
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not bool isVisible)
                return Visibility.Collapsed;
                
            bool invert = parameter != null && parameter.ToString()?.ToLower() == "true";
            
            if (invert)
                isVisible = !isVisible;
                
            return isVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not Visibility visibility)
                return false;
                
            bool invert = parameter != null && parameter.ToString()?.ToLower() == "true";
            bool result = visibility == Visibility.Visible;
            
            if (invert)
                result = !result;
                
            return result;
        }
    }

    [ValueConversion(typeof(object), typeof(Visibility))]
    public class NullToVisibilityConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            bool isNull = value == null;
            bool invert = parameter != null && parameter.ToString()?.ToLower() == "true";
            
            if (invert)
                isNull = !isNull;
                
            return isNull ? Visibility.Visible : Visibility.Collapsed;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return null; // Can't convert back
        }
    }

    [ValueConversion(typeof(string), typeof(Visibility))]
    public class StringToVisibilityConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            string? str = value as string;
            bool isEmpty = string.IsNullOrWhiteSpace(str);
            bool invert = parameter != null && parameter.ToString()?.ToLower() == "true";
            
            if (invert)
                isEmpty = !isEmpty;
                
            return isEmpty ? Visibility.Collapsed : Visibility.Visible;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return null; // Can't convert back
        }
    }
} 