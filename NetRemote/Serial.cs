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
using System.Windows.Forms;

namespace SDRSharp.NetRemote
{
    class Serial
    {
        public event EventHandler SerialError;

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
            catch (IOException ex)
            {
                OnSerialError();
                string msg = "Serial Port Error:\n" + ex.Message;
                MessageBox.Show(msg, Info.Title() + "Serial Port Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close(port);
                return;
            }
            catch (UnauthorizedAccessException ex)
            {
                OnSerialError();
                string msg = "Serial Port Error:\n" + ex.Message;
                MessageBox.Show(msg, Info.Title() + "Serial Port Error",
                     MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close(port);
                return;
            }

            Send(port, _parser.Motd());

            _signal.Reset();
            _signal.WaitOne();

            Close(port);
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
            if (_data.Split('{').Length == _data.Split('}').Length)
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

        private void Close(SerialPort port)
        {
            try
            {
                port.Close();
            }
            catch (IOException)
            {
            }

        }

        protected virtual void OnSerialError()
        {
            EventHandler handler = SerialError;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }
    }
}
