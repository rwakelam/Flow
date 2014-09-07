using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using WrapperLibrary;

namespace FlowLibrary
{
    public class SmartSynchroniser// RealTimeSynchronsier, LiveSynchronsier, OnDemandSynchroniser, 
    {// BatchSynchroniser, WatchfulSynchroniser, WatcherCopier
        // TODO:: implement Directory handling
        // TODO:: work out relationship between directory and file events - 
        // if the former is created, do I have to create all of the latter as well
        // treat directory creates as creates rather than as copies?
        // TODO:: write directory create/delete units tests. check what happens with child files.
        // TODO:: implement IDisposable
        // TODO:: implement error handling/
        // TODO:: implement reporting
        // delete directory causes both directory and file events
        // file delete detected first, then directory delete
        // directory delete could remove queued file deletes on its path

        #region Fields

        private FileSystemWatcher _watcher;
        private SyncQueue _directorySyncQueue;
        private SyncQueue _fileSyncQueue;
        private IFileSystemWrapper _fileSystem;

        #endregion

        #region Properties

        public bool Enabled
        {
            get
            {
                return _watcher.EnableRaisingEvents;
            }
            set
            {
                _watcher.EnableRaisingEvents = value;
            }
        }

        public string FilePattern
        {
            get 
            {
                return _watcher.Filter;
            }
        }
        
        public string SourceDirectory { get; set; }

        public bool SubDirectoriesIncluded
        {
            get
            {
                return _watcher.IncludeSubdirectories;
            }
        }

        public string TargetDirectory { get; set; }

        #endregion

        #region Methods

        #region Public Methods

        public SmartSynchroniser(IFileSystemWrapper fileSystem, string sourceDirectory, 
            string targetDirectory, bool subDirectoriesIncluded = false, string filePattern = null)
        { 
            _directorySyncQueue = new SyncQueue();
            _fileSyncQueue = new SyncQueue();

            _watcher = new FileSystemWatcher(sourceDirectory, filePattern);// get from filesystem?
            _watcher.IncludeSubdirectories = subDirectoriesIncluded;

            //_Watcher.NotifyFilter = NotifyFilters..LastWrite;
            _watcher.Changed += new FileSystemEventHandler(OnChanged);
            _watcher.Created += new FileSystemEventHandler(OnCreated);
            _watcher.Deleted += new FileSystemEventHandler(OnDeleted);
            _watcher.Renamed += new RenamedEventHandler(OnRenamed);
            
            _watcher.Error += new System.IO.ErrorEventHandler(OnError);

            _fileSystem = fileSystem;
            SourceDirectory = sourceDirectory;//restyle as source?
            TargetDirectory = targetDirectory;// restyle as target?
            //FilePattern = filePattern;

        }

        public void Synchronise()
        {
            ProcessDirectoryActions(_directorySyncQueue.Dequeue());
            ProcessFileActions(_fileSyncQueue.Dequeue());
        }

        #endregion

        #region Private Methods

        #region EventHandlers

        private void OnChanged(object source, FileSystemEventArgs e)
        {
            QueueCopyActionIfRequired(e.FullPath);
        }

        private void OnCreated(object source, FileSystemEventArgs e)
        {
            QueueCopyActionIfRequired(e.FullPath);
        }

        private void OnDeleted(object source, FileSystemEventArgs e)
        {
            QueueDeleteAction(e.FullPath);
        }   

        private void OnError(object source, ErrorEventArgs e)
        {
            //RaiseErrorEvent(new ExceptionEventArgs(e.GetException()));
        }
        
        private void OnRenamed(object source, RenamedEventArgs e)
        {
            if (_fileSystem.DirectoryContains(SourceDirectory, e.OldFullPath)) 
            {
                QueueDeleteAction(e.OldFullPath);
            }
            QueueCopyActionIfRequired(e.FullPath);
        }

        #endregion

        private string GetRelativePath(string fullPath) 
        {
            return fullPath.Substring(SourceDirectory.Length + 1); //TODO:: get file system to do this?
        }
        
