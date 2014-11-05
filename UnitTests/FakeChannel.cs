using System;
using System.Collections.Generic;
using System.Linq;
using FlickerBox.Communication;
using FlickerBox.Events;
using NLog;

namespace UnitTests
{
    class FakeChannel : IChannel
    {
        private static Dictionary<FakeChannel, string> connected = new Dictionary<FakeChannel, string>();
        private Logger log = LogManager.GetCurrentClassLogger();

        public FakeChannel(string subject)
        {
            Subject = subject;
            connected.Add(this, subject);
            Id = connected.Count.ToString();
            log.Info(String.Format("New Channel created with Id : {0}", Id));
        }

        public void MessageReceived(string m)
        {
            log.Info(String.Format("{0} Received : {1}",Id,m));
            this.OnMessageReceived.RaiseEvent(this,m);
        }

        public string Id { get; private set; }
        public string Subject { get; private set; }
        public bool SendMessage(string message)
        {
            connected.Where(o => o.Key != this && o.Value == Subject).Select(o=>o.Key).ToList().ForEach(c => c.MessageReceived(message));
            return true;
        }

        public event EventHandler<string> OnMessageReceived;
    }
}
