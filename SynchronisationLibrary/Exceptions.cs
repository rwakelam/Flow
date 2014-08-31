using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SynchronisationLibrary
{
    public class SyncFileNotFoundException : FileNotFoundException
    {
        public SyncFileNotFoundException(string path)
            : base(String.Format("File not found. Path: '{0}'.", path)) 
        { }
    }
    
    public class SyncDirectoryNotFoundException : DirectoryNotFoundException
    {
        public SyncDirectoryNotFoundException(string path)
            : base(String.Format("Directory not found. Path: '{0}'.", path))
        { }
    }
}
