using System;
using System.Runtime.InteropServices;

namespace Decchi.Utilities.InteropServices
{
    public abstract class UnmanagedMemoryBase : IDisposable
    {
        public IntPtr   Handle { get; protected set; }
        public int      Length { get; protected set; } 

        ~UnmanagedMemoryBase()
        {
            this.Dispose(false);
        }

        private bool m_disposed = false;
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected void Dispose(bool disposing)
        {
            if (this.m_disposed) return;
            this.m_disposed = true;

            if (disposing)
                this.Free();
        }

        protected virtual void Free()
        {
            if (this.Handle != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(this.Handle);
                this.Handle = IntPtr.Zero;
                this.Length = -1;
            }
        }

        public static implicit operator IntPtr(UnmanagedMemoryBase alloc)
        {
            return alloc.Handle;
        }
    }

    public sealed class UnmanagedMemory : UnmanagedMemoryBase
    {
        public UnmanagedMemory(int size)
        {
            this.Reallocate(size);
        }
        public void Reallocate(int size)
        {
            if (size <= 0) throw new ArgumentOutOfRangeException();

            this.Free();

            try
            {
                this.Length = size;
                this.Handle = Marshal.AllocHGlobal(this.Length);
            }
            catch (Exception e)
            {
                this.Free();
                throw e;
            }
        }

        public UnmanagedMemory(byte[] array)
        {
            this.Reallocate(array);
        }
        public void Reallocate(byte[] array)
        {
            if (array == null || array.Length == 0) throw new ArgumentNullException();

            this.Free();

            try
            {
                this.Length = array.Length;
                this.Handle = Marshal.AllocHGlobal(this.Length);
                Marshal.Copy(array, 0, this.Handle, this.Length);
            }
            catch (Exception e)
            {
                this.Free();
                throw e;
            }
        }
    }

    public sealed class UnmanagedStruct<T> : UnmanagedMemoryBase
        where T: struct
    {
        public UnmanagedStruct()
        {
            try
            {
                this.Length = Marshal.SizeOf(typeof(T));
                this.Handle = Marshal.AllocHGlobal(this.Length);
            }
            catch (Exception e)
            {
                this.Free();
                throw e;
            }
        }
        public UnmanagedStruct(T obj)
            : this()
        {
            Marshal.StructureToPtr(obj, this.Handle, false);
        }

        public void Reallocate()
        {
            this.Free();

            try
            {
                this.Length = Marshal.SizeOf(typeof(T));
                this.Handle = Marshal.AllocHGlobal(this.Length);
            }
            catch (Exception e)
            {
                this.Free();
                throw e;
            }
        }
        public void Reallocate(T obj)
        {
            this.Reallocate();
            Marshal.StructureToPtr(obj, this.Handle, false);
        }

        public T PtrToStructure()
        {
            return (T)Marshal.PtrToStructure(this.Handle, typeof(T));
        }
    }
}
