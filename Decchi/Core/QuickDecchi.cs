using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Decchi.Core.Windows;
using Decchi.ParsingModule;
using Decchi.PublishingModule.Twitter;

namespace Decchi.Core
{

	class QuickDecchi
	{
		public static bool isQuickRun { get; private set; }

		public static void Run( )
		{
			isQuickRun = true;
			var infos = SongInfo.GetCurrentPlayingSong();
			Globals.Instance.LoadSettings( );
			TwitterCommunicator.Instance.Login( );

			bool success = false;

			if ( infos.Count >= 2 )
			{
				if ( !Globals.Instance.AutoSelect )
				{
					// 여기 어쩌지
					return;
				}

				App.Debug( "AutoSelect" );

				var topHwnd = NativeMethods.GetTopMostWindow(infos.Select(e => e.Handle).ToArray());
				App.Debug( "topHwnd : " + topHwnd.ToString( "X8" ) );

				var top     = infos.Where(e => e.Handle == topHwnd).ToArray();
				App.Debug( "topWindow Count : " + top.ToString( ) );

				if ( top.Length == 1 )
					success = TwitterCommunicator.Instance.Publish( top[0] );
				else
				{
					var main = top.Where(e => e.MainTab).ToArray();
					if ( main.Length == 1 )
						success = TwitterCommunicator.Instance.Publish( main[0] );
					else
						success = TwitterCommunicator.Instance.Publish( top[0] );
				}
			}
			else
			{
				if ( infos.Count == 0 )
				{
					success = true;
				}
				else
				{
					success = TwitterCommunicator.Instance.Publish( infos[0] );
				}
			}
		}
	}
}
