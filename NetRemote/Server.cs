/*
 SDRSharp Net Remote

 http://eartoearoak.com/software/sdrsharp-net-remote

 Copyright 2014 Al Brown

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
using System.Web.Script.Serialization;

namespace SDRSharp.NetRemote
{

    // Based on http://msdn.microsoft.com/en-us/library/fx6588te.aspx

    class Server
    {
        private const int PORT = 3382;
        private const int MAX_CLIENTS = 4;
        private static string[] COMMANDS = { "get", "set", "exe" };
        private static string[] METHODS = { "audiogain",
                                            "audioismuted",
                                            "centerfrequency",
                                            "detectortype",
                                            "isplaying",
                                            "sourceistunable",
                                            "start",
                                            "stop",
                                            "close"};

        private ManualResetEvent _signal = new ManualResetEvent(false);

        private ISharpControl _control;
        private Object lockClients = new Object();
        private List<Client> clients = new List<Client>();
        private volatile bool _cancel = false;
        private JavaScriptSerializer _json = new JavaScriptSerializer();

        private byte[] warnMaxClients = Encoding.ASCII.GetBytes(
            "Too many connections");

        public Server(ISharpControl control)
        {
            _control = control;
        }

        public void Start()
        {
            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, PORT);
            Socket socket = new Socket(AddressFamily.InterNetwork,
                                       SocketType.Stream, ProtocolType.Tcp);

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
            catch (SocketException)
            {
                if (socket.IsBound)
                    socket.Shutdown(SocketShutdown.Both);
            }
            finally
            {
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

        private void ClientAdd(Client client)
        {
            lock (lockClients)
                clients.Add(client);

            Motd(client);
            client.socket.BeginReceive(client.buffer, 0, Client.BUFFER_SIZE, 0,
                                       new AsyncCallback(ReadCallback),
                                       client);
        }

        private void ClientRemove(Client client)
        {
            lock (lockClients)
                clients.Remove(client);
            client.socket.Shutdown(SocketShutdown.Both);
            client.socket.Close();
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


            try
            {
                int read = client.socket.EndReceive(ar);
                if (read > 0)
                {
                    client.data.Append(Encoding.ASCII.GetString(client.buffer,
                                                                0, read));
                    data = client.data.ToString();
                    if (data.IndexOf("\n") > -1)
                        Parse(client);

                    client.socket.BeginReceive(client.buffer, 0,
                                                Client.BUFFER_SIZE, 0,
                                                new AsyncCallback(ReadCallback),
                                                client);
                }
            }
            catch (ObjectDisposedException)
            {
            }
        }

        private void Send(Client client, String data)
        {
            var byteData = Encoding.ASCII.GetBytes(data);
            try
            {
                client.socket.BeginSend(byteData, 0, byteData.Length, 0,
                                        new AsyncCallback(SendCallback), client);
            }
            catch (SocketException)
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
            catch (SocketException)
            {
                ClientRemove(client);
            }
        }

        private void Motd(Client client)
        {
            Dictionary<string, string> version = new Dictionary<string, string>
            {
                {"Name", Info.Title()},
                {"Version", Info.Version()}
            };

            Send(client, _json.Serialize(version) + "\r\n");
        }

        private void Error(Client client, string type, string message)
        {
            Dictionary<string, string> version = new Dictionary<string, string>
            {
                {"Result", "Error"},
                {"Type", type},
                {"Message", message}
            };

            Send(client, _json.Serialize(version) + "\r\n");
        }

        private void Response(Client client, string key, string value)
        {
            Dictionary<string, string> version = new Dictionary<string, string>
            {
                {key, value}
            };
            Send(client, _json.Serialize(version) + "\r\n");
        }

        private void Response(Client client, string key, bool value)
        {
            Dictionary<string, bool> version = new Dictionary<string, bool>
            {
                {key, value}
            };
            Send(client, _json.Serialize(version) + "\r\n");
        }

        private void Response(Client client, string key, int value)
        {
            Dictionary<string, int> version = new Dictionary<string, int>
            {
                {key, value}
            };
            Send(client, _json.Serialize(version) + "\r\n");
        }

        private void Response(Client client, string key, long value)
        {
            Dictionary<string, long> version = new Dictionary<string, long>
            {
                {key, value}
            };
            Send(client, _json.Serialize(version) + "\r\n");
        }

        private void Parse(Client client)
        {
            string data = Regex.Replace(client.data.ToString(),
                                        @"[^\u0020-\u007F]", string.Empty);
            data = data.ToLower();

            try
            {
                Dictionary<string, object> requests =
                    (Dictionary<string, object>)_json.DeserializeObject(data);

                if (requests != null)
                {
                    object objCommand;
                    object objMethod;
                    object value;

                    requests.TryGetValue("command", out objCommand);
                    requests.TryGetValue("method", out objMethod);
                    requests.TryGetValue("value", out value);

                    if (!(objCommand is string))
                        throw new CommandException("Command should be a string");
                    if (!(objMethod is string))
                        throw new MethodException("Method should be a string");

                    string command = (string)objCommand;
                    string method = (string)objMethod;

                    if (command == null)
                        throw new CommandException("Command key not found");
                    if (Array.IndexOf(COMMANDS, command) == -1)
                        throw new CommandException(String.Format("Unknown command: {0}",
                            command));

                    if (method == null)
                        throw new MethodException("Method key not found");
                    if (Array.IndexOf(METHODS, method) == -1)
                        throw new MethodException(String.Format("Unknown method: {0}",
                            method));

                    if (string.Equals(command, "set") && value==null)
                        throw new ValueException("Value missing");

                    Command(client, command, method, value);
                }
            }
            catch (Exception ex)
            {
                if (ex is ArgumentException || ex is InvalidOperationException)
                    Error(client, "Syntax error", data);
                else if (ex is CommandException)
                    Error(client, "Command error", ex.Message);
                else if (ex is MethodException)
                    Error(client, "Method error", ex.Message);
                else if (ex is ValueException)
                    Error(client, "Value error", ex.Message);
                else if (ex is SourceException)
                    Error(client, "Source error", ex.Message);
                else
                    throw;
            }
            finally
            {
                client.data.Length = 0;
            }
        }

        private void Command(Client client, string command, string method, object value)
        {
            if (string.Equals(command, "exe"))
                switch (method)
                {
                    case "start":
                        _control.StartRadio();
                        break;
                    case "stop":
                        _control.StopRadio();
                        break;
                    case "close":
                        ClientRemove(client);
                        break;
                    default:
                        throw new MethodException(String.Format("Unknown Exe method: {0}",
                                                  method));
                }
            else
            {
                bool set = string.Equals(command, "set");
                switch (method)
                {
                    case "audiogain":
                        if (set)
                        {
                            int gain = (int)CheckValue<int>(value);
                            CheckRange(gain, 0, 40);
                            _control.AudioGain = gain;
                        }
                        else
                            Response(client, "AudioGain",
                                     _control.AudioGain);
                        break;
                    case "audioismuted":
                        if (set)
                            _control.AudioIsMuted = (bool)CheckValue<bool>(value);
                        else
                            Response(client, "AudioIsMuted",
                                     _control.AudioIsMuted);
                        break;
                    case "centerfrequency":
                        if (set)
                        {
                            if (!_control.SourceIsTunable)
                                throw new SourceException("Not tunable");
                            long freq =
                                _json.ConvertToType<long>(CheckValue<long>(value));
                            CheckRange(freq, 1, 999999999999);
                            _control.CenterFrequency = freq;
                        }
                        else
                            Response(client, "CenterFrequency",
                                     _control.CenterFrequency);
                        break;
                    case "detectortype":
                        if (set)
                        {
                            string det = (string)(CheckValue<string>(value));
                            _control.DetectorType =
                                (DetectorType)CheckEnum(det, typeof(DetectorType));
                        }
                        else
                            Response(client, "DetectorType",
                                     _control.DetectorType.ToString());
                        break;
                    case "isplaying":
                        if (set)
                            throw new MethodException("Read only");
                        else
                            Response(client, "IsPlaying",
                                     _control.IsPlaying);
                        break;
                    case "sourceistunable":
                        if (set)
                            throw new MethodException("Read only");
                        else
                            Response(client, "SourceIsTunable",
                                     _control.SourceIsTunable);
                        break;
                    default:
                        string type = set ? "Set" : "Get";
                        string error = String.Format("Unknown {0} method: {1}",
                                                     type, method);
                        throw new MethodException(error);
                }
                if (set)
                    Response(client, "Result", "OK");
            }
        }

        private object CheckValue<T>(object value)
        {
            Type typeExpected = typeof(T);
            Type typePassed = value.GetType();

            if (typeExpected == typeof(long))
                if (typePassed == typeof(long) || typePassed == typeof(int))
                    return value;

            if (typePassed != typeExpected)
            {
                if (typeExpected == typeof(bool))
                    throw new ValueException("Expected a boolean");
                if (typeExpected == typeof(int) || typeExpected == typeof(long))
                    throw new ValueException("Expected an integer");
                if (typeExpected == typeof(string))
                    throw new ValueException("Expected a string");
            }

            return value;
        }

        private void CheckRange(long value, long start, long end)
        {
            if (value < start)
                throw new ValueException(String.Format("Smaller than {0}", start));
            if (value > end)
                throw new ValueException(String.Format("Greater than {0}", end));
        }

        private object CheckEnum(string value, Type type)
        {
            return Enum.Parse(type, value, true);
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

    class CommandException : Exception
    {
        public CommandException(string message) : base(message) { }
    }

    class MethodException : Exception
    {
        public MethodException(string message) : base(message) { }
    }

    class ValueException : Exception
    {
        public ValueException(string message) : base(message) { }
    }

    class SourceException : Exception
    {
        public SourceException(string message) : base(message) { }
    }
}
