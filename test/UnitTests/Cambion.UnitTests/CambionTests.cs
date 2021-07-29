using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Whitestone.Cambion.Interfaces;
using Xunit;

namespace Whitestone.Cambion.Test
{
    public class CambionTests
    {
        private Mock<ITransport> _transport;
        private Mock<ISerializer> _serializer;
        private Mock<ILogger<Cambion>> _logger;

        private ICambion _cambion;

        public CambionTests()
        {
            _transport = new Mock<ITransport>();
            _serializer = new Mock<ISerializer>();
            _logger = new Mock<ILogger<Cambion>>();

            _cambion = new Cambion(_transport.Object, _serializer.Object, _logger.Object);
        }


        [Fact]
        public void Register_NullValue_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => { _cambion.Register(null); });
        }

        [Fact]
        public void Register_TwoOfSameObject_ThrowsArgumentException()
        {
            TwoOfSameObjectTest obj = new TwoOfSameObjectTest();

            _cambion.Register(obj);

            Assert.Throws<ArgumentException>(() => _cambion.Register(obj));
        }

        [Fact]
        public void CallEventHandler_DefaultObjects_InterfaceSubscription()
        {
            string value = Guid.NewGuid().ToString();

            EventHandler obj = new EventHandler();
            _cambion.Register(obj);

            _cambion.PublishEventAsync(new TestEvent(value));

            TestEvent @event = obj.GetEvent();

            Assert.Equal(@event.Value, value);
        }

        [Fact]
        public void CallEventHandler_DefaultObjects_DirectSubscription()
        {
            ManualResetEvent mre = new ManualResetEvent(false);
            string value = Guid.NewGuid().ToString();
            string response = null;

            _cambion.AddEventHandler<TestEvent>(e =>
            {
                response = e.Value;
                mre.Set();
            });

            _cambion.PublishEventAsync(new TestEvent(value));

            mre.WaitOne(new TimeSpan(0, 0, 5));

            Assert.Equal(response, value);
        }

        [Fact]
        public async Task CallSynchronizedHandler_DefaultObjects_InterfaceSubscription()
        {
            string value = Guid.NewGuid().ToString();

            SynchronizedHandler obj = new SynchronizedHandler(value);
            _cambion.Register(obj);

            TestResponse response = await _cambion.CallSynchronizedHandlerAsync<TestRequest, TestResponse>(new TestRequest());

            Assert.Equal(response.Value, value);
        }

        [Fact]
        public async Task CallSynchronizedHandler_DefaultObjects_DirectSubscription()
        {
            string value = Guid.NewGuid().ToString();

            _cambion.AddSynchronizedHandler<TestRequest, TestResponse>(req => new TestResponse(value));

            TestResponse response = await _cambion.CallSynchronizedHandlerAsync<TestRequest, TestResponse>(new TestRequest());

            Assert.Equal(response.Value, value);
        }
    }

    class TestEvent
    {
        public string Value { get; }

        public TestEvent(string value)
        {
            Value = value;
        }
    }

    class TestRequest { }

    class TestResponse
    {
        public string Value { get; }

        public TestResponse(string value)
        {
            Value = value;
        }
    }


    class TwoOfSameObjectTest : ISynchronizedHandler<TestRequest, TestResponse>
    {
        public TestResponse HandleSynchronized(TestRequest input)
        {
            return new TestResponse(null);
        }
    }
    class SynchronizedHandler : ISynchronizedHandler<TestRequest, TestResponse>
    {
        private readonly string _value;

        public SynchronizedHandler(string value)
        {
            _value = value;
        }

        public TestResponse HandleSynchronized(TestRequest input)
        {
            return new TestResponse(_value);
        }
    }

    class EventHandler : IEventHandler<TestEvent>
    {
        private readonly ManualResetEvent _mre = new ManualResetEvent(false);
        private TestEvent _eventValue;

        public void HandleEvent(TestEvent input)
        {
            _eventValue = input;
            _mre.Set();
        }

        public TestEvent GetEvent()
        {
            _mre.WaitOne(new TimeSpan(0, 0, 5));
            return _eventValue;
        }
    }
}
