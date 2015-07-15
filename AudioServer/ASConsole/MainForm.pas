unit MainForm;

interface

uses
  Windows, Messages, SysUtils, Classes, Graphics, Controls, Forms, Dialogs,
  Grids, ExtCtrls, ComCtrls, StdCtrls, ControlThread, Registry, Menus,
  ToolWin, ImgList;

const
  AppVersion = '1.0';
  AppKey = '\Software\KrAZ\ASConsole\' + AppVersion;
  AppTitle = 'Консоль аудиосервера ' + AppVersion;
  AboutString = 'Консоль аудиосервера (ASConsole), версия ' + AppVersion + #$0D#$0A +
                'Разработал инженер-программист лаборатории ИТС'#$0D#$0A +
                'Громов И.В. (тел. 30-65)'#$0D#$0A +
                '(C) 2000 Красноярский Алюминиевый Завод';

type
  TChannelInfo = record
    Id: Integer;
    Name: String;
    PlayingInfo: String;
  end;
  TChannelInfoArray = array of TChannelInfo;

  TInputInfo = record
    Id: Integer;
    Name: String;
  end;
  TInputInfoArray = array of TInputInfo;

  TfMain = class(TForm)
    sbStatus: TStatusBar;
    pmMenu: TPopupMenu;
    miTransmit: TMenuItem;
    N1: TMenuItem;
    miExit: TMenuItem;
    N2: TMenuItem;
    miAbout: TMenuItem;
    N3: TMenuItem;
    miShutdownServer: TMenuItem;
    miEndTransmit: TMenuItem;
    dgChannels: TDrawGrid;
    tbTools: TToolBar;
    tbClearSelection: TToolButton;
    ToolButton3: TToolButton;
    tbTransmit: TToolButton;
    ToolButton6: TToolButton;
    tbSelectAll: TToolButton;
    ImageList1: TImageList;
    N4: TMenuItem;
    miSelectAll: TMenuItem;
    N5: TMenuItem;
    miToolbar: TMenuItem;
    miStatusbar: TMenuItem;
    miClearSelection: TMenuItem;
    tHotKeys: TTimer;
    procedure FormCreate(Sender: TObject);
    procedure dgChannelsDrawCell(Sender: TObject; ACol, ARow: Integer;
      Rect: TRect; State: TGridDrawState);
    procedure dgChannelsKeyDown(Sender: TObject; var Key: Word;
      Shift: TShiftState);
    procedure FormDestroy(Sender: TObject);
    procedure miExitClick(Sender: TObject);
    procedure miAboutClick(Sender: TObject);
    procedure miTransmitClick(Sender: TObject);
    procedure miShutdownServerClick(Sender: TObject);
    procedure miEndTransmitClick(Sender: TObject);
    procedure dgChannelsKeyUp(Sender: TObject; var Key: Word;
      Shift: TShiftState);
    procedure tbSelectAllClick(Sender: TObject);
    procedure tbTransmitClick(Sender: TObject);
    procedure dgChannelsMouseDown(Sender: TObject; Button: TMouseButton;
      Shift: TShiftState; X, Y: Integer);
    procedure miToolbarClick(Sender: TObject);
    procedure tHotKeysTimer(Sender: TObject);
  private
    ControlThread: TControlThread;
    Reg: TRegistry;
    FInitialized: Boolean;
    FFormRect: TRect;
    FFormState: Integer;
    F12Pressed: Boolean;
    HotPressed: Boolean;
    procedure SaveRect;
    procedure WMMove(var Message: TWMMove); message WM_MOVE;
    procedure WMSize(var Message: TWMSize); message WM_SIZE;
  public
    Channels: TChannelInfoArray;
    SelectedChannels: array of Boolean;
    ChannelHotKeys: array of array of Char;
    ThreadChannels: TChannelInfoArray;
    Inputs: TChannelInfoArray;
    ThreadInputs: TChannelInfoArray;
    ConnStatus: array [0..AS_NETCONTROLREPLAYLENGTH] of Char;
    CommonStatus: array [0..AS_NETCONTROLREPLAYLENGTH] of Char;
    NetCommandPosted: Boolean;
    NetCommand: array [0..AS_NETCONTROLREPLAYLENGTH] of Char;
    NetReplayWaitPosted: Boolean;
    NetReplay: array [0..AS_NETCONTROLREPLAYLENGTH] of Char;
    ConnectionEstablished: Boolean;
    procedure ChannelsChanged;
    procedure InputsChanged;
    procedure StatusChanged;
  end;

