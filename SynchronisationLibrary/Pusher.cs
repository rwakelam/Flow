using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WrapperLibrary;

namespace SynchronisationLibrary
{
    public class Pusher 
    {
        #region Enumerators

        public enum EntryResult
        {
            Verified,
            Created,
            Updated,
            Failed
        }

        #endregion

        #region Classes

        public class DirectoryResult
        {
            public EntryResult Result;
            public List<string> CreatedFiles = new List<string>();
            public List<string> UpdatedFiles = new List<string>();
            public List<string> VerifiedFiles = new List<string>(); 
            public Dictionary<string, Exception> FailedEntries = new Dictionary<string, Exception>(); // NB can include both files and directories.
            public Dictionary<string, DirectoryResult> SubDirectoryResults = new Dictionary<string, DirectoryResult>();             
        }

        public class DirectoryPushedEventArgs : EntryEventArgs
        {
            #region Fields

            private DirectoryResult _result;

            #endregion

            #region Properties

            public DirectoryResult Result
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

            public DirectoryPushedEventArgs(string path, DirectoryResult result)
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
        public static DirectoryResult Push(IDirectory sourceDirectory, IDirectory targetDirectory, 
            string sourceFilePattern = null, FileAttributesWrapper sourceFileAttributes = FileAttributesWrapper.Any)
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
          
            DirectoryResult result = new DirectoryResult();

            // If the target directory doesn't exist, try to create it now.
            if (!targetDirectory.Exists())
            {
                targetDirectory.Create();//raise directory created event?
                targetDirectory.Attributes = sourceDirectory.Attributes;
                result.Result = EntryResult.Created;
                RaiseEntryEvent(DirectoryCreated, targetDirectory.Path);
            }
            // If the target's attributes are different, update them now.
            else if (targetDirectory.Attributes != sourceDirectory.Attributes)
            {
                targetDirectory.Attributes = sourceDirectory.Attributes;
                result.Result = EntryResult.Updated;
                RaiseEntryEvent(DirectoryUpdated, targetDirectory.Path);
            }
            
            // Loop through each file in the source directory.
            foreach (IFile sourceFile in sourceDirectory.GetFiles(sourceFilePattern, sourceFileAttributes))
            {
                IFile targetFile = targetDirectory.GetFile(sourceFile.Name);
                try
                {
                    switch (Push(sourceFile, targetFile))
                    {
                        case EntryResult.Created:
                            result.CreatedFiles.Add(targetFile.Name);
                            RaiseEntryEvent(FileCreated, targetFile.Path);
                            break;
                        case EntryResult.Updated:
                            result.UpdatedFiles.Add(targetFile.Name);
                            RaiseEntryEvent(FileUpdated, targetFile.Path);
                            break;
                        case EntryResult.Verified:
                            result.VerifiedFiles.Add(targetFile.Name);
                            RaiseEntryEvent(FileVerified, targetFile.Path);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    result.FailedEntries.Add(targetFile.Name, ex);
                    RaiseErrorEvent(targetFile.Path, ex);
                }
            }        
                
            // Loop through each sub-directory in the source directory.
            foreach (IDirectory sourceSubDirectory in sourceDirectory.GetDirectories())
            {
                IDirectory targetSubDirectory = targetDirectory.GetSubDirectory(sourceSubDirectory.Name);
                try
                {
                    // I don't want the failure of one sub directory to kncker up all the others
                    DirectoryResult subDirectoryResult = 
                        Push(sourceSubDirectory, targetSubDirectory, sourceFilePattern, sourceFileAttributes);
                    result.SubDirectoryResults.Add(targetSubDirectory.Name, subDirectoryResult);
                }
                catch (Exception ex)
                {
                    RaiseErrorEvent(targetSubDirectory.Path, ex);
                    result.FailedEntries.Add(targetSubDirectory.Name, ex);
                }              
            }

            // Flag up the push.
            RaiseDirectorySynchronisedEvent(targetDirectory.Path, result);
            return result;
        }

        public static EntryResult Push(IFile sourceFile, IFile targetFile) 
        {
            // If the source file is null, throw an exception.
            if (sourceFile == null) 
            {
                throw new ArgumentNullException("sourceFile", "SourceFile null.");
            }
            // If the target file is null, throw an exception.
            if (targetFile == null)
            {
                throw new ArgumentNullException("targetFile", "TargetFile null.");
            }            
            // If the source file doesn't exist, throw an exception.
            if (!sourceFile.Exists())
            {
                throw new SyncFileNotFoundException(sourceFile.Path);
            }
                        
            // If the target doesn't exist, try to create it now.
            if (!targetFile.Exists()) 
            {
                targetFile.WriteAllBytes(sourceFile.ReadAllBytes());
                targetFile.Attributes = sourceFile.Attributes;
                return EntryResult.Created;
            }
            
            // If the target is identical to the source, mark it as verified.
            if (BytesEqual(sourceFile, targetFile)) 
            {
                if (targetFile.Attributes == sourceFile.Attributes) 
                { 
                    return EntryResult.Verified;                
                }
                targetFile.Attributes = sourceFile.Attributes;
                return EntryResult.Updated;          
            }
            targetFile.Delete();
            targetFile.WriteAllBytes(sourceFile.ReadAllBytes());
            targetFile.Attributes = sourceFile.Attributes;                        
            return EntryResult.Updated;            
        }

        private static bool BytesEqual(IFile sourceFile, IFile targetFile)
        {
            if (sourceFile.Length != targetFile.Length)
            {
                return false;
            }

            int bufferCount = (int)Math.Ceiling((double)sourceFile.Length / BufferSize);
            using (IFileReader sourceReader = sourceFile.GetReader())
            using (IFileReader targetReader = targetFile.GetReader())
            {
                byte[] sourceBuffer = new byte[BufferSize];
                byte[] targetBuffer = new byte[BufferSize];
                for (int i = 0; i < bufferCount; i++)
                {
                    sourceReader.Read(sourceBuffer, 0, BufferSize);
                    targetReader.Read(targetBuffer, 0, BufferSize);
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

        private static void RaiseDirectorySynchronisedEvent(string path, DirectoryResult result)
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
