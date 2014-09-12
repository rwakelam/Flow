using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SynchronisationService
{
    
    private class PushableEventWatcher
    {

        #region Events

        public delegate void OnPushableEventHandler(OnPushableEventArgs e);
        public event OnPushableEventHandler OnPushable;

        #endregion

        #region Fields

        private FileSystemWatcher _watcher;
        private string _targetPath;
        private FileAttributes _attributes;

        #endregion

        #region Properties

        public FileAttributes Attributes
        {
            get
            {
                return _attributes;
            }
            private set
            {
                _attributes = value;
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

        public string SourcePath
        {
            get 
            {
                return _watcher.Path;
            }
        }

        public string TargetPath
        {
            get
            {
                return _targetPath;
            }
            private set 
            {
                if (value == null)                
                {
                    throw new ArgumentNullException("TargetPath", "TargetPath null.");
                }
                _targetPath = value;
            }
        }

        #endregion

        #region Properties

        public PushableEventWatcher(string sourcePath, string targetPath, string pattern = "*.*", 
            FileAttributes attributes = FileAttributes.Normal) 
        {
            _watcher = new FileSystemWatcher(sourcePath, pattern);
            _watcher.IncludeSubdirectories = true;
            _watcher.Created += new FileSystemEventHandler(OnPushableEvent);
            _watcher.Changed += new FileSystemEventHandler(OnPushableEvent);
            _watcher.Renamed += new RenamedEventHandler(OnPushableEvent);
            Attributes = attributes;
            TargetPath = targetPath;
        }

        private void OnPushableEvent(object source, FileSystemEventArgs e)
        {
            var file = new FileInfo(e.FullPath);
            if ((file.Attributes & _attributes) != _attributes)
            {
                return;
            }            
            var relativePath = e.FullPath.Substring(SourcePath.Length);
            var targetFilePath = Path.Combine(_targetPath, relativePath);
            RaisePushRequiredEvent(e.FullPath, targetFilePath);
        }
        
        private void RaisePushRequiredEvent(string sourcePath, string targetPath)
        {
            OnPushableEventHandler temp = OnPushable;
            if (temp != null)
            {
                var eventArgs = new OnPushableEventArgs(sourcePath, targetPath);
                temp(eventArgs);
            }
        }

    #endregion

    }
}
