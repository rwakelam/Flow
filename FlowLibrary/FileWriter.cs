using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Flow.Library.API;

namespace FlowLibrary
{
    public class FileWriter : IReceiver<Byte[]>
    {
        public void OnReceive(IMessage<Byte[]> message)
        {
           // File.Create(message.Headers.TryGetValue("Path"))
        }
    }
}
