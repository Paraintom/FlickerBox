using System;
using FlickerBox.Messages;

namespace FlickerBox.Communication
{
    public interface IMessagesManager
    {
        void Send(Message m);
        void AcknowledgeRead(Ack ack);
        event EventHandler<Message> OnReceived;
        event EventHandler<Ack> OnAcknowledged;
    }
}
