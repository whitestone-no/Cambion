using NUnit.Framework;
using System;
using System.Threading;
using Whitestone.Cambion.Configurations;
using Whitestone.Cambion.Interfaces;
using Whitestone.Cambion.Serializer.JsonNet;
using Whitestone.Cambion.Transport.Loopback;

namespace Whitestone.Cambion.Test
{
    [Order(3)]
    class CambionTests
    {
        private ICambion _cambion;

        [SetUp]
        public void Setup()
        {
            _cambion = new CambionConfiguration()
                .Serializer.UseJsonNet()
                .Transport.UseLoopback()
                .Create();
        }

        [TearDown]
        public void TearDown()
        {
            _cambion.Dispose();
        }


        [Test]
        public void Register_NullValue_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => { _cambion.Register(null); });
        }

        [Test]
        public void Register_TwoOfSameObject_ThrowsArgumentException()
        {
            TwoOfSameObjectTest obj = new TwoOfSameObjectTest();

            _cambion.Register(obj);

            Assert.Throws<ArgumentException>(() => _cambion.Register(obj));
        }

        [Test]
        public void CallEventHandler_DefaultObjects_InterfaceSubscription()
        {
            string value = Guid.NewGuid().ToString();

            EventHandler obj = new EventHandler();
            _cambion.Register(obj);

            _cambion.PublishEvent(new TestEvent(value));

            TestEvent @event = obj.GetEvent();

            Assert.AreEqual(@event.Value, value);
        }

        [Test]
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

            _cambion.PublishEvent(new TestEvent(value));

            mre.WaitOne(new TimeSpan(0, 0, 5));

            Assert.AreEqual(response, value);
        }

        [Test]
        public void CallSynchronizedHandler_DefaultObjects_InterfaceSubscription()
        {
            string value = Guid.NewGuid().ToString();

            SynchronizedHandler obj = new SynchronizedHandler(value);
            _cambion.Register(obj);

            TestResponse response = _cambion.CallSynchronizedHandler<TestRequest, TestResponse>(new TestRequest());

            Assert.AreEqual(response.Value, value);
        }

        [Test]
        public void CallSynchronizedHandler_DefaultObjects_DirectSubscription()
        {
            string value = Guid.NewGuid().ToString();

            SynchronizedHandler obj = new SynchronizedHandler(value);

            _cambion.AddSynchronizedHandler<TestRequest, TestResponse>(req => new TestResponse(value));

            TestResponse response = _cambion.CallSynchronizedHandler<TestRequest, TestResponse>(new TestRequest());

            Assert.AreEqual(response.Value, value);
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
