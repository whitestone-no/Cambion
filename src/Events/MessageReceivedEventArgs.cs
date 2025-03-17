namespace Whitestone.Cambion.Events
{
    public class MessageReceivedEventArgs
    {
        public byte[] MessageBytes { get; set; }

        public MessageReceivedEventArgs(byte[] messageBytes)
        {
            MessageBytes = messageBytes;
        }
    }
}
