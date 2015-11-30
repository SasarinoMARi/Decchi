using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;


namespace Decchi
{
    public class Globals
    {
        private static Dictionary<string, string> settings = new Dictionary<string, string>(); // 설정 뭉텅이
        private const string SettingFilePath = "publish.ini"; // 설정 파일 주소

        /// <summary>
        /// 이 클래스는 인스턴스를 만들 수 없습니다.
        /// </summary>
        private Globals()
        {

        }

        /// <summary>
        /// 프로퍼티 뭉텅이를 저장합니다. 설정이 바뀌거나 프로그램이 종료되기 직전에 호출해주세요.
        /// </summary>
        public static void SaveSettings()
        {
            using (var writeStream = new FileStream(SettingFilePath, FileMode.Create))
            {
                using (var writer = new StreamWriter(writeStream))
                {
                    var list = new List<string>(settings.Keys);
                    for (int i = 0; i < list.Count; i++)
                    {
                        writer.WriteLine(string.Format("{0}={1}", list[i], settings[list[i]]));
                    }
                }
            }
        }

        /// <summary>
        /// 프로퍼티 뭉텅이를 읽어옵니다. 프로그램 시작시 반드시 한번 실행해주세요.
        /// 이후 Set / Get Value 메서드를 통해 프로퍼티에 접근하고 종료 전 SaveSetting() 메서드를 호출해주세요
        /// </summary>
        public static void ReadSettings()
        {
            using (var readStream = new FileStream(SettingFilePath, FileMode.OpenOrCreate))
            {
                using (var reader = new StreamReader(readStream))
                {
                    while (true)
                    {
                        var line = reader.ReadLine();
                        if (string.IsNullOrEmpty(line)) break;

                        var splitedLine = line.Split('=');
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
            settings[PropertyName] = Value;
        }

        /// <summary>
        /// 프로퍼티 이름으로 해당하는 값을 얻어옵니다.
        /// </summary>
        /// <param name="PropertyName">찾고자 하는 프로퍼티 이름</param>
        /// <returns>ProtertyName에 대응하는 값</returns>
        public static string GetValue(string PropertyName)
        {
            if (settings.Keys.Contains(PropertyName))
            {
                return settings[PropertyName];
            }
            else
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// 기본 브라우저에서 url을 엽니다.
        /// </summary>
        /// <param name="url">열고자 하는 주소</param>
        public static void OpenWebSite(string url)
        {
            Process myProcess = new Process();
            try
            {
                myProcess.StartInfo.UseShellExecute = true;
                myProcess.StartInfo.FileName = url;
                myProcess.Start();
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        /// <summary>
        /// 네트워크 상에서 Image객체를 얻어옵니다
        /// </summary>
        /// <param name="Url">이미지 파일의 주소</param>
        /// <returns>이미지 객체</returns>
        public static Image GetImageFromUrl(string Url)
        {
            HttpWebRequest httpWebRequest = (HttpWebRequest)HttpWebRequest.Create(Url);
            HttpWebResponse httpWebReponse = (HttpWebResponse)httpWebRequest.GetResponse();
            Stream stream = httpWebReponse.GetResponseStream();
            return Image.FromStream(stream);
        }


        /// <summary>
        /// 클래스 이름과 타이틀로 윈도우 핸들 값을 얻어옵니다
        /// </summary>
        /// <param name="strClassName">찾을 윈도우의 클래스 네임</param>
        /// <param name="strWindowName">찾을 윈도우의 타이틀</param>
        /// <returns>윈도우 핸들 값</returns>
        [DllImport("User32.dll")]
        public static extern IntPtr FindWindow(string strClassName, string strWindowName);
    }
}