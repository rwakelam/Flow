using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlowLibrary;
using Flow.API;

namespace FileFlow
{
    public class FileWatcherGateway : Sender
    {
        #region Fields

        private readonly FileSystemWatcher _watcher;
        private readonly FileAttributes _attributes;

        #endregion

        #region Properties

        public FileAttributes Attributes
        {
            get
            {
                return _attributes;
            }
        }

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

        public string Pattern
        {
            get
            {
                return _watcher.Filter;
            }
        }

        public string Path
        {
            get
            {
                return _watcher.Path;
            }
        }
        
        #endregion

        public FileWatcherGateway(string path, string pattern = "*.*", 
            FileAttributes attributes = FileAttributes.Normal, 
            WatcherChangeTypes changeType = WatcherChangeTypes.All,
            bool includeSubDirectories = true) 
        {
            _watcher = new FileSystemWatcher(path, pattern);
            _watcher.IncludeSubdirectories = includeSubDirectories;
            if ((changeType & WatcherChangeTypes.Created) !=  0)
            {
                _watcher.Created += new FileSystemEventHandler(OnFileSystemEvent);
            }
            if ((changeType & WatcherChangeTypes.Changed) != 0)
            {
                _watcher.Changed += new FileSystemEventHandler(OnFileSystemEvent);
            }
            if ((changeType & WatcherChangeTypes.Deleted) != 0)
            {
                _watcher.Deleted += new FileSystemEventHandler(OnFileSystemEvent);
            }
            if ((changeType & WatcherChangeTypes.Renamed) != 0)
            {
                _watcher.Renamed += new RenamedEventHandler(OnRenamedEvent);
            }
            _attributes = attributes;
        }

        private void OnRenamedEvent(object source, RenamedEventArgs e)
        {
            IMessage message = GetMessage(e);
            if (message != null)
            {
                // add e.OldFullPath, e.OldName to headers
                RaiseSendEvent(message);
            }
        }

        private void OnFileSystemEvent(object source, FileSystemEventArgs e)
        {
            IMessage message = GetMessage(e);
            if (message != null)
            {
                RaiseSendEvent(message);
            }
        }

        private IMessage GetMessage(FileSystemEventArgs e)
        {
            var file = new FileInfo(e.FullPath);
            if ((file.Attributes & _attributes) != _attributes)
            {
                return null;
            }
            IMessage message = new FileEventMessage(e.FullPath, e.ChangeType);
            return message;
        }
    }
}
