using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flow.Library.API
{
    public delegate void OnSendEventHandler<T>(IMessage<T> message);

    public interface ISender<T>
    {
        //delegate void OnSendEventHandler(IMessage<T> message);
        event OnSendEventHandler<T> OnSend;
    }
}
