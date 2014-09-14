using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flow.API
{
    public interface IMessage
    {
        Dictionary<string, object> Headers { get; set; }
    }

    public interface IMessage<T> : IMessage
    {
        T Payload { get; set; }
    }
}
