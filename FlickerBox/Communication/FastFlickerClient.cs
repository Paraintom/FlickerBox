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
                throw new ApplicationException("Cannot connect to FastFlicker!");
            }
        }

        private bool TryToConnect()
        {
            bool success;
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

        private void websocket_Closed(object sender, EventArgs e)
        {
            log.Warn("The socket has been closed, trying to reconnect ...");
            while (websocket.State != WebSocketState.Open)
            {
                try
                {
                    log.Warn("Trying to connect again ... ");
                    TryToConnect();
                }
                catch (Exception ex)
                {
                    log.Warn("unable to connect to webSocket server : " + ex);
                }
                Thread.Sleep(30000);
            }
        }

        private void websocket_Error(object sender, ErrorEventArgs e)
        {
            log.Error("Error whith websocket : ", e.Exception);
        }

        private void websocket_Opened(object sender, EventArgs e)
        {
            resetEvent.Set();
            numberMessageReceived = 0;
            websocket.Send(Subject);
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
