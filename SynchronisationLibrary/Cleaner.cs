using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WrapperLibrary;

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
            public Dictionary<string, Result> SubDirectoryResults = new Dictionary<string, Result>();
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
                        
        public static Result Clean(IDirectory targetDirectory, IDirectory sourceDirectory, 
            string targetFilePattern = null, FileAttributesWrapper targetFileAttributes = FileAttributesWrapper.Any)
        {
            // If the source directory is null, throw an exception.
            if (sourceDirectory == null)
            {
                throw new ArgumentNullException("sourceDirectory", "SourceDirectory null.");
            }

            // If the target directory is null, throw an exception.
            if (targetDirectory == null)
            {
                throw new ArgumentNullException("targetDirectory", "TargetDirectory null.");
            }
            
            // If the source directory doesn't exist, throw an exception.
            if (!sourceDirectory.Exists())
            {
                throw new SyncDirectoryNotFoundException(sourceDirectory.Path);
            }

            // If the target directory doesn't exist, throw an exception.
            if (!targetDirectory.Exists())
            {
                throw new SyncDirectoryNotFoundException(targetDirectory.Path);
            }

            Result result = new Result();
            IEnumerable<string> sourceFileNames = sourceDirectory.GetFiles().Select<IFile, string>(f => f.Name); 
            IEnumerable<string> sourceSubDirectoryNames = sourceDirectory.GetDirectories().Select<IDirectory, string>(d => d.Name);
           
            // Loop through each file in the target directory.
            foreach (IFile targetFile in targetDirectory.GetFiles(targetFilePattern, targetFileAttributes))
            {
                // If it matches a file in the source directory, skip it.
                if (sourceFileNames.Contains(targetFile.Name))
                {
                    result.MatchedFiles.Add(targetFile.Name);
                    RaiseEntryEvent(FileMatched, targetFile.Path);
                    continue;
                }
                // Otherwise, try to delete it.
                try
                {
                    targetFile.Delete();
                    RaiseEntryEvent(EntryDeleted, targetFile.Path);
                    result.DeletedEntries.Add(targetFile.Name);
                }
                catch (Exception ex)
                {
                    RaiseErrorEvent(targetFile.Path, ex);
                    result.FailedEntries.Add(targetFile.Name, ex);
                }
            }

            // Loop through each sub-directory in the target directory.
            foreach (IDirectory targetSubDirectory in targetDirectory.GetDirectories())
            {
                // If it matches a subdirectory in the source, clean it as well.
                if (sourceSubDirectoryNames.Contains(targetSubDirectory.Name))
                {
                    IDirectory sourceSubDirectory = sourceDirectory.GetDirectories().First(d => d.Name == targetSubDirectory.Name);
                    Result subDirectoryResult = Clean(targetSubDirectory, sourceSubDirectory, targetFilePattern);
                    result.SubDirectoryResults.Add(targetSubDirectory.Name, subDirectoryResult);
                    continue;
                }
                // Otherwise, try to delete it.
                try
                {
                    targetSubDirectory.Delete();
                    RaiseEntryEvent(EntryDeleted, targetSubDirectory.Path);
                    result.DeletedEntries.Add(targetSubDirectory.Name);
                }
                catch (Exception ex)
                {
                    RaiseErrorEvent(targetSubDirectory.Path, ex);
                    result.FailedEntries.Add(targetSubDirectory.Name, ex);
                }
            }

            // Flag up the clean.
            RaiseDirectoryCleanedEvent(targetDirectory.Path, result);
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
