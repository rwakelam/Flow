using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SynchronisationLibrary
{
    public class SyncFileNotFoundException : Exception
    {
        public SyncFileNotFoundException(string path)
            : base(String.Format("Synchronisation file not found. Path: '{0}'.", path)) 
        { }
    }
    
    public class SyncDirectoryNotFoundException : Exception
    {
        public SyncDirectoryNotFoundException(string path)
            : base(String.Format("Synchronisation directory not found. Path: '{0}'.", path))
        { }
    }
}
