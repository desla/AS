using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Timers;
using Alvasoft.AudioServer.Communication.CommandParsing;
using Alvasoft.Tcp;
using log4net;
using Timer = System.Timers.Timer;

namespace Alvasoft.AudioServer.Communication
{
    /// <summary>
    /// Сессия управления.
    /// </summary>
    public class ControlSession : AbstractClientSession, ClientConnectionCallback
    {
        private static readonly ILog Logger = LogManager.GetLogger("ControllSession");

        public enum State
        {
            JUST_CREATED = 0,
            LOGIN_PROMPTED = 1,
            LOGGED_IN = 2
        };

        private string login;

        private ControlSessionCallback callback;
        private Timer channelInfoTimer;
        private CommandParser commandParser = new CommandParser();
        private LineReader lineReader = new LineReader();
        //private Mutex receiveLock = new Mutex();
        private object receiveLock = new object();
        private bool started;
        private State state;

        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="aClientConnection">Клиентское подключение.</param>
        public ControlSession(ClientConnection aClientConnection)
            : base(aClientConnection)
        {            
            state = State.JUST_CREATED;
            callback = null;
            GetClientConnection().SetCallback(this);

            // Формируемый ответ.
            String reply = "Login:";
            SendReply(reply);
            state = State.LOGIN_PROMPTED;

            channelInfoTimer = new Timer();
            channelInfoTimer.Interval = 500;
            channelInfoTimer.Elapsed += ChannelInfoTimeEvent;

            channelInfoTimer.Start();
        }

        /// <inheritdoc />
        public void OnReceivedFrom(ClientConnection aClientConnection, byte[] aBuffer, int aBufferSize)
        {            
            lock (receiveLock) {                
                var buffer = Encoding.UTF8.GetString(aBuffer, 0, aBufferSize);
                lineReader.AddBuffer(buffer);
                // Извлекаем текст команды управления.                                
                var line = string.Empty;
                while ((line = lineReader.ReadLine()) != null) {
                    if (state == State.LOGIN_PROMPTED) {
                        // Проверка пароля.
                        try {
                            var password = line;
                            if (login.Equals(password)) {
                                Logger.Debug("Login is OK.");
                                SendReply("\r\nLogin is OK, greetings from Audio Server 3.0\r\n");
                                state = State.LOGGED_IN;
                            }
                            else {
                                Logger.Debug("Login failed.");
                                SendReply("\r\nLogin is incorrect\r\n");
                            }
                        }
                        catch {
                        }
                    }
                    else {
                        var controlCommand = line;
                        ProcessControlCommand(controlCommand);
                    }
                }
            }            
        }

        /// <summary>
        /// Инициализирует интерфейс обратных вызовов.
        /// </summary>
        /// <param name="aCallback">Интерфейс обратных вызовов.</param>
        public void SetCallback(ControlSessionCallback aCallback)
        {
            callback = aCallback;
        }

        /// <summary>
        /// Устанавливает логин для клиентов.
        /// </summary>
        /// <param name="aLogin">Логин.</param>
        public void SetLogin(string aLogin)
        {
            login = aLogin;
        }

        /// <summary>
        /// Посылает ответ (результат выполнения команды).
        /// </summary>
        /// <param name="aCommandResult">Ответ.</param>
        private bool SendReply(string aReply)
        {
            var connection = GetClientConnection();            
            byte[] buffer = Encoding.UTF8.GetBytes(aReply);
            if (connection.SendAsync(buffer, buffer.Length)) {
                Logger.Debug("Reply: \"" + aReply + "\"");
                return true;
            }
            else {
                Logger.Info("Reply: Failed");
                return false;
            }
        }

