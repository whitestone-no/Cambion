namespace Whitestone.Cambion.Events
{
    public class MessageReceivedEventArgs
    {
        public byte[] Data { get; set; }

        public MessageReceivedEventArgs(byte[] data)
        {
            Data = data;
        }
    }
}
