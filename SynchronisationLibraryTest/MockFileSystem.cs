using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using SynchronisationLibrary;
using WrapperLibrary;

namespace SynchronisationTest
{
    internal class MockFileSystem : IFileSystemWrapper
    {
        public const string MatchAllFilesPattern = "^.*\\..*$";
        public const string MatchAllDirectoriesPattern = "^.*$";
        private MockDirectory _rootDirectory;

        public MockDirectory RootDirectory 
        {
            get
            {
                return _rootDirectory;
            }
            private set
            {
                _rootDirectory = value;
            }
        }

        public MockFileSystem(MockDirectory rootDirectory)
        {
            RootDirectory = rootDirectory;
        }
        
        public bool FileExists(string path)
        {
            MockFile file = null;
            try
            {
                file = GetFile(path);
            }
            catch 
            { 
            }
            return file != null;
        }

        public IFileSystemInfoWrapper GetFileInfo(string path)
        {
            return GetFile(path);
        }
        
        public void DeleteFile(string path)
        {
            GetFile(path);
            string fileName = ParseBranch(ref path);
            MockDirectory parentDirectory = GetDirectory(path);
            parentDirectory.Files.Remove(fileName);
        }

        public void CopyFile(string sourcePath, string targetPath, bool overwrite)
        {
            MockFile sourceFile = GetFile(sourcePath);
            if (FileExists(targetPath)) 
            {
                if (overwrite)
                {
                    DeleteFile(targetPath);
                }
                else 
                {
                    throw new Exception();
                }
            }
            string targetFileName = ParseBranch(ref targetPath);
            MockDirectory targetDirectory = GetDirectory(targetPath);
            targetDirectory.Files.Add(targetFileName, new MockFile(sourceFile.Attributes, sourceFile.LastWriteTimeUtc));
        }

        public bool DirectoryContains(string path, string path2)
        { return false; }

        public bool DirectoryExists(string path)
        {
            MockDirectory directory = null;
            try
            {
                directory = GetDirectory(path);
            }
            catch
            {
            }
            return directory != null;
        }

        public void DeleteDirectory(string path, bool recursive)// TODO: check what does recursive means in this context??
        {
            GetDirectory(path);
            string name = ParseBranch(ref path);
            MockDirectory parentDirectory = GetDirectory(path);
            parentDirectory.Directories.Remove(name);            
        }

        public IFileSystemInfoWrapper GetDirectoryInfo(string path)
        {
            return GetDirectory(path);
        }

        public void CreateDirectory(string path)
        {
            string name = ParseBranch(ref path);
            MockDirectory parentDirectory = GetDirectory(path);
            parentDirectory.Directories.Add(name, new MockDirectory(FileAttributesWrapper.Normal, DateTime.UtcNow)); 
        }

        public string[] GetDirectories(string path, string pattern = null, bool includeSubDirectories = false)
        {
            MockDirectory parentDirectory = GetDirectory(path);
            if (String.IsNullOrEmpty(pattern))
            {
                pattern = MatchAllDirectoriesPattern;
            }
            Regex regex = new Regex(pattern);
            return parentDirectory.Directories.Keys.Where(k => regex.IsMatch(k)).Select(
                k => string.Format(@"{0}\{1}", path, k)).ToArray();
            // TODO:: include subdirectories?
        }

        public IEnumerable<string> EnumerateFiles(string path, string pattern = null, bool includeSubDirectories = false)
        {
            MockDirectory parentDirectory = GetDirectory(path);            
            if (String.IsNullOrEmpty(pattern))
            {
                pattern = MatchAllFilesPattern;
            }
            Regex regex = new Regex(pattern);
            return parentDirectory.Files.Keys.Where(k => regex.IsMatch(k)).Select(
                k => string.Format(@"{0}\{1}", path, k)).ToArray();
            // TODO:: include subdirectories?
        }

        private MockFile GetFile(string path)
        {
            string fileName = ParseBranch(ref path);
            return GetChildFile(GetDirectory(path), fileName);
        }

        private MockDirectory GetDirectory(string path)
        {
            MockDirectory parentDirectory = _rootDirectory;
            while (!String.IsNullOrEmpty(path))
            {
                parentDirectory = GetChildDirectory(parentDirectory, ParseRoot(ref path));
            }
            return parentDirectory;
        }

        private MockDirectory GetChildDirectory(MockDirectory parentDirectory, string childName)
        {
            if (!parentDirectory.Directories.ContainsKey(childName))
            {
                throw new DirectoryNotFoundException();
            }
            return parentDirectory.Directories[childName];
        }

        private MockFile GetChildFile(MockDirectory parentDirectory, string childName)
        {
            if (!parentDirectory.Files.ContainsKey(childName))
            {
                throw new FileNotFoundException();
            }
            return parentDirectory.Files[childName];
        }

        private string ParseRoot(ref string path)
        {
            string root;
            int index = path.IndexOf("\\");
            if (index == -1)
            {
                root = path;
                path = String.Empty;
                return root;
            }
            root = path.Substring(0, index);
            path = path.Substring(index + 1);
            return root;
        }

        private string ParseBranch(ref string path)
        {
            string branch;
            int index = path.LastIndexOf("\\");
            if (index == -1)
            {
                branch = path;
                path = String.Empty;
                return branch;
            }
            branch = path.Substring(index + 1);
            path = path.Substring(0, index);
            return branch;
        }
        
        public string BuildPath(string path1, string path2)
        {
            if (String.IsNullOrEmpty(path1))
            {
                return path2;
            }
            if (String.IsNullOrEmpty(path2))
            {
                return path1;
            }
            return String.Join("\\", path1, path2);
        }

        public string GetFileName(string path)
        {
            return ParseBranch(ref path);
        }

        public string GetFileExtension(string path)
        {
            return "";// Path.GetExtension(path);
        }
    }   
}
