using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flow.API
{
    public delegate void OnSendEventHandler(IMessage message);
    public delegate void OnSendEventHandler<T>(IMessage<T> message);

    public interface ISender
    {
        event OnSendEventHandler OnSend;
    }

    public interface ISender<T>
    {
        event OnSendEventHandler<T> OnSend;
    }
}
