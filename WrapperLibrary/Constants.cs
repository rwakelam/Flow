using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace WrapperLibrary
{
    public enum FileAttributesWrapper
    {
        Any = 0,
        Archive = FileAttributes.Archive,
        Normal = FileAttributes.Normal,
        Hidden = FileAttributes.Hidden,
        System = FileAttributes.System

        //  FILE_ATTRIBUTE_ARCHIVE = 32
        //FILE_ATTRIBUTE_COMPRESSED = 2048
        //FILE_ATTRIBUTE_DEVICE = 64
        //FILE_ATTRIBUTE_DIRECTORY = 16
        //FILE_ATTRIBUTE_ENCRYPTED = 16384
        //FILE_ATTRIBUTE_HIDDEN = 2
        //FILE_ATTRIBUTE_NORMAL = 128
        //FILE_ATTRIBUTE_NOT_CONTENT_INDEXED = 8192
        //FILE_ATTRIBUTE_OFFLINE = 4096
        //FILE_ATTRIBUTE_READONLY = 1
        //FILE_ATTRIBUTE_REPARSE_POINT = 1024
        //FILE_ATTRIBUTE_SPARSE_FILE = 512
        //FILE_ATTRIBUTE_SYSTEM = 4
        //FILE_ATTRIBUTE_TEMPORARY = 256
        //FILE_ATTRIBUTE_VIRTUAL = 65536
    }
}
