using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flow.API
{
    public interface IFilter : IReceiver, ISender
    {
        string Expression { get; set; }
    }

    public interface IFilter<T> : IReceiver<T>, ISender<T>, IFilter
    {
    }
}
