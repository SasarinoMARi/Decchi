using System;
using System.Windows.Forms;

namespace Decchi
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main( )
		{
            Globals.ReadSettings();
			Application.EnableVisualStyles( );
			Application.SetCompatibleTextRenderingDefault( false );
			DecchiCore.Instance.Login( );
            Globals.SaveSettings( );

            Application.Run(new Main());
		}

		
	}
}
