using System;

namespace FlickerBox.Communication
{
    public interface IChannel
    {
        string Subject { get; }
        bool SendMessage(string message);
        event EventHandler<string> OnMessageReceived;
    }
}
