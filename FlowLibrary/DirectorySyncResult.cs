using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlowLibrary
{
    public class DirectorySyncResult //: FileSyncResult
    {

        #region Constants

        #endregion

        #region Fields

        private Dictionary<string, FileSyncResult> _FileSyncResults = new Dictionary<string, FileSyncResult>();
        private Dictionary<string, DirectorySyncResult> _DirectorySyncResults = new Dictionary<string, DirectorySyncResult>();
        private Exception _Exception;

        #endregion

        #region Properties

        public Dictionary<string, DirectorySyncResult> DirectorySyncResults
        {
            get
            {
                return _DirectorySyncResults;
            }
            private set
            {
                _DirectorySyncResults = value;
            }
        }

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

        public Exception Exception
        {
            get { return _Exception; }
            private set { _Exception = value; }
        }

        #endregion

        #region Methods
        
        public DirectorySyncResult(Dictionary<string, DirectorySyncResult> directorySyncResults,
            Dictionary<String, FileSyncResult> fileSyncResults) 
        {
            FileSyncResults = fileSyncResults;
            DirectorySyncResults = directorySyncResults;
        }

        public DirectorySyncResult(Dictionary<string, DirectorySyncResult> directorySyncResults, 
            Dictionary<String, FileSyncResult> fileSyncResults, Exception ex) : this(directorySyncResults, fileSyncResults)
        {
            Exception = ex;
        }     

        #endregion

    }
}
