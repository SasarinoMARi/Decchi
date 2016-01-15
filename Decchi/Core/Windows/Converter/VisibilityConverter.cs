using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Decchi.Core.Windows.Converter
{
	public sealed class VisibilityConverter : IValueConverter
	{
        public Visibility Visible   { get; set; }
		public Visibility Hidden	{ get; set; }
		public Visibility Collapsed	{ get; set; }

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
            var visibility = (Visibility)value;

            if (visibility == Visibility.Visible)
                return this.Visible;

            if (visibility == Visibility.Hidden)
                return this.Hidden;
            
            return this.Collapsed;
		}
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
            /*
            var visibility = (Visibility)value;

            if (visibility == this.Visible)
                return Visibility.Visible;

            if (visibility == this.Hidden)
                return Visibility.Hidden;

            return Visibility.Collapsed;
            */
		}
	}
}
