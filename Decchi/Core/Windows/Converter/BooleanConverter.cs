using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Decchi.Core.Windows.Converter
{
	public sealed class BooleanConverter : IValueConverter
	{
		public object True	{ get; set; }
		public object False	{ get; set; }

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return value is bool && (bool)value ? this.True : this.False;
		}
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
            return value == this.True ? true : false;
		}
	}
}
