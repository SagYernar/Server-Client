using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TcpServer
{
    public class ClientObj
    {
        protected internal string Id { get; private set; }
        protected internal NetworkStream Stream { get; private set; }
        public string userName;
        TcpClient client;
        ServerObj server; 

        public ClientObj(TcpClient tcpClient, ServerObj serverObject)
        {
            Id = Guid.NewGuid().ToString();
            client = tcpClient;
            server = serverObject;
            serverObject.AddConnection(this);
        }

        public void Process()
        {
            try
            {
                bool isAutoriz = false;
                string message;
                while (!isAutoriz)
                {
                    Stream = client.GetStream();
                    // получаем login|password пользователя
                    message = GetMessage();
                    isAutoriz = CheckUser(message);
                    if (!isAutoriz)
                    {
                        Stream.Write(Encoding.Unicode.GetBytes("N"), 0, 2);
                    }
                }
                Stream.Write(Encoding.Unicode.GetBytes("Y"),0,2);
                message = userName + " вошел в чат";

                server.BroadcastMessage(message, this.Id);
                //отправляем пользователю список пользователей уже находящихся в чате
                //foreach (var cl in server.clients)
                //{
                //    if (cl.userName == this.userName)
                //    {
                //        Stream = cl.Stream;
                //    }
                //}
                //foreach (var cl in server.clients)
                //{
                //    if (cl.userName != this.userName) {
                //        message = cl.userName + " в чате\n";
                //        Stream.Write(Encoding.Unicode.GetBytes(message), 0, message.Length);
                //    }
                //}

                Console.WriteLine(message);

                while (true)
                {
                    try
                    {
                        message = GetMessage();
                        message = String.Format("{0}: {1}", userName, message);
                        Console.WriteLine(message);
                        server.BroadcastMessage(message, this.Id);
                    }
                    catch
                    {
                        message = String.Format("{0}: покинул чат", userName);
                        Console.WriteLine(message);
                        server.BroadcastMessage(message, this.Id);
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {

                server.RemoveConnection(this.Id);
                Close();
            }
        }

        private bool CheckUser(string lp)
        {
            foreach (string str in server.users)
            {
                if (str == lp)
                {
                    foreach(var cl in server.clients)
                    {
                        if(str == cl.userName)
                        {
                            return false;
                        }
                    }
                    int i;
                    for (i = 0; str[i] != '|'; i++){}
                    userName = str.Remove(i);
                    return true;
                }
            }
            return false;
        }


        private string GetMessage()
        {
            byte[] data = new byte[64]; 
            StringBuilder builder = new StringBuilder();
            int bytes = 0;
            do
            {
                bytes = Stream.Read(data, 0, data.Length);
                builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
            }
            while (Stream.DataAvailable);

            return builder.ToString();
        }
        
        public void Close()
        {
            if (Stream != null)
                Stream.Close();
            if (client != null)
                client.Close();
        }
    }
}
