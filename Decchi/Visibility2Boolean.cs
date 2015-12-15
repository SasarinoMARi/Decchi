using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Decchi
{
	public class Visibility2Boolean : IValueConverter
	{
		public Visibility WhenTrue	{ get; set; }
		public Visibility WhenFalse	{ get; set; }

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return value is Visibility && (Visibility)value == this.WhenTrue ? true : false;
		}
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return value is bool && (bool)value ? this.WhenTrue : this.WhenFalse;
		}
	}
}
