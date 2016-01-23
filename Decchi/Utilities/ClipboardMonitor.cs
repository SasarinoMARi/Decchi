using System;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;

namespace Decchi.Utilities
{
    internal sealed class ClipboardMonitor : NativeWindow
    {
        public event EventHandler ClipboardChanged;
        
        private IntPtr m_handle;

        public ClipboardMonitor(Window window)
        {
            var helper = new WindowInteropHelper(window);
            if (helper.Handle == IntPtr.Zero) helper.EnsureHandle();

            this.m_handle = helper.Handle;

            this.AssignHandle(this.m_handle);
            NativeMethods.AddClipboardFormatListener(this.m_handle);
        }

        public override void ReleaseHandle()
        {
            NativeMethods.RemoveClipboardFormatListener(this.Handle);
            base.ReleaseHandle();
        }

        //private const int WM_CLIPBOARDUPDATE = 0x031D;
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0x031D) //WM_CLIPBOARDUPDATE
                if (this.ClipboardChanged != null)
                    this.ClipboardChanged.Invoke(this, new EventArgs());

            base.WndProc(ref m);
        }
    }
}
