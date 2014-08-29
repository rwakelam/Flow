using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WrapperLibrary
{
    public interface IFileSystemInfoWrapper
    {
        FileAttributesWrapper Attributes { get; set; }
        DateTime LastWriteTimeUtc { get; set; }
    }
}
