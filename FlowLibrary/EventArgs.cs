using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlowLibrary
{
    public class SyncEventArgs : EventArgs
    {
        private Direction _Direction;

        public Direction Direction
        {
            get
            {
                return _Direction;
            }
            private set
            {
                _Direction = value;
            }
        }

        public SyncEventArgs(Direction direction)
        {
            Direction = direction;
        }
    }
    
    public class FileSyncEventArgs : SyncEventArgs
    {

        #region Fields

        private SyncResult _Result;
        private string _Path;

        #endregion

        #region Properties

        public SyncResult Result
        {
            get
            {
                return _Result;
            }
            private set
            {
                _Result = value;
            }
        }

        public string Path
        {
            get
            {
                return _Path;
            }
            private set
            {
                _Path = value;
            }
        }

        #endregion
        
        public FileSyncEventArgs(Direction direction, string path, SyncResult result)
            : base(direction)
        {
            Path = path;
            Result = result;
        }
    }

    public class DirectorySyncEventArgs : SyncEventArgs
    {

        private Dictionary<string, FileSyncResult> _FileSyncResults;
        private string _Path;

        public Dictionary<string, FileSyncResult> FileSyncResults
        {
            get
            {
                return _FileSyncResults;
            }
            private set
            {
                _FileSyncResults = value;
            }
        }

        public string Path
        {
            get
            {
                return _Path;
            }
            private set
            {
                _Path = value;
            }
        }

        public DirectorySyncEventArgs(Direction direction, string path, Dictionary<string, FileSyncResult> fileSyncResults)
            : base(direction)
        {
            Path = path;
            FileSyncResults = fileSyncResults;
        }
    }
    
    public class SyncErrorEventArgs : SyncEventArgs
    {

        #region Fields

        private Exception _Exception;

        #endregion

        #region Properties
        
        public Exception Exception
        {
            get
            {
                return _Exception;
            }
            private set
            {
                _Exception = value;
            }
        }

        #endregion

        public SyncErrorEventArgs(Direction direction, Exception exception)
            : base(direction)
        {
            Exception = exception;
        }
    }
    
    public class EntryEventArgs 
    {

        #region Fields

        private string _Path;

        #endregion

        #region Properties
        
        public string Path
        {
            get
            {
                return _Path;
            }
            private set
            {
                if (String.IsNullOrEmpty(value)) 
                {
                    throw new ArgumentNullException("Path", "Path null or empty.");
                }
                _Path = value;
            }
        }

        #endregion

        #region Methods

        public EntryEventArgs(string path)
        {
            Path = path;
        }

        #endregion

    }
        
    public class EntryErrorEventArgs : EntryEventArgs
    {

        #region Fields

        private Exception _exception;

        #endregion

        #region Properties

        public Exception Exception
        {
            get
            {
                return _exception;
            }
            private set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("Exception", "Exception null.");
                }
                _exception = value;
            }
        }



        #endregion

        #region Methods

        public EntryErrorEventArgs(string path, Exception exception) : base(path)
        {
            Exception = exception;
        }

        #endregion

    }

}
