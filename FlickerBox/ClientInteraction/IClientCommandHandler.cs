using System;
using FlickerBox.Directory;
using FlickerBox.Messages;

namespace FlickerBox.ClientInteraction
{
    public interface IClientCommandHandler
    {
        void ResendMessages(DateTime dateTime);
        void AcknowledgeRead(Ack ack);
        void Send(Message message);
        event EventHandler<Message> OnReceived;
        event EventHandler<Ack> OnAcknowledged;

        void Discover(FriendRequest request);
        void ResendAllFriends();
        event EventHandler<Friend> OnFriendToSend;
    }
}
