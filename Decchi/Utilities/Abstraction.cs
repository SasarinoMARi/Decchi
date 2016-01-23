using System;
using System.IO;

namespace Decchi.Utilities
{
    internal class Abstraction : TagLib.File.IFileAbstraction, IDisposable
    {
        public Abstraction(string path, bool openForWrite = false)
        {
            Name = Path.GetFileName(path);

            this.m_stream = openForWrite ? 
                new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite) :
                new FileStream(path, FileMode.Open, FileAccess.Read,      FileShare.Read);
        }
        ~Abstraction()
        {
            this.Dispose(false);
        }

        private bool m_disposed = false;
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (this.m_disposed) return;
            this.m_disposed = true;

            if (disposing)
            {
                if (this.m_stream != null)
                {
                    try
                    {
                        this.m_stream.Dispose();	
                    }
                    catch
                    { }
                    this.m_stream = null;
                }
            }
        }

        private Stream  m_stream;
        public Stream ReadStream    { get { return this.m_stream; } }
        public Stream WriteStream   { get { return this.m_stream; } }
        public string Name          { get; private set; }

        public void CloseStream(Stream stream)
        {
            if (stream != null)
                stream.Close();
        }
    }
}
