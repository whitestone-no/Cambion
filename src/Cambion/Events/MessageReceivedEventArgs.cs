using System.Collections.Generic;

namespace Whitestone.Cambion.Events
{
    public class MessageReceivedEventArgs
    {
        public MessageWrapper Data { get; set; }

        public MessageReceivedEventArgs(MessageWrapper data)
        {
            Data = data;
        }
    }
}
