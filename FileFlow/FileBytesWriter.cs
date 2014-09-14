using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Flow.API;
using FileFlow.API;

namespace FileFlow
{
    public class FileBytesWriter : IReceiver<Byte[]>
    {
        public void OnReceive(IMessage<Byte[]> message)
        {
            string path = ((IFileMessage)message).Path;
            File.WriteAllBytes(path, message.Payload);
        }
    }
}
