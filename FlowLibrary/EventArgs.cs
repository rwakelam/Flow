using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlowLibrary
{
            
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
