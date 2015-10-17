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
using System;
using System.Windows.Forms;

namespace SDRSharp.NetRemote
{
    public class NetRemotePlugin : ISharpPlugin
    {
        private ControlPanel _controlPanel;

        public string DisplayName
        {
            get { return Info.Title(); }
        }

        public bool HasGui
        {
            get { return true; }
        }

        public UserControl Gui
        {
            get { return _controlPanel; }
        }

        public void Initialize(ISharpControl control)
        {
            _controlPanel = new ControlPanel(control);
        }

        public void Close()
        {
            _controlPanel.Close();
        }
    }
}
