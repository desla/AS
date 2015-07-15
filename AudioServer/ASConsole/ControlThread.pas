unit ControlThread;

interface

uses
  Windows, SysUtils, Classes, Forms, Winsock;

const
  AS_NETCONTROLREPLAYLENGTH = 600;

type
  TControlThread = class(TThread)
  private
    IPAddr: String;
    Port: Word;
    Login: String;
    Sock: TSocket;
    Idle: Boolean;
    StatusTime: Integer;
    SingleCommandsDone: Boolean;
    Pos: Integer;
    Buf: array [0..AS_NETCONTROLREPLAYLENGTH] of Char;
    procedure OpenSock;
    procedure CloseSock;
    function ReadSock: Boolean;
    procedure SendToSock(s: String);
    procedure ProcessIncomingData;
    procedure PostPeriodicCommands;
  protected
    procedure Execute; override;
  public
    constructor Create(IPAddr: String; Port: Word; Login: String);
  end;

implementation

uses
  MainForm;

type
  EControlThreadInLoop = class(Exception);

constructor TControlThread.Create(IPAddr: String; Port: Word; Login: String);
begin
  Self.IPAddr := IPAddr;
  Self.Port := Port;
  Self.Login := Login;
  Sock := INVALID_SOCKET;
  inherited Create(False);
end;

procedure TControlThread.OpenSock;
var Addr: TSockAddrIn;
    phe: PHostEnt;
    i: Integer;
