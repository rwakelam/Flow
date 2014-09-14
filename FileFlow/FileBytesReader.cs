using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlowLibrary;
using Flow.API;
using FileFlow.API;

namespace FileFlow
{
    public class FileBytesReader : Sender<Byte[]>, IReceiver
    {
        public void OnReceive(IMessage message)
        {
            string path = ((IFileMessage)message).Path;
            Byte[] data = File.ReadAllBytes(path);
            FileDataMessage response = new FileDataMessage(path, data);
            RaiseSendEvent(response);
        }
    }
}
