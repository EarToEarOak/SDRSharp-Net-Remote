#define MyAppName "SDRSharp Net Remote"
#define MyAppVersion "1.0"
#define MyAppPublisher "Al Brown"
#define MyAppURL "http://eartoearoak.com/software/sdrsharp-net-remote"

[Setup]
AppId={{9B6C377F-1560-4441-8602-78CECC537F07}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={pf}\SDRSharp
LicenseFile=license.txt
InfoBeforeFile=help.txt
OutputBaseFilename=setup
Compression=lzma
SolidCompression=yes
EnableDirDoesntExistWarning=True
DirExistsWarning=no
DisableProgramGroupPage=yes
MinVersion=0,5.01sp3

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Files]
Source: "SDRSharp.NetRemote.dll"; DestDir: "{app}"; Flags: ignoreversion;

[Icons]
Name: "{group}\{cm:ProgramOnTheWeb,{#MyAppName}}"; Filename: "{#MyAppURL}"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"

[ThirdParty]
UseRelativePaths=True

[Messages]
SelectDirLabel3=Please select the SDR# installation folder

[Code]
procedure Error(Message: String);
begin
  MsgBox('An error occured!' + #13#10 + Message, mbError, MB_OK);
end;

function XMLName(): String;
begin
  result := ExpandConstant('{app}\Plugins.xml')
end;

function XMLLoad(): Variant;
var
  Filename: String;
  XMLDocument: Variant;
begin
  Filename := XMLName();
  FileCopy(Filename, Filename + '.bak', False);
  XMLDocument := CreateOleObject('Msxml2.DOMDocument.6.0');
  XMLDocument.async := False;
  XMLDocument.load(Filename);
  if XMLDocument.parseError.errorCode <> 0 then
      MsgBox(ExpandConstant('SDRSharp was not found in {app}'), mbError, MB_OK);
  result := XMLDocument;
end;

procedure XMLSave(XMLDocument: Variant);
var
  Filename: String;
begin
  Filename := XMLName();
  XMLDocument.save(Filename);
end;

procedure AddXML();
var
  XMLDocument: Variant; 
  XMLNode: Variant;
  XMLNodeNew: Variant; 
begin
  try
    XMLDocument := XMLLoad();
    if (XMLDocument.parseError.errorCode = 0) then
    begin
      XMLNode := XMLDocument.selectSingleNode('sharpPlugins');
      XMLNodeNew := XMLDocument.createNode(1, 'add', '');
      XMLNodeNew.setAttribute('key', 'Net Remote');
      XMLNodeNew.setAttribute('value','SDRSharp.NetRemote.NetRemotePlugin,SDRSharp.NetRemote');
      XMLNode.appendChild(XMLNodeNew)
      XMLSave(XMLDocument);
    end;
  except
    Error(GetExceptionMessage);
  end;
end;

procedure DelXML();
var
  XMLDocument: Variant; 
  XMLNode: Variant;
begin
  try
    XMLDocument := XMLLoad();
    if XMLDocument.parseError.errorCode = 0 then
    begin
      XMLNode := XMLDocument.selectSingleNode('/sharpPlugins/add[@key="Net Remote"]');
      if not VarIsNull(XMLNode) then
      begin
        XMLNode.parentNode.removeChild(XMLNode);
        XMLSave(XMLDocument);
      end;
    end;
  except
    {Error(GetExceptionMessage);} {VarIsNull(XMLNode) fails}
  end;
end;

procedure CurStepChanged(Step: TSetupStep);
begin
  if (Step = ssPostInstall) then
  begin
    DelXML();
    AddXML();
  end;
end;

procedure CurUninstallStepChanged(Step: TUninstallStep);
begin
  if (Step = usUninstall) then
    DelXML();   
end;
