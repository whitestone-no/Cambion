using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using RandomTestValues;
using Whitestone.Cambion.Interfaces;
using Whitestone.Cambion.Serializer.JsonNet;
using Whitestone.Cambion.Transport.Loopback;
using Xunit;

namespace Whitestone.Cambion.IntegrationTests.Cambion
{
    public class CambionTests
    {
        private readonly Whitestone.Cambion.Cambion _cambion;

        public CambionTests()
        {
            Mock<ILogger<Whitestone.Cambion.Cambion>> logger = new Mock<ILogger<Whitestone.Cambion.Cambion>>();

            _cambion = new Whitestone.Cambion.Cambion(new LoopbackTransport(), new JsonNetSerializer(), logger.Object);
            
            _cambion.StartAsync(default).GetAwaiter().GetResult();
        }

        [Fact]
        public async Task DirectEventSubscription()
        {
            TestEvent expectedEvent = new TestEvent(RandomValue.String());

            TestEvent actualEvent = null;
            _cambion.AddEventHandler<TestEvent>(e =>
            {
                actualEvent = e;
            });

            await _cambion.PublishEventAsync(expectedEvent);

            // Allow some time for the event to propagate
            await Task.Delay(1000);

            // Compare the values. As these will actually be different instances of TestEvent they will not assert equal even though they are
            Assert.Equal(expectedEvent.EventValue, actualEvent.EventValue);
        }

        [Fact]
        public async Task InterfaceEventSubscription()
        {
            TestEvent expectedEvent = new TestEvent(RandomValue.String());
            TestEventSubscriber subscriber = new TestEventSubscriber();

            _cambion.Register(subscriber);

            await _cambion.PublishEventAsync(expectedEvent);

            await Task.Delay(1000);

            Assert.Equal(expectedEvent.EventValue, subscriber.ActualEvent.EventValue);
        }

        [Fact]
        public async Task MultipleDirectAndInterfaceSubscription()
        {
            TestEvent expectedEvent = new TestEvent(RandomValue.String());

            TestEvent actualEvent1 = null;
            _cambion.AddEventHandler<TestEvent>(e =>
            {
                actualEvent1 = e;
            });

            TestEvent actualEvent2 = null;
            _cambion.AddEventHandler<TestEvent>(e =>
            {
                actualEvent2 = e;
            });

            TestEventSubscriber subscriber1 = new TestEventSubscriber();
            TestEventSubscriber subscriber2 = new TestEventSubscriber();

            _cambion.Register(subscriber1);
            _cambion.Register(subscriber2);

            await _cambion.PublishEventAsync(expectedEvent);

            await Task.Delay(1000);
            
            Assert.Equal(expectedEvent.EventValue, actualEvent1.EventValue);
            Assert.Equal(expectedEvent.EventValue, actualEvent2.EventValue);
            Assert.Equal(expectedEvent.EventValue, subscriber1.ActualEvent.EventValue);
            Assert.Equal(expectedEvent.EventValue, subscriber2.ActualEvent.EventValue);
        }

        [Fact]
        public async Task DirectSynchronizedSubscription()
        {
            TestRequest expectedRequest = new TestRequest(RandomValue.String());
            TestResponse expectedResponse = new TestResponse(RandomValue.String());

            TestRequest actualRequest = null;
            _cambion.AddSynchronizedHandler<TestRequest, TestResponse>(request =>
            {
                actualRequest = request;
                return expectedResponse;
            });

            TestResponse actualResponse = await _cambion.CallSynchronizedHandlerAsync<TestRequest, TestResponse>(expectedRequest);

            await Task.Delay(1000);

            Assert.Equal(expectedRequest.RequestValue, actualRequest.RequestValue);
            Assert.Equal(expectedResponse.ResponseValue, actualResponse.ResponseValue);
        }

        [Fact]
        public async Task InterfaceSynchronizedSubscription()
        {
            TestRequest expectedRequest = new TestRequest(RandomValue.String());
            TestResponse expectedResponse = new TestResponse(RandomValue.String());

            TestSynchronizedSubscriber subscriber = new TestSynchronizedSubscriber(expectedResponse);

            _cambion.Register(subscriber);

            TestResponse actualResponse = await _cambion.CallSynchronizedHandlerAsync<TestRequest, TestResponse>(expectedRequest);

            await Task.Delay(1000);

            Assert.Equal(expectedRequest.RequestValue, subscriber.ActualRequest.RequestValue);
            Assert.Equal(expectedResponse.ResponseValue, actualResponse.ResponseValue);
        }
    }

    public class TestEvent
    {
        public string EventValue { get; }
        
        public TestEvent(string eventValue)
        {
            EventValue = eventValue;
        }
    }

    public class TestRequest
    {
        public string RequestValue { get; }
        
        public TestRequest(string requestValue)
        {
            RequestValue = requestValue;
        }
    }

    public class TestResponse
    {
        public string ResponseValue { get; }

        public TestResponse(string responseValue)
        {
            ResponseValue = responseValue;
        }
    }

    public class TestEventSubscriber : IEventHandler<TestEvent>
    {
        public TestEvent ActualEvent { get; private set; }

        public void HandleEvent(TestEvent input)
        {
            ActualEvent = input;
        }
    }

    public class TestSynchronizedSubscriber : ISynchronizedHandler<TestRequest, TestResponse>
    {
        public TestRequest ActualRequest { get; private set; }

        private readonly TestResponse _expectedResponse;

        public TestSynchronizedSubscriber(TestResponse expectedResponse)
        {
            _expectedResponse = expectedResponse;
        }

        public TestResponse HandleSynchronized(TestRequest input)
        {
            ActualRequest = input;

            return _expectedResponse;
        }
    }
}
