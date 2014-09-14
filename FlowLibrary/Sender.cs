using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Flow.API;

namespace FlowLibrary
{
    public abstract class Sender : ISender
    {
        public event OnSendEventHandler OnSend;

        public void RaiseSendEvent(IMessage message)
        {
            OnSendEventHandler temp = OnSend;
            if (temp != null)
            {
                temp(message);
            }
        }
    }

    public abstract class Sender<T> : ISender<T>
    {
        public event OnSendEventHandler<T> OnSend;

        public void RaiseSendEvent(IMessage<T> message)
        {
            OnSendEventHandler<T> temp = OnSend;
            if (temp != null)
            {
                temp(message);
            }
        }
    }
}
