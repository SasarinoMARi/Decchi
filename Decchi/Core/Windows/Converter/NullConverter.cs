using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Decchi.Core.Windows.Converter
{
	public sealed class NullConverter : IValueConverter
    {
        public object IsNull    { get; set; }
        public object IsNotNull { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null ? this.IsNull : this.IsNotNull;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
	}
}