function IsCommandLineCorrect: Boolean;

var
  fMain: TfMain;

implementation

{$R *.DFM}

function IsCommandLineCorrect: Boolean;
var i, j: Integer;
begin
  Result := True;
  i := Pos('@', ParamStr(1));
  j := Pos(':', ParamStr(1));
  if (i = 0) or (j = 0) or (i > j) then
    begin
    Application.MessageBox('Использование: ASConsole Login@host:port', AppTitle, MB_OK);
    Result := False;
    end;
end;

procedure TfMain.FormCreate(Sender: TObject);
begin
  Application.Title := AppTitle;

  dgChannels.DoubleBuffered := True;

  HotPressed := False;

  FFormRect := BoundsRect;
  FFormState := Integer(WindowState);
  Reg := TRegistry.Create;
  Reg.RootKey := HKEY_LOCAL_MACHINE;
  Reg.OpenKey(AppKey, True);

  Reg.ReadBinaryData('MainFormRect', FFormRect, SizeOf(TRect));
  if Reg.ValueExists('MainFormState') then FFormState := Reg.ReadInteger('MainFormState');

  if Reg.ValueExists('ToolbarVisible') then tbTools.Visible := Reg.ReadBool('ToolbarVisible');
  if Reg.ValueExists('StatusbarVisible') then sbStatus.Visible := Reg.ReadBool('StatusbarVisible');
  miToolbar.Checked := tbTools.Visible;
  miStatusbar.Checked := sbStatus.Visible;

  Reg.CloseKey;

  with FFormRect do
    if (Left >= 0) and (Top >= 0) and (Right <= Screen.Width) and (Bottom <= Screen.Height) then
      begin
      BoundsRect := FFormRect;
      WindowState := TWindowState(FFormState);
      end;

  ControlThread := TControlThread.Create(Copy(ParamStr(1), Pos('@', ParamStr(1))+1, Pos(':', ParamStr(1)) - Pos('@', ParamStr(1)) - 1),
                                         StrToInt(Copy(ParamStr(1), Pos(':', ParamStr(1))+1, MaxInt)),
                                         Copy(ParamStr(1), 1, Pos('@', ParamStr(1)) - 1));

  FInitialized := True;
end;

procedure TfMain.FormDestroy(Sender: TObject);
var ExitCode: DWORD;
begin
  FInitialized := False;

  if ControlThread <> nil then
    begin
    ControlThread.Terminate;
    repeat
      if not GetExitCodeThread(ControlThread.Handle, ExitCode) then
        RaiseLastWin32Error;
      Application.ProcessMessages;
    until ExitCode <> STILL_ACTIVE;
    ControlThread.Free;
    ControlThread := nil;
    end;

  Reg.Free;
end;

procedure TfMain.ChannelsChanged;
var i, j: Integer;
    Buf: array [0..9] of Char;
begin
  Channels := ThreadChannels;
  SetLength(Channels, Length(Channels));
  SetLength(SelectedChannels, Length(Channels));

  SetLength(ChannelHotKeys, Length(Channels));
  Reg.OpenKey(AppKey + '\HotKeys', True);
  for i := 0 to Length(Channels)-1 do
    begin
    FillChar(Buf[0], 10, 0);
    Reg.ReadBinaryData(Channels[i].Name, Buf[0], 10);
    SetLength(ChannelHotKeys[i], 0);
    for j := 0 to 9 do
      begin
      if Buf[j] = #0 then
        Break;
      SetLength(ChannelHotKeys[i], Length(ChannelHotKeys[i]) + 1);
      ChannelHotKeys[i][Length(ChannelHotKeys[i]) - 1] := Buf[j];
      end;
    end;
  Reg.CloseKey;

  dgChannels.ColCount := Length(Channels);
  dgChannels.Invalidate;
  miTransmit.Enabled := (Length(Channels) > 0) and (Length(Inputs) > 0);
