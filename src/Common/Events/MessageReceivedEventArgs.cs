using Whitestone.Cambion.Common.Types;

namespace Whitestone.Cambion.Common.Events
{
    public class MessageReceivedEventArgs
    {
        public MessageWrapper Message { get; set; }

        public MessageReceivedEventArgs(MessageWrapper message)
        {
            Message = message;
        }
    }
}
