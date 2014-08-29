using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace WrapperLibrary
{
    public class FileReader : IFileReader // make idisposable
    {
        private FileStream _stream;
        private bool _disposed = false;

        public FileReader(string path)
        { 
            _stream = new FileStream(path, FileMode.Open, FileAccess.Read);
        }

        public int Read(byte[] buffer, int offset, int count)
        {
            return _stream.Read(buffer, offset, count);
        }
                
        ~FileReader()
        {
            Dispose(false);
        }

        public void Dispose() 
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _stream.Close();
                    _stream.Dispose();
                }
                _disposed = true;
            }
        }
    }
}