        /// <summary>
        /// Выполнение команды управления.
        /// </summary>
        /// <param name="aCommand">Команда.</param>
        private void ProcessControlCommand(string aCommand)
        {
            Logger.Debug("\n\nПолучена команда управления: " + aCommand);

            // Инициалируем парсер командой.
            commandParser.SetCommand(aCommand);

            // Формируемый ответ.
            String reply;

            // Начинаем разбор команды...
            String word0 = commandParser.GetWord(0);

            if ((word0 == "BYE") || (word0 == "QUIT") || (word0 == "EXIT")) {
                Logger.Debug("Command: EXIT");

                channelInfoTimer.Stop();
                channelInfoTimer.Dispose();

                if (callback != null) {
                    callback.OnCloseConnection(this);
                }
            }

            if (word0 == "STATUS") {
                Logger.Debug("Command: STATUS");

                int sessionsCount = 0;
                if (callback != null) {
                    callback.OnQueryStatus(this, out sessionsCount);
                }

                reply = "STATUS: ControlSessions=" + sessionsCount + "\r\n";
                SendReply(reply);
            }

            if (word0 == "CHANNELINFO") {
                String word1 = commandParser.GetWord(1);
                if (word1 == null) {
                    Logger.Debug("Command: CHANNELINFO");

                    reply = "CHANNELINFO: ";

                    if (callback != null) {
                        int[] channelIds = null;
                        string[] channelNames = null;
                        callback.OnChannelInfo(this, out channelIds, out channelNames);
                        for (int i = 0; i < channelIds.Length; i++) {
                            reply += String.Format("\"{0}={1}\" ", channelIds[i], channelNames[i]);
                        }
                    }

                    reply += "\r\n";
                    SendReply(reply);
                }
                if (word1 == "START") {
                    Logger.Debug("Command: CHANNELINFO START");

                    started = true;

                    reply = "CHANNELINFO START: OK\r\n";
                    SendReply(reply);
                }
                if (word1 == "STOP") {
                    Logger.Debug("Command: CHANNELINFO STOP");

                    started = false;

                    reply = "CHANNELINFO STOP: OK\r\n";
                    SendReply(reply);
                }
            }

            if (word0 == "INPUTLINEINFO") {
                Logger.Debug("Command: INPUTLINEINFO");

                reply = "INPUTLINEINFO: ";

                if (callback != null) {
                    int[] inputLineIds = null;
                    string[] inputLineNames = null;
                    callback.OnInputLine(this, out inputLineIds, out inputLineNames);
                    for (int i = 0; i < inputLineIds.Length; i++) {
                        reply += String.Format("\"{0}={1}\" ", inputLineIds[i], inputLineNames[i]);
                    }
                }

                reply += "\r\n";
                SendReply(reply);
            }

            if (word0 == "TRANSMIT") {
                Logger.Debug("Command: TRANSMIT");
                String word1 = commandParser.GetWord(1);
                String word2 = commandParser.GetWord(2);
                String word3 = commandParser.GetWord(3);
                String word4 = commandParser.GetWord(4);
                String word5 = commandParser.GetWord(5);
                if ((word1 != "FROM") || (word2 != "INPUTLINE") || (word4 != "TO") || (word5 != "CHANNEL")) {
                    reply = "Syntax Error\r\n";
                    SendReply(reply);
                    return;
                }

                var inputLineId = Int32.Parse(word3);
                var channelsIds = new List<int>();
                Logger.Debug("InputLineId = " + inputLineId);
                String word6, word7;
                var index = 0;
                do {
                    word6 = commandParser.GetWord(6 + index);
                    word7 = commandParser.GetWord(7 + index);
                    int channelId = Int32.Parse(word6);
                    Logger.Debug("ChannelId = " + channelId);
                    channelsIds.Add(channelId);
                    if ((word7 == null) || (word7 != ",")) {
                        break;
                    }
                    index += 2;
                } while (word6 != null);                

                reply = "TRANSMIT: OK\r\n";
                SendReply(reply);

                if (callback != null) {
                    callback.OnStartRecordSound(this, inputLineId, channelsIds.ToArray());
                }
            }

            if (word0 == "END") {
                Logger.Debug("Command: END");
                String word1 = commandParser.GetWord(1);
                if (word1 != "TRANSMIT") {
                    reply = "Syntax Error\r\n";
                    SendReply(reply);
                    return;
                }                

                reply = "END TRANSMIT: OK\r\n";
                SendReply(reply);

                if (callback != null) {
                    callback.OnStopRecordSound(this);
                }
            }

            if (word0 == "SHUTDOWN") {
                Logger.Debug("Command: SHUTDOWN");

                if (callback != null) {
                    callback.OnShutdown(this);
                }

                reply = "SHUTDOWN: OK\r\n";
                SendReply(reply);
            }
        }

        /// <summary>
        /// Посылает данные по каналам клиентам по таймеру.
        /// </summary>
        /// <param name="aSource"></param>
        /// <param name="aArgs"></param>
        private void ChannelInfoTimeEvent(object aSource, ElapsedEventArgs aArgs)
        {
            if (!started) {
                return;
            }            

            String reply;
            reply = "CHANNELINFO DATA: ";

            if (callback != null) {
                int[] channelIds = null;
                int[] channelLevels = null;
                callback.OnChannelData(this, out channelIds, out channelLevels);
                for (int i = 0; i < channelIds.Length; i++) {
                    reply += String.Format("\"{0}={1}\" ", channelIds[i], channelLevels[i]);
                }
            }

            reply += "\r\n";
            if (!SendReply(reply)) {
                started = false;
                channelInfoTimer.Stop();
            }
        }        
    }
}