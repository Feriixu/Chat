using HD;
using System;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;

namespace SuperChat
{
    public partial class Dashboard : Form
    {
        public static Dashboard instance;

        public Dashboard()
        {
            InitializeComponent();
            var localIPs = Dns.GetHostAddresses(Dns.GetHostName());
            foreach (var address in localIPs)
            {
                if (address.AddressFamily == AddressFamily.InterNetwork)
                {
                    textBoxIp.Text = address.ToString();
                }
            }
            textBoxPort.Text = Globals.Port.ToString();
            textBoxNickname.Text = Environment.UserName;
            instance = this;
        }

        private void buttonServerStart_Click(object sender, EventArgs e)
        {
            buttonClientConnect.Enabled = false;
            buttonServerStart.Enabled = false;
            var tcpChat = new TCPChat() { isServer = true };
            tcpChat.Awake();
        }

        private void buttonClientConnect_Click(object sender, EventArgs e)
        {
            buttonClientConnect.Enabled = false;
            buttonServerStart.Enabled = false;
            var tcpChat = new TCPChat() { isServer = false, serverIp = IPAddress.Parse(textBoxIp.Text) };
            tcpChat.Awake();
        }

        private void buttonSend_Click(object sender, EventArgs e)
        {
            TCPChat.instance.Send(textBoxMessage.Text);
            textBoxMessage.Clear();
        }

        private void SuperChat_FormClosing(object sender, FormClosingEventArgs e) => TCPChat.instance?.OnApplicationQuit();

        delegate void AppendStreamDelegate(string message);
        public void AppendStream(string message)
        {
            if (textBoxStream.InvokeRequired)
            {
                AppendStreamDelegate d = new AppendStreamDelegate(AppendStream);
                this.Invoke(d, new object[] { message });
            }
            else
            {
                textBoxStream.AppendText(message + Environment.NewLine);
            }
        }
    }
}