        private void QueueCopyActionIfRequired(string path)
        {
            FileAttributesWrapper attributes;
            SyncQueue syncQueue;
            if (IsDirectory(path))
            {
                attributes = _fileSystem.GetDirectoryInfo(path).Attributes;
                syncQueue = _directorySyncQueue;
            }
            else
            {
                attributes = _fileSystem.GetFileInfo(path).Attributes;
                syncQueue = _fileSyncQueue;            
            }
            if (CopyRequired(attributes))
            {
                string relativePath = GetRelativePath(path);
                syncQueue.Enqueue(relativePath, SynchronisationAction.Copy);
            }
        }

        private void QueueDeleteAction(string path)
        {
            string relativePath = GetRelativePath(path);
            bool isDirectory = IsDirectory(relativePath);
            SyncQueue syncQueue = isDirectory ? _directorySyncQueue : _fileSyncQueue;
            syncQueue.Enqueue(relativePath, SynchronisationAction.Delete);
            if (isDirectory) 
            {
                //foreach (kvp)
                //{
                //}
                //IEnumerator<KeyValuePair<string, SynchronisationAction>> enumerator = _fileSyncQueue.GetEnumerator();
                //while (enumerator.Current != null) 
                //{ 
                //}
                //_fileSyncQueue.GetEnumerator().
                //_fileSyncQueue.Dequeue();
            }
            // if it's a directory, remove any pre-existing file deletes on the same path?
            // dont think I can remove item from collection whilst looping through it with foreach
        }               

        private void ProcessFileActions(Dictionary<string, SynchronisationAction> actions)
        {
            // Perform each queued actions.            
            foreach (KeyValuePair<string, SynchronisationAction> kvp in actions)
            {
                string targetPath = _fileSystem.BuildPath(TargetDirectory, kvp.Key);
                switch (kvp.Value)
                {
                    case SynchronisationAction.Copy:
                        string sourcePath = _fileSystem.BuildPath(SourceDirectory, kvp.Key);
                        _fileSystem.CopyFile(sourcePath, targetPath, true);
                        //TODO:: set UTCLastWriteTime?
                        break;
                    case SynchronisationAction.Delete:
                        if (_fileSystem.FileExists(targetPath))
                        {
                            IFileSystemInfoWrapper fileInfo = _fileSystem.GetFileInfo(targetPath);
                            if (DeletePermitted(fileInfo.Attributes))
                            {
                                _fileSystem.DeleteFile(targetPath);
                            }
                        }
                        break;
                }
            }
        }

        private void ProcessDirectoryActions(Dictionary<string, SynchronisationAction> actions)
        {
            // Perform each queued actions.            
            foreach (KeyValuePair<string, SynchronisationAction> kvp in actions)
            {
                string targetPath = _fileSystem.BuildPath(TargetDirectory, kvp.Key);
                switch (kvp.Value)
                {
                    case SynchronisationAction.Copy:
                        string sourcePath = _fileSystem.BuildPath(SourceDirectory, kvp.Key);
                        //_fileSystem.CopyDirectory(sourcePath, targetPath, true);
                        //TODO:: set UTCLastWriteTime?
                        break;
                    case SynchronisationAction.Delete:
                        if (_fileSystem.DirectoryExists(targetPath))
                        {
                            IFileSystemInfoWrapper fileInfo = _fileSystem.GetDirectoryInfo(targetPath);
                            if (DeletePermitted(fileInfo.Attributes))
                            {
                                _fileSystem.DeleteDirectory(targetPath, true);
                            }
                        }
                        break;
                }
            }
        }

        private static bool CopyRequired(FileAttributesWrapper attributes)
        {
            return
                (attributes & FileAttributesWrapper.Hidden) == 0
                &&
                (attributes & FileAttributesWrapper.System) == 0;
        }

        private static bool DeletePermitted(FileAttributesWrapper attributes)
        {
            return true;// (attributes & FileAttributesWrapper.Archive) == 0;
        }

        private bool IsDirectory(string path) 
        {
            return _fileSystem.GetFileExtension(path) == string.Empty;
        }

        #endregion

        #endregion
    }
}
