using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flow.Library.API
{
    public interface IMessage<T>
    {
        Dictionary<string, object> Headers { get; set; }
        T Payload { get; set; }
    }
}
