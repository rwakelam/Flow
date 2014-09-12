using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Flow.Library.API;

namespace FlowLibrary
{
    private abstract class Message<T> : IMessage<T>
    {
        //private Dictionary<string, Object> _headers;

        public Dictionary<string, Object> Headers { get; set; }
        public T Payload { get; set; }

        public Message(Dictionary<string, Object> headers, T payload) 
        {
            this.Headers = headers;
            this.Payload = payload;
        }
    }
}
