using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flow.Library.API
{
    public interface IReceiver<T>
    {
        void OnReceive(IMessage<T> message);
    }
}
