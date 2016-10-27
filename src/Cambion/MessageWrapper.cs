using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whitestone.Cambion
{
    public class MessageWrapper
    {
        public object Message { get; set; }
        public string SomeMeta { get; set; }
        public int SomeOtherMeta { get; set; }
    }
}