end;

procedure TfMain.InputsChanged;
begin
  Inputs := ThreadInputs;
  SetLength(Inputs, Length(Inputs));
  miTransmit.Enabled := (Length(Channels) > 0) and (Length(Inputs) > 0);
end;

procedure TfMain.dgChannelsDrawCell(Sender: TObject; ACol, ARow: Integer;
  Rect: TRect; State: TGridDrawState);
const CBChecked: array [Boolean] of DWORD = (0, DFCS_PUSHED);
var Rect2: TRect;
    OldBrushColor, OldPenColor: TColor;
    OldBrushStyle: TBrushStyle;
    W: Integer;
    Avg: Integer;
    Add3D: Integer;
    R, G, B: Integer;
begin
  with TDrawGrid(Sender).Canvas do
    begin
    if ACol >= Length(Channels) then
      begin
      if ACol = 0 then
        begin
        Brush.Color := clBtnFace;
        FillRect(Rect);
        end;
      Exit;
      end;

    Brush.Color := clBtnFace;
    FillRect(Rect);
    Rect2 := Rect;
    Inc(Rect2.Left, 2);
    Inc(Rect2.Top, 2);
    Dec(Rect2.Right, 2);
    Dec(Rect2.Bottom, 2);
    DrawFrameControl(Handle, Rect2, DFC_BUTTON, DFCS_BUTTONPUSH or CBChecked[SelectedChannels[ACol]]);

    if gdFocused in State then
      DrawFocusRect(Rect);

    Add3D := 0;
    if SelectedChannels[ACol] then
      Add3D := 2;

    OldBrushColor := Brush.Color;
    OldBrushStyle := Brush.Style;
    OldPenColor := Pen.Color;
    if Channels[ACol].PlayingInfo = '' then
      Avg := 0
    else
      Avg := StrToInt(Channels[ACol].PlayingInfo);
    if Avg > 10000 then
      Avg := 10000;
    R := (ColorToRGB(Brush.Color) and $FF);
    G := ((ColorToRGB(Brush.Color) shr 8) and $FF);
    B := ((ColorToRGB(Brush.Color) shr 16) and $FF);
    R := ((Avg * ($FF - R)) div 10000) + R;
    G := G - ((Avg * G) div 10000);
    B := B - ((Avg * B) div 10000);
    Brush.Color := (B shl 16) or (G shl 8) or R;
    Pen.Color := Brush.Color;
    Ellipse(Rect.Left + 5 + Add3D, Rect.Top + 5 + Add3D, Rect.Right - 5 + Add3D, Rect.Bottom - 5 + Add3D);
    Pen.Color := OldPenColor;

    Brush.Style := bsClear;
    Font.Style := Font.Style + [fsBold];
    W := TextWidth(Channels[ACol].Name);
    TextOut(Rect.Left + 13 - 1 - (W div 2) + Add3D, Rect.Top + 8 - 1 + Add3D, Channels[ACol].Name);
    Font.Style := Font.Style - [fsBold];
    Brush.Color := OldBrushColor;
    Brush.Style := OldBrushStyle;
    end;
end;

procedure TfMain.dgChannelsKeyDown(Sender: TObject; var Key: Word; Shift: TShiftState);
begin
  case Key of
    VK_SPACE:
      begin
      SelectedChannels[TDrawGrid(Sender).Col] := not SelectedChannels[TDrawGrid(Sender).Col];
      TDrawGrid(Sender).Refresh;
      end;
    VK_F12:
      begin
      if not F12Pressed then
        begin
        F12Pressed := True;
        miTransmitClick(nil);
        tbTransmit.Down := True;
        end;
      end;
  end;
