using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ReqV1.Client.Desktop
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string _connectionString = "Endpoint=sb://mirzaevolution21.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=LrtPyL+CWb/1FFtAVgZBpgySsdaEnSQCGjaCaScnphI=";

        private string _queueClientName = "reqresv1_client_queue";
        private string _queueServerName = "reqresv1_server_queue";

        private ManagementClient _managementClient;
        private QueueClient _queueClient;

        private void InitializeServiceBus()
        {
            _managementClient = new ManagementClient(_connectionString);
            if (!_managementClient.QueueExistsAsync(_queueClientName).Result)
            {
                _managementClient.CreateQueueAsync(new QueueDescription(_queueClientName)
                {
                    AutoDeleteOnIdle = TimeSpan.FromHours(1)
                }).GetAwaiter().GetResult();
            }
            if (!_managementClient.QueueExistsAsync(_queueServerName).Result)
            {
                _managementClient.CreateQueueAsync(new QueueDescription(_queueServerName)
                {
                    AutoDeleteOnIdle = TimeSpan.FromHours(1),
                    RequiresSession = true
                }).GetAwaiter().GetResult();
            }
            _queueClient = new QueueClient(_connectionString, _queueClientName);

        }

        public MainWindow()
        {
            InitializeComponent();
            InitializeServiceBus();
        }

        private async void SendButtonHandler(object sender, RoutedEventArgs e)
        {
            string message = MessageTextBox.Text.Trim();
            if (!string.IsNullOrEmpty(message))
            {
                string newSessionId = Guid.NewGuid().ToString();
                await SendMessage(message, newSessionId);
                MessageTextBox.Text = string.Empty;
                await ReceiveResponse(newSessionId);
            }
        }
        private async Task SendMessage(string message, string sessionId)
        {
            try
            {
                byte[] messageBytes = Encoding.UTF8.GetBytes(message);
                Message messagePayload = new Message(messageBytes)
                {
                    MessageId = Guid.NewGuid().ToString(),
                    ReplyToSessionId = sessionId
                };
                await _queueClient.SendAsync(messagePayload);
                ReplyListBox.Items.Add(ConstructMessageListBox(sessionId, "Request sent to the server."));

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private async Task ReceiveResponse(string sessionId)
        {
            try
            {
                SessionClient sessionClient = new SessionClient(_connectionString, _queueServerName);
                IMessageSession messageSession = await sessionClient.AcceptMessageSessionAsync(sessionId);
                Message messagePayload = await messageSession.ReceiveAsync();
                if (messagePayload != null)
                {
                    string message = Encoding.UTF8.GetString(messagePayload.Body);
                    ReplyListBox.Items.Add(ConstructMessageListBox(sessionId, message, false));
                }
                else
                {
                    ReplyListBox.Items.Add(ConstructMessageListBox(sessionId, "Failed getting response", false));
                }
                await messageSession.CloseAsync();
                await sessionClient.CloseAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);

            }
        }
        private string ConstructMessageListBox(string sessionId, string message, bool sent = true)
        {
            return $"{(sent ? ">>" : "<<")} [{sessionId}] {message}";
        }

        private void MessageTextBoxKeyDownHandler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SendButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            }
        }
    }
}
