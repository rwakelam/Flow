using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WrapperLibrary;
using SynchronisationLibrary;

namespace SynchronisationTest
{
    internal class MockDirectory : MockFile
    {
        public readonly Dictionary<string, MockFile> Files = new Dictionary<string, MockFile>();
        public readonly Dictionary<string, MockDirectory> Directories = new Dictionary<string, MockDirectory>();
                
        public MockDirectory(FileAttributesWrapper attributes, DateTime lastWriteTimeUtc) : base(attributes, lastWriteTimeUtc)
        {
        }
    }
}
