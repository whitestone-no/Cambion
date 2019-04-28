using System;

namespace Whitestone.Cambion.Common.Types
{
    public class MessageWrapper
    {
        public object Data { get; set; }
        public Type DataType { get; set; }
        public Type ResponseType { get; set; }
        public MessageType MessageType { get; set; }
        public Guid CorrelationId { get; set; }
    }

    public enum MessageType
    {
        Event,
        SynchronizedRequest,
        SynchronizedResponse
    }
}
