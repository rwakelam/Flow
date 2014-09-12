using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Flow.Library.API;

namespace FlowLibrary
{
    public abstract class Sender<T> : ISender<T>
    {
        //public delegate void OnSendEventHandler(IMessage<T> message);
        public event OnSendEventHandler<T> OnSend;

        protected void RaiseSendEvent(IMessage<T> message)
        {
            OnSendEventHandler<T> temp = OnSend;
            if (temp != null)
            {
                //var eventArgs = new OnPushableEventArgs(sourcePath, targetPath);
                temp(message);
            }
        }
    }
}
