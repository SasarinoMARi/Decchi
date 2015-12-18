using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Threading;

namespace Decchi
{
	public class TrayIcon
	{
		private System.Windows.Forms.NotifyIcon trayIcon;
		private const int taryIconShowInterval = 2;
		Action TrayClickcallback;

		public void Create( Action onTrayiconClick )
		{
			//make up tray icon
			trayIcon = new System.Windows.Forms.NotifyIcon( );
			trayIcon.Text = Process.GetCurrentProcess( ).ProcessName;
			trayIcon.Icon = System.Drawing.Icon.ExtractAssociatedIcon( Assembly.GetExecutingAssembly( ).Location );
			trayIcon.Click += m_notifyIcon_Click;

			//make up test timer.
			DispatcherTimer timer = new DispatcherTimer();
			timer.Interval = new TimeSpan( 0, 0, taryIconShowInterval );
			timer.Tick += timer_Tick;
			timer.Start( );

			TrayClickcallback = onTrayiconClick;
		}

		void timer_Tick( object sender, EventArgs e )
		{
			//if you want alert something , use this method. and change Text
			trayIcon.BalloonTipTitle = Process.GetCurrentProcess( ).ProcessName;
			trayIcon.BalloonTipText = "트레이에서 실행 중이에요!";
		}

		public void Show( bool show )
		{
			if ( show )
			{
				trayIcon.Visible = true;
				trayIcon.ShowBalloonTip( taryIconShowInterval * 1000 );
			}
			else
			{
				trayIcon.Visible = false;
			}
		}

		void m_notifyIcon_Click( object sender, EventArgs e )
		{
			if ( TrayClickcallback != null )
			{
				TrayClickcallback( );
			}
		}

	}
}
