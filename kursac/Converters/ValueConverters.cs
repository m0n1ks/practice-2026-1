using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using FamilyBudget.Models;

namespace FamilyBudget.Converters
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object v, Type t, object p, CultureInfo c)
            => v is bool b && b ? Visibility.Visible : Visibility.Collapsed;
        public object ConvertBack(object v, Type t, object p, CultureInfo c) => throw new NotImplementedException();
    }

    public class InverseBoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object v, Type t, object p, CultureInfo c)
            => v is bool b && b ? Visibility.Collapsed : Visibility.Visible;
        public object ConvertBack(object v, Type t, object p, CultureInfo c) => throw new NotImplementedException();
    }

    public class TransactionTypeToColorConverter : IValueConverter
    {
        public object Convert(object v, Type t, object p, CultureInfo c)
        {
            if (v is TransactionType tt)
                return tt == TransactionType.Income
                    ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#66BB6A"))
                    : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EF5350"));
            return Brushes.White;
        }
        public object ConvertBack(object v, Type t, object p, CultureInfo c) => throw new NotImplementedException();
    }

    public class TransactionTypeBgConverter : IValueConverter
    {
        public object Convert(object v, Type t, object p, CultureInfo c)
        {
            if (v is TransactionType tt)
                return tt == TransactionType.Income
                    ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1B3A1B"))
                    : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3A1515"));
            return Brushes.Transparent;
        }
        public object ConvertBack(object v, Type t, object p, CultureInfo c) => throw new NotImplementedException();
    }

    public class HexToBrushConverter : IValueConverter
    {
        public object Convert(object v, Type t, object p, CultureInfo c)
        {
            try { return new SolidColorBrush((Color)ColorConverter.ConvertFromString(v?.ToString() ?? "#E91E8C")); }
            catch { return new SolidColorBrush(Colors.HotPink); }
        }
        public object ConvertBack(object v, Type t, object p, CultureInfo c) => throw new NotImplementedException();
    }

    public class ProgressToWidthConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type t, object p, CultureInfo c)
        {
            if (values.Length >= 2
                && values[0] is double progress
                && values[1] is double totalWidth)
                return Math.Max(0, Math.Min(totalWidth, totalWidth * progress));
            return 0.0;
        }
        public object[] ConvertBack(object v, Type[] t, object p, CultureInfo c) => throw new NotImplementedException();
    }

    public class DecimalToMoneyConverter : IValueConverter
    {
        public object Convert(object v, Type t, object p, CultureInfo c)
            => v is decimal d ? $"{d:N0} ₽" : "0 ₽";
        public object ConvertBack(object v, Type t, object p, CultureInfo c) => throw new NotImplementedException();
    }

    public class BalanceToColorConverter : IValueConverter
    {
        public object Convert(object v, Type t, object p, CultureInfo c)
        {
            if (v is decimal d)
                return d >= 0
                    ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#66BB6A"))
                    : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EF5350"));
            return Brushes.White;
        }
        public object ConvertBack(object v, Type t, object p, CultureInfo c) => throw new NotImplementedException();
    }
}
