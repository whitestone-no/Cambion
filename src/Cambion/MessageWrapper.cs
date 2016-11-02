using System;

namespace Whitestone.Cambion
{
    public class MessageWrapper
    {
        public object Message { get; set; }
        public Type MessageType { get; set; }
        public int SomeOtherMeta { get; set; }
    }
}
