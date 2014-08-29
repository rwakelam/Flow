using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WrapperLibrary
{
    public interface IFileSystemObject//IEntry
    {
        string Name { get; }
        string Path { get; }
        DateTime LastWriteTimeUtc { get; set; }
        FileAttributesWrapper Attributes { get; set; }
        bool Exists();
        void Delete();
        //IFile Copy(IDirectory directory, bool overwrite = false);
    }
}
