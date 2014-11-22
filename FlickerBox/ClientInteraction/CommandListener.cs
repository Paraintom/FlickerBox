using System;
using System.Collections.Generic;
using FlickerBox.Communication;
using FlickerBox.Directory;
using FlickerBox.Events;
using FlickerBox.Identity;
using FlickerBox.Messages;
using Newtonsoft.Json;
using NLog;

namespace FlickerBox.ClientInteraction
{
    public class CommandListener : ICommandListener
    {
        private readonly Logger log = LogManager.GetCurrentClassLogger();
        private readonly IChannel clientChannel;
        private readonly string publicId;
        public CommandListener(IIdentityManager identity, IChannelFactory channelFactory)
        {
            clientChannel = channelFactory.GetNew(identity.PrivateId);
            clientChannel.OnMessageReceived+= OnCommandReceived;
            publicId = identity.PublicId;
        }

        private void OnCommandReceived(object sender, string commandString)
        {
            log.Info(String.Format("Message received : {0}",commandString));
            Type messageType;
            if (TryGetType(commandString, out messageType))
            {
                var command = JsonConvert.DeserializeObject(commandString, messageType);

                HandleCommand(command);
            }
            else
            {
                log.Warn("Message type not reconized, skipping message");
            }
        }

        private void HandleCommand(object command)
        {
            //Messages
            var getAllMessageCommand = command as GetAllMessagesCommand;
            if (getAllMessageCommand != null)
            {
                log.Info("New GetAllMessagesCommand received...");
                this.OnGetAllMessagesFromReceived.RaiseEvent(this, getAllMessageCommand);
                return;
            }
            var messageCommand = command as Message;
            if (messageCommand != null)
            {
                log.Info("New message received...");
                messageCommand.FromPublicId = this.publicId;
                this.OnMessageToSendReceived.RaiseEvent(this, messageCommand);
                return;
            }
            var ackCommand = command as Ack;
            if (ackCommand != null)
            {
                log.Info("New Ack received...");
                this.OnFlagMessageRead.RaiseEvent(this, ackCommand);
                return;
            }
            //Friends
            var friendRequestCommand = command as FriendRequest;
            if (friendRequestCommand != null)
            {
                log.Info("New friend request received...");
                this.OnFriendRequestReceived.RaiseEvent(this, friendRequestCommand);
                return;
            }
            var getAllFriendRequestCommand = command as GetAllFriendRequest;
            if (getAllFriendRequestCommand != null)
            {
                log.Info("New friend request received...");
                this.OnGetAllFriendsReceived.RaiseEvent(this, getAllFriendRequestCommand);
                return;
            }


            log.Warn("Handler not defined for type " + command.GetType() + ", Ignoring command");
        }

        private bool TryGetType(string commandString, out Type messageType)
        {
            var dico = JsonConvert.DeserializeObject<Dictionary<string, string>>(commandString);
            if (dico.ContainsKey(MessageConst.TypeKey))
            {
                var typeString = dico[MessageConst.TypeKey];
                //Hum not very elegant, we should perhaps find another way...
                messageType = Type.GetType("FlickerBox.Messages." + typeString); ;
                return true;
            }
            messageType = null;
            return false;
        }

        public event EventHandler<GetAllFriendRequest> OnGetAllFriendsReceived;
        public event EventHandler<FriendRequest> OnFriendRequestReceived;
        public void SendFriends(List<Friend> all)
        {
            var toSend = new FriendList() {Result = all};
            SendObject(toSend);
        }

        public event EventHandler<GetAllMessagesCommand> OnGetAllMessagesFromReceived;
        public event EventHandler<Message> OnMessageToSendReceived;
        public event EventHandler<Ack> OnFlagMessageRead;
        public void SendMessages(List<Message> messages)
        {
            SendObject(messages);
        }

        public void SendStateChanged(Ack newState)
        {
            SendObject(newState);
        }


        private void SendObject(object toSend)
        {
            string jsonString = JsonConvert.SerializeObject(toSend);
            this.clientChannel.SendMessage(jsonString);
        }
    }
}
