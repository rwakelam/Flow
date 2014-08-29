using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace WrapperLibrary
{
    public class DirectoryWrapper : IDirectory
    {
        private string _path;

        #region Properties

        public FileAttributesWrapper Attributes
        {
            set
            {
                DirectoryInfo directory = new DirectoryInfo(_path);
                directory.Attributes = (FileAttributes)value;
            }
            get
            {
                DirectoryInfo directory = new DirectoryInfo(_path);
                return (FileAttributesWrapper)directory.Attributes;
            }
        }

        public string Name
        {
            get
            {
                return System.IO.Path.GetFileName(_path); // Looks wrong; isn't.
            }
        }
        
        public string Path
        {
            private set
            {
                _path = value;
            }
            get
            {
                return _path;
            }
        }
        
        public DateTime LastWriteTimeUtc
        {
            set
            {
                Directory.SetLastWriteTimeUtc(_path, value);
            }
            get
            {
                return Directory.GetLastAccessTimeUtc(_path);
            }
        }

        #endregion

        #region Methods

        #region Constructors

        public DirectoryWrapper(string path)
        {
            Path = path;
        }

        # endregion

        public IFile GetFile(string name)
        {
            return new FileWrapper(System.IO.Path.Combine(Path, name));
        }

        public IEnumerable<IFile> GetFiles(string searchPattern = null, 
            FileAttributesWrapper attributes = FileAttributesWrapper.Any)
        {
            IEnumerable<string> paths = searchPattern == null ? Directory.GetFiles(_path) : 
                Directory.GetFiles(_path, searchPattern);        
            return   paths.Select<string, FileWrapper>(p => new FileWrapper(p)).Where(
                f => (f.Attributes & attributes) == attributes);            
        }
        
        public IDirectory GetSubDirectory(string name)
        {
            return new DirectoryWrapper(System.IO.Path.Combine(Path, name));
        }

        public IEnumerable<IDirectory> GetDirectories(string searchPattern = null,
            FileAttributesWrapper attributes = FileAttributesWrapper.Any)
        {
            IEnumerable<string> paths = searchPattern == null ? Directory.GetDirectories(_path) : 
                Directory.GetDirectories(_path, searchPattern);           
            return paths.Select<string, DirectoryWrapper>(p => new DirectoryWrapper(p)).Where(
                f => (f.Attributes & attributes) == attributes);
        }

        public void Create()
        {
            Directory.CreateDirectory(_path);
        }

        public void Delete()
        {
            Directory.Delete(_path, true);
        }
        
        // redundant?
        public IFile CreateFile(string name, FileAttributesWrapper attributes, byte[] bytes) 
        {
            return null;
        }

        public bool Exists()
        {
            return Directory.Exists(_path);
        }

        #endregion
        
    }
}
