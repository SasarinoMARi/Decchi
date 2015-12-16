using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Decchi.Core.Windows.Converter
{
	public sealed class Boolean2Visibility : IValueConverter
	{
		public Visibility TrueVisibility	{ get; set; }
		public Visibility FalseVisibility	{ get; set; }

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return value is bool && (bool)value == true ? this.TrueVisibility : this.FalseVisibility;
		}
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return value is Visibility && (Visibility)value == this.TrueVisibility ? true : false;
		}
	}
}
