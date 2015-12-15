using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Decchi.ParsingModule;
using MahApps.Metro.Controls;

namespace Decchi.Core.Windows
{
	public partial class ClientSelectWindow : MetroWindow
	{
		private Action<SongInfo>	m_callback;
		private List<SongInfo>		m_songinfo;

		public ClientSelectWindow(SongInfo[] songs, Action<SongInfo> callback)
		{
			InitializeComponent();

			this.m_callback	= callback;

			this.m_songinfo = new List<SongInfo>();
			for (int i = 0; i < songs.Length; ++i)
				if (songs[i].Loaded)
					this.m_songinfo.Add(songs[i]);

			this.ctlSongList.ItemsSource = this.m_songinfo;
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			if (this.m_callback != null && this.ctlSongList.SelectedIndex != -1)
				this.m_callback.Invoke((SongInfo)this.ctlSongList.SelectedItem);

			this.Close();
		}
	}
}
