using SuperChat;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace HD
{
    public class TCPChat
    {
        #region Data
        public static TCPChat instance;

        public bool isServer;

        /// <summary>
        /// IP for clients to connect to. Null if you are the server.
        /// </summary>
        public IPAddress serverIp;

        /// <summary>
        /// For Clients, there is only one and it's the connection to the server.
        /// For Servers, there are many - one per connected client.
        /// </summary>
        private List<TcpConnectedClient> clientList = new List<TcpConnectedClient>();

        /// <summary>
        /// Accepts new connections.  Null for clients.
        /// </summary>
        private TcpListener listener;
        #endregion

        #region Unity Events
        public void Awake()
        {
            instance = this;

            if (serverIp == null)
            { // Server: start listening for connections
                Console.WriteLine("Starting Server...");
                isServer = true;
                listener = new TcpListener(localaddr: IPAddress.Any, port: Globals.Port);
                listener.Start();
                listener.BeginAcceptTcpClient(OnServerConnect, null);
            }
            else
            { // Client: try connecting to the server
                Console.WriteLine("Trying to connect to Server...");
                TcpClient client = new TcpClient();
                TcpConnectedClient connectedClient = new TcpConnectedClient(client);
                clientList.Add(connectedClient);
                client.BeginConnect(serverIp, Globals.Port, (ar) => connectedClient.EndConnect(ar), null);
            }
        }

        public void OnApplicationQuit()
        {
            listener?.Stop();
            for (int i = 0; i < clientList.Count; i++)
            {
                clientList[i].Close();
            }
        }

        //protected void Update() => text.text = messageToDisplay;
        #endregion

        #region Async Events
        private void OnServerConnect(IAsyncResult ar)
        {
            TcpClient tcpClient = listener.EndAcceptTcpClient(ar);
            clientList.Add(new TcpConnectedClient(tcpClient));

            listener.BeginAcceptTcpClient(OnServerConnect, null);
        }
        #endregion

        #region API
        public void OnDisconnect(TcpConnectedClient client) => clientList.Remove(client);

        internal void Send(
      string message)
        {
            BroadcastChatMessage(message);

            if (isServer)
            {
                Dashboard.instance.AppendStream(message);
            }
        }

        internal static void BroadcastChatMessage(string message)
        {
            for (int i = 0; i < instance.clientList.Count; i++)
            {
                TcpConnectedClient client = instance.clientList[i];
                client.Send(message);
            }
        }
        #endregion
    }
}
