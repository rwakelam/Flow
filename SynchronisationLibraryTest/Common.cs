using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using WrapperLibrary;

namespace SynchronisationTest
{
    public class Common
    {
        public const string SourceDirectoryPath = @"C:\Temp\Source";
        public const string TargetDirectoryPath = @"C:\Temp\Target";
        public const string SubDirectoryName = "SubDirectory";
        public const string SourceSubDirectoryPath = SourceDirectoryPath + @"\" + SubDirectoryName;
        public const string TargetSubDirectoryPath = TargetDirectoryPath + @"\" + SubDirectoryName;

        public static IDirectory CreateDirectory(string path, FileAttributesWrapper attributes = FileAttributesWrapper.Normal)
        {
            DirectoryWrapper directory = new DirectoryWrapper(path); //TODO:: replace with MockFile
            if (directory.Exists())
            {
                directory.Delete();
            }
            directory.Create();
            directory.Attributes = attributes;
            return directory;
        }

        public static IFile CreateFile(string name, string directory, byte[] bytes, FileAttributesWrapper attributes = FileAttributesWrapper.Normal)
        {
            FileWrapper file = new FileWrapper(Path.Combine(directory, name));//TODO:: replace with MockFile
            if (file.Exists())
            {
                file.Delete();
            }
            file.WriteAllBytes(bytes);
            file.Attributes = attributes;
            return file;
        }
    }
}
