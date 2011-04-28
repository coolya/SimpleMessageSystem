using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMS
{
    public interface IMessageHandler<T>
    {
        void MessageArrived(Messenger.Message<T> message);
    }
}
