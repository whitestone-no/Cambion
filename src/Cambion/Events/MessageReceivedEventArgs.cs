namespace Whitestone.Cambion.Events
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
