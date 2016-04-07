using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using Decchi.Core;
using iTunesLib;

namespace Decchi.ParsingModule.Rules
{
    internal sealed class iTunes : IParseRule
    {
        public iTunes() : base(            
            new IParseRuleOption
            {
                Client      = "iTunes",
                ParseFlag   = ParseFlags.ManualParse,
                WndClass    = "iTunes",
                WndClassTop = true,
                ClientIcon  = "itunes"
            })
        { }
        
        private iTunesApp m_itunes;

        private bool m_ad;
        private Timer m_timerDetect;
        private Timer m_timer;
        private IITTrack m_adTrack;

        private bool Init()
        {
            if (this.m_itunes == null)
            {
                // 아이튠즈가 실행중인지 확인
                if (this.GetWindowHandle() != IntPtr.Zero)
                {
                    try
                    {
                        this.m_itunes = new iTunesAppClass();
                        this.m_itunes.OnQuittingEvent += new _IiTunesEvents_OnQuittingEventEventHandler(this.m_itunes_OnQuittingEvent);
                        this.m_itunes.OnPlayerPlayEvent += new _IiTunesEvents_OnPlayerPlayEventEventHandler(this.iTunes_SongChanged);
                        this.m_itunes.OnPlayerPlayingTrackChangedEvent += new _IiTunesEvents_OnPlayerPlayingTrackChangedEventEventHandler(this.iTunes_SongChanged);
                        return true;
                    }
                    catch
                    { }
                }
            }
            else
            {
                // 아이튠즈가 실행중인지 확인
                if (this.GetWindowHandle() != IntPtr.Zero)
                    return true;
                else
                    this.DeleteITunes();
            }

            return false;
        }

        private void DeleteITunes()
        {
            try
            {
                this.m_itunes.OnQuittingEvent -= this.m_itunes_OnQuittingEvent;
                this.m_itunes.OnPlayerPlayEvent -= this.iTunes_SongChanged;
                this.m_itunes.OnPlayerPlayingTrackChangedEvent -= this.iTunes_SongChanged;
            }
            catch
            { }

            try
            {
                Marshal.ReleaseComObject(this.m_itunes);
            }
            catch
            { }

            GC.Collect();
            this.m_itunes = null;

            if (this.m_ad)
            {
                this.m_timer.Change(Timeout.Infinite, Timeout.Infinite);
                this.m_timerDetect.Change(IParseRule.RefreshTimeSpan, IParseRule.RefreshTimeSpan);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.DeleteITunes();
            }
        }

        public override bool ParseManual(SongInfo si, IntPtr hwnd)
        {
            if (!this.Init()) return false;

            try
            {
                if (Get(si, hwnd, this.m_itunes.CurrentTrack))
                    return true;
                else if (!string.IsNullOrWhiteSpace(this.m_itunes.CurrentStreamTitle))
                {
                    si.Title = this.m_itunes.CurrentStreamTitle;
                    if (!string.IsNullOrWhiteSpace(this.m_itunes.CurrentStreamURL))
                    {
                        try
                        {
                        	var uri = new Uri(this.m_itunes.CurrentStreamURL);
                            si.Url = this.m_itunes.CurrentStreamURL;
                        }
                        catch
                        { }
                    }

                    return true;
                }
            }
            catch
            { }

            return false;
        }

