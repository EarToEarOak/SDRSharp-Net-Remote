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

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;

namespace SDRSharp.NetRemote
{
    class Serial
    {
        private Parser _parser;
        private string _data;
        private string _port;

        private ManualResetEvent _signal = new ManualResetEvent(false);

        public Serial(Parser parser, string port)
        {
            _parser = parser;
            _port = port;
        }

        public static string[] GetPorts()
        {
            string[] ports = SerialPort.GetPortNames();
            Array.Sort(ports);

            return ports;
        }

        public void Start()
        {
            SerialPort port = new SerialPort(_port, 115200);
            port.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);
            try
            {
                port.Open();
            }
            catch (IOException) {

            }
            catch (UnauthorizedAccessException)
            {

            }

            Send(port, _parser.Motd());

            _signal.Reset();
            _signal.WaitOne();

            port.Close();
        }

        public void Stop()
        {
            _signal.Set();
        }

        private void Send(SerialPort port, String data)
        {
            if (port.IsOpen)
                port.Write(data);
        }

        private void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort port = (SerialPort)sender;
            string result;

            _data += port.ReadExisting();
            if (_data.IndexOf("\n") > -1 || _data.IndexOf("\r") > -1)
            {
                try
                {
                    result = _parser.Parse(_data);
                    Send(port, result);
                }
                catch (CommandException)
                {
                }

                _data = "";
            }
        }
    }
}
