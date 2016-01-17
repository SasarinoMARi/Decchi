using System;
using System.Globalization;
using System.Windows.Data;

namespace Decchi.Core.Windows.Converter
{
	public sealed class Int2Double : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return (double)((int)value);
		}
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (int)((double)value);
		}
	}
}
