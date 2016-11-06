using System;
using System.Globalization;
using System.Text;
using System.Windows;
using Whitestone.Cambion;
using Whitestone.Cambion.Backend.Loopback;
using Whitestone.Cambion.Backend.NetMQ;
using Whitestone.Cambion.Interfaces;

namespace Whitestone.CambionTester
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : IEventHandler<TestMessageSimple>
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

        private void btnConnectMessageHost_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _messageHandler = new CambionMessageHandler();
                _messageHandler.Initialize(init => { init.UseNetMq("tcp://localhost:9990", "tcp://localhost:9991"); });
            }
            catch (Exception ex)
            {
                MessageBox.Show(PrintException(ex), "Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnReinitialize_Click(object sender, RoutedEventArgs e)
        {
            _messageHandler.Reinitialize(init => { init.UseNetMq("tcp://localhost:9990", "tcp://localhost:9991"); });
        }

        private void btnInitLoopback_Click(object sender, RoutedEventArgs e)
        {
            _messageHandler = new CambionMessageHandler();
            _messageHandler.Initialize(init => { init.UseLoopback(); });
        }

        public void Handle(TestMessageSimple input)
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
        }

        private TempObject _tempObject;

        private void btnCreateTempObject_Click(object sender, RoutedEventArgs e)
        {
            _tempObject = new TempObject();
            _messageHandler.Register(_tempObject);
        }

        private void btnNullTempObject_Click(object sender, RoutedEventArgs e)
        {
            _tempObject = null;
            GC.Collect();
        }

        private class TempObject : IEventHandler<TestMessageSimple>
        {
            public void Handle(TestMessageSimple input)
            {
                ;
            }
        }
    }
}