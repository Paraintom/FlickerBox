using System;
using System.Collections.Generic;
using FlickerBox.Directory;
using FlickerBox.Messages;

namespace FlickerBox.ClientInteraction
{
    public interface ICommandListener
    {
        //Friends Management
        event EventHandler<GetAllFriendRequest> OnGetAllFriendsReceived;
        event EventHandler<FriendRequest> OnFriendRequestReceived;
        void SendFriends(List<Friend> all);

        //Messages Management
        event EventHandler<GetAllMessagesRequest> OnGetAllMessagesFromReceived;
        event EventHandler<Message> OnMessageToSendReceived;
        event EventHandler<Ack> OnFlagMessageRead;
        void SendMessages(List<Message> messages);
        void SendStateChanged(Ack newState);
    }
}