        private bool Get(SongInfo si, IntPtr hwnd, IITTrack track)
        {
            if (this.m_itunes.PlayerState != ITPlayerState.ITPlayerStatePlaying || track == null)
                return false;

            if (si == null)
                si = new SongInfo(this);

            si.Title    = track.Name;
            si.Album    = track.Album;

                 if (!string.IsNullOrWhiteSpace(track.Artist))   si.Artist = track.Artist;
            else if (!string.IsNullOrWhiteSpace(track.Composer)) si.Artist = track.Composer;

            si.Track    = track.TrackNumber > 0 ? unchecked((uint)track.TrackNumber) : 0;
            si.TTrack   = track.TrackCount  > 0 ? unchecked((uint)track.TrackCount)  : 0;
            si.Disc     = track.DiscNumber  > 0 ? unchecked((uint)track.DiscNumber)  : 0;
            si.TDisc    = track.DiscCount   > 0 ? unchecked((uint)track.DiscCount)   : 0;
            si.Year     = track.Year        > 0 ? unchecked((uint)track.Year)        : 0;
            si.Genre    = track.Genre;

            si.Handle   = hwnd;

            if (track.Artwork.Count > 0)
            {
                var artwork = track.Artwork[1];
                var temp    = Path.GetTempFileName();

                try
                {
                    artwork.SaveArtworkToFile(temp);

                    si.Cover = new MemoryStream(64 * 1024); // 64 KiB
                    using (var image = Image.FromFile(temp))
                        image.Save(si.Cover, ImageFormat.Png);
                    si.Cover.Seek(0, SeekOrigin.Begin);
                }
                catch
                {
                    if (si.Cover != null)
                        si.Cover.Dispose();
                }
                finally
                {
                    if (!string.IsNullOrWhiteSpace(temp) && File.Exists(temp))
                        File.Delete(temp);
                }
            }

            if (track.Kind == ITTrackKind.ITTrackKindFile)
            {
                var fileTrack = track as IITFileOrCDTrack;
                if (fileTrack != null && !string.IsNullOrWhiteSpace(fileTrack.Location))
                    si.Local = fileTrack.Location;
            }

            return true;
        }

        public override void EnableAD()
        {
            if (this.m_ad) return;
            this.m_ad = true;
            
            this.m_timer = new Timer(timer_Callback);
            this.m_timerDetect = new Timer(timerDetect_Callback);

            if (!this.Init())
            {
                this.m_timerDetect.Change(IParseRule.RefreshTimeSpan, IParseRule.RefreshTimeSpan);
            }
            else
            {
                // 실행중인 아이튠즈를 찾았는데 이미 재생중인 경우 바로 트윗할 수 있도록
                if (this.m_itunes.PlayerState == ITPlayerState.ITPlayerStatePlaying)
                {
                    iTunes_SongChanged(this.m_itunes.CurrentTrack);
                    this.m_timer.Change(IParseRule.RefreshTimeSpan, System.Threading.Timeout.Infinite);
                }
            }
        }

        private void timerDetect_Callback(object state)
        {
            if (this.Init())
            {
                this.m_timer.Change(Timeout.Infinite, Timeout.Infinite);
                this.m_timerDetect.Change(Timeout.Infinite, Timeout.Infinite);
            }
        }

        private void m_itunes_OnQuittingEvent()
        {
            // 종료시에 항상 호출된다고 보장할 수 없음. 타이머 초기화는 DeleteITunes에서 하는게 안전함
            this.DeleteITunes();
        }

        private void iTunes_SongChanged(object iTrack)
        {
            if (!this.m_ad) return;

            this.m_adTrack = iTrack as IITTrack;

            // 30 초 이내로 변경되지 않은 경우에만 트윗
            if (this.m_adTrack != null)
                this.m_timer.Change(IParseRule.RefreshTimeSpan, System.Threading.Timeout.Infinite);
        }

        private void timer_Callback(object state)
        {
            if (this.m_adTrack == null) return;

            var si = new SongInfo(this);
            if (this.Get(si, IntPtr.Zero, this.m_adTrack))
                DecchiCore.Run(si);
        }

        public override void DisableAD()
        {
            if (!this.m_ad) return;
            this.m_ad = false;
            
            if (this.m_timer == null)
            {
                this.m_timer.Change(Timeout.Infinite, Timeout.Infinite);
                this.m_timer.Dispose();
                this.m_timer = null;
            }
            if (this.m_timerDetect == null)
            {
                this.m_timerDetect.Change(Timeout.Infinite, Timeout.Infinite);
                this.m_timerDetect.Dispose();
                this.m_timerDetect = null;
            }
        }
    }
}
