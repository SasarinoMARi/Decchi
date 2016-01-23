using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Decchi.Core.Windows.Converter
{
	public sealed class Visibility2Boolean : IValueConverter
	{
		public bool Visible	  { get; set; }
		public bool Hidden	  { get; set; }
		public bool Collapsed { get; set; }

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
            var v = (Visibility)value;
            if (v == Visibility.Visible)   return this.Visible;
            if (v == Visibility.Hidden)    return this.Hidden;
            if (v == Visibility.Collapsed) return this.Collapsed;
            return null;
		}
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
		}
	}
}
