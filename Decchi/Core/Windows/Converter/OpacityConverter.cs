using System;
using System.Globalization;
using System.Windows.Data;

namespace Decchi.Core.Windows.Converter
{
    public sealed class PersentConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
            return (int)((double)value * 100);
		}
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
            throw new NotImplementedException();
            //return (int)value / 10d
		}
	}
}
