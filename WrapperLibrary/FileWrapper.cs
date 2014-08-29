using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace WrapperLibrary
{
    public class FileWrapper : IFile
    {
        #region Fields

        private string _path;

        #endregion

        #region Property

        public FileAttributesWrapper Attributes
        {
            set
            {
                File.SetAttributes(_path, (FileAttributes)value);
            }
            get
            {
                return (FileAttributesWrapper)File.GetAttributes(_path);
            }
        }
        
        public DateTime LastWriteTimeUtc
        {
            set
            {
                File.SetLastWriteTimeUtc(_path, value);
            }
            get
            {
                return File.GetLastAccessTimeUtc(_path);
            }
        }

        public Int64 Length
        {
            get 
            {
                return new FileInfo(Path).Length;
            }
        }

        public string Name
        {
            get
            {
                return System.IO.Path.GetFileName(_path);
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
        
        #endregion

        #region Methods

        #region Constructors

        public FileWrapper(string path)
        {
            Path = path;
        }

        #endregion

        // redundant?
        public IFile Copy(IDirectory directory, bool overwrite = false) 
        {
            string path = System.IO.Path.Combine(directory.Path, Name);
            IFile result = new FileWrapper(path);
            if (result.Exists() && !overwrite)
            {
                throw new Exception("Filename in use.");//"File exists already, 
            }
            else 
            {
                result.Delete();
            }
            result.WriteAllBytes(ReadAllBytes());// what if file name is already in use? provide overwrite arg?
            return result;
        }

        public void Delete()
        {
            File.Delete(_path);
        }

        //public bool Equals(IFile file) 
        //{
        //    if (Length != file.Length) 
        //    {
        //        return false;
        //    }
        //}

        public bool Exists()
        {
            return File.Exists(_path);
        }
        
        // is this unique, according to file contents? like checksum
        //public int GetHashCode() 
        //{
            
        //    return new FileInfo(Path).GetHashCode();
        //}

        //redundant?
        public byte[] ReadAllBytes()
        {
            return File.ReadAllBytes(_path);
        }

        public IFileReader GetReader() 
        {
            return new FileReader(Path);
        }
 
        // redundant?
        public void WriteAllBytes(byte[] bytes)
        {
            File.WriteAllBytes(_path, bytes);
        }

        #endregion
    }
}
