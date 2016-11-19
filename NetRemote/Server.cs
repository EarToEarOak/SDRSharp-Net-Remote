/*
 SDRSharp Net Remote

 http://eartoearoak.com/software/sdrsharp-net-remote

 Copyright 2014 - 2015 Al Brown

 A network remote control plugin for SDRSharp


 This program is free software: you can redistribute it and/or modify
 it under the terms of the GNU General Public License as published by
 the Free Software Foundation, or (at your option)
 any later version.

 This program is distributed in the hope that it will be useful,
 but WITHOUT ANY WARRANTY; without even the implied warranty of
 MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 GNU General Public License for more details.

 You should have received a copy of the GNU General Public License
 along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

using SDRSharp.Common;
using SDRSharp.Radio;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Timers;
using System.Web.Script.Serialization;
using System.Windows.Forms;

namespace SDRSharp.NetRemote
{

    // Based on http://msdn.microsoft.com/en-us/library/fx6588te.aspx

    class Server
    {
        public event EventHandler ServerError;

        private const int PORT = 3382;
        private const int MAX_CLIENTS = 4;

        private ManualResetEvent _signal = new ManualResetEvent(false);

        private Object lockClients = new Object();
        private List<Client> clients = new List<Client>();
        private volatile bool _cancel = false;
        private Parser _parser;

        private byte[] warnMaxClients = Encoding.ASCII.GetBytes(
            "Too many connections");

        public Server(Parser parser)
        {
            _parser = parser;
        }

        public void Start()
        {
            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, PORT);
            Socket socket = new Socket(AddressFamily.InterNetwork,
                                       SocketType.Stream, ProtocolType.Tcp);

            System.Timers.Timer timerAlive = new System.Timers.Timer();
            timerAlive.Elapsed += new ElapsedEventHandler(OnTimerAlive);
            timerAlive.Interval = 1000;
            timerAlive.Enabled = true;

            try
            {
                socket.Bind(localEndPoint);
                socket.Listen(100);

                while (!_cancel)
                {
                    _signal.Reset();
                    socket.BeginAccept(new AsyncCallback(ConnectCallback),
                        socket);
                    _signal.WaitOne();
                }
            }
            catch (SocketException ex)
            {
                OnServerError();

                if (socket.IsBound)
                    socket.Shutdown(SocketShutdown.Both);

                string msg = "Network Error:\n" + ex.Message;
                MessageBox.Show(msg, Info.Title(),
                     MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                timerAlive.Close();
                socket.Close();
                foreach (var client in clients.ToArray())
                    ClientRemove(client);
            }
        }

        public void Stop()
        {
            _cancel = true;
            _signal.Set();
        }

        private bool IsConnected(Socket socket)
        {
            try
            {
                return !(socket.Poll(1, SelectMode.SelectRead) && socket.Available == 0);
            }
            catch (SocketException) { return false; }
        }

        private void ClientAdd(Client client)
        {
            lock (lockClients)
                clients.Add(client);

            Send(client, _parser.Motd());
            try
            {
                client.socket.BeginReceive(client.buffer, 0, Client.BUFFER_SIZE, 0,
                                           new AsyncCallback(ReadCallback),
                                           client);
            }
            catch (Exception)
            {
                ClientRemove(client);
            }
        }

        private void ClientRemove(Client client)
        {
            lock (lockClients)
                clients.Remove(client);

            try
            {
                client.socket.Shutdown(SocketShutdown.Both);
                client.socket.Close();
            }
            catch (SocketException) { }
            catch (ObjectDisposedException) { }
        }

        private void ConnectCallback(IAsyncResult ar)
        {
            _signal.Set();

            try
            {

                Socket socketServer = (Socket)ar.AsyncState;
                Socket socketClient = socketServer.EndAccept(ar);
                Client client = new Client(socketClient);

                if (clients.Count < MAX_CLIENTS)
                    ClientAdd(client);
                else
                    ClientRemove(client);
            }
            catch (ObjectDisposedException)
            {
            }
        }

        private void ReadCallback(IAsyncResult ar)
        {
            String data = String.Empty;
            Client client = (Client)ar.AsyncState;

            string result;

            try
            {
                int read = client.socket.EndReceive(ar);
                if (read > 0)
                {
                    client.data.Append(Encoding.ASCII.GetString(client.buffer,
                                                                0, read));
                    data = client.data.ToString();
                    if (data.Split('{').Length == data.Split('}').Length)
                    {
                        try
                        {
                            result = _parser.Parse(client.data.ToString());
                            Send(client, result);
                        }
                        catch (CommandException)
                        {
                            ClientRemove(client);
                        }
                        catch (ClientException)
                        {
                            ClientRemove(client);
                        }
                        client.data.Length = 0;
                    }

                    client.socket.BeginReceive(client.buffer, 0,
                                                Client.BUFFER_SIZE, 0,
                                                new AsyncCallback(ReadCallback),
                                                client);
                }
            }
            catch (Exception)
            {
                ClientRemove(client);
            }
        }

        private void Send(Client client, String data)
        {
            if (data == null)
                return;

            var byteData = Encoding.ASCII.GetBytes(data);
            try
            {
                client.socket.BeginSend(byteData, 0, byteData.Length, 0,
                                        new AsyncCallback(SendCallback), client);
            }
            catch (Exception)
            {
                ClientRemove(client);
            }
        }

        private void SendCallback(IAsyncResult ar)
        {
            var client = (Client)ar.AsyncState;
            try
            {
                client.socket.EndSend(ar);
            }
            catch (Exception)
            {
                ClientRemove(client);
            }
        }

        private void OnTimerAlive(object source, ElapsedEventArgs e)
        {
            List<Client> disconnected = new List<Client>();

            lock (lockClients)
            {
                foreach (Client client in clients)
                    if (!IsConnected(client.socket))
                        disconnected.Add(client);
                foreach (Client client in disconnected)
                    ClientRemove(client);
            }
        }

        protected virtual void OnServerError()
        {
            EventHandler handler = ServerError;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }
    }


    class Client
    {
        public const int BUFFER_SIZE = 256;
        public Socket socket = null;
        public byte[] buffer = new byte[BUFFER_SIZE];
        public StringBuilder data = new StringBuilder();

        public Client(Socket socket)
        {
            this.socket = socket;
        }
    }
}
