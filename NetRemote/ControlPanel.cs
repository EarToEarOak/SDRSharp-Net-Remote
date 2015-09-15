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
using System.Threading;
using System.Windows.Forms;

namespace SDRSharp.NetRemote
{
    public partial class ControlPanel : UserControl
    {
        private const string _settingNotFirstRun = "netRemoteNotFirstRun";
        private const string _settingEnabled = "netRemoteEnable";

        private ISharpControl _control;

        private Thread _threadServer;
        private Parser _parser;
        private Server _server = null;
        private bool _isEnabled;

        public ControlPanel(ISharpControl control)
        {
            InitializeComponent();

            _parser = new Parser(control);
            _control = control;

            if (!Utils.GetBooleanSetting(_settingNotFirstRun))
                _isEnabled = true;
            else
                _isEnabled = Utils.GetBooleanSetting(_settingEnabled);

            ServerControl();
            checkEnable.Checked = _isEnabled;
        }

        public void Close()
        {
            Utils.SaveSetting(_settingNotFirstRun, true);
            Utils.SaveSetting(_settingEnabled, _isEnabled);
            _isEnabled = false;
            ServerControl();
        }

        private void ServerControl()
        {
            if (_isEnabled)
            {
                if (_threadServer == null)
                {
                    _server = new Server(_parser);
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

        private void CheckChangedEnable(object sender, EventArgs e)
        {
            _isEnabled = checkEnable.Checked;
            ServerControl();
        }
    }
}
