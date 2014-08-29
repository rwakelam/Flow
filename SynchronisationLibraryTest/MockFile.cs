using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SynchronisationLibrary;
using WrapperLibrary;

namespace SynchronisationTest
{
    internal class MockFile : IFileSystemInfoWrapper
    {
        public FileAttributesWrapper Attributes { get; set; }
        public DateTime LastWriteTimeUtc { get; set; }

        public MockFile(FileAttributesWrapper attributes, DateTime lastWriteTimeUtc) 
        {
            Attributes = attributes;
            LastWriteTimeUtc = lastWriteTimeUtc;
        }
    }
}
