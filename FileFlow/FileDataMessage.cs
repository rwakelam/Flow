using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using FlowLibrary;
using Flow.API;
using FileFlow.API;

namespace FileFlow
{
  
    public class FileDataMessage : Message<Byte[]>, IFileBytesMessage
    {
        private const string PATH_KEY = "Path";

        public FileDataMessage(string path, Byte[] payload) : 
            base(new Dictionary<string, Object>() { { PATH_KEY, path } }, payload) 
        {         
        }

        public string Path
        {
            get
            {
                return (string)Headers[PATH_KEY];
            }
            set { }
        }

    }
}
