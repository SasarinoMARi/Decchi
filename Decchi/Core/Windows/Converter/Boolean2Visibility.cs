using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Decchi.Core.Windows.Converter
{
	public sealed class Boolean2Visibility : IValueConverter
	{
		public Visibility True	{ get; set; }
		public Visibility False	{ get; set; }

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return value is bool && (bool)value == true ? this.True : this.False;
		}
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
            throw new NotImplementedException();
			//return value is Visibility && (Visibility)value == this.True ? true : false;
		}
	}
}
