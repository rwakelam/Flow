using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WrapperLibrary
{
    public interface IFile : IFileSystemObject
    {
        void WriteAllBytes(byte[] bytes);
        byte[] ReadAllBytes();
        Int64 Length { get; }
        IFileReader GetReader();
    }
}
