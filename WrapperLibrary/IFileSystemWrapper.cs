using System;
using System.Collections.Generic;

namespace WrapperLibrary
{
    public interface IFileSystemWrapper
    {
        void CopyFile(string sourcePath, string targetPath, bool overwrite);
        void CreateDirectory(string path);
        void DeleteDirectory(string path, bool recursive);
        void DeleteFile(string path);
        bool DirectoryExists(string path);
        bool FileExists(string path);
        IFileSystemInfoWrapper GetFileInfo(string path);
        IFileSystemInfoWrapper GetDirectoryInfo(string path);
        string[] GetDirectories(string path, string pattern = null, bool includeSubDirectories = false);
        IEnumerable<string> EnumerateFiles(string path, string pattern= null, bool includeSubDirectories = false);
        string BuildPath(string path1, string path2);
        string GetFileName(string path);
        string GetFileExtension(string path);
        //string MatchAllFilesPattern { get; }
        //string MatchAllDirectoriesPattern { get; }
        bool DirectoryContains(string directoryPath, string path);
    }
}