end;

procedure TfMain.dgChannelsKeyUp(Sender: TObject; var Key: Word; Shift: TShiftState);
begin
  case Key of
    VK_F12:
      begin
      F12Pressed := False;
      tbTransmit.Down := False;
      miEndTransmitClick(nil);
      end;
  end;
end;

procedure TfMain.StatusChanged;
begin
  sbStatus.Panels[0].Text := String(ConnStatus);
  sbStatus.Hint := String(ConnStatus);
  sbStatus.Panels[1].Text := String(CommonStatus);
  miShutdownServer.Enabled := ConnectionEstablished;
end;

procedure TfMain.SaveRect;
var FormPlacement: TWindowPlacement;
begin
  if not FInitialized then Exit;
  FormPlacement.length := SizeOf(TWindowPlacement);
  if not GetWindowPlacement(Handle, @FormPlacement) then RaiseLastWin32Error;
  FFormRect := FormPlacement.rcNormalPosition;
  FFormState := Integer(WindowState);
  Reg.OpenKey(AppKey, True);
  Reg.WriteBinaryData('MainFormRect', FFormRect, SizeOf(TRect));
  Reg.WriteInteger('MainFormState', FFormState);
  Reg.CloseKey;
end;

procedure TfMain.WMSize(var Message: TWMSize);
begin
  inherited;
  SaveRect;
end;

procedure TfMain.WMMove(var Message: TWMMove);
begin
  inherited;
  SaveRect;
end;

procedure TfMain.miExitClick(Sender: TObject);
begin
  Close;
end;

procedure TfMain.miAboutClick(Sender: TObject);
begin
  Application.MessageBox(AboutString, AppTitle, MB_OK);
end;

procedure TfMain.miTransmitClick(Sender: TObject);
var i: Integer;
    SelFound: Boolean;

    h: THandle;
    b: Cardinal;
    s: String;
