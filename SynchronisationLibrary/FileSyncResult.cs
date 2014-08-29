using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SynchronisationLibrary
{
    public class FileSyncResult 
    {
        private SyncResult _Result = SyncResult.Unchanged;
        private Exception _Exception;

        public SyncResult Result
        {
          get { return _Result; }
          private set { _Result = value; }
        }

        public Exception Exception
        {
          get { return _Exception; }
          private set { _Exception = value; }
        }

        public FileSyncResult(SyncResult result) 
        {
            Result = result;
        }

        public FileSyncResult(Exception ex) : this(SyncResult.Failed)
        {
            Exception = ex;
        }

    }
}
