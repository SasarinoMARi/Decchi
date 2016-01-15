using System;
using System.Globalization;
using System.Windows.Data;

namespace Decchi.Core.Windows.Converter
{
	public sealed class Boolean2Double : IValueConverter
	{
		public double True	{ get; set; }
		public double False	{ get; set; }

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return value is bool && (bool)value == true ? this.True : this.False;
		}
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
            throw new NotImplementedException();
			//return value is double && (double)value == this.True ? true : false;
		}
	}
}
