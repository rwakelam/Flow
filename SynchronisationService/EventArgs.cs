using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SynchronisationService
{

    public class OnPushableEventArgs : EventArgs
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

        public OnPushableEventArgs(string sourcePath, string targetPath)
        {
            SourcePath = sourcePath;
            TargetPath = targetPath;
        }

        #endregion

    }

}
