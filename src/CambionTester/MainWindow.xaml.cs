using System;
using System.Globalization;
using System.Text;
using System.Windows;
using Whitestone.Cambion;
using Whitestone.Cambion.Backend.Loopback;
using Whitestone.Cambion.Backend.NetMQ;
using Whitestone.Cambion.Interfaces;
using Whitestone.Cambion.Serializers.JsonNet;

namespace Whitestone.CambionTester
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : IEventHandler<TestMessageSimple>, ISynchronizedHandler<TestMessageRequest, TestMessageResponse>
    {
        private ICambion _messageHandler;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnStartMessageHost_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _messageHandler = new CambionMessageHandler();
                _messageHandler.Initialize(init =>
                {
                    init.UseNetMq(
                        "tcp://localhost:9990",
                        "tcp://localhost:9991",
                        netmq => { netmq.StartMessageHost(); });
                    init.UseJsonNet();
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(PrintException(ex), "Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string PrintException(Exception ex)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("***** ");
            sb.Append(ex.Message);
            sb.AppendLine(" *****");
            sb.AppendLine();
            sb.AppendLine(ex.StackTrace);

            if (ex.InnerException != null)
            {
                sb.AppendLine();
                sb.Append("***** ");
                sb.Append(ex.InnerException.Message);
                sb.AppendLine(" *****");
                sb.AppendLine();
                sb.AppendLine(ex.InnerException.StackTrace);
            }

            return sb.ToString();
        }

        private void btnPublish_Click(object sender, RoutedEventArgs e)
        {
            TestMessageSimple msg = new TestMessageSimple
            {
                CurrentDateTime = DateTime.ParseExact("1977-03-18", "yyyy-MM-dd", CultureInfo.InvariantCulture)
            };

            _messageHandler.PublishEvent(msg);
        }

        private void btnCall_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                TestMessageResponse response = _messageHandler.CallSynchronizedHandler<TestMessageRequest, TestMessageResponse>(new TestMessageRequest { Id = 47 }, 1000);
            }
            catch (Exception)
            {
            }
        }

        private void btnConnectMessageHost_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _messageHandler = new CambionMessageHandler();
                _messageHandler.Initialize(init =>
                {
                    init.UseNetMq("tcp://localhost:9990", "tcp://localhost:9991");
                    init.UseJsonNet();
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(PrintException(ex), "Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnReinitialize_Click(object sender, RoutedEventArgs e)
        {
            _messageHandler.Reinitialize(init =>
            {
                init.UseNetMq("tcp://localhost:9990", "tcp://localhost:9991");
                init.UseJsonNet();
            });
        }

        private void btnInitLoopback_Click(object sender, RoutedEventArgs e)
        {
            _messageHandler = new CambionMessageHandler();
            _messageHandler.Initialize(init =>
            {
                init.UseLoopback();
                init.UseJsonNet();
            });
        }

        public void HandleEvent(TestMessageSimple input)
        {
            ;
        }

        private void btnRegister_Click(object sender, RoutedEventArgs e)
        {
            _messageHandler.Register(this);

            _messageHandler.AddEventHandler<TestMessageSimple>(ev =>
            {
                ;
            });

            _messageHandler.AddSynchronizedHandler<TestMessageSimple, TestMessageSimple>(sync =>
            {
                return new TestMessageSimple { CurrentDateTime = DateTime.Now.AddDays(7) };
            });
        }

        private TempObject _tempObject;

        private void btnCreateTempObject_Click(object sender, RoutedEventArgs e)
        {
            _tempObject = new TempObject(_messageHandler);
            _messageHandler.Register(_tempObject);
        }

        private void btnNullTempObject_Click(object sender, RoutedEventArgs e)
        {
            _tempObject = null;
            GC.Collect();
        }

        public TestMessageResponse HandleSynchronized(TestMessageRequest input)
        {
            return new TestMessageResponse { Value = "Yay, response!! :D" };
        }

        private class TempObject : IEventHandler<TestMessageSimple>
        {
#pragma warning disable 414
            private int _one = 1;
#pragma warning restore 414
            public TempObject(ICambion messageHandler)
            {
                messageHandler.AddEventHandler<TestMessageSimple>(ev =>
                {
                    _one = 2;
                    ;
                });
            }

            public void HandleEvent(TestMessageSimple input)
            {
                ;
            }
        }
    }
}
