using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using WrapperLibrary;
using System.IO.Abstractions;
using System.IO;

namespace SynchronisationLibrary
{

    public class Cleaner
    {

        #region Classes

        public class Result
        {
            public List<string> MatchedFiles = new List<string>();
            public List<string> DeletedEntries = new List<string>(); // NB can include both files and directories. Cleaned?
            public Dictionary<string, Exception> FailedEntries = new Dictionary<string, Exception>(); // NB can include both files and directories.
            public Dictionary<string, Result> DirectoryResults = new Dictionary<string, Result>();
        }
        
        public class DirectoryCleanedEventArgs : EntryEventArgs
        {
            #region Fields

            private Result _result;

            #endregion

            #region Properties

            public Result Result
            {
                get
                {
                    return _result;
                }
                private set
                {
                    if (value == null)
                    {
                        throw new ArgumentNullException("Result", "Result null.");
                    }
                    _result = value;
                }
            }

            #endregion

            #region Methods

            public DirectoryCleanedEventArgs(string path, Result result)
                : base(path)
            {
                Result = result;
            }

            #endregion

        }

        #endregion

        #region Events

        public delegate void EntryEventHandler(EntryEventArgs e);
        public static event EntryEventHandler FileMatched;
        public static event EntryEventHandler EntryDeleted;// split into dedicated File and Directory events?
        public delegate void DirectoryCleanedEventHandler(DirectoryCleanedEventArgs e);
        public static event DirectoryCleanedEventHandler DirectoryCleaned;
        public delegate void EntryErrorEventHandler(EntryErrorEventArgs e);
        public static event EntryErrorEventHandler Error;

        #endregion
        
        #region Methods
                        
        public static Result Clean(IFileSystem fileSystem, string targetPath, string sourcePath, 
            string pattern = null, FileAttributes attributes = FileAttributes.Normal)
        {
            // If the target path is null, throw an exception.
            if (targetPath == null)
            {
                throw new ArgumentNullException("targetPath", "TargetPath null.");
            }

            // If the source path is null, throw an exception.
            if (sourcePath == null)
            {
                throw new ArgumentNullException("sourcePath", "SourcePath null.");
            }
            
            // If the target directory doesn't exist, throw an exception.
            if (!fileSystem.Directory.Exists(targetPath))
            {
                throw new SyncDirectoryNotFoundException(targetPath);
            }

            // If the source directory doesn't exist, throw an exception.
            if (!fileSystem.Directory.Exists(sourcePath))
            {
                throw new SyncDirectoryNotFoundException(sourcePath);
            }

            Result result = new Result();
            IEnumerable<string> sourceFilePaths = fileSystem.Directory.GetFiles(sourcePath); 
            IEnumerable<string> sourceDirectoryPaths = fileSystem.Directory.GetDirectories(sourcePath);
           
            // Loop through each file in the target directory.
            foreach (string targetFilePath in fileSystem.Directory.GetFiles(targetPath, pattern))
            {
                FileInfoBase targetFileInfo = fileSystem.FileInfo.FromFileName(targetFilePath);
                if ((targetFileInfo.Attributes & attributes) != attributes)
                {
                    continue;
                }

                // If it matches a file in the source directory, skip it.
                string sourceFilePath = Path.Combine(sourcePath, targetFileInfo.Name);//use FullName?? check if Name includes Extension
                if (fileSystem.File.Exists(sourceFilePath))
                {
                    result.MatchedFiles.Add(targetFileInfo.Name);
                    RaiseEntryEvent(FileMatched, targetFilePath);
                    continue;
                }
                // Otherwise, try to delete it.
                try
                {
                    targetFileInfo.Delete();
                    RaiseEntryEvent(EntryDeleted, targetFilePath);
                    result.DeletedEntries.Add(targetFileInfo.Name);
                }
                catch (Exception ex)
                {
                    RaiseErrorEvent(targetFilePath, ex);
                    result.FailedEntries.Add(targetFileInfo.Name, ex);
                }
            }

            // Loop through each sub-directory in the target directory.
            foreach (string targetDirectoryPath in fileSystem.Directory.GetDirectories(targetPath))
            {
                // If it matches a subdirectory in the source, clean it as well.
                DirectoryInfoBase targetDirectoryInfo = fileSystem.DirectoryInfo.FromDirectoryName(targetDirectoryPath);        
                string sourceDirectoryPath = Path.Combine(sourcePath, targetDirectoryInfo.Name);
                

                if (fileSystem.Directory.Exists(sourceDirectoryPath))
                {
                    Result subDirectoryResult = Clean(fileSystem, targetDirectoryPath, sourceDirectoryPath, pattern, attributes);
                    result.DirectoryResults.Add(targetDirectoryInfo.Name, subDirectoryResult);
                    continue;
                }
                // Otherwise, try to delete it.
                try
                {
                    targetDirectoryInfo.Delete();
                    RaiseEntryEvent(EntryDeleted, targetDirectoryPath);
                    result.DeletedEntries.Add(targetDirectoryInfo.Name);
                }
                catch (Exception ex)
                {
                    RaiseErrorEvent(targetDirectoryPath, ex);
                    result.FailedEntries.Add(targetDirectoryInfo.Name, ex);
                }
            }

            // Flag up the clean.
            RaiseDirectoryCleanedEvent(targetPath, result);
            return result;
        }
        
        #region Event raisers
        
        private static void RaiseEntryEvent(EntryEventHandler eventHandler, string path)
        {
            EntryEventHandler temp = eventHandler;
            if (temp != null)
            {
                EntryEventArgs eventArgs = new EntryEventArgs(path);
                temp(eventArgs);
            }
        }

        private static void RaiseErrorEvent(string path, Exception ex)
        {
            EntryErrorEventHandler temp = Error;
            if (temp != null)
            {
                EntryErrorEventArgs eventArgs = new EntryErrorEventArgs(path, ex);
                temp(eventArgs);
            }
        }
        
        private static void RaiseDirectoryCleanedEvent(string path, Result result)
        {
            DirectoryCleanedEventHandler temp = DirectoryCleaned;
            if (temp != null)
            {
                DirectoryCleanedEventArgs eventArgs = new DirectoryCleanedEventArgs(path, result);
                temp(eventArgs);
            }
        }
        
        #endregion

        #endregion

    }
}
