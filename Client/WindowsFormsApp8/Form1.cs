using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp8
{
    public partial class Form1 : Form
    {
        static string userName;
        private const string host = "127.0.0.1";
        private const int port = 8888;
        static TcpClient client = new TcpClient();
        static NetworkStream stream;
        bool conected = false;
        List<string> users = new List<string>();
        public Form1()
        {
            InitializeComponent();
            sendButton.Enabled = false;
        }

        private void ConnectButton_Click(object sender, EventArgs e)
        {
            if (!conected)
            {
                client.Connect(host, port);
                conected = true;
            }
            if (!(String.IsNullOrWhiteSpace(loginTextBox.Text) && String.IsNullOrWhiteSpace(passwordTextBox.Text)))
            {
                stream = client.GetStream();
                string autorizMes = loginTextBox.Text + '|' + passwordTextBox.Text;
                byte[] data = Encoding.Unicode.GetBytes(autorizMes);
                stream.Write(data, 0, data.Length);

                stream.Read(data, 0, data.Length);
                string res = Encoding.Unicode.GetString(data, 0, 2);
                if (res == "Y")
                {
                    connectButton.Enabled = false;
                    sendButton.Enabled = true;
                    userName = loginTextBox.Text;
                }
                else
                {
                    return;
                }
            }
            
            Thread receiveThread = new Thread(new ThreadStart(ReceiveMessage));
            receiveThread.Start(); 
            richTextBox.Text += "Добро пожаловать, " + userName;
           
            users.Add(userName);
            userList.Clear();
            foreach (var str in users)
            {
                userList.Text += str + '\n';
            }
            //SendMessage();
        }

        
        void SendMessage()
        {
            if (!string.IsNullOrWhiteSpace(messageTextBox.Text))
            {
                string message = messageTextBox.Text;
                richTextBox.Text += '\n' + userName + ": " + message;
                byte[] data = Encoding.Unicode.GetBytes(message);
                stream.Write(data, 0, data.Length);
            }

        }

        void ReceiveMessage()
        {
            while (true)
            {
                try
                {
                    byte[] data = new byte[64];
                    StringBuilder builder = new StringBuilder();
                    int bytes = 0;
                    do
                    {
                        bytes = stream.Read(data, 0, data.Length);
                        builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                    }
                    while (stream.DataAvailable);

                    string message = builder.ToString();
                    lock (richTextBox)
                    {
                        richTextBox.Text += '\n' + message;

                        if(message.Contains("вошел в чат"))
                        {
                            string[] strs = message.Split(' ');
                            users.Add(strs[0]);
                            userList.Clear();
                            foreach(var str in users)
                            {
                                userList.Text += str + '\n';
                            }
                        }
                        else if(message.Contains("покинул чат"))
                        {
                            string[] strs = message.Split(' ');
                            users.Add(strs[0]);
                            userList.Clear();
                            foreach (var str in users)
                            {
                                userList.Text += str + '\n';
                            }
                        }
                        else if (message.Contains("в чате"))
                        {
                            string[] strs = message.Split(' ');
                            users.Add(strs[0]);
                            userList.Clear();
                            foreach (var str in users)
                            {
                                userList.Text += str + '\n';
                            }
                        }

                    }
                    
                }
                catch
                {
                    Disconnect();
                }
            }
        }
        static void Disconnect()
        {
            if (stream != null)
                stream.Close();
            if (client != null)
                client.Close();
            Environment.Exit(0);
        }

        private void SendButton_Click(object sender, EventArgs e)
        {
            SendMessage();
        }
    }
}
