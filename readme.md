# SDRSharp Net Remote #

Copyright 2014 Al Brown

al [at] eartoearoak.com


A network remote control plugin for SDRSharp.

More details can be found [here](http://eartoearoak.com/software/sdrsharp-net-remote).

Tested on:

- Windows XP
- Windows 7
- Windows 8.1

## Installation ##
Run the installer and point it to an installation of SDRSharp, this will copy the necessary files and register the plugin.

## Testing ##
Start SDRSharp and check the plugin control panel is shown on the left side of the main window and 'Enable' is ticked.

Run:

    telnet localhost 3382

You should get a JSON response showing the plugin name and version, for example:

    {"Name":"Net Remote","Version":"1.0.5282.28765"}

## Commands ##
Commands are JSON formatted strings containing a *Command*, *Method* and an optional *Value* attributes. For example to set the current volume to 30:

    {"Command": "Set", "Method": "AudioGain", "Value": 30}


Or to test if SDRSharp is currently playing:

    {"Command": "Get", "Method": "IsPlaying"}
Which returns:

    {"IsPlaying":true}

All attributes are case insensitive.

### Command Attribute ###
The command attribute may be one of the following:

    Set
    Get
    Exe 

The value attribute only used with the Set command.

### Method Attribute ###
For the *Get* and *Set* commands the method can be one of the following:

    AudioGain			- Volume <0-40>  
    AudioIsMuted		- Mute <true|false>
    CenterFrequency		- Frequency <0-999999999999>
    DetectorType		- Demodulation <AM|CW|DSB|LSB|NFM|RAW|USB|WFM>
    IsPlaying			- Currently playing <true|false>
	SourceIsTunable		- Tunable device <true|false>

For the *Exe* command these methods are available:

    Start				- Start playing
    Stop				- Stop playing
    Close				- Close network connection

## License ##

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.
