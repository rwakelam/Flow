using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WrapperLibrary;

namespace FlowLibrary
{

    public class Synchroniser
    {

        #region Constants
   
        private const string RootRelativePath = "";

        public class Default
        {
            public const bool DeleteUnmatchedEnabled = false;
        }

        #endregion

        #region Events

        public delegate void SyncEventHandler(object sender, SyncEventArgs e);
        public event SyncEventHandler SyncStarted;
        public event SyncEventHandler SyncCompleted;
        public delegate void SyncErrorEventHandler(object sender, SyncErrorEventArgs e);
        public event SyncErrorEventHandler Error;
        public delegate void FileSyncEventHandler(object sender, FileSyncEventArgs e);
        public event FileSyncEventHandler FileSyncCompleted;
        public delegate void DirectorySyncEventHandler(object sender, DirectorySyncEventArgs e);
        public event DirectorySyncEventHandler DirectorySyncCompleted;
        // TODO: institute error event? have specialised errorEventArgs? do I need one for dir and one for file? or will common one suffice?
        // event args need to provide fuller information than mere result
        // result is hierarchical, so there is noneed to repeat info at all levels
        // event has to be self contained, so it has to have everthing

        #endregion

        #region Fields

        private IFileSystemWrapper _FileSystem;
        private string _ClientDirectory;
        private string _ServerDirectory;
        private string _FilePattern;
        private bool _DeleteUnmatchedEnabled = false;
        private object _Lock = new Object();

        #endregion

        #region Properties
        
