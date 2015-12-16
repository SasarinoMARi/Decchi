using System.Windows;

namespace Decchi.Core
{
	internal partial class App : Application
	{
		private void Application_Exit(object sender, ExitEventArgs e)
		{
			Globals.SaveSettings();
		}
	}
}
