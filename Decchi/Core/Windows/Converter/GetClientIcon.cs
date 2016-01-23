using System;
using System.Globalization;
using System.Windows.Data;
using Decchi.ParsingModule;

namespace Decchi.Core.Windows.Converter
{
	public sealed class GetClientIcon : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
            var rule = value as IParseRule;
            return rule != null ? rule.ClientIcon : null;
		}
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
		}
	}
}
