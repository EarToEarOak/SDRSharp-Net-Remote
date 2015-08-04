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

using System.Linq;
using System.Reflection;

namespace SDRSharp.NetRemote
{
    class Info
    {
        public static string Title()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();

            return assembly
                .GetCustomAttributes(typeof(AssemblyTitleAttribute), false)
                .OfType<AssemblyTitleAttribute>().FirstOrDefault().Title;
        }

        public static string Version()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            return assembly.GetName().Version.ToString();
        }
    }
}
