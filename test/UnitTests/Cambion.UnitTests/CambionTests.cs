using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using RandomTestValues;
using Whitestone.Cambion.Events;
using Whitestone.Cambion.Interfaces;
using Whitestone.Cambion.Types;
using Xunit;

// ReSharper disable PossibleNullReferenceException

namespace Whitestone.Cambion.UnitTests.Cambion
{
    public class CambionTests
    {
        private readonly Mock<ITransport> _transport;
        private readonly Mock<ISerializer> _serializer;
        private readonly Mock<ILogger<Whitestone.Cambion.Cambion>> _logger;

        public CambionTests()
        {
            _transport = new Mock<ITransport>();
            _serializer = new Mock<ISerializer>();
            _logger = new Mock<ILogger<Whitestone.Cambion.Cambion>>();

            _logger.Setup(x => x.IsEnabled(It.IsAny<LogLevel>()))
                .Returns(true);
            _logger.Setup(x => x.IsEnabled(It.Is<LogLevel>(y => y == LogLevel.Trace)))
                .Returns(false);
        }

        #region Constructor

        [Fact]
        public void Construct_WithNullTransportAndSerializer_LogsInfo()
        {
            // Arrange
            
            ServiceCollection services = new();

            // Act

            _ = new Whitestone.Cambion.Cambion(services.BuildServiceProvider(), _logger.Object);

            // Assert

            _logger.Verify(
                x => x.Log(
                    It.Is<LogLevel>(y => y == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == "No transport found. Falling back to loopback implementation."),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);

            _logger.Verify(
                x => x.Log(
                    It.Is<LogLevel>(y => y == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == "No serializer found. Falling back to loopback implementation."),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        [Fact]
        public void Construct_WithNullTransport_LogsInfo()
        {
            // Arrange

            ServiceCollection services = new();
            services.AddSingleton(_serializer.Object);

            // Act

            _ = new Whitestone.Cambion.Cambion(services.BuildServiceProvider(), _logger.Object);

            // Assert

            _logger.Verify(
                x => x.Log(
                    It.Is<LogLevel>(y => y == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == "No transport found. Falling back to loopback implementation."),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        [Fact]
        public void Construct_WithNullSerializer_LogsInfo()
        {
            // Arrange
            
            ServiceCollection services = new();
            services.AddSingleton(_transport.Object);

            // Act

            _ = new Whitestone.Cambion.Cambion(services.BuildServiceProvider(), _logger.Object);

            // Assert

            _logger.Verify(
                x => x.Log(
                    It.Is<LogLevel>(y => y == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == "No serializer found. Falling back to loopback implementation."),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        [Fact]
        public void Construct_WithNullLogger_ThrowsException()
        {
            // Arrange

            ServiceCollection services = new();

            // Act

            var actualException = Assert.Throws<TypeInitializationException>(() => new Whitestone.Cambion.Cambion(services.BuildServiceProvider(), null));

            // Assert

            Assert.Equal("Whitestone.Cambion.Cambion", actualException.TypeName);
            Assert.IsType<ArgumentException>(actualException.InnerException);
            Assert.Equal("Missing logger", actualException.InnerException.Message);
        }

        #endregion

        #region ReinitializeAsync()

        [Fact]
        public async Task ReinitializeAsync_Success()
        {
            // Arrange

            ServiceCollection services = new();
            services.AddSingleton(_transport.Object);
            services.AddSingleton(_serializer.Object);

            Whitestone.Cambion.Cambion sut = new(services.BuildServiceProvider(), _logger.Object);

            // Act

            await sut.ReinitializeAsync();

            // Assert

            _logger.Verify(
                x => x.Log(
                    It.Is<LogLevel>(y => y == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == "Reinitializing Transport Castle.Proxies.ITransportProxy"),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);

            _transport.VerifyRemove(x => x.MessageReceived -= It.IsAny<EventHandler<MessageReceivedEventArgs>>(), Times.Once);
            _transport.Verify(x => x.StopAsync(), Times.Once);
            _transport.VerifyAdd(x => x.MessageReceived += It.IsAny<EventHandler<MessageReceivedEventArgs>>(), Times.Once);
            _transport.Verify(x => x.StartAsync(), Times.Once);
        }

        [Fact]
        public async Task ReinitializeAsync_Loopback()
        {
            // Arrange

            ServiceCollection services = new();

            Whitestone.Cambion.Cambion sut = new(services.BuildServiceProvider(), _logger.Object);

            // Act

            await sut.ReinitializeAsync();

            // Assert

            _logger.Verify(
                x => x.Log(
                    It.Is<LogLevel>(y => y == LogLevel.Warning),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == "No transport or serializer defined. Using fallback. Nothing to reinitialize."),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);

            _transport.VerifyRemove(x => x.MessageReceived -= It.IsAny<EventHandler<MessageReceivedEventArgs>>(), Times.Never);
            _transport.Verify(x => x.StopAsync(), Times.Never);
            _transport.VerifyAdd(x => x.MessageReceived += It.IsAny<EventHandler<MessageReceivedEventArgs>>(), Times.Never);
            _transport.Verify(x => x.StartAsync(), Times.Never);
        }

        #endregion

        #region Register()

        [Fact]
        public void Register_EventHandler_Success()
        {
            // Arrange

            ServiceCollection services = new();
            EventHandler handler = new();

            Whitestone.Cambion.Cambion sut = new(services.BuildServiceProvider(), _logger.Object);

            // Act

            sut.Register(handler);

            // Assert

            _logger.Verify(
                x => x.Log(
                    It.Is<LogLevel>(y => y == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == $"Registered <{handler.GetType().FullName}> as event handler for <{typeof(TestEvent).FullName}>"),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        [Fact]
        public void Register_AsyncEventHandler_Success()
        {
            // Arrange

            ServiceCollection services = new();
            AsyncEventHandler handler = new();

            Whitestone.Cambion.Cambion sut = new(services.BuildServiceProvider(), _logger.Object);

            // Act

            sut.Register(handler);

            // Assert

            _logger.Verify(
                x => x.Log(
                    It.Is<LogLevel>(y => y == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == $"Registered <{handler.GetType().FullName}> as async event handler for <{typeof(TestEvent).FullName}>"),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        [Fact]
        public void Register_SynchronizedHandler_Success()
        {
            // Arrange

            ServiceCollection services = new();
            SynchronizedHandler handler = new(RandomValue.String());

            Whitestone.Cambion.Cambion sut = new(services.BuildServiceProvider(), _logger.Object);

            // Act

            sut.Register(handler);

            // Assert

            _logger.Verify(
                x => x.Log(
                    It.Is<LogLevel>(y => y == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == $"Registered <{handler.GetType().FullName}> as synchronized handler for <{typeof(TestRequest).FullName}, {typeof(TestResponse).FullName}>"),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        [Fact]
        public void Register_AsyncSynchronizedHandler_Success()
        {
            // Arrange

            ServiceCollection services = new();
            AsyncSynchronizedHandler handler = new(RandomValue.String());

            Whitestone.Cambion.Cambion sut = new(services.BuildServiceProvider(), _logger.Object);

            // Act

            sut.Register(handler);

            // Assert

            _logger.Verify(
                x => x.Log(
                    It.Is<LogLevel>(y => y == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == $"Registered <{handler.GetType().FullName}> as async synchronized handler for <{typeof(TestRequest).FullName}, {typeof(TestResponse).FullName}>"),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        [Fact]
        public void Register_NullValue_ThrowsArgumentNullException()
        {
            // Arrange

            ServiceCollection services = new();
            Whitestone.Cambion.Cambion sut = new(services.BuildServiceProvider(), _logger.Object);

            // Act

            var actualException = Assert.Throws<ArgumentNullException>(() => sut.Register(null));

            // Assert

            Assert.Equal("handler", actualException.ParamName);
        }

        [Fact]
        public void Register_TwoOfSameObject_ThrowsArgumentException()
        {
            // Arrange

            ServiceCollection services = new();
            TwoOfSameObjectTest obj = new();

            Whitestone.Cambion.Cambion sut = new(services.BuildServiceProvider(), _logger.Object);
            sut.Register(obj);

            // Act

            var actualException = Assert.Throws<ArgumentException>(() => sut.Register(obj));

            // Assert
            Assert.Equal($"A SynchronizedHandler already exists for request type {typeof(TestRequest).FullName} and response type {typeof(TestResponse).FullName} (Parameter 'delegate')", actualException.Message);
        }

        [Fact]
        public void Register_TwoOfSameHandlers_ThrowsArgumentException()
        {
            // Arrange

            ServiceCollection services = new();
            TwoOfSameObjectTest obj1 = new();
            TwoOfSameObjectTest obj2 = new();

            Whitestone.Cambion.Cambion sut = new(services.BuildServiceProvider(), _logger.Object);

            // Act

            sut.Register(obj1);

            var actualException = Assert.Throws<ArgumentException>(() => sut.Register(obj2));

            // Assert
            Assert.Equal($"A SynchronizedHandler already exists for request type {typeof(TestRequest).FullName} and response type {typeof(TestResponse).FullName} (Parameter 'delegate')", actualException.Message);
        }

        [Fact]
        public void Register_TwoOfSameAsyncObject_ThrowsArgumentException()
        {
            // Arrange

            ServiceCollection services = new();
            TwoOfSameAsyncObjectTest obj = new();

            Whitestone.Cambion.Cambion sut = new(services.BuildServiceProvider(), _logger.Object);
            sut.Register(obj);

            // Act

            var actualException = Assert.Throws<ArgumentException>(() => sut.Register(obj));

            // Assert
            Assert.Equal($"An AsyncSynchronizedHandler already exists for request type {typeof(TestRequest).FullName} and response type {typeof(TestResponse).FullName} (Parameter 'delegate')", actualException.Message);
        }

        [Fact]
        public void Register_TwoOfSameAsyncHandlers_ThrowsArgumentException()
        {
            // Arrange

            ServiceCollection services = new();
            TwoOfSameAsyncObjectTest obj1 = new();
            TwoOfSameAsyncObjectTest obj2 = new();

            Whitestone.Cambion.Cambion sut = new(services.BuildServiceProvider(), _logger.Object);

            // Act

            sut.Register(obj1);

            var actualException = Assert.Throws<ArgumentException>(() => sut.Register(obj2));

            // Assert
            Assert.Equal($"An AsyncSynchronizedHandler already exists for request type {typeof(TestRequest).FullName} and response type {typeof(TestResponse).FullName} (Parameter 'delegate')", actualException.Message);
        }

        #endregion

        #region AddEventHandler()

        [Fact]
        public void AddEventHandler_Success()
        {
            // Arrange

            ServiceCollection services = new();
            Action<TestEvent> handler = _ => { };
            Whitestone.Cambion.Cambion sut = new(services.BuildServiceProvider(), _logger.Object);

            // Act

            sut.AddEventHandler(handler);

            // Assert

            _logger.Verify(
                x => x.Log(
                    It.Is<LogLevel>(y => y == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == $"Added <{handler.Target.GetType().FullName}> as event handler for <{typeof(TestEvent).FullName}>"),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        [Fact]
        public void AddEventHandler_NullInput_ThrowsException()
        {
            // Arrange

            ServiceCollection services = new();
            Whitestone.Cambion.Cambion sut = new(services.BuildServiceProvider(), _logger.Object);

            // Act

            var actualException = Assert.Throws<ArgumentNullException>(() => sut.AddEventHandler<TestEvent>(null));

            // Assert

            Assert.Equal("callback", actualException.ParamName);
        }

        [Fact]
        public void AddEventHandler_StaticInput_ThrowsException()
        {
            // Arrange

            ServiceCollection services = new();
            Whitestone.Cambion.Cambion sut = new(services.BuildServiceProvider(), _logger.Object);

            // Act

            var actualException = Assert.Throws<ArgumentException>(() => sut.AddEventHandler((Action<TestEvent>)EventHandler.HandleEventStatic));

            // Assert

            Assert.Equal("Can't use static methods in callbacks. (Parameter 'callback')", actualException.Message);
            Assert.Equal("callback", actualException.ParamName);
        }

        #endregion

        #region AddAsyncEventHandler()

        [Fact]
        public void AddAsyncEventHandler_Success()
        {
            // Arrange

            ServiceCollection services = new();
            Func<TestEvent, Task> handler = _ => Task.CompletedTask;
            Whitestone.Cambion.Cambion sut = new(services.BuildServiceProvider(), _logger.Object);

            // Act

            sut.AddAsyncEventHandler(handler);

            // Assert

            _logger.Verify(
                x => x.Log(
                    It.Is<LogLevel>(y => y == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == $"Added <{handler.Target.GetType().FullName}> as async event handler for <{typeof(TestEvent).FullName}>"),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        [Fact]
        public void AddAsyncEventHandler_NullInput_ThrowsException()
        {
            // Arrange

            ServiceCollection services = new();
            Whitestone.Cambion.Cambion sut = new(services.BuildServiceProvider(), _logger.Object);

            // Act

            var actualException = Assert.Throws<ArgumentNullException>(() => sut.AddAsyncEventHandler<TestEvent>(null));

            // Assert

            Assert.Equal("callback", actualException.ParamName);
        }

        [Fact]
        public void AddAsyncEventHandler_StaticInput_ThrowsException()
        {
            // Arrange

            ServiceCollection services = new();
            Whitestone.Cambion.Cambion sut = new(services.BuildServiceProvider(), _logger.Object);

            // Act

            var actualException = Assert.Throws<ArgumentException>(() => sut.AddAsyncEventHandler((Func<TestEvent, Task>)AsyncEventHandler.HandleAsyncEventStatic));

            // Assert

            Assert.Equal("Can't use static methods in callbacks. (Parameter 'callback')", actualException.Message);
            Assert.Equal("callback", actualException.ParamName);
        }

        #endregion

        #region AddSynchronizedHandler()

        [Fact]
        public void AddSynchronizedHandler_Success()
        {
            // Arrange

            ServiceCollection services = new();
            Func<TestRequest, TestResponse> handler = _ => new TestResponse(RandomValue.String());

            Whitestone.Cambion.Cambion sut = new(services.BuildServiceProvider(), _logger.Object);

            // Act

            sut.AddSynchronizedHandler(handler);

            // Assert

            _logger.Verify(
                x => x.Log(
                    It.Is<LogLevel>(y => y == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == $"Added <{handler.Target.GetType().FullName}> as synchronized handler for <{typeof(TestRequest).FullName}, {typeof(TestResponse).FullName}>"),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        [Fact]
        public void AddSynchronizedHandler_NullInput_ThrowsException()
        {
            // Arrange

            ServiceCollection services = new();
            Whitestone.Cambion.Cambion sut = new(services.BuildServiceProvider(), _logger.Object);

            // Act

            var actualException = Assert.Throws<ArgumentNullException>(() => sut.AddSynchronizedHandler<TestRequest, TestResponse>(null));

            // Assert

            Assert.Equal("callback", actualException.ParamName);

        }

        [Fact]
        public void AddSynchronizedHandler_StaticInput_ThrowsException()
        {
            // Arrange

            ServiceCollection services = new();
            Whitestone.Cambion.Cambion sut = new(services.BuildServiceProvider(), _logger.Object);

            // Act

            var actualException = Assert.Throws<ArgumentException>(() => sut.AddSynchronizedHandler((Func<TestRequest, TestResponse>)SynchronizedHandler.HandleSynchronizedStatic));

            // Assert

            Assert.Equal("Can't use static methods in callbacks. (Parameter 'callback')", actualException.Message);
            Assert.Equal("callback", actualException.ParamName);

        }

        [Fact]
        public void AddSynchronizedHandler_AlreadyExists_ThrowsException()
        {
            // Arrange

            // ReSharper disable once ConvertToLocalFunction
            // because these are static, and can't use static methods as synchronized handlers
#pragma warning disable IDE0039 // Use local function
            Func<TestRequest, TestResponse> handler = _ => new TestResponse(RandomValue.String());
#pragma warning restore IDE0039 // Use local function

            ServiceCollection services = new();
            Whitestone.Cambion.Cambion sut = new(services.BuildServiceProvider(), _logger.Object);

            sut.AddSynchronizedHandler(handler);

            // Act

            var actualException = Assert.Throws<ArgumentException>(() => sut.AddSynchronizedHandler(handler));

            // Assert

            Assert.Equal($"A SynchronizedHandler already exists for request type {typeof(TestRequest).FullName} and response type {typeof(TestResponse).FullName} (Parameter 'callback')", actualException.Message);
            Assert.Equal("callback", actualException.ParamName);
        }

        #endregion

        #region AddAsyncSynchronizedHandler()

        [Fact]
        public void AddAsyncSynchronizedHandler_Success()
        {
            // Arrange

            ServiceCollection services = new();
            Func<TestRequest, Task<TestResponse>> handler = _ => Task.FromResult(new TestResponse(RandomValue.String()));

            Whitestone.Cambion.Cambion sut = new(services.BuildServiceProvider(), _logger.Object);

            // Act

            sut.AddAsyncSynchronizedHandler(handler);

            // Assert

            _logger.Verify(
                x => x.Log(
                    It.Is<LogLevel>(y => y == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == $"Added <{handler.Target.GetType().FullName}> as async synchronized handler for <{typeof(TestRequest).FullName}, {typeof(TestResponse).FullName}>"),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        [Fact]
        public void AddAsyncSynchronizedHandler_NullInput_ThrowsException()
        {
            // Arrange

            ServiceCollection services = new();
            Whitestone.Cambion.Cambion sut = new(services.BuildServiceProvider(), _logger.Object);

            // Act

            var actualException = Assert.Throws<ArgumentNullException>(() => sut.AddAsyncSynchronizedHandler<TestRequest, Task<TestResponse>>(null));

            // Assert

            Assert.Equal("callback", actualException.ParamName);

        }

        [Fact]
        public void AddAsyncSynchronizedHandler_StaticInput_ThrowsException()
        {
            // Arrange

            ServiceCollection services = new();
            Whitestone.Cambion.Cambion sut = new(services.BuildServiceProvider(), _logger.Object);

            // Act

            var actualException = Assert.Throws<ArgumentException>(() => sut.AddAsyncSynchronizedHandler((Func<TestRequest, Task<TestResponse>>)AsyncSynchronizedHandler.HandleSynchronizedStaticAsync));

            // Assert

            Assert.Equal("Can't use static methods in callbacks. (Parameter 'callback')", actualException.Message);
            Assert.Equal("callback", actualException.ParamName);

        }

        [Fact]
        public void AddAsyncSynchronizedHandler_AlreadyExists_ThrowsException()
        {
            // Arrange

            // ReSharper disable once ConvertToLocalFunction
            // because these are static, and can't use static methods as synchronized handlers
#pragma warning disable IDE0039 // Use local function
            Func<TestRequest, Task<TestResponse>> handler = _ => Task.FromResult(new TestResponse(RandomValue.String()));
#pragma warning restore IDE0039 // Use local function

            ServiceCollection services = new();
            Whitestone.Cambion.Cambion sut = new(services.BuildServiceProvider(), _logger.Object);

            sut.AddAsyncSynchronizedHandler(handler);

            // Act

            var actualException = Assert.Throws<ArgumentException>(() => sut.AddAsyncSynchronizedHandler(handler));

            // Assert

            Assert.Equal($"An AsyncSynchronizedHandler already exists for request type {typeof(TestRequest).FullName} and response type {typeof(TestResponse).FullName} (Parameter 'callback')", actualException.Message);
            Assert.Equal("callback", actualException.ParamName);
        }

        #endregion

        #region PublishEventAsync()

        [Fact]
        public async Task PublishEventAsync_Success()
        {
            // Arrange

            ServiceCollection services = new();
            services.AddSingleton(_transport.Object);
            services.AddSingleton(_serializer.Object);

            TestEvent expectedEvent = new(RandomValue.String());
            byte[] expectedBytes = RandomValue.Array<byte>();

            MessageWrapper actualMessageWrapper = null;
            _serializer.Setup(x => x.SerializeAsync(It.IsAny<MessageWrapper>()))
                .Callback<MessageWrapper>(wrapper =>
                {
                    actualMessageWrapper = wrapper;
                })
                .ReturnsAsync(expectedBytes);

            Whitestone.Cambion.Cambion sut = new(services.BuildServiceProvider(), _logger.Object);

            // Act

            await sut.PublishEventAsync(expectedEvent);

            // Assert

            _logger.Verify(
                x => x.Log(
                    It.Is<LogLevel>(y => y == LogLevel.Debug),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == $"Publishing event <{expectedEvent.GetType().FullName}> to Transport"),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);

            _transport.Verify(x => x.PublishAsync(It.Is<byte[]>(y => y == expectedBytes)));

            Assert.Equal(expectedEvent, actualMessageWrapper.Data);
            Assert.Equal(MessageType.Event, actualMessageWrapper.MessageType);
        }

        [Fact]
        public async Task PublishEventAsync_Success_TraceLogging()
        {
            // Arrange

            ServiceCollection services = new();
            services.AddSingleton(_transport.Object);
            services.AddSingleton(_serializer.Object); 
            
            TestEvent expectedEvent = new(RandomValue.String());
            byte[] expectedBytes = RandomValue.Array<byte>();

            _serializer.Setup(x => x.SerializeAsync(It.IsAny<MessageWrapper>()))
                .ReturnsAsync(expectedBytes);

            _logger.Setup(x => x.IsEnabled(It.Is<LogLevel>(y => y == LogLevel.Trace)))
                .Returns(true);

            Whitestone.Cambion.Cambion sut = new(services.BuildServiceProvider(), _logger.Object);

            // Act

            await sut.PublishEventAsync(expectedEvent);

            // Assert

            _logger.Verify(
                x => x.Log(
                    It.Is<LogLevel>(y => y == LogLevel.Trace),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == $"Publishing event <{expectedEvent.GetType().FullName}> to Transport with data {Convert.ToBase64String(expectedBytes)}"),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        #endregion

        #region CallSynchronizedHandlerAsync()

        [Fact]
        public async Task CallSynchronizedHandlerAsync_Success()
        {
            // Arrange

            ServiceCollection services = new();
            services.AddSingleton(_transport.Object);
            services.AddSingleton(_serializer.Object);

            TestRequest expectedRequest = new();
            TestResponse expectedResponse = new(RandomValue.String());
            byte[] expectedRequestBytes = RandomValue.Array<byte>();

            Whitestone.Cambion.Cambion sut = new(services.BuildServiceProvider(), _logger.Object);

            MessageWrapper actualWrapper = null;
            _serializer.Setup(x => x.SerializeAsync(It.IsAny<MessageWrapper>()))
                .Callback<MessageWrapper>(wrapper =>
                {
                    actualWrapper = wrapper;
                })
                .ReturnsAsync(expectedRequestBytes);

            _transport.Setup(x => x.PublishAsync(It.IsAny<byte[]>()))
                .Callback<byte[]>(_ =>
                {
                    foreach ((Guid _, SynchronizedDataPackage pkg) in sut._synchronizationPackages)
                    {
                        pkg.Data = expectedResponse;
                        pkg.ResetEvent.Set();
                    }
                });

            // Act

            TestResponse actualResponse = await sut.CallSynchronizedHandlerAsync<TestRequest, TestResponse>(expectedRequest);

            // Assert

            _logger.Verify(
                x => x.Log(
                    It.Is<LogLevel>(y => y == LogLevel.Debug),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == $"Publishing synchronized <{expectedRequest.GetType().FullName}> to Transport"),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);

            _transport.Verify(x => x.PublishAsync(It.Is<byte[]>(y => y == expectedRequestBytes)), Times.Once);

            Assert.Equal(expectedResponse.Value, actualResponse.Value);

            Assert.Equal(expectedRequest, actualWrapper.Data);
            Assert.Equal(MessageType.SynchronizedRequest, actualWrapper.MessageType);
        }

        [Fact]
        public async Task CallSynchronizedHandlerAsync_SuccessTraceLogging()
        {
            // Arrange

            ServiceCollection services = new();
            services.AddSingleton(_transport.Object);
            services.AddSingleton(_serializer.Object);

            TestRequest expectedRequest = new();
            TestResponse expectedResponse = new(RandomValue.String());
            byte[] expectedRequestBytes = RandomValue.Array<byte>();

            Whitestone.Cambion.Cambion sut = new(services.BuildServiceProvider(), _logger.Object);

            MessageWrapper actualWrapper = null;
            _serializer.Setup(x => x.SerializeAsync(It.IsAny<MessageWrapper>()))
                .Callback<MessageWrapper>(wrapper =>
                {
                    actualWrapper = wrapper;
                })
                .ReturnsAsync(expectedRequestBytes);

            _transport.Setup(x => x.PublishAsync(It.IsAny<byte[]>()))
                .Callback<byte[]>(_ =>
                {
                    foreach ((Guid _, SynchronizedDataPackage pkg) in sut._synchronizationPackages)
                    {
                        pkg.Data = expectedResponse;
                        pkg.ResetEvent.Set();
                    }
                });

            _logger.Setup(x => x.IsEnabled(It.Is<LogLevel>(y => y == LogLevel.Trace)))
                .Returns(true);

            // Act

            TestResponse actualResponse = await sut.CallSynchronizedHandlerAsync<TestRequest, TestResponse>(expectedRequest);

            // Assert

            _logger.Verify(
                x => x.Log(
                    It.Is<LogLevel>(y => y == LogLevel.Trace),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == $"Publishing synchronized <{expectedRequest.GetType().FullName}> to Transport with data {Convert.ToBase64String(expectedRequestBytes)}"),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);

            _transport.Verify(x => x.PublishAsync(It.Is<byte[]>(y => y == expectedRequestBytes)), Times.Once);

            Assert.Equal(expectedResponse.Value, actualResponse.Value);

            Assert.Equal(expectedRequest, actualWrapper.Data);
            Assert.Equal(MessageType.SynchronizedRequest, actualWrapper.MessageType);
        }

        [Fact]
        public async Task CallSynchronizedHandlerAsync_NoHandler_ThrowsException()
        {
            // Arrange

            ServiceCollection services = new();
            services.AddSingleton(_transport.Object);
            services.AddSingleton(_serializer.Object);

            TestRequest expectedRequest = new();
            byte[] expectedBytes = RandomValue.Array<byte>();

            _serializer.Setup(x => x.SerializeAsync(It.IsAny<MessageWrapper>()))
                .ReturnsAsync(expectedBytes);

            Whitestone.Cambion.Cambion sut = new(services.BuildServiceProvider(), _logger.Object);

            // Act

            TimeoutException actualException = await Assert.ThrowsAsync<TimeoutException>(async () => await sut.CallSynchronizedHandlerAsync<TestRequest, TestResponse>(expectedRequest, 500));

            // Assert

            Assert.Equal("Timeout waiting for synchronous call", actualException.Message);
        }

        #endregion

        #region Transport_MessageReceived()

        [Fact]
        public async Task Transport_MessageReceived_Event_Success()
        {
            // Arrange

            ServiceCollection services = new();
            services.AddSingleton(_transport.Object);
            services.AddSingleton(_serializer.Object);

            byte[] expectedWrapperBytes = RandomValue.Array<byte>();
            string expectedValue = RandomValue.String();
            MessageWrapper expectedWrapper = new()
            {
                MessageType = MessageType.Event,
                DataType = typeof(TestEvent),
                Data = new TestEvent(expectedValue)
            };

            _serializer.Setup(x => x.DeserializeAsync(It.Is<byte[]>(y => y == expectedWrapperBytes)))
                .ReturnsAsync(expectedWrapper);

            Whitestone.Cambion.Cambion sut = new(services.BuildServiceProvider(), _logger.Object);

            string actualData = null;
            sut.AddEventHandler<TestEvent>(e => { actualData = e.Value; });

            // Act

            sut.Transport_MessageReceived(this, new MessageReceivedEventArgs(expectedWrapperBytes));

            // Assert

            await Task.Delay(1000); // Wait for event to actually be fired

            Assert.NotNull(actualData);
            Assert.Equal(expectedValue, actualData);
        }

        [Fact]
        public async Task Transport_MessageReceived_EventThrowsException_ExceptionEventFired()
        {
            // Arrange

            ServiceCollection services = new();
            services.AddSingleton(_transport.Object);
            services.AddSingleton(_serializer.Object);

            byte[] expectedWrapperBytes = RandomValue.Array<byte>();
            string expectedValue = RandomValue.String();
            Exception expectedException = new(RandomValue.String());
            MessageWrapper expectedWrapper = new()
            {
                MessageType = MessageType.Event,
                DataType = typeof(TestEvent),
                Data = new TestEvent(expectedValue)
            };

            _serializer.Setup(x => x.DeserializeAsync(It.Is<byte[]>(y => y == expectedWrapperBytes)))
                .ReturnsAsync(expectedWrapper);

            Whitestone.Cambion.Cambion sut = new(services.BuildServiceProvider(), _logger.Object);

            Exception actualException = null;
            sut.UnhandledException += (_, eventArgs) =>
            {
                actualException = eventArgs.GetException();
            };

            sut.AddEventHandler<TestEvent>(_ => throw expectedException);

            // Act

            sut.Transport_MessageReceived(this, new MessageReceivedEventArgs(expectedWrapperBytes));

            // Assert

            await Task.Delay(1000); // Wait for event to actually be fired

            Assert.NotNull(actualException);
            Assert.Equal(expectedException.Message, actualException.InnerException.Message);
        }

        [Fact]
        public async Task Transport_MessageReceived_SynchronizedRequest_Success()
        {
            // Arrange

            ServiceCollection services = new();
            services.AddSingleton(_transport.Object);
            services.AddSingleton(_serializer.Object);

            byte[] expectedRequestWrapperBytes = RandomValue.Array<byte>();
            byte[] expectedResponseWrapperBytes = RandomValue.Array<byte>();
            string expectedValue = RandomValue.String();
            TestRequest expectedRequest = new();
            MessageWrapper expectedRequestWrapper = new()
            {
                MessageType = MessageType.SynchronizedRequest,
                DataType = typeof(TestRequest),
                ResponseType = typeof(TestResponse),
                Data = expectedRequest,
                CorrelationId = Guid.NewGuid()
            };

            _serializer.Setup(x => x.DeserializeAsync(It.Is<byte[]>(y => y == expectedRequestWrapperBytes)))
                .ReturnsAsync(expectedRequestWrapper);

            MessageWrapper actualResponseWrapper = null;
            _serializer.Setup(x => x.SerializeAsync(It.IsAny<MessageWrapper>()))
                .Callback<MessageWrapper>(w =>
                {
                    actualResponseWrapper = w;
                })
                .ReturnsAsync(expectedResponseWrapperBytes);

            Whitestone.Cambion.Cambion sut = new(services.BuildServiceProvider(), _logger.Object);

            TestRequest actualRequest = null;
            sut.AddSynchronizedHandler<TestRequest, TestResponse>(request =>
            {
                actualRequest = request;
                return new TestResponse(expectedValue);
            });

            // Act

            sut.Transport_MessageReceived(this, new MessageReceivedEventArgs(expectedRequestWrapperBytes));

            // Assert

            await Task.Delay(1000); // Wait for synchronized to actually be fired

            Assert.NotNull(actualRequest);
            Assert.Equal(expectedRequest, actualRequest);

            Assert.NotNull(actualResponseWrapper);
            Assert.Equal(MessageType.SynchronizedResponse, actualResponseWrapper.MessageType);
            Assert.Equal(expectedRequestWrapper.CorrelationId, actualResponseWrapper.CorrelationId);
            Assert.Equal(expectedRequestWrapper.DataType, actualResponseWrapper.DataType);
            Assert.Equal(expectedRequestWrapper.ResponseType, actualResponseWrapper.ResponseType);

            Assert.IsType<TestResponse>(actualResponseWrapper.Data);
            var t = actualResponseWrapper.Data as TestResponse;
            Assert.Equal(expectedValue, t.Value);

            _transport.Verify(x => x.PublishAsync(It.Is<byte[]>(y => y == expectedResponseWrapperBytes)), Times.Once);

            _logger.Verify(
                x => x.Log(
                    It.Is<LogLevel>(y => y == LogLevel.Debug),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, type) => v.ToString() == $"Publishing synchronized reply <{typeof(TestResponse).FullName}> to Transport"),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, type) => true)),
                Times.Once);
        }

        [Fact]
        public async Task Transport_MessageReceived_SynchronizedRequest_Success_TraceLogging()
        {
            // Arrange

            ServiceCollection services = new();
            services.AddSingleton(_transport.Object);
            services.AddSingleton(_serializer.Object);

            byte[] expectedRequestWrapperBytes = RandomValue.Array<byte>();
            byte[] expectedResponseWrapperBytes = RandomValue.Array<byte>();
            string expectedValue = RandomValue.String();
            TestRequest expectedRequest = new();
            MessageWrapper expectedRequestWrapper = new()
            {
                MessageType = MessageType.SynchronizedRequest,
                DataType = typeof(TestRequest),
                ResponseType = typeof(TestResponse),
                Data = expectedRequest,
                CorrelationId = Guid.NewGuid()
            };

            _logger.Setup(x => x.IsEnabled(It.Is<LogLevel>(y => y == LogLevel.Trace)))
                .Returns(true);

            _serializer.Setup(x => x.DeserializeAsync(It.Is<byte[]>(y => y == expectedRequestWrapperBytes)))
                .ReturnsAsync(expectedRequestWrapper);

            MessageWrapper actualResponseWrapper = null;
            _serializer.Setup(x => x.SerializeAsync(It.IsAny<MessageWrapper>()))
                .Callback<MessageWrapper>(w =>
                {
                    actualResponseWrapper = w;
                })
                .ReturnsAsync(expectedResponseWrapperBytes);

            Whitestone.Cambion.Cambion sut = new(services.BuildServiceProvider(), _logger.Object);

            TestRequest actualRequest = null;
            sut.AddSynchronizedHandler<TestRequest, TestResponse>(request =>
            {
                actualRequest = request;
                return new TestResponse(expectedValue);
            });

            // Act

            sut.Transport_MessageReceived(this, new MessageReceivedEventArgs(expectedRequestWrapperBytes));

            // Assert

            await Task.Delay(1000); // Wait for synchronized to actually be fired

            Assert.NotNull(actualRequest);
            Assert.Equal(expectedRequest, actualRequest);

            Assert.NotNull(actualResponseWrapper);
            Assert.Equal(MessageType.SynchronizedResponse, actualResponseWrapper.MessageType);
            Assert.Equal(expectedRequestWrapper.CorrelationId, actualResponseWrapper.CorrelationId);
            Assert.Equal(expectedRequestWrapper.DataType, actualResponseWrapper.DataType);
            Assert.Equal(expectedRequestWrapper.ResponseType, actualResponseWrapper.ResponseType);

            Assert.IsType<TestResponse>(actualResponseWrapper.Data);
            var t = actualResponseWrapper.Data as TestResponse;
            Assert.Equal(expectedValue, t.Value);

            _transport.Verify(x => x.PublishAsync(It.Is<byte[]>(y => y == expectedResponseWrapperBytes)), Times.Once);

            _logger.Verify(
                x => x.Log(
                    It.Is<LogLevel>(y => y == LogLevel.Trace),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, type) => v.ToString() == $"Received message from Transport with data {Convert.ToBase64String(expectedRequestWrapperBytes)}"),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, type) => true)),
                Times.Once);

            _logger.Verify(
                x => x.Log(
                    It.Is<LogLevel>(y => y == LogLevel.Trace),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, type) => v.ToString() == $"Publishing synchronized reply <{typeof(TestResponse).FullName}> to Transport with data {Convert.ToBase64String(expectedResponseWrapperBytes)}"),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, type) => true)),
                Times.Once);
        }

        [Fact]
        public async Task Transport_MessageReceived_SynchronizedRequestThrowsException_ExceptionEventFired()
        {
            // Arrange

            ServiceCollection services = new();
            services.AddSingleton(_transport.Object);
            services.AddSingleton(_serializer.Object);

            byte[] expectedRequestWrapperBytes = RandomValue.Array<byte>();
            TestRequest expectedRequest = new();
            Exception expectedException = new(RandomValue.String());
            MessageWrapper expectedRequestWrapper = new()
            {
                MessageType = MessageType.SynchronizedRequest,
                DataType = typeof(TestRequest),
                ResponseType = typeof(TestResponse),
                Data = expectedRequest,
                CorrelationId = Guid.NewGuid()
            };

            _serializer.Setup(x => x.DeserializeAsync(It.Is<byte[]>(y => y == expectedRequestWrapperBytes)))
                .ReturnsAsync(expectedRequestWrapper);

            Whitestone.Cambion.Cambion sut = new(services.BuildServiceProvider(), _logger.Object);

            sut.AddSynchronizedHandler<TestRequest, TestResponse>(_ => throw expectedException);

            Exception actualException = null;
            sut.UnhandledException += (_, eventArgs) =>
            {
                actualException = eventArgs.GetException();
            };

            // Act

            sut.Transport_MessageReceived(this, new MessageReceivedEventArgs(expectedRequestWrapperBytes));

            // Assert

            await Task.Delay(1000); // Wait for synchronized to actually be fired

            Assert.NotNull(actualException);
            Assert.Equal(expectedException.Message, actualException.InnerException.Message);
        }

        [Fact]
        public void Transport_MessageReceived_SynchronizedResponse_Success()
        {
            // Arrange

            ServiceCollection services = new();
            services.AddSingleton(_transport.Object);
            services.AddSingleton(_serializer.Object);

            byte[] expectedResponseWrapperBytes = RandomValue.Array<byte>();
            string expectedData = RandomValue.String();
            var correlationId = Guid.NewGuid();
            ManualResetEvent expectedResetEvent = new(false);
            SynchronizedDataPackage expectedSyncPackage = new(expectedResetEvent);

            MessageWrapper expectedResponseWrapper = new()
            {
                MessageType = MessageType.SynchronizedResponse,
                DataType = typeof(TestRequest),
                ResponseType = typeof(TestResponse),
                Data = expectedData,
                CorrelationId = correlationId
            };

            _serializer.Setup(x => x.DeserializeAsync(It.Is<byte[]>(y => y == expectedResponseWrapperBytes)))
                .ReturnsAsync(expectedResponseWrapper);

            Whitestone.Cambion.Cambion sut = new(services.BuildServiceProvider(), _logger.Object);

            sut._synchronizationPackages.Add(correlationId, expectedSyncPackage);

            // Act

            sut.Transport_MessageReceived(this, new MessageReceivedEventArgs(expectedResponseWrapperBytes));

            // Assert

            if (expectedResetEvent.WaitOne(1000))
            {
                Assert.Equal(expectedData, expectedSyncPackage.Data);
            }
            else
            {
                throw new Xunit.Sdk.XunitException("Reset event never fired");
            }
        }



        [Fact]
        public async Task Transport_MessageReceived_UnknownException_FiresExceptionEvent()
        {
            // Arrange

            ServiceCollection services = new();
            services.AddSingleton(_transport.Object);
            services.AddSingleton(_serializer.Object);

            Exception expectedException = new(RandomValue.String());
            byte[] expectedWrapperBytes = RandomValue.Array<byte>();

            _serializer.Setup(x => x.DeserializeAsync(It.IsAny<byte[]>()))
                .ThrowsAsync(expectedException);

            Whitestone.Cambion.Cambion sut = new(services.BuildServiceProvider(), _logger.Object);

            Exception actualException = null;
            sut.UnhandledException += (_, eventArgs) =>
            {
                actualException = eventArgs.GetException();
            };

            // Act

            sut.Transport_MessageReceived(this, new MessageReceivedEventArgs(expectedWrapperBytes));

            // Assert

            await Task.Delay(1000);

            Assert.NotNull(actualException);
            Assert.Equal(expectedException.Message, actualException.Message);
        }
        #endregion
    }

    internal class TestEvent(string value)
    {
        public string Value { get; } = value;
    }

    internal class TestRequest;

    internal class TestResponse(string value)
    {
        public string Value { get; } = value;
    }

    internal class TwoOfSameObjectTest : ISynchronizedHandler<TestRequest, TestResponse>
    {
        public TestResponse HandleSynchronized(TestRequest input)
        {
            return new TestResponse(null);
        }
    }

    internal class TwoOfSameAsyncObjectTest : IAsyncSynchronizedHandler<TestRequest, TestResponse>
    {
        public Task<TestResponse> HandleSynchronizedAsync(TestRequest input)
        {
            return Task.FromResult(new TestResponse(null));
        }
    }

    internal class SynchronizedHandler(string value) : ISynchronizedHandler<TestRequest, TestResponse>
    {
        public TestResponse HandleSynchronized(TestRequest input)
        {
            return new TestResponse(value);
        }

#pragma warning disable IDE0060 // Remove unused parameter
        public static TestResponse HandleSynchronizedStatic(TestRequest input)
        {
            return null;
        }
#pragma warning restore IDE0060 // Remove unused parameter
    }

    internal class AsyncSynchronizedHandler(string value) : IAsyncSynchronizedHandler<TestRequest, TestResponse>
    {
        public async Task<TestResponse> HandleSynchronizedAsync(TestRequest input)
        {
            await Task.Delay(1);
            return new TestResponse(value);
        }

#pragma warning disable IDE0060 // Remove unused parameter
        public static async Task<TestResponse> HandleSynchronizedStaticAsync(TestRequest input)
        {
            await Task.Delay(1);
            return null;
        }
#pragma warning restore IDE0060 // Remove unused parameter
    }

    internal class EventHandler : IEventHandler<TestEvent>
    {
        private readonly ManualResetEvent _mre = new(false);

        public void HandleEvent(TestEvent input)
        {
            _mre.Set();
        }

#pragma warning disable IDE0060 // Remove unused parameter
        public static void HandleEventStatic(TestEvent input) { }
#pragma warning restore IDE0060 // Remove unused parameter
    }

    internal class AsyncEventHandler : IAsyncEventHandler<TestEvent>
    {
        private readonly ManualResetEvent _mre = new(false);

        public async Task HandleEventAsync(TestEvent input)
        {
            await Task.Delay(1);
            _mre.Set();
        }

#pragma warning disable IDE0060 // Remove unused parameter
        public static Task HandleAsyncEventStatic(TestEvent input)
        {
            return Task.CompletedTask;
        }
#pragma warning restore IDE0060 // Remove unused parameter
    }
}
