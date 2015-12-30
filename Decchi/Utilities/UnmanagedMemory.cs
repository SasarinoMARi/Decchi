using System;
using System.Runtime.InteropServices;

namespace Decchi.Utilities
{
    public sealed class UnmanagedMemory : IDisposable
    {
        public IntPtr   Handle { get; private set; }
        public int      Length { get; private set; } 

        public UnmanagedMemory(int size)
        {
            this.Reallocate(size);
        }
        public void Reallocate(int size)
        {
            if (size <= 0) throw new ArgumentOutOfRangeException();

            this.Free();

            this.Length = size;
            this.Handle = Marshal.AllocHGlobal(this.Length);
        }

        public UnmanagedMemory(object obj)
        {
            this.Reallocate(obj);
        }
        public void Reallocate(object obj)
        {
            if (obj == null) throw new ArgumentNullException();

            this.Free();

            this.Length = Marshal.SizeOf(obj);
            this.Handle = Marshal.AllocHGlobal(this.Length);
            Marshal.StructureToPtr(obj, this.Handle, false);
        }

        public UnmanagedMemory(byte[] array)
        {
            this.Reallocate(array);
        }
        public void Reallocate(byte[] array)
        {
            if (array == null || array.Length == 0) throw new ArgumentNullException();

            this.Free();

            this.Length = array.Length;
            this.Handle = Marshal.AllocHGlobal(this.Length);
            Marshal.Copy(array, 0, this.Handle, this.Length);
        }

        ~UnmanagedMemory()
        {
            this.Dispose(false);
        }

        private bool m_disposed = false;
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }
        private void Dispose(bool disposing)
        {
            if (this.m_disposed) return;
            this.m_disposed = true;

            if (disposing)
                this.Free();
        }

        private void Free()
        {
            if (this.Handle != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(this.Handle);
                this.Handle = IntPtr.Zero;
                this.Length = -1;
            }
        }

        public static implicit operator IntPtr(UnmanagedMemory alloc)
        {
            return alloc.Handle;
        }
    }
}
