using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Flow.API;

namespace FileFlow.API
{    
    public interface IFileMessage : IMessage
    {
        string Path { get; set; }
    }

    public interface IFileEventMessage : IFileMessage
    {
        WatcherChangeTypes ChangeType { get; set; }
    }

    public interface IFileBytesMessage : IFileMessage, IMessage<Byte[]>
    {
    }
}
