// https://github.com/RyuaNerin/ResizeTo3MB
// 

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace Decchi.Utilities
{
    public static class ImageResize
    {
        public static Stream LoadImageResized(Stream orig, int targetSize = 3145728) // 3 MB
        {
            if (orig == null || orig.Length < targetSize) return orig;

            var stream = new MemoryStream(3 * 1024 * 1024);

            using (var image = Image.FromStream(orig))
            {
                ImageCodecInfo		codec;
                EncoderParameters	param;

                if (image.RawFormat.Guid == ImageFormat.Jpeg.Guid || !IsImageTransparent(image))
                {
                    codec = ImageCodecInfo.GetImageDecoders().First(e => e.FormatID == ImageFormat.Jpeg.Guid);
                    param = new EncoderParameters(1);

                    long quality = 90;
                    if (image.PropertyIdList.Any(e => e == 0x5010)) quality = image.PropertyItems.First(e => e.Id == 0x5010).Value[0];

                    param.Param[0] = new EncoderParameter(Encoder.Quality, quality);

                    ResizeJpg(image, stream, codec, param, targetSize);
                }
                else
                {
                    codec = ImageCodecInfo.GetImageDecoders().First(e => e.FormatID == ImageFormat.Png.Guid);
                    param = new EncoderParameters(1);
                    param.Param[0] = new EncoderParameter(Encoder.ColorDepth, Bitmap.GetPixelFormatSize(image.PixelFormat));

                    ResizePng(image, stream, codec, param, targetSize);
                }
            }

            orig.Dispose();

            stream.Seek(0, SeekOrigin.Begin);
            return stream;
        }

        private static void ResizeJpg(Image image, Stream stream, ImageCodecInfo codec, EncoderParameters param, int targetSize)
        {
            int w = image.Width;
            int h = image.Height;

            do
            {
                ResizeBySize(image, stream, codec, param, w, h);

                w = (int)(w * 0.9);
                h = (int)(h * 0.9);
            }
            while (stream.Length > targetSize);
        }

        private static void ResizePng(Image image, Stream stream, ImageCodecInfo codec, EncoderParameters param, int targetSize)
        {
            int w, h;

            GetSizeFromPixels(targetSize * param.Param[0].NumberOfValues / 8 * 2, image.Width, image.Height, out w, out h);

            do
            {
                ResizeBySize(image, stream, codec, param, w, h);

                w = (int)(w * 0.9);
                h = (int)(h * 0.9);
            }
            while (stream.Length > targetSize);
        }

        private static void GetSizeFromPixels(int pixels, int oriW, int oriH, out int newW, out int newH)
        {
            newW = (int)Math.Ceiling(Math.Sqrt(pixels * oriW / oriH));
            newH = (int)Math.Ceiling(Math.Sqrt(pixels * oriH / oriW));

            if (newW > oriW) newW = oriW;
            if (newH > oriH) newH = oriH;
        }

        private static void ResizeBySize(Image image, Stream stream, ImageCodecInfo codec, EncoderParameters param, int w, int h)
        {
            using (Image imageNew = new Bitmap(w, h, image.PixelFormat))
            {
                using (Graphics g = Graphics.FromImage(imageNew))
                {
                    foreach (PropertyItem propertyItem in image.PropertyItems)
                        imageNew.SetPropertyItem(propertyItem);

                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    g.SmoothingMode = SmoothingMode.AntiAlias;

                    g.DrawImage(image, 0, 0, w, h);
                }

                stream.SetLength(0);
                imageNew.Save(stream, codec, param);
            }
        }

        private static PixelFormat[] formatsWithAlpha =
		{
			PixelFormat.Indexed,				PixelFormat.Gdi,				PixelFormat.Alpha,
			PixelFormat.PAlpha,					PixelFormat.Canonical,			PixelFormat.Format1bppIndexed,
			PixelFormat.Format4bppIndexed,		PixelFormat.Format8bppIndexed,	PixelFormat.Format16bppArgb1555,
			PixelFormat.Format32bppArgb,		PixelFormat.Format32bppPArgb,	PixelFormat.Format64bppArgb,
			PixelFormat.Format64bppPArgb
		};
        private static bool IsImageTransparent(Image image)
        {
            bool isTransparent = false;

            if (formatsWithAlpha.Contains(image.PixelFormat))
            {
                Bitmap bitmap = image as Bitmap;
                BitmapData binaryImage = bitmap.LockBits(new Rectangle(Point.Empty, bitmap.Size), ImageLockMode.ReadOnly, PixelFormat.Format64bppArgb);

                int x, y;

                for (y = 0; y < bitmap.Height; ++y)
                {
                    for (x = 0; x < bitmap.Width; ++x)
                    {
                        if (bitmap.GetPixel(x, y).A != 255)
                        {
                            isTransparent = true;
                            break;
                        }
                    }

                    if (isTransparent)
                        break;
                }
            }

            return isTransparent;
        }
    }
}
