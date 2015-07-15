program ASConsole;

uses
  Forms,
  MainForm in 'MainForm.pas' {fMain},
  ControlThread in 'ControlThread.pas';

{$R *.RES}

begin
  Application.Initialize;
  if IsCommandLineCorrect then
    begin
    Application.CreateForm(TfMain, fMain);
    end
  else
    begin
    Application.Terminate;
    end;
  Application.Run;
end.
