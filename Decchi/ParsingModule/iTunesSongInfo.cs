using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using iTunesLib;

namespace Decchi.ParsingModule
{
	public sealed class iTunesSongInfo : SongInfo
	{
		public override string Client { get { return "iTunes"; } }
		public override string ClientIcon { get { return "/Decchi;component/ParsingModule/IconImages/ITunes.png"; } }

		public override bool GetCurrentPlayingSong()
		{
			var hwnd = NativeMethods.FindWindow("iTunes", null);
			if (hwnd == IntPtr.Zero) return false;

			try
			{
				var itunes = new iTunesAppClass();
				var track = itunes.CurrentTrack;

				this.Title	= track.Name;
				this.Album	= track.Album;
				this.Artist	= track.Artist;
				this.Loaded	= true;

				if (track.Artwork.Count > 0)
				{
					var artwork		= track.Artwork[1];
					var thumbnail	= Path.GetTempFileName();

					switch (artwork.Format)
					{
						case ITArtworkFormat.ITArtworkFormatBMP:
							artwork.SaveArtworkToFile(thumbnail);
							using (var image = Image.FromFile(thumbnail))
								image.Save(thumbnail, ImageFormat.Png);
							break;

						case ITArtworkFormat.ITArtworkFormatJPEG:
						case ITArtworkFormat.ITArtworkFormatPNG:
							artwork.SaveArtworkToFile(thumbnail);
							break;
					}

					this.Thumbnail = thumbnail;
				}

				return true;
			}
			catch
			{
				return false;
			}
		}
	}
}
