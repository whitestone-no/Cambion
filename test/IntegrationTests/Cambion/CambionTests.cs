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
        public async Task DirectAsyncEventSubscription()
        {
            TestEvent expectedEvent = new(RandomValue.String());

            TestEvent actualEvent = null;
            _cambion.AddAsyncEventHandler<TestEvent>(async e =>
            {
                actualEvent = e;
                await Task.CompletedTask;
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
        public async Task InterfaceAsyncEventSubscription()
        {
            TestEvent expectedEvent = new TestEvent(RandomValue.String());
            TestAsyncEventSubscriber subscriber = new TestAsyncEventSubscriber();

            _cambion.Register(subscriber);

            await _cambion.PublishEventAsync(expectedEvent);

            await Task.Delay(1000);

            Assert.Equal(expectedEvent.EventValue, subscriber.ActualEvent.EventValue);
        }

        [Fact]
        public async Task MultipleDirectAndInterfaceSubscription()
        {
            TestEvent expectedEvent = new TestEvent(RandomValue.String());

            TestEvent actualEvent1_1 = null;
            _cambion.AddEventHandler<TestEvent>(e =>
            {
                actualEvent1_1 = e;
            });

            TestEvent actualEvent1_2 = null;
            _cambion.AddEventHandler<TestEvent>(e =>
            {
                actualEvent1_2 = e;
            });

            TestEvent actualEvent2_1 = null;
            _cambion.AddAsyncEventHandler<TestEvent>(async e =>
            {
                await Task.Delay(1);
                actualEvent2_1 = e;
            });

            TestEvent actualEvent2_2 = null;
            _cambion.AddAsyncEventHandler<TestEvent>(async e =>
            {
                await Task.Delay(1);
                actualEvent2_2 = e;
            });

            TestEventSubscriber subscriber1_1 = new();
            TestEventSubscriber subscriber1_2 = new();
            TestAsyncEventSubscriber subscriber2_1 = new();
            TestAsyncEventSubscriber subscriber2_2 = new();

            _cambion.Register(subscriber1_1);
            _cambion.Register(subscriber1_2);
            _cambion.Register(subscriber2_1);
            _cambion.Register(subscriber2_2);

            await _cambion.PublishEventAsync(expectedEvent);

            await Task.Delay(1000);
            
            Assert.Equal(expectedEvent.EventValue, actualEvent1_1.EventValue);
            Assert.Equal(expectedEvent.EventValue, actualEvent1_2.EventValue);
            Assert.Equal(expectedEvent.EventValue, subscriber1_1.ActualEvent.EventValue);
            Assert.Equal(expectedEvent.EventValue, subscriber1_2.ActualEvent.EventValue);

            Assert.Equal(expectedEvent.EventValue, actualEvent2_1.EventValue);
            Assert.Equal(expectedEvent.EventValue, actualEvent2_2.EventValue);
            Assert.Equal(expectedEvent.EventValue, subscriber2_1.ActualEvent.EventValue);
            Assert.Equal(expectedEvent.EventValue, subscriber2_2.ActualEvent.EventValue);
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

        [Fact]
        public async Task DirectAsyncSynchronizedSubscription()
    {
            TestRequest expectedRequest = new(RandomValue.String());
            TestResponse expectedResponse = new(RandomValue.String());
        
            TestRequest actualRequest = null;
            _cambion.AddAsyncSynchronizedHandler<TestRequest, TestResponse>(async request =>
        {
                await Task.Delay(1);
                actualRequest = request;
                return expectedResponse;
            });

            TestResponse actualResponse = await _cambion.CallSynchronizedHandlerAsync<TestRequest, TestResponse>(expectedRequest);

            await Task.Delay(1000);

            Assert.Equal(expectedRequest.RequestValue, actualRequest.RequestValue);
            Assert.Equal(expectedResponse.ResponseValue, actualResponse.ResponseValue);
        }
    }

        [Fact]
        public async Task InterfaceAsyncSynchronizedSubscription()
    {
            TestRequest expectedRequest = new(RandomValue.String());
            TestResponse expectedResponse = new(RandomValue.String());
        
            TestAsyncSynchronizedSubscriber subscriber = new(expectedResponse);

            _cambion.Register(subscriber);

            TestResponse actualResponse = await _cambion.CallSynchronizedHandlerAsync<TestRequest, TestResponse>(expectedRequest);

            await Task.Delay(1000);

            Assert.Equal(expectedRequest.RequestValue, subscriber.ActualRequest.RequestValue);
            Assert.Equal(expectedResponse.ResponseValue, actualResponse.ResponseValue);
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

    public class TestAsyncEventSubscriber : IAsyncEventHandler<TestEvent>
    {
        public TestEvent ActualEvent { get; private set; }

        public async Task HandleEventAsync(TestEvent input)
        {
            ActualEvent = input;
            await Task.Delay(1);
        }
    }

    public class TestSynchronizedSubscriber : ISynchronizedHandler<TestRequest, TestResponse>
    {
        public TestRequest ActualRequest { get; private set; }

        private readonly TestResponse _expectedResponse;

    public class TestAsyncSynchronizedSubscriber(TestResponse expectedResponse) : IAsyncSynchronizedHandler<TestRequest, TestResponse>
        {
        public TestRequest ActualRequest { get; private set; }

        public async Task<TestResponse> HandleSynchronizedAsync(TestRequest input)
        {
            await Task.Delay(1);

            ActualRequest = input;

            return expectedResponse;
        }
    }
}
