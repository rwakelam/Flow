using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using WrapperLibrary;

namespace FlowLibrary
{
    class FileSystemInfoWrapper : IFileSystemInfoWrapper
    {
        private FileSystemInfo _core;
        
        public FileSystemInfoWrapper(FileSystemInfo core) 
        {
            if (core == null)
            {
                throw new ArgumentNullException("core");
            }
            _core = core;
        }

        public FileAttributesWrapper Attributes 
        { 
            get
            {
                return (FileAttributesWrapper)_core.Attributes;
            }
            set
            {
                _core.Attributes = (FileAttributes)value;
            }
        }

        public DateTime LastWriteTimeUtc 
        {
            get
            {
                return _core.LastWriteTimeUtc;
            }
            set 
            {
                _core.LastWriteTimeUtc = value;
            }
        }
    }
}
