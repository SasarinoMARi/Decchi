// By RyuaNerin

using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interactivity;
using System.Windows.Interop;

namespace Decchi.Utilities
{
    internal class MangeticWindowBehavior : Behavior<Window>
    {
        private MagneticWindow m_window = new MagneticWindow();

        private static void PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var behavior = d as MangeticWindowBehavior;

            if (e.Property == GapProp)
                behavior.m_window.Gap = (int)((double)e.NewValue);

            else if (e.Property == IsEnabledProp)
                behavior.m_window.IsEnabled = (bool)e.NewValue;
        }

        private static readonly DependencyProperty IsEnabledProp = DependencyProperty.Register("IsEnabled", typeof(bool), typeof(MangeticWindowBehavior), new FrameworkPropertyMetadata(false, MangeticWindowBehavior.PropertyChangedCallback));
        public bool IsEnabled
        {
            get { return (bool)this.GetValue(IsEnabledProp); }
            set { this.SetValue(IsEnabledProp, value); }
        }

        private static readonly DependencyProperty GapProp = DependencyProperty.Register("Gap", typeof(double), typeof(MangeticWindowBehavior), new FrameworkPropertyMetadata(5d, MangeticWindowBehavior.PropertyChangedCallback));
        public double Gap
        {
            get { return (double)this.GetValue(GapProp); }
            set { this.SetValue(GapProp, value); }
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            this.m_window.AssignHandle(this.AssociatedObject);
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            if (this.m_window != null)
                this.m_window.ReleaseHandle();
        }
    }

    internal class MagneticWindow : NativeWindow
    {
        private class NativeMethods
        {
            [StructLayout(LayoutKind.Sequential)]
            public struct WINDOWPOS
            {
                public IntPtr hwnd;
                public IntPtr hwndInsertAfter;
                public int x;
                public int y;
                public int cx;
                public int cy;
                public uint flags;
            }
        }

        private Window m_window;
        private IntPtr m_handle;

        private bool m_isEnabled = false;
        public bool IsEnabled
        {
            get { return this.m_isEnabled; }
            set { this.m_isEnabled = value; }
        }

        private int m_gap = 5;
        public int Gap
        {
            get { return this.m_gap; }
            set { this.m_gap = value; }
        }

        public void AssignHandle(Window window)
        {
            this.m_window = window;

            var helper = new WindowInteropHelper(window);
            if (helper.Handle == IntPtr.Zero)
                helper.EnsureHandle();

            this.m_handle = helper.Handle;

            this.AssignHandle(helper.Handle);
        }

        private const int WM_WINDOWPOSCHANGING = 0x0046;
        protected override void WndProc(ref Message m)
        {
            if (this.m_isEnabled && m.Msg == WM_WINDOWPOSCHANGING)
            {
                var screen = (Screen.FromHandle(this.m_handle)).WorkingArea;
                var winPos = (NativeMethods.WINDOWPOS)m.GetLParam(typeof(NativeMethods.WINDOWPOS));
                if (winPos.cx > 0 && winPos.cy > 0)
                {
                    if (Math.Abs(winPos.x - screen.Left) <= this.m_gap) winPos.x = screen.Left;
                    if (Math.Abs(winPos.y - screen.Top)  <= this.m_gap) winPos.y = screen.Top;

                    if (Math.Abs(winPos.x + winPos.cx - screen.Right)  <= this.m_gap) winPos.x = screen.Right  - winPos.cx;
                    if (Math.Abs(winPos.y + winPos.cy - screen.Bottom) <= this.m_gap) winPos.y = screen.Bottom - winPos.cy;

                    Marshal.StructureToPtr(winPos, m.LParam, false);

                    m.Result = new IntPtr(0);

                    return;
                }
            }
            
            base.WndProc(ref m);
        }
    }
}
