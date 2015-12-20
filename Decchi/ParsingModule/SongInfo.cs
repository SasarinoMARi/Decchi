using System;
using System.Collections.Generic;
using System.Text;

namespace Decchi.ParsingModule
{
    public abstract class SongInfo
    {
        public const string Via = "#뎃찌NP";

        public abstract string Client		{ get; }
        public abstract string ClientIcon	{ get; }

        public string	Title		{ get; protected set; }
        public string	Album		{ get; protected set; }
        public string	Artist		{ get; protected set; }
        public bool		Loaded		{ get; protected set; }

        /// <summary>트윗할 섬네일은 항상 임시 파일로 지정해야합니다</summary>
        public string	Thumbnail	{ get; protected set; }

        public abstract bool GetCurrentPlayingSong( );

        public const string defaultFormat = "{/Artist/의 }{/Title/{ (/Album/)}을/를 }듣고 있어요! {/Via/} - {/Client/} #NowPlaying";

        public static bool CheckFormat(string format)
        {
            try
            {
                ToFormat(format, null, true);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public override string ToString( )
        {
            try
            {
                return ToFormat(Globals.Instance.PublishFormat, this);
            }
            catch
            {
                return null;
            }
        }

        private static string ToFormat(string format, SongInfo info, bool checkFormat = false)
        {
            StringBuilder			total	= !checkFormat ? new StringBuilder() : null;
            
            StringBuilder			sb	= null;
            Queue<StringBuilder>	queue = new Queue<StringBuilder>();
            string					str;

            char c;
            bool b;

            int i = 0;
            while (i < format.Length)
            {
                // { 부터 } 까지다
                c = format[i++];

                switch (c)
                {
                    case '{':
                        {
                            if (sb != null) queue.Enqueue(sb);
                            sb = new StringBuilder();
                        }
                        break;

                    case '}':
                        {
                            str = sb.ToString();

                            b = false;

                            if (checkFormat)
                            {
                                b = str.IndexOf("/Title/")	>= 0 ||
                                    str.IndexOf("/Artist/")	>= 0 ||
                                    str.IndexOf("/Album/")	>= 0 ||
                                    str.IndexOf("/Client/")	>= 0 ||
                                    str.IndexOf("/Via/")	>= 0;

                                if (b)
                                {
                                    if (queue.Count > 0)
                                    {
                                        sb = queue.Dequeue();
                                        sb.Append(str);
                                    }
                                    else
                                    {
                                        sb = null;
                                    }
                                }
                                else
                                {
                                    // { } 안에는 최소한 하나가 있어야함
                                    throw new Exception();
                                }
                            }
                            else
                            {
                                // b -> { } 안에 포멧 변환된게 있음
                                str = Replace(str, "/Title/",	info.Title,		ref b);
                                str = Replace(str, "/Artist/",	info.Artist,	ref b);
                                str = Replace(str, "/Album/",	info.Album,		ref b);
                                str = Replace(str, "/Client/",	info.Client,	ref b);
                                str = Replace(str, "/Via/",		SongInfo.Via,	ref b);

                                if (b)
                                {
                                    if (queue.Count > 0)
                                    {
                                        sb = queue.Dequeue();
                                        sb.Append(str);
                                    }
                                    else
                                    {
                                        total.Append(str);
                                        sb = null;
                                    }
                                }
                                else
                                {
                                    if (queue.Count > 0)
                                        sb = queue.Dequeue();
                                    else
                                        sb = null;
                                }
                            }
                        }
                        break;

                    default:
                        if (sb != null)
                            sb.Append(c);
                        else if (!checkFormat)
                            total.Append(c);
                        break;
                }
            }

            if (checkFormat && (queue.Count > 0 || sb != null))
                throw new Exception();
            
            return !checkFormat ? total.ToString() : null;
        }

        private static string Replace(string str, string find, string replace, ref bool b)
        {
            if (str.IndexOf(find) >= 0 && !string.IsNullOrEmpty(replace))
            {
                b = true;
                return str.Replace(find, replace);
            }
            return str;
        }
    }
}
