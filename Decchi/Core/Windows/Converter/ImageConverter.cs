using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Decchi.Core.Windows.Converter
{
	public sealed class ImageConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			try
			{
				var bitmap = new BitmapImage();
				bitmap.CacheOption = BitmapCacheOption.OnDemand;
				bitmap.BeginInit();
				bitmap.UriSource = new Uri((string)value);
				bitmap.EndInit();

				return bitmap;
			}
			catch
			{
				return null;
			}
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
