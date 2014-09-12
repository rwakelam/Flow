using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Flow.Library.API;

namespace FlowLibrary
{
    public class FileMessage : Message<Byte[]>
    {
        public FileMessage(string path, Byte[] payload) : base(new Dictionary<string, Object>()
        { {"Path", path } }, payload) 
        { 
        
        }
    }
}
