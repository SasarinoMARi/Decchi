using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Decchi
{
	/// <summary>
	/// 이 클래스는 인스턴스를 만들 수 없습니다.
	/// </summary>
	public static class Globals
	{
		private static Dictionary<string, string> settings = new Dictionary<string, string>(); // 설정 뭉텅이
		private const string SettingFilePath = "publish.ini"; // 설정 파일 주소

		/// <summary>
		/// 프로퍼티 뭉텅이를 저장합니다. 설정이 바뀌거나 프로그램이 종료되기 직전에 호출해주세요.
		/// </summary>
		public static void SaveSettings()
		{
			// SIMPLE IS GOOD
			using (var writer = new StreamWriter(SettingFilePath))
			{
				var list = new List<string>(settings.Keys);
				for (int i = 0; i < list.Count; i++)
				{
					writer.WriteLine("{0}={1}", list[i], settings[list[i]]);
				}
			}
		}

		/// <summary>
		/// 프로퍼티 뭉텅이를 읽어옵니다. 프로그램 시작시 반드시 한번 실행해주세요.
		/// 이후 Set / Get Value 메서드를 통해 프로퍼티에 접근하고 종료 전 SaveSetting() 메서드를 호출해주세요
		/// </summary>
		public static void ReadSettings()
		{
			if (File.Exists(SettingFilePath))
			{
				using (var reader = new StreamReader(SettingFilePath))
				{
					string line;
					string[] splitedLine;

					while ((line = reader.ReadLine()) != null)
					{
						splitedLine = line.Split('=');
						settings[splitedLine[0]] = splitedLine[1];
					}
				}
			}
		}

		/// <summary>
		/// 프로퍼티를 초기화합니다.
		/// </summary>
		/// <param name="PropertyName">설정하고자 하는 프로퍼티 이름</param>
		/// <param name="Value">PropertyName에 대응하는 값</param>
		public static void SetValue(string PropertyName, string Value)
		{
			lock (settings)
			{
				if (string.IsNullOrEmpty(Value))
					settings[PropertyName] = Value;
			}
		}

		/// <summary>
		/// 프로퍼티 이름으로 해당하는 값을 얻어옵니다.
		/// </summary>
		/// <param name="PropertyName">찾고자 하는 프로퍼티 이름</param>
		/// <returns>ProtertyName에 대응하는 값</returns>
		public static string GetValue(string PropertyName)
		{
			lock (settings)
			{
				if (settings.ContainsKey(PropertyName))
				{
					return settings[PropertyName];
				}
				else
				{
					return null;
				}
			}
		}

		/// <summary>
		/// 기본 브라우저에서 url을 엽니다.
		/// </summary>
		/// <param name="url">열고자 하는 주소</param>
		public static void OpenWebSite(string url)
		{
			try
			{
				using (Process.Start(new ProcessStartInfo { UseShellExecute = true, FileName = url }))
				{ }
			}
			catch
			{ }
		}
	}
}