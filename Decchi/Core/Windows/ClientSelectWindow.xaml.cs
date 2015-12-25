using System.Collections.Generic;
using System.Windows;
using Decchi.ParsingModule;
using MahApps.Metro.Controls;

namespace Decchi.Core.Windows
{
	public partial class ClientSelectWindow : MetroWindow
	{
		public	SongInfo		SongInfo	{ get; private set; }
		private	List<SongInfo>	m_songinfo;

		public ClientSelectWindow()
		{
			InitializeComponent();

			this.m_songinfo = new List<SongInfo>();
			for (int i = 0; i < SongInfo.SongInfos.Length; ++i)
				if (SongInfo.SongInfos[i].Loaded)
					this.m_songinfo.Add(SongInfo.SongInfos[i]);

			this.ctlSongList.ItemsSource = this.m_songinfo;
		}

		private void ctlSongList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
		{
			this.ctlSelectClient.IsEnabled = true;
		}

		private void ctlSelectClient_Click(object sender, RoutedEventArgs e)
		{
			this.SongInfo = (SongInfo)this.ctlSongList.SelectedItem;

			this.Close();
		}
	}
}
