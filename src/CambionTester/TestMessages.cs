using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whitestone.CambionTester
{
    public class TestMessageSimple
    {
        public DateTime CurrentDateTime { get; set; }
    }

    public class TestMessageComplex
    {
        public TestMessageSimple CurrentDateTime { get; set; }
        public Guid MyId { get; set; }
        public string SomeText { get; set; }
    }

    public class TestMessageRequest
    {
        public int Id { get; set; }
    }

    public class TestMessageResponse
    {
        public string Value { get; set; }
    }
}
