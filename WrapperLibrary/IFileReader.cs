using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WrapperLibrary
{
    public interface IFileReader : IDisposable
    {
        int Read(byte[] buffer, int offset, int count);
    }
}
