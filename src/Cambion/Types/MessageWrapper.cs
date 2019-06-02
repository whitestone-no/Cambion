using System;

namespace Whitestone.Cambion.Types
{
    public class MessageWrapper
    {
        public object Data { get; set; }
        public Type DataType { get; set; }
        public Type ResponseType { get; set; }
        public MessageType MessageType { get; set; }
        public Guid CorrelationId { get; set; }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            MessageWrapper objWrapper = (MessageWrapper)obj;

            // Wrap the Data property in "dynamic" to allow for value comparison (i.e. (int)1 == (long)1 will be true)
            return ((dynamic)Data == (dynamic)objWrapper.Data) &&
                DataType == objWrapper.DataType &&
                ResponseType == objWrapper.ResponseType &&
                MessageType == objWrapper.MessageType &&
                CorrelationId == objWrapper.CorrelationId;
        }

        public override int GetHashCode()
        {
            int hash = Data.GetHashCode();
            hash = (hash * 397) ^ DataType.GetHashCode();
            hash = (hash * 397) ^ ResponseType.GetHashCode();
            hash = (hash * 397) ^ MessageType.GetHashCode();
            hash = (hash * 397) ^ CorrelationId.GetHashCode();
            return hash;
        }
    }

    public enum MessageType
    {
        Event,
        SynchronizedRequest,
        SynchronizedResponse
    }
}
