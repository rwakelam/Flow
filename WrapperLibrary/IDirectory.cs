using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WrapperLibrary
{
    public interface IDirectory : IFileSystemObject
    {
        void Create();
        IEnumerable<IFile> GetFiles(string searchPattern = null, 
            FileAttributesWrapper attributes = FileAttributesWrapper.Any);
        IEnumerable<IDirectory> GetDirectories(string searchPattern = null,
            FileAttributesWrapper attributes = FileAttributesWrapper.Any);

        //redundant?
        IFile CreateFile(string name, FileAttributesWrapper attributes, byte[] bytes);
        
        IFile GetFile(string name);
        IDirectory GetSubDirectory(string name);
    }
}
