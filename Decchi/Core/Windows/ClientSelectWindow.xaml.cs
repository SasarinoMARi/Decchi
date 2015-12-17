using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using Decchi.ParsingModule;
using MahApps.Metro.Controls;

namespace Decchi.Core.Windows
{
	public partial class ClientSelectWindow : MetroWindow
	{
		public SongInfo SongInfo { get; private set; } 

		private List<SongInfo>		m_songinfo;

		public ClientSelectWindow(SongInfo[] songs)
		{
			InitializeComponent();

			this.m_songinfo = new List<SongInfo>();
			for (int i = 0; i < songs.Length; ++i)
				if (songs[i].Loaded)
					this.m_songinfo.Add(songs[i]);

			this.ctlSongList.ItemsSource = this.m_songinfo;
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			this.SongInfo = (SongInfo)this.ctlSongList.SelectedItem;

			this.Close();
		}
	}
}
