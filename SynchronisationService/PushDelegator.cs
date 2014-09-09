using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SynchronisationService
{
    internal class PushDelegator//watcher delegator, PushFileDelegator
    {
        public delegate void PushRequiredEventHandler(PushRequiredEventArgs e);
        public event PushRequiredEventHandler PushRequired;

        private FileSystemWatcher _watcher;
        private string _targetPath;
        private FileAttributes _attributes;


        public class PushRequiredEventArgs : EventArgs
        {
            #region Fields
            
            private string _sourcePath;
            private string _targetPath;

            #endregion

            #region Properties

            public string SourcePath
            {
                get
                {
                    return _sourcePath;
                }
                private set
                {
                    if (value == null)
                    {
                        throw new ArgumentNullException("SourcePath", "SourcePath null.");
                    }
                    _sourcePath = value;
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

            #region Methods

            public PushRequiredEventArgs(string sourcePath, string targetPath)
            {
                SourcePath = sourcePath;
                TargetPath = targetPath;
            }

            #endregion

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
                _targetPath = value;
            }
        }

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

        public PushDelegator(string sourcePath, string targetPath, string pattern = "*.*", 
            FileAttributes attributes = FileAttributes.Normal) 
        {
            _watcher = new FileSystemWatcher(sourcePath, pattern);
            _watcher.IncludeSubdirectories = true;
            _watcher.Changed += new FileSystemEventHandler(OnPushableEvent);
            _watcher.Created += new FileSystemEventHandler(OnPushableEvent);
            _watcher.Renamed += new RenamedEventHandler(OnPushableEvent);

           // _watcher.Error += new System.IO.ErrorEventHandler(OnError);
            Attributes = attributes;
            TargetPath = targetPath;
        }

        private void OnPushableEvent(object source, FileSystemEventArgs e)
        {
            FileInfo file = new FileInfo(e.FullPath);
            if ((file.Attributes & _attributes) != _attributes)
            {
                return;
            }

            string relativePath = e.FullPath.Substring(SourcePath.Length);
            string targetFilePath = Path.Combine(_targetPath, relativePath);
            RaisePushRequiredEvent(e.FullPath, targetFilePath);
        }
        
        private void RaisePushRequiredEvent(string sourcePath, string targetPath)
        {
            PushRequiredEventHandler temp = PushRequired;
            if (temp != null)
            {
                PushRequiredEventArgs eventArgs = new PushRequiredEventArgs(sourcePath, targetPath);
                temp(eventArgs);
            }
        }
    }
}
