using System;
using System.Collections.Generic;
using FlickerBox.Communication;
using FlickerBox.Directory;
using FlickerBox.Events;
using FlickerBox.Identity;
using FlickerBox.Messages;
using NLog;

namespace FlickerBox.ClientInteraction
{
    public class ClientCommandHandler : IClientCommandHandler
    {
        private readonly Logger log = LogManager.GetCurrentClassLogger();
        private readonly IFriendDirectory friendDirectory;
        private readonly IMessagesManager messagesManager;
        public ClientCommandHandler(IIdentityManager identityManager, IChannelFactory channelFactory)
        {
            string publicId = identityManager.PublicId;
            friendDirectory = new FriendDirectory(publicId, channelFactory);
            friendDirectory.OnDiscoverResult += (sender, friend) => this.OnFriendToSend.RaiseEvent(sender, friend);

            messagesManager = new MessagesManager(friendDirectory, channelFactory, publicId);
            messagesManager.OnAcknowledged += (sender, ack) => this.OnAcknowledged.RaiseEvent(sender, ack);
            messagesManager.OnReceived += (sender, message) => this.OnReceived.RaiseEvent(sender, message);
        }

        public event EventHandler<Friend> OnFriendToSend;

        public void ResendAllFriends()
        {
            List<Friend> friends = friendDirectory.GetAll();
            if (friends != null)
            {
                log.Debug("friendDirectory.GetAll() did return {0} results", friends.Count);
                foreach (var friend in friends)
                {
                    this.OnFriendToSend.RaiseEvent(this, friend);
                }
            }
            else
            {
                log.Warn("friendDirectory.GetAll() did return null!");
            }
        }

        public void Discover(FriendRequest request)
        {
            friendDirectory.Discover(request);
        }

        /// <summary>
        /// This method seems not that usefull now as there is no interesing information appart from the name.
        /// Perhaps it will make more sense for futur dev? (as adding the information CreationTime?)
        /// </summary>
        /// <param name="friendName"></param>
        /// <returns></returns>
        public Friend Get(string friendName)
        {
            return friendDirectory.Get(friendName);
        }

        public void Send(Message message)
        {
            try
            {
                this.messagesManager.Send(message);
            }
            catch (Exception e)
            {
                log.Error("Error while trying to send new message :" + e.Message);
                this.OnAcknowledged.RaiseEvent(this, new Ack() { Id = message.Id, State = AckStates.Error.ToString() });
            }
        }

        public void AcknowledgeRead(Ack ack)
        {
            this.messagesManager.AcknowledgeRead(ack);
        }

        public void ResendMessages(DateTime from)
        {
            this.messagesManager.Resend(from);
        }

        public event EventHandler<Message> OnReceived;
        public event EventHandler<Ack> OnAcknowledged;
    }
}
