using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlowLibrary;

namespace FileFlow
{
    public class FileEventMessage : Message
    {
        public FileEventMessage(string path, WatcherChangeTypes changeType)
            : base(new Dictionary<string, Object>() { { "Path", path }, { "ChangeType", changeType } })
        {

        }
    }
}