        public IFileSystemWrapper FileSystem
        {
            get 
            { 
                return _FileSystem; 
            }
            private set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("FileSystem", "Null FileSystem.");
                } 
                _FileSystem = value; 
            }
        }

        public string ClientDirectory
        {
            get { return _ClientDirectory; }
            private set 
            {
                if (String.IsNullOrEmpty(value))
                {
                    throw new ArgumentNullException("ClientDirectory", "Null or empty ClientDirectory.");
                }
                _ClientDirectory = value; 
            }
        }

        public string ServerDirectory
        {
            get { return _ServerDirectory; }
            private set
            {
                if (String.IsNullOrEmpty(value))
                {
                    throw new ArgumentNullException("ServerDirectory", "Null or empty ServerDirectory.");
                }
                _ServerDirectory = value; 
            }
        }

        public string FilePattern
        {
            get { return _FilePattern; }
            private set { _FilePattern = value; }
        }

        public bool DeleteUnmatchedEnabled
        {
            get { return _DeleteUnmatchedEnabled; }
            private set { _DeleteUnmatchedEnabled = value; }
        }     

        #endregion

        #region Methods

        public Synchroniser(IFileSystemWrapper fileSystem, string clientDirectory, string serverDirectory, string filePattern = null, 
            bool deleteUnmatchedEnabled = Default.DeleteUnmatchedEnabled) 
        {
            FileSystem = fileSystem;
            ClientDirectory = clientDirectory;
            ServerDirectory = serverDirectory;
            FilePattern = filePattern;
            DeleteUnmatchedEnabled = deleteUnmatchedEnabled;
        }

        private static bool CopyRequired(IFileSystemInfoWrapper fileSystemInfo)
        {
            return 
                (fileSystemInfo.Attributes & FileAttributesWrapper.Hidden) == 0 
                &&
                (fileSystemInfo.Attributes & FileAttributesWrapper.System) == 0;
        }

        private static bool DeletePermitted(IFileSystemInfoWrapper fileSystemInfo)
        {
            return true;// (fileSystemInfo.Attributes & FileAttributes2.Archive) == 0;
        }

        private static DateTime RoundToSecond(DateTime dateTime) 
        {
            return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, dateTime.Second);
        }

        private string GetSourcePath(string relativePath, Direction direction) 
        {
            return _FileSystem.BuildPath((direction == Direction.Push ? _ClientDirectory : _ServerDirectory), relativePath);    
        }

        private string GetTargetPath(string relativePath, Direction direction)
        {
            return _FileSystem.BuildPath((direction == Direction.Push ? _ServerDirectory : _ClientDirectory), relativePath);
        }


        private SyncResult DeleteFile(string path)
        {
            IFileSystemInfoWrapper targetInfo = _FileSystem.GetFileInfo(path);
            if (DeletePermitted(targetInfo))
            {
                _FileSystem.DeleteFile(path);
                return SyncResult.Deleted;
            }
            return SyncResult.Ignored;
        }

        private FileSyncResult SynchroniseFile(string relativePath, Direction direction)
        {
            string sourcePath = GetSourcePath(relativePath, direction);
            string targetPath = GetTargetPath(relativePath, direction);
            SyncResult syncResult = SyncResult.Unchanged;
            FileSyncResult result;
            IFileSystemInfoWrapper targetInfo;
            try
            {
                if (!_FileSystem.FileExists(sourcePath)) 
                {
                    //if (direction == Direction.Push && DeleteUnmatchedEnabled)
                    //{
                    //    if (_FileSystem.FileExists(targetPath))
                    //    {
                    //        targetInfo = _FileSystem.GetFileInfo(targetPath);
                    //        if (DeletePermitted(targetInfo))
                    //        {
                    //            _FileSystem.DeleteFile(targetPath);
                    //            syncResult = SyncResult.Deleted;
                    //            RaiseFileSyncEvent(direction, relativePath, syncResult);
                    //            return new FileSyncResult(syncResult);
                    //        }
                    //        syncResult = SyncResult.Ignored;
                    //        RaiseFileSyncEvent(direction, relativePath, syncResult);
                    //        return new FileSyncResult(syncResult);
                    //    }
                    //    RaiseFileSyncEvent(direction, relativePath, syncResult);
                    //    return new FileSyncResult(syncResult);
                    //}
                    throw new SyncFileNotFoundException(sourcePath);
                }

                IFileSystemInfoWrapper sourceInfo = _FileSystem.GetFileInfo(sourcePath);
                if (CopyRequired(sourceInfo))
                {
                    DateTime sourceLastWriteTime = RoundToSecond(sourceInfo.LastWriteTimeUtc);
                    // _FileSystem.GetFileLastWriteTimeUtc(sourcePath));
                    
                    if (!_FileSystem.FileExists(targetPath))
                    {
                        _FileSystem.CopyFile(sourcePath, targetPath, true);
                        targetInfo = _FileSystem.GetFileInfo(targetPath);
                        targetInfo.LastWriteTimeUtc = sourceLastWriteTime;
                        //_FileSystem.SetFileLastWriteTimeUtc(targetPath, sourceLastWriteTime);
                        syncResult = SyncResult.Created; 
                        RaiseFileSyncEvent(direction, relativePath, syncResult);
                        return new FileSyncResult(syncResult);
                    }

                    targetInfo = _FileSystem.GetFileInfo(targetPath);
                    DateTime targetLastWriteTime = RoundToSecond(targetInfo.LastWriteTimeUtc);
                    //_FileSystem.GetFileLastWriteTimeUtc(targetPath));
                    if (targetLastWriteTime < sourceLastWriteTime)
                    {
                        _FileSystem.CopyFile(sourcePath, targetPath, true);
                        targetInfo.LastWriteTimeUtc = sourceLastWriteTime;
                        //_FileSystem.SetFileLastWriteTimeUtc(targetPath, sourceLastWriteTime);
                        syncResult = SyncResult.Updated;
                        RaiseFileSyncEvent(direction, relativePath, syncResult);
                        return new FileSyncResult(syncResult);
                    }
                    RaiseFileSyncEvent(direction, relativePath, syncResult);
                    return new FileSyncResult(syncResult);
                }
                syncResult = SyncResult.Ignored;
                RaiseFileSyncEvent(direction, relativePath, syncResult);
                result = new FileSyncResult(syncResult);
            }
            catch (Exception ex)
            {
                RaiseErrorEvent(direction, ex);
                result = new FileSyncResult(ex);
            }
            return result;
        }

        public DirectorySyncResult Pull()
        {
            return Synchronise(Direction.Pull);
        }

        public DirectorySyncResult Push()
        {
            return Synchronise(Direction.Push);
        }

        private DirectorySyncResult Synchronise(Direction direction)
        {
            lock (_Lock)
            {
                RaiseSyncStarted(direction);
                DirectorySyncResult sync = SynchroniseDirectory(RootRelativePath, direction);
                RaiseSyncCompleted(direction);
                return sync;
            }
        }

        #region EventRaisers

        private void RaiseSyncStarted(Direction direction)
        {
            SyncEventHandler temp = SyncStarted;
            if (temp != null)
            {
                SyncEventArgs eventArgs = new SyncEventArgs(direction);
                temp(this, eventArgs);
            }
        }

        private void RaiseSyncCompleted(Direction direction)
        {
            SyncEventHandler temp = SyncCompleted;
            if (temp != null)
            {
                SyncEventArgs eventArgs = new SyncEventArgs(direction);
                temp(this, eventArgs);
            }
        }

        private void RaiseErrorEvent(Direction direction, Exception ex)
        {
            SyncErrorEventHandler temp = Error;
            if (temp != null)
            {
                SyncErrorEventArgs eventArgs = new SyncErrorEventArgs(direction, ex);
                temp(this, eventArgs);
            }
        }
        
        private void RaiseFileSyncEvent(Direction direction, string relativePath, SyncResult result)
        {
            FileSyncEventHandler temp = FileSyncCompleted;
            if (temp != null)
            {
                string path = _FileSystem.BuildPath("$", relativePath);
                FileSyncEventArgs eventArgs = new FileSyncEventArgs(direction, path, result);
                temp(this, eventArgs);
            }
        }

        private void RaiseDirectorySyncEvent(Direction direction, string relativePath, 
            Dictionary<string, FileSyncResult> fileSyncResults)
        {
            DirectorySyncEventHandler temp = DirectorySyncCompleted;
            if (temp != null)
            {
                string path = _FileSystem.BuildPath("$", relativePath);
                DirectorySyncEventArgs eventArgs = new DirectorySyncEventArgs(direction, path, fileSyncResults);
                temp(this, eventArgs);
            }
        }
        
        #endregion
        
        private DirectorySyncResult SynchroniseDirectory(string relativePath, Direction direction)
        {
            string sourcePath = GetSourcePath(relativePath, direction);
            string targetPath = GetTargetPath(relativePath, direction);
            Dictionary<String, FileSyncResult> fileSyncResults = new Dictionary<string,FileSyncResult>();
            Dictionary<string, DirectorySyncResult> directorySyncResults = new Dictionary<string, DirectorySyncResult>();
            DirectorySyncResult result;

            try
            {
                if (!_FileSystem.DirectoryExists(sourcePath)) 
                {
                    //if (direction == Direction.Push && DeleteUnmatchedEnabled)
                    //{
                    //    if (_FileSystem.DirectoryExists(targetPath))
                    //    {
                    //        IFileSystemInfoWrapper targetInfo = _FileSystem.GetDirectoryInfo(targetPath);
                    //        if (DeletePermitted(targetInfo))
                    //        {
                    //            _FileSystem.DeleteDirectory(targetPath, true);
                    //            RaiseFileSyncEvent(direction, relativePath, SyncResult.Deleted);
                    //            fileSyncResults.Add(relativePath, new FileSyncResult(SyncResult.Deleted));
                    //            RaiseDirectorySyncEvent(direction, relativePath, fileSyncResults);
                    //            return new DirectorySyncResult(directorySyncResults, fileSyncResults);
                    //        }
                    //    }
                    //    RaiseDirectorySyncEvent(direction, relativePath, fileSyncResults);
                    //    return new DirectorySyncResult(directorySyncResults, fileSyncResults);
                    //}
                    throw new SyncDirectoryNotFoundException(sourcePath);                       
                }

                IFileSystemInfoWrapper sourceInfo = _FileSystem.GetDirectoryInfo(sourcePath);
                if (CopyRequired(sourceInfo))
                {
                    // If the target directory doesn't exist, try to create it now.
                    if (!_FileSystem.DirectoryExists(targetPath))
                    {
                        _FileSystem.CreateDirectory(targetPath);
                        RaiseFileSyncEvent(direction, relativePath, SyncResult.Created);
                        fileSyncResults.Add(relativePath, new FileSyncResult(SyncResult.Created));
                    }

                    // Synchronize files.
                    IEnumerable<string> sourceFiles = _FileSystem.EnumerateFiles(sourcePath, FilePattern);
                    // exclude source files which were written before the oldest target write
                    foreach (string sourceFile in sourceFiles)
                    {
                        string fileName = _FileSystem.GetFileName(sourceFile);
                        string fileRelativePath = _FileSystem.BuildPath(relativePath, fileName);
                        FileSyncResult fileSyncResult = SynchroniseFile(fileRelativePath, direction);
                        fileSyncResults.Add(fileName, fileSyncResult);
                    }

                    // Synchronise sub-directories.
                    string[] sourceSubDirectories = _FileSystem.GetDirectories(sourcePath);//, null, false);
                    foreach (string sourceSubDirectory in sourceSubDirectories)
                    {
                        string subDirectoryName = _FileSystem.GetFileName(sourceSubDirectory);
                        string subDirectoryRelativePath = _FileSystem.BuildPath(relativePath, subDirectoryName);
                        DirectorySyncResult subDirectoryResult = SynchroniseDirectory(subDirectoryRelativePath, direction);
                        directorySyncResults.Add(subDirectoryName, subDirectoryResult);
                    }

                    // Delete unmatched files.
                    if (direction == Direction.Push && DeleteUnmatchedEnabled)
                    {
                        IEnumerable<string> targetFiles = _FileSystem.EnumerateFiles(targetPath, FilePattern);
                        foreach (string targetFile in targetFiles)
                        {
                            string fileName = _FileSystem.GetFileName(targetFile);
                            if (fileSyncResults.ContainsKey(fileName))
                            {
                                continue;
                            }
                            SyncResult syncResult = DeleteFile(targetFile);
                            RaiseFileSyncEvent(direction, _FileSystem.BuildPath(relativePath, fileName), syncResult);
                            FileSyncResult fileSyncResult = new FileSyncResult(syncResult);

                            //FileSyncResult fileSyncResult = SynchroniseFile(_FileSystem.BuildPath(relativePath, fileName), direction);
                            fileSyncResults.Add(fileName, fileSyncResult);
                        }
                        
                        // Delete unmatched sub-directories. 
                        string[] targetSubDirectories = _FileSystem.GetDirectories(targetPath);
                        foreach (string targetSubDirectory in targetSubDirectories)
                        {
                            string subDirectoryName = _FileSystem.GetFileName(targetSubDirectory);
                            if (directorySyncResults.ContainsKey(subDirectoryName))
                            {
                                continue;
                            }
                            //DirectorySyncResult subDirectoryResult = SynchroniseDirectory(_FileSystem.BuildPath(relativePath, subDirectoryName), direction);
                            //directorySyncResults.Add(subDirectoryName, subDirectoryResult);

                            //call delete file? 
                            // this depends on whether IO can treat directories as files
                            // if it can, MockFileSystem would have to mimic the behaviour.
                            //FileSyncResult fileSyncResult = SynchroniseFile(_FileSystem.BuildPath(relativePath, subDirectoryName), direction);
                            //fileSyncResults.Add(subDirectoryName, fileSyncResult);
                            //continue;

                            IFileSystemInfoWrapper targetInfo = _FileSystem.GetDirectoryInfo(targetSubDirectory);
                            if (DeletePermitted(targetInfo))
                            {
                                _FileSystem.DeleteDirectory(targetSubDirectory, true);
                                string subDirectoryRelativePath = _FileSystem.BuildPath(relativePath, subDirectoryName);
                                RaiseFileSyncEvent(direction, subDirectoryRelativePath, SyncResult.Deleted);
                                fileSyncResults.Add(subDirectoryRelativePath, new FileSyncResult(SyncResult.Deleted));
                                //RaiseDirectorySyncEvent(direction, relativePath, fileSyncResults);
                            }
                            

                        }
                    }
                }
                result = new DirectorySyncResult(directorySyncResults, fileSyncResults);
            }
            catch (Exception ex)
            {
                RaiseErrorEvent(direction, ex);
                result = new DirectorySyncResult(directorySyncResults, fileSyncResults, ex);
            }
            finally
            {
                RaiseDirectorySyncEvent(direction, relativePath, fileSyncResults);
            }
            return result;
        }

        #endregion

    }

}