begin
  if Sock <> INVALID_SOCKET then Exit;
  StatusTime := 0;
  SingleCommandsDone := False;
  Idle := False;
  Sock := socket(PF_INET, SOCK_STREAM, 0);
  if Sock = INVALID_SOCKET then
    raise Exception.CreateFmt('Не удаётся заказать сокет, ошибка WinSock: %d (%s)', [WSAGetLastError, SysErrorMessage(WSAGetLastError)]);
  Addr.sin_family := AF_INET;
  Addr.sin_port := htons(Port);
  Addr.sin_addr.s_addr := inet_addr(PChar(IPAddr));
  if DWORD(Addr.sin_addr.s_addr) = INADDR_NONE then
    begin
    phe := gethostbyname(PChar(IPAddr));
    if phe = nil then
      raise Exception.CreateFmt('Неверный адрес "%s", ошибка WinSock: %d (%s)', [IPAddr, WSAGetLastError, SysErrorMessage(WSAGetLastError)]);
    Addr.sin_addr.s_addr := (PInteger((phe^.h_addr_list)^))^;
    end;
  if connect(Sock, Addr, SizeOf(TSockAddrIn)) = SOCKET_ERROR then
    raise EControlThreadInLoop.CreateFmt('Не удаётся подключиться к "%s", ошибка WinSock: %d (%s)', [IPAddr, WSAGetLastError, SysErrorMessage(WSAGetLastError)]);
  i := 1;
  if ioctlsocket(Sock, FIONBIO, i) = SOCKET_ERROR then
    raise Exception.CreateFmt('Не удаётся установить сокет в неблокирующее состояние, ошибка WinSock: %d (%s)', [WSAGetLastError, SysErrorMessage(WSAGetLastError)]);
  fMain.ConnectionEstablished := True;
  StrCopy(fMain.ConnStatus, PChar(IPAddr + ':' + IntToStr(Port)));
  Synchronize(fMain.StatusChanged);

  SendToSock(Login + #$0D#$0A'CHANNELINFO START'#$0D#$0A);
end;

procedure TControlThread.CloseSock;
begin
  if Sock = INVALID_SOCKET then Exit;
  closesocket(Sock);
  Sock := INVALID_SOCKET;
//  fMain.ConnStatus := 'Отсоединён';
//  Synchronize(fMain.StatusChanged);
//  raise Exception.CreateFmt('Не удаётся закрыть сокет, ошибка WinSock: %d (%s)', [WSAGetLastError, SysErrorMessage(WSAGetLastError)]);
end;

function TControlThread.ReadSock: Boolean;
var j: Integer;
begin
  Result := False;
  while True do
    begin
    j := recv(Sock, Buf[Pos], 1, 0);
    if (j <> SOCKET_ERROR) then
      begin
      if j = 0 then Exit;
      Idle := False;
      Inc(Pos);
      if Pos < 2 then Continue;
      if Pos >= AS_NETCONTROLREPLAYLENGTH then Break;
      if (Buf[Pos-2] <> #$0D) or (Buf[Pos-1] <> #$0A) then Continue;
      Dec(Pos, 2);
      Break;
      end;
//    if WSAGetLastError = NO_ERROR then   // Непонятная ситуация, но так случается
//      raise EControlThreadInLoop.Create('Не удаётся прочитать из сокета - сервер закрыл соединение');
    if WSAGetLastError <> WSAEWOULDBLOCK then
      raise EControlThreadInLoop.CreateFmt('Не удаётся прочитать из сокета, ошибка WinSock: %d (%s)', [WSAGetLastError, SysErrorMessage(WSAGetLastError)]);
    Exit;
    end;
  Buf[Pos] := #0;
  Pos := 0;
  Result := True;
end;

procedure TControlThread.SendToSock(s: String);
begin
  if send(Sock, PChar(s)[0], Length(s), 0) = SOCKET_ERROR then
    raise EControlThreadInLoop.CreateFmt('Не удаётся отправить данные в сокет, ошибка WinSock: %d (%s)', [WSAGetLastError, SysErrorMessage(WSAGetLastError)]);
end;

procedure TControlThread.ProcessIncomingData;
var Info: TStringList;
    i, j, k: Integer;
begin
  if StrLIComp(Buf, 'STATUS: ', 8) = 0 then
    begin
    StrCopy(fMain.CommonStatus, @Buf[8]);
    StrCopy(fMain.CommonStatus, PChar(StringReplace(String(fMain.CommonStatus), 'ControlSessions=', 'Управляющих соединений: ', [rfReplaceAll, rfIgnoreCase])));
    Synchronize(fMain.StatusChanged);
    end
  else if StrLIComp(Buf, 'CHANNELINFO: ', 13) = 0 then
    begin
    Info := TStringList.Create;
    try
      Info.CommaText := String(@Buf[13]);
      SetLength(fMain.ThreadChannels, Info.Count);
      for i := 0 to Info.Count-1 do
        begin
        j := System.Pos('=', Info[i]);
        if j = 0 then raise Exception.CreateFmt('Неверная строка ответа в команде CHANNELINFO (%s)', [Info[i]]);
        fMain.ThreadChannels[i].Id := StrToInt(Copy(Info[i], 1, j-1));
        fMain.ThreadChannels[i].Name := Copy(Info[i], j+1, MaxInt);
        end;
    finally
      Info.Free;
    end;
    Synchronize(fMain.ChannelsChanged);
    end
  else if StrLIComp(Buf, 'INPUTLINEINFO: ', 15) = 0 then
    begin
    Info := TStringList.Create;
    try
      Info.CommaText := String(@Buf[15]);
      SetLength(fMain.ThreadInputs, Info.Count);
      for i := 0 to Info.Count-1 do
        begin
        j := System.Pos('=', Info[i]);
        if j = 0 then raise Exception.CreateFmt('Неверная строка ответа в команде INPUTLINEINFO (%s)', [Info[i]]);
        fMain.ThreadInputs[i].Id := StrToInt(Copy(Info[i], 1, j-1));
        fMain.ThreadInputs[i].Name := Copy(Info[i], j+1, MaxInt);
        end;
    finally
      Info.Free;
    end;
    Synchronize(fMain.InputsChanged);
    end
  else if StrLIComp(Buf, 'CHANNELINFO DATA: ', 18) = 0 then
    begin
    Info := TStringList.Create;
    try
      Info.CommaText := String(@Buf[18]);
      for i := 0 to Info.Count-1 do
        begin
        j := System.Pos('=', Info[i]);
        if j = 0 then raise Exception.CreateFmt('Неверная строка ответа в команде CHANNELINFO DATA (%s)', [Info[i]]);
        k := 0;
        while k < Length(fMain.ThreadChannels) do
          begin
          if fMain.ThreadChannels[k].Id = StrToInt(Copy(Info[i], 1, j-1)) then
            begin
            fMain.ThreadChannels[k].PlayingInfo := Copy(Info[i], j+1, MaxInt);
            Break;
            end;
          Inc(k);
          end;
        if k >= Length(fMain.ThreadChannels) then raise Exception.CreateFmt('Неверный идентификатор (%d) в строке ответа команды CHANNELINFO DATA', [StrToInt(Copy(Info[i], 1, j-1))]);
        end;
    finally
      Info.Free;
    end;
    Synchronize(fMain.ChannelsChanged);
    end
  else if fMain.NetReplayWaitPosted and (StrLIComp(Buf, fMain.NetReplay, StrLen(fMain.NetReplay)) = 0) then
    begin
    StrCopy(fMain.NetReplay, Buf);
    fMain.NetReplayWaitPosted := False;
    end;
end;

procedure TControlThread.PostPeriodicCommands;
begin
  if not SingleCommandsDone then
    begin
    SingleCommandsDone := True;
    SendToSock('CHANNELINFO'#$0D#$0A'INPUTLINEINFO'#$0D#$0A'CHANNELINFO START'#$0D#$0A);
    end;
  Inc(StatusTime);
  if StatusTime >= 200 then
    begin
    StatusTime := 0;
    Idle := False;
    SendToSock('STATUS'#$0D#$0A);
    end;
  if fMain.NetCommandPosted then
    begin
    fMain.NetCommandPosted := False;
    SendToSock(String(fMain.NetCommand));
    end;
end;

procedure TControlThread.Execute;
var Data: WSAData;
begin
  try
    WSAStartup(MAKEWORD(1, 1), Data);
    try
      while not Terminated do
        try
          Idle := True;
          OpenSock;
          if ReadSock then
            ProcessIncomingData;
          PostPeriodicCommands;
          if Idle then
            Sleep(10);
        except
          on E: EControlThreadInLoop do
            begin
            fMain.ConnectionEstablished := False;
            fMain.ThreadChannels := nil;
            Synchronize(fMain.ChannelsChanged);
            fMain.ThreadInputs := nil;
            Synchronize(fMain.InputsChanged);
            CloseSock;
            StrCopy(fMain.ConnStatus, PChar(E.Message));
            StrCopy(fMain.CommonStatus, '');
            Synchronize(fMain.StatusChanged);
            Sleep(200);
            end;
        end;
      if Sock <> INVALID_SOCKET then SendToSock('BYE'#$0D#$0A);
    finally
      fMain.ConnectionEstablished := False;
      fMain.ThreadChannels := nil;
      fMain.ThreadInputs := nil;
      CloseSock;
      WSACleanup;
    end;
  except
    Application.HandleException(Self);
    Synchronize(fMain.Close);
  end;
end;

end.
