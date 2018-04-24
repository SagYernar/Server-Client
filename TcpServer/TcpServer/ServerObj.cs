using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace TcpServer
{
    public class ServerObj
    {
        static TcpListener tcpListener; 
        public List<ClientObj> clients = new List<ClientObj>(); 
        public List<string> users = new List<string>();

        public ServerObj()
        {
            users.Add("Yernar|5555");
            users.Add("Lol|5555");
            users.Add("KOKOKO|5555");
            users.Add("BitchInAHole|666");
        }

        public void AddConnection(ClientObj clientObject)
        {
            clients.Add(clientObject);
        }
        public void RemoveConnection(string id)
        {
            ClientObj client = clients.FirstOrDefault(c => c.Id == id);

            if (client != null)
                clients.Remove(client);
        }

        public void Listen()
        {
            try
            {
                tcpListener = new TcpListener(IPAddress.Any, 8888);
                tcpListener.Start();
                Console.WriteLine("Сервер запущен. Ожидание подключений...");

                while (true)
                {
                    TcpClient tcpClient = tcpListener.AcceptTcpClient();

                    ClientObj clientObject = new ClientObj(tcpClient, this);
                    Thread clientThread = new Thread(new ThreadStart(clientObject.Process));
                    clientThread.Start();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Disconnect();
            }
        }
        
        public void BroadcastMessage(string message, string id)
        {
            byte[] data = Encoding.Unicode.GetBytes(message);
            for (int i = 0; i < clients.Count; i++)
            {
                if (clients[i].Id != id) 
                {
                    clients[i].Stream.Write(data, 0, data.Length); 
                }
            }
        }

        public void Disconnect()
        {
            tcpListener.Stop(); 

            for (int i = 0; i < clients.Count; i++)
            {
                clients[i].Close(); 
            }
            Environment.Exit(0); 
        }
    }
}
