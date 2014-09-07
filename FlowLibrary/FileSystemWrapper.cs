using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using WrapperLibrary;

namespace FlowLibrary
{

    public class FileSystemWrapper : IFileSystemWrapper
    {
        public const string MatchAllFilesPattern = "*.*";
        public const string MatchAllDirectoriesPattern = "*";
        private static FileSystemWrapper _singleton = null;

        private FileSystemWrapper() 
        { }

        public static IFileSystemWrapper GetFileSystemWrapper() 
        {
            if (_singleton == null) 
            {
                _singleton = new FileSystemWrapper();
            }
            return _singleton;
        }

        public bool FileExists(string path) 
        {
            return File.Exists(path);
        }
        
        public IFileSystemInfoWrapper GetFileInfo(string path)
        {
            return new FileSystemInfoWrapper(new FileInfo(path));
        }
        
        public IFileSystemInfoWrapper GetDirectoryInfo(string path)
        {
            return new FileSystemInfoWrapper(new DirectoryInfo(path));
        }

        public void DeleteFile(string path)
        {
            File.Delete(path);
        }

        public void CopyFile(string sourcePath, string targetPath, bool overwrite)
        {
            File.Copy(sourcePath, targetPath, overwrite);    
        }
        
        public bool DirectoryExists(string path)
        {
            return Directory.Exists(path);
        }

        //public void CopyFile(string sourcePath, string targetPath, bool overwrite)
        //{
        //    //Directory.Copy(sourcePath, targetPath, overwrite);
        //}
        
        public void DeleteDirectory(string path, bool recursive)
        {
            Directory.Delete(path, recursive);
        }
        
        public void CreateDirectory(string path)
        {
            Directory.CreateDirectory(path);
        }

        public string[] GetDirectories(string path, string pattern = null, bool includeSubDirectories = false)
        {
            if (String.IsNullOrEmpty(pattern))
            {
                pattern = MatchAllDirectoriesPattern;
            }
            return Directory.GetDirectories(path, pattern, 
                includeSubDirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
        }

        public IEnumerable<string> EnumerateFiles(string path, string pattern = MatchAllFilesPattern, bool includeSubDirectories = false)
        {
            if (String.IsNullOrEmpty(pattern))
            {
                pattern = MatchAllFilesPattern;
            }
            return Directory.EnumerateFiles(path, pattern,
                includeSubDirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
        }

        public string BuildPath(string path1, string path2)
        {
            return Path.Combine(path1, path2);
        }

        public string GetFileName(string path)
        {
            return Path.GetFileName(path);
        }

        public string GetFileExtension(string path)
        {
            return Path.GetExtension(path);
        }

        public bool DirectoryContains(string directoryPath, string path) 
        {         
            return path.StartsWith(directoryPath);            
        }
    }
}
