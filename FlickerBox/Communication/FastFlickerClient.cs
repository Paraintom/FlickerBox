using System;
using System.Threading;
using FlickerBox.Events;
using NLog;
using SuperSocket.ClientEngine;
using WebSocket4Net;

namespace FlickerBox.Communication
{
    public class FastFlickerClient : IChannel
    {
        private Logger log = LogManager.GetCurrentClassLogger();
        private readonly WebSocket websocket;
        private readonly AutoResetEvent resetEvent;
        private readonly object internalLock = new object();
        public FastFlickerClient(string url, string subject)
        {
            log.Debug("In constructor");
            resetEvent = new AutoResetEvent(false);
            Subject = subject;
            websocket = new WebSocket(url);
            websocket.Opened += websocket_Opened;
            websocket.Error += websocket_Error;
            websocket.Closed += websocket_Closed;
            websocket.MessageReceived += websocket_MessageReceived;
            var success = TryToConnect();
            if (success)
            {
                string errorString = "Cannot connect to FastFlicker (with url {0})".FormatWith(url);
                log.Error(errorString);
                throw new ApplicationException(errorString);
            }
        }

        private bool TryToConnect()
        {
            bool success = false;
            log.Warn("Trying to connect to subject {0}, currentState {1}", this.Subject, websocket.State);
            try
            {

                if (websocket.State != WebSocketState.Connecting)
                {
                    websocket.Open();
                }
                else
                {
                    log.Info("We are connecting, closing then opening again...");
                    websocket.Close();
                    websocket.Open();
                }
                success = !resetEvent.WaitOne(TimeSpan.FromSeconds(2));

            }
            catch (Exception ex)
            {
                log.Warn("unable to connect to webSocket server : " + ex);
            }
            return success;
        }

        private int numberMessageReceived;
        private void websocket_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            if (log.IsDebugEnabled)
                log.Debug(String.Format("Message received : {0}", e.Message));
            //The first message is an echo of the subject, we can safely ignore it...
            if (numberMessageReceived != 0)
            {
                OnMessageReceived.RaiseEvent(this, e.Message);
            }
            numberMessageReceived++;
        }

        private bool reconnecting = false;
        private void websocket_Closed(object sender, EventArgs e)
        {
            lock (internalLock)
            {
                if (reconnecting)
                {
                    log.Info("The socket has been closed for subject {0}, current state {1}, already reconnecting ...", this.Subject, websocket.State);
                    return;
                }
                reconnecting = true;
            }
            log.Warn("The socket has been closed for subject {0}, current state {1}, trying to reconnect ...", this.Subject, websocket.State);
            int tryNumber = 0;
            while (websocket.State != WebSocketState.Open)
            {
                
                    tryNumber++;
                TimeSpan waitPeriod = GetWaitPeriod(tryNumber);
                if (!TryToConnect())
                {
                    log.Warn(string.Format("Connection failed, trying again in  {0} s", waitPeriod.TotalSeconds));
                    Thread.Sleep(waitPeriod);
                }
            }
            reconnecting = false;
        }

        private static TimeSpan GetWaitPeriod(int tryNumber)
        {
            switch (tryNumber)
            {
                case 0:
                case 1:
                    return TimeSpan.FromSeconds(2);
                case 2:
                    return TimeSpan.FromSeconds(5);
                case 3:
                    return TimeSpan.FromSeconds(10);
                case 4:
                case 5:
                    return TimeSpan.FromSeconds(30);
                case 6:
                    return TimeSpan.FromSeconds(60);
                case 7:
                case 8:
                case 9:
                    return TimeSpan.FromSeconds(60 * 5);
                default:
                    return TimeSpan.FromSeconds(60 * 20);
            }
        }

        private void websocket_Error(object sender, ErrorEventArgs e)
        {
            log.Error(String.Format("Error with websocket : {0}", e.Exception.Message));
        }

        private void websocket_Opened(object sender, EventArgs e)
        {
            log.Info(String.Format("The socket has been opened for subject {0}.", this.Subject));
            websocket.Send(Subject);
            numberMessageReceived = 0;
            resetEvent.Set();
        }

        public event EventHandler<string> OnMessageReceived;
        public string Subject
        {
            get;
            private set;
        }

        public bool SendMessage(string message)
        {
            bool messageSent = false;
            //A little useless as if we are not connected, we will not try to send a message...
            if (websocket.State == WebSocketState.Open)
            {
                websocket.Send(message);
                messageSent = true;
            }
            return messageSent;
        }
    }
}
