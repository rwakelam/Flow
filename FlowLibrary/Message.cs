using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Flow.API;

namespace FlowLibrary
{
    public class Message : IMessage
    {
        //private Dictionary<string, Object> _headers;

        public Dictionary<string, Object> Headers { get; set; }

        public Message(Dictionary<string, Object> headers)
        {
            this.Headers = headers;
        }
    }//TODO:: protect header keys and values from interference

    public class Message<T> : Message, IMessage<T>
    {
        public T Payload { get; set; }

        public Message(Dictionary<string, Object> headers, T payload) : base(headers) 
        {
            this.Payload = payload;
        }
    }
}
