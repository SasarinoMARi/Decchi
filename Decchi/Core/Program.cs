using System;
using System.Windows.Forms;
using System.Reflection;

namespace Decchi
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
 			AppDomain.CurrentDomain.AssemblyResolve += (s, e) =>
			{
				var eInfo = new AssemblyName(e.Name);

				var currentAssembly = Assembly.GetExecutingAssembly();

				foreach (string resourceName in currentAssembly.GetManifestResourceNames())
				{
					if (resourceName.Contains(eInfo.Name))
					{
						using (var stream = currentAssembly.GetManifestResourceStream(resourceName))
						{
							byte[] buff = new byte[stream.Length];
							stream.Read(buff, 0, buff.Length);

							return Assembly.Load(buff);
						}
					}
				}

				return null;
 			};

			Globals.ReadSettings();
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			DecchiCore.Login();
			Globals.SaveSettings();

			Application.Run(new Main());
		}
		
	}
}
