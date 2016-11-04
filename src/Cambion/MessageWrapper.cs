using System;

namespace Whitestone.Cambion
{
    public class MessageWrapper
    {
        public object Data { get; set; }
        public Type DataType { get; set; }
        public MessageType MessageType { get; set; }
    }

    public enum MessageType
    {
        Event,
        Synchronized,
        TargetableSynchronized
    }
}
