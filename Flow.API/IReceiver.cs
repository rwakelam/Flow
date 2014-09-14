using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flow.API
{
    public interface IReceiver
    {
        void OnReceive(IMessage message);
    }

    public interface IReceiver<T>
    {
        void OnReceive(IMessage<T> message);
    }
}
