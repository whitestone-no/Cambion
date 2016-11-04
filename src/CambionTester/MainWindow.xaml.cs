using System;
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
    public partial class MainWindow : Window, IEventHandler<TestMessageSimple>
    {
        private MessageHandler _messageHandler;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnStartMessageHost_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _messageHandler = new MessageHandler();
                _messageHandler.Initialize(init =>
                {
                    init.UseNetMq(
                        "tcp://localhost:9990",
                        "tcp://localhost:9991",
                        netmq =>
                        {
                            netmq.StartMessageHost();
                        });
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
            _messageHandler.Publish(new TestMessageSimple { CurrentDateTime = DateTime.Now});
        }

        private void btnConnectMessageHost_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _messageHandler = new MessageHandler();
                _messageHandler.Initialize(init =>
                {
                    init.UseNetMq("tcp://localhost:9990", "tcp://localhost:9991");
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
            });
        }

        private void btnInitLoopback_Click(object sender, RoutedEventArgs e)
        {
            _messageHandler = new MessageHandler();
            _messageHandler.Initialize(init =>
            {
                init.UseLoopback();
            });
        }

        public void Handle(TestMessageSimple input)
        {
            ;
        }

        private void btnRegister_Click(object sender, RoutedEventArgs e)
        {
            _messageHandler.Register(this);
        }
    }
}
