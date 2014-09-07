using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.IO;
using System.Linq;
using System.Text;
//using WrapperLibrary;

namespace SynchronisationLibrary
{
    public class Pusher 
    {
        #region Enumerators

        public enum PushEntryResult
        {
            Verified,
            Created,
            Updated,
            Failed
        }

        #endregion

        #region Classes

        public class PushDirectoryResult
        {
            public PushEntryResult Result;
            public List<string> CreatedFiles = new List<string>();
            public List<string> UpdatedFiles = new List<string>();
            public List<string> VerifiedFiles = new List<string>(); 
            public Dictionary<string, Exception> FailedEntries = new Dictionary<string, Exception>(); // NB can include both files and directories.
            public Dictionary<string, PushDirectoryResult> DirectoryResults = new Dictionary<string, PushDirectoryResult>();             
        }

        public class DirectoryPushedEventArgs : EntryEventArgs
        {
            #region Fields

            private PushDirectoryResult _result;

            #endregion

            #region Properties

            public PushDirectoryResult Result
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

            public DirectoryPushedEventArgs(string path, PushDirectoryResult result)
                : base(path)
            {
                Result = result;
            }

            #endregion

        }

        #endregion

        #region Fields


        
        private const int BufferSize = sizeof(Int64);

        #endregion

        #region Events

        public delegate void EntryEventHandler(EntryEventArgs e);
        public static event EntryEventHandler DirectoryCreated; 
        public static event EntryEventHandler DirectoryUpdated; 
        public static event EntryEventHandler FileVerified;
        public static event EntryEventHandler FileCreated; // split into FileCreated/DirectoryCreated?
        public static event EntryEventHandler FileUpdated; // have separate diretory updated event as well?
        public delegate void DirectoryPushedEventHandler(DirectoryPushedEventArgs e);
        public static event DirectoryPushedEventHandler DirectoryPushed;
        // more like "processed" than synchronised - it hasn't neccessarily all worked
        public delegate void EntryErrorEventHandler(EntryErrorEventArgs e);
        public static event EntryErrorEventHandler Error;

        #endregion

        #region Methods

        // add separte method for top level call
        // and make recursive one private, so that different rules can apply
        // does this method need top level error handling?
        public static PushDirectoryResult PushDirectory(IFileSystem fileSystem, string sourcePath, string targetPath, 
            string pattern = "*.*", FileAttributes attributes = FileAttributes.Normal)
        {
            // If the source directory is null, throw an exception.
            if (sourcePath == null)
            {
                throw new ArgumentNullException("sourcePath", "SourcePath null.");
            }
            // If the target directory is null, throw an exception.
            if (targetPath == null)
            {
                throw new ArgumentNullException("targetPath", "TargetPath null.");
            }
            // If the source directory doesn't exist, throw an exception.
            if (!fileSystem.Directory.Exists(sourcePath))
            {
                throw new SyncDirectoryNotFoundException(sourcePath);
            }
            var sourceInfo = fileSystem.DirectoryInfo.FromDirectoryName(sourcePath);
            DirectoryInfoBase targetInfo;
            PushDirectoryResult result = new PushDirectoryResult();

            // If the target directory doesn't exist, try to create it now.
            if (!fileSystem.Directory.Exists(targetPath))
            {
                targetInfo = fileSystem.Directory.CreateDirectory(targetPath);
                targetInfo.Attributes = sourceInfo.Attributes;
                result.Result = PushEntryResult.Created;
                RaiseEntryEvent(DirectoryCreated, targetPath);
            }
            
            // If the target's attributes are different, update them now.
            else
            {
                targetInfo = fileSystem.DirectoryInfo.FromDirectoryName(targetPath);
                if (targetInfo.Attributes != sourceInfo.Attributes)
                {
                    targetInfo.Attributes = sourceInfo.Attributes;
                    result.Result = PushEntryResult.Updated;
                    RaiseEntryEvent(DirectoryUpdated, targetPath);
                }
            }     
            
            // Loop through each file in the source directory.
            foreach (string sourceFilePath in fileSystem.Directory.GetFiles(sourcePath, pattern))//, attributes))
            {
                // If the source file doesn't have the required attributes, skip it.
                var sourceFileInfo = fileSystem.FileInfo.FromFileName(sourceFilePath);
                if ((sourceFileInfo.Attributes & attributes) != attributes)
                {
                    continue;
                }

                string targetFilePath = Path.Combine(targetPath, sourceFileInfo.Name);
                try
                {
                    switch (PushFile(fileSystem, sourceFilePath, targetFilePath))
                    {
                        case PushEntryResult.Created:
                            result.CreatedFiles.Add(sourceFileInfo.Name);
                            RaiseEntryEvent(FileCreated, targetFilePath);
                            break;
                        case PushEntryResult.Updated:
                            result.UpdatedFiles.Add(sourceFileInfo.Name);
                            RaiseEntryEvent(FileUpdated, targetFilePath);
                            break;
                        case PushEntryResult.Verified:
                            result.VerifiedFiles.Add(sourceFileInfo.Name);
                            RaiseEntryEvent(FileVerified, targetFilePath);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    result.FailedEntries.Add(sourceFileInfo.Name, ex);
                    RaiseErrorEvent(targetFilePath, ex);
                }
            }        
                
            // Loop through each sub-directory in the source directory.
            foreach (string sourceDirectoryPath in fileSystem.Directory.GetDirectories(sourcePath))
            {
                var sourceDirectory = fileSystem.DirectoryInfo.FromDirectoryName(sourceDirectoryPath);
                string targetDirectoryPath = Path.Combine(targetPath, sourceDirectory.Name);
                try
                {
                    // I don't want the failure of one sub directory to kncker up all the others
                    PushDirectoryResult subDirectoryResult = 
                        PushDirectory(fileSystem, sourceDirectoryPath, targetDirectoryPath, pattern, attributes);
                    result.DirectoryResults.Add(sourceDirectory.Name, subDirectoryResult);
                }
                catch (Exception ex)
                {
                    RaiseErrorEvent(targetDirectoryPath, ex);
                    result.FailedEntries.Add(sourceDirectory.Name, ex);
                }              
            }

            // Flag up the push.
            RaiseDirectorySynchronisedEvent(targetPath, result);
            return result;
        }

        public static PushEntryResult PushFile(IFileSystem fileSystem, string sourcePath, string targetPath) 
        {
            // If the source file is null, throw an exception.
            if (sourcePath == null) 
            {
                throw new ArgumentNullException("sourcePath", "SourcePath null.");
            }
            // If the target file is null, throw an exception.
            if (targetPath == null)
            {
                throw new ArgumentNullException("targetPath", "TargetPath null.");
            }            
            // If the source file doesn't exist, throw an exception.
            if (!fileSystem.File.Exists(sourcePath))
            {
                throw new SyncFileNotFoundException(sourcePath);
            }
                        
            // If the target doesn't exist, try to create it now.
            if (!fileSystem.File.Exists(targetPath)) 
            {
                fileSystem.File.Copy(sourcePath, targetPath);
                return PushEntryResult.Created;
            }
            
            // Before opening the files, cache their attributes.
            // This is done because opening some files appears to change their attributes 
            // (ie from Hidden to Normal).  
            var sourceAttributes = fileSystem.File.GetAttributes(sourcePath);
            var targetAttributes = fileSystem.File.GetAttributes(targetPath);

            // If the target is identical to the source, mark it as verified.
            var sourceInfo = fileSystem.FileInfo.FromFileName(sourcePath);
            var targetInfo = fileSystem.FileInfo.FromFileName(targetPath);
            if (BytesEqual(sourceInfo, targetInfo)) 
            {
                if (targetAttributes == sourceAttributes)
                { 
                    return PushEntryResult.Verified;                
                }
                fileSystem.File.SetAttributes(targetPath, sourceAttributes);
                return PushEntryResult.Updated;          
            }
            fileSystem.File.Copy(sourcePath, targetPath, true);                  
            return PushEntryResult.Updated;            
        }

        private static bool BytesEqual(FileInfoBase file1, FileInfoBase file2)
        {
            if (file1.Length != file2.Length)
            {
                return false;
            }

            int bufferCount = (int)Math.Ceiling((double)file1.Length / BufferSize);
            using (Stream sourceStream = file1.OpenRead())
            using (Stream targetStream = file2.OpenRead())
            {
                byte[] sourceBuffer = new byte[BufferSize];
                byte[] targetBuffer = new byte[BufferSize];
                for (int i = 0; i < bufferCount; i++)
                {
                    sourceStream.Read(sourceBuffer, 0, BufferSize);
                    targetStream.Read(targetBuffer, 0, BufferSize);
                    if (BitConverter.ToInt64(sourceBuffer, 0) != BitConverter.ToInt64(targetBuffer, 0))
                    {
                        return false;
                    }
                }
            }
            return true;
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

        private static void RaiseDirectorySynchronisedEvent(string path, PushDirectoryResult result)
        {
            DirectoryPushedEventHandler temp = DirectoryPushed;
            if (temp != null)
            {
                DirectoryPushedEventArgs eventArgs = new DirectoryPushedEventArgs(path, result);
                temp(eventArgs);
            }
        }

        #endregion

        #endregion

    }
}
