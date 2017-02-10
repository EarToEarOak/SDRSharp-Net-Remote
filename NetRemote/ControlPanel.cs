/*
 SDRSharp Net Remote

 http://eartoearoak.com/software/sdrsharp-net-remote

 Copyright 2014 - 2017 Al Brown

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
using System.Threading;
using System.Windows.Forms;

namespace SDRSharp.NetRemote
{
    public partial class ControlPanel : UserControl
    {
        private const string _settingServerEn = "netRemoteServerEnable";
        private const string _settingServerPort = "netRemoteServerPort";
        private const string _settingSerialEn = "netRemoteSerialEnable";
        private const string _settingSerialPort = "netRemoteSerialPort";

        private const int PORT = 3382;

        private ISharpControl _control;

        private Thread _threadServer;
        private Thread _threadSerial;
        private Parser _parser;
        private Server _server = null;
        private Serial _serial = null;

        public ControlPanel(ISharpControl control)
        {
            InitializeComponent();

            _parser = new Parser(control);
            _control = control;

            checkNetwork.Checked = Utils.GetBooleanSetting(_settingServerEn, true);
            numPort.Value = Utils.GetIntSetting(_settingServerPort, PORT);
            numPort.Enabled = !checkNetwork.Checked;

            checkSerial.Checked = Utils.GetBooleanSetting(_settingSerialEn, true);
            string[] ports = Serial.GetPorts();
            if (ports.Length > 0)
            {
                comboSerial.Enabled = !checkSerial.Checked;
                comboSerial.Items.AddRange(Serial.GetPorts());
                comboSerial.SelectedIndex = 0;
                comboSerial.SelectedItem = Utils.GetStringSetting(_settingSerialPort, "");
            }
            else
            {
                checkSerial.Checked = false;
                checkSerial.Enabled = false;
                comboSerial.Enabled = false;
            }

            ServerControl();
            SerialControl();
        }

        public void Close()
        {
            Utils.SaveSetting(_settingServerEn, checkNetwork.Checked);
            Utils.SaveSetting(_settingServerPort, numPort.Value);
            Utils.SaveSetting(_settingSerialEn, checkSerial.Checked);
            Utils.SaveSetting(_settingSerialPort, comboSerial.SelectedItem);

            checkNetwork.Checked = false;
            checkSerial.Checked = false;
            ServerControl();
            SerialControl();
        }

        private void ServerControl()
        {
            if (checkNetwork.Checked)
            {
                if (_threadServer == null)
                {
                    _server = new Server(_parser, (int)numPort.Value);
                    _server.ServerError += OnServerError;
                    _threadServer = new Thread(new ThreadStart(_server.Start));
                    _threadServer.Start();
                }
            }
            else
            {
                if (_threadServer != null)
                {
                    _server.Stop();
                    _threadServer.Join(1000);
                    _threadServer = null;
                }
            }
        }

        private void SerialControl()
        {
            if (checkSerial.Checked)
            {
                if (_threadSerial == null)
                {
                    _serial = new Serial(_parser, comboSerial.SelectedItem.ToString());
                    _serial.SerialError += OnSerialError;
                    _threadSerial = new Thread(new ThreadStart(_serial.Start));
                    _threadSerial.Start();
                }
            }
            else
            {
                if (_threadSerial != null)
                {
                    _serial.Stop();
                    _threadSerial.Join(1000);
                    _threadSerial = null;
                }
            }
        }

        private void CheckChangedNetwork(object sender, EventArgs e)
        {
            numPort.Enabled = !checkNetwork.Checked;
            ServerControl();
        }

        private void CheckChangedSerial(object sender, EventArgs e)
        {
            comboSerial.Enabled = !checkSerial.Checked;
            SerialControl();
        }

        private void OnSerialError(object sender, EventArgs e)
        {
            _threadSerial = null;
            checkSerial.Checked = false;
        }

        private void OnServerError(object sender, EventArgs e)
        {
            _threadServer = null;
            checkNetwork.Checked = false;
        }
    }
}