begin
  h := CreateFile('C:\message.log', GENERIC_WRITE, FILE_SHARE_READ, nil, OPEN_ALWAYS, FILE_ATTRIBUTE_NORMAL, 0);
  s := FormatDateTime('dd.mm.yyyy hh:nn:ss', Now) + ' StartTransmit' + #$0D#$0A;
  SetFilePointer(h, 0, nil, FILE_END);
  WriteFile(h, PChar(s)^, Length(s), b, nil);
  CloseHandle(h);

  StrCopy(NetCommand, 'TRANSMIT FROM INPUTLINE ');
  StrCat(NetCommand, PChar(IntToStr(Inputs[0{dgInputs.Row}].Id)));
  StrCat(NetCommand, ' TO CHANNEL ');
  SelFound := False;
  for i := 0 to Length(Channels)-1 do
    if SelectedChannels[i] then
      begin
      if SelFound then
        StrCat(NetCommand, ',');
      StrCat(NetCommand, PChar(IntToStr(Channels[i].Id)));
      SelFound := True;
      end;
  if not SelFound then
    begin
    tbTransmit.Down := False;
    raise Exception.Create('Ни одного канала не выделено');
    end;
  StrCat(NetCommand, #$0D#$0A);
  for i := 0 to Length(Channels)-1 do
    SelectedChannels[i] := False;
  dgChannels.Invalidate;

  StrCopy(NetReplay, 'TRANSMIT:');
  NetReplayWaitPosted := True;
  NetCommandPosted := True;   // Именно в таком порядке - многовитковость, да...

  miShutdownServer.Enabled := False;
  miTransmit.Enabled := False;
  miEndTransmit.Enabled := False;
  try
    i := 0;
    repeat
      Application.ProcessMessages;
      Sleep(10);
      Inc(i);
    until (ControlThread = nil) or (not NetReplayWaitPosted) or (i >= 100);
    if NetReplayWaitPosted then
      raise Exception.CreateFmt('Не удалось дождаться ответа от сервера на команду "%s"', [String(NetCommand)]);
    if StrIComp(NetReplay, 'TRANSMIT: OK') <> 0 then
      raise Exception.CreateFmt('Ошибка выполнения команды "%s", ответ сервера: "%s"', [String(NetCommand), String(NetReplay)]);
  finally
    NetReplayWaitPosted := False;
    NetCommandPosted := False;
    miShutdownServer.Enabled := True;
    miTransmit.Enabled := True;
    miEndTransmit.Enabled := True;
  end;
end;

procedure TfMain.miShutdownServerClick(Sender: TObject);
var i: Integer;
begin
  StrCopy(NetCommand, 'SHUTDOWN'#$0D#$0A);
  NetCommandPosted := True;
  miShutdownServer.Enabled := False;
  miTransmit.Enabled := False;
  miEndTransmit.Enabled := False;
  try
    i := 0;
    repeat
      Application.ProcessMessages;
      Sleep(10);
      Inc(i);
    until (ControlThread = nil) or (not NetCommandPosted) or (i >= 100);
    if NetCommandPosted then
      raise Exception.CreateFmt('Не удалось дождаться ответа от сервера на команду "%s"', [String(NetCommand)]);
  finally
    NetCommandPosted := False;
    miShutdownServer.Enabled := True;
    miTransmit.Enabled := True;
    miEndTransmit.Enabled := True;
  end;
end;

procedure TfMain.miEndTransmitClick(Sender: TObject);
var i: Integer;

    h: THandle;
    b: Cardinal;
    s: String;
begin
  h := CreateFile('C:\message.log', GENERIC_WRITE, FILE_SHARE_READ, nil, OPEN_ALWAYS, FILE_ATTRIBUTE_NORMAL, 0);
  s := FormatDateTime('dd.mm.yyyy hh:nn:ss', Now) + ' EndTransmit' + #$0D#$0A;
  SetFilePointer(h, 0, nil, FILE_END);
  WriteFile(h, PChar(s)^, Length(s), b, nil);
  CloseHandle(h);

  StrCopy(NetCommand, 'END TRANSMIT'#$0D#$0A);
  StrCopy(NetReplay, 'END TRANSMIT:');
  NetReplayWaitPosted := True;
  NetCommandPosted := True;

  miShutdownServer.Enabled := False;
  miTransmit.Enabled := False;
  miEndTransmit.Enabled := False;
  try
    i := 0;
    repeat
      Application.ProcessMessages;
      Sleep(10);
      Inc(i);
    until (ControlThread = nil) or (not NetReplayWaitPosted) or (i >= 100);
    if NetReplayWaitPosted then
      raise Exception.CreateFmt('Не удалось дождаться ответа от сервера на команду "%s"', [String(NetCommand)]);
    if StrIComp(NetReplay, 'END TRANSMIT: OK') <> 0 then
      raise Exception.CreateFmt('Ошибка выполнения команды "%s", ответ сервера: "%s"', [String(NetCommand), String(NetReplay)]);
  finally
    NetReplayWaitPosted := False;
    NetCommandPosted := False;
    miShutdownServer.Enabled := True;
    miTransmit.Enabled := True;
    miEndTransmit.Enabled := True;
  end;
end;

procedure TfMain.tbSelectAllClick(Sender: TObject);
var i: Integer;
begin
  for i := 0 to Length(SelectedChannels)-1 do
    SelectedChannels[i] := ((TComponent(Sender).Name = 'tbSelectAll') or (TComponent(Sender).Name = 'miSelectAll'));
  dgChannels.Invalidate;
end;

procedure TfMain.tbTransmitClick(Sender: TObject);
begin
  if TToolButton(Sender).Down then
    miTransmitClick(nil)
  else
    miEndTransmitClick(nil);
end;

procedure TfMain.dgChannelsMouseDown(Sender: TObject; Button: TMouseButton; Shift: TShiftState; X, Y: Integer);
var P: TPoint;
    C, R: Integer;
begin
  P.x := X;
  P.y := Y;
  C := -1;
  R := -1;
  dgChannels.MouseToCell(P.x, P.y, C, R);
  if (C = -1) or (R = -1) then
    Exit;
  if not (ssDouble in Shift) then
    Exit;
  if C >= Length(SelectedChannels) then
    Exit;
  SelectedChannels[C] := not SelectedChannels[C];
  dgChannels.Invalidate;
end;

procedure TfMain.miToolbarClick(Sender: TObject);
begin
  TMenuItem(Sender).Checked := not TMenuItem(Sender).Checked;

  tbTools.Visible := miToolbar.Checked;
  sbStatus.Visible := miStatusbar.Checked;
  Reg.OpenKey(AppKey, True);
  Reg.WriteBool('ToolbarVisible', tbTools.Visible);
  Reg.WriteBool('StatusbarVisible', sbStatus.Visible);
  Reg.CloseKey;
end;

procedure TfMain.tHotKeysTimer(Sender: TObject);
var i, j, k, flag, asyncResult: Integer;
var prevHotPressed: Bool;
begin


  prevHotPressed := HotPressed;
  i := 0;
  flag := 0;

  for i := 0 to Length(ChannelHotKeys)-1 do begin
        if( ChannelHotKeys[i] = nil ) then
          Continue;
        asyncResult := GetAsyncKeyState(Integer(ChannelHotKeys[i][0]));
        if (asyncResult and $8000) <> 0 then
          begin
          //SelectedChannels[i] := True;
          Inc( flag );
          for k := 0 to Length(ChannelHotKeys)-1 do
            begin
              if( (ChannelHotKeys[k] <> nil) and (ChannelHotKeys[i] <> nil) and (ChannelHotKeys[i][0] = ChannelHotKeys[k][0]) ) then
                SelectedChannels[k] := True
            end;
          end;
         //else
         // SelectedChannels[i] := False;
    //end;
  end;

{*  while i <= Length(ChannelHotKeys) do
    begin
    if Length(ChannelHotKeys[i]) > 0 then
      begin
      j := 0;
      while j <= Length(ChannelHotKeys[i]) do
        begin
        if (GetAsyncKeyState(Integer(ChannelHotKeys[i][j])) and $8000) <> 0 then
          begin
          SelectedChannels[j] := True;
          Inc( flag );
          end
         else
          SelectedChannels[j] := False;
        Inc(j);
        end;
      if j >= Length(ChannelHotKeys[i]) then
        Break;
      end;
    Inc(i);
    end;*}

    
    if flag > 0 then
      begin
        HotPressed := True;
        if prevHotPressed = False then
          //tbTransmit.Down := True;
          miTransmitClick(nil);{ Application.MessageBox('Начали запись', AppTitle, MB_OK); }
      end
    else
      begin
        HotPressed := False;
        if prevHotPressed = True then
        begin
          //tbTransmit.Down := False;
          miEndTransmitClick(nil); { Application.MessageBox('Закончили запись', AppTitle, MB_OK); }
          for k := 0 to Length(ChannelHotKeys)-1 do
            begin
                SelectedChannels[k] := False
            end;
        end;
      end;

    dgChannels.Invalidate();

(*
  i := 0;
  while i < Length(ChannelHotKeys) do
    begin
    if Length(ChannelHotKeys[i]) > 0 then
      begin
      j := 0;
      while j < Length(ChannelHotKeys[i]) do
        begin
        if (GetAsyncKeyState(Integer(ChannelHotKeys[i][j])) and $8000) = 0 then
         Break;
        Inc(j);
        end;
      if j >= Length(ChannelHotKeys[i]) then
        Break;
      end;
    Inc(i);
    end;

  if i >= Length(ChannelHotKeys) then
    begin
    if HotPressed then
      begin
      HotPressed := False;
      miEndTransmitClick(nil);
      end;
    Exit;
    end;

  if HotPressed then
    Exit;

  HotPressed := True;
  for j := 0 to Length(SelectedChannels)-1 do
    SelectedChannels[j] := (j = i);
  miTransmitClick(nil); *)
end;

end.
