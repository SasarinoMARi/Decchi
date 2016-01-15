using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Decchi.Core.Windows.Converter
{
	public sealed class Visibility2Boolean : IValueConverter
	{
		public Visibility True	{ get; set; }
		public Visibility False	{ get; set; }

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return value is Visibility && (Visibility)value == this.True ? true : false;
		}
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
            //return value is bool && (bool)value ? this.True : this.False;
		}
	}
}
