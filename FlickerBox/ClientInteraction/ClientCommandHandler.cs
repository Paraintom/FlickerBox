using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading.Tasks;
using FlickerBox.Communication;
using FlickerBox.Directory;
using FlickerBox.Events;
using FlickerBox.Identity;
using FlickerBox.Messages;

namespace FlickerBox.ClientInteraction
{
    public class ClientCommandHandler : IClientCommandHandler
    {
        private readonly IFriendDirectory friendDirectory;
        private readonly IMessagesManager messagesManager;
        public ClientCommandHandler(IIdentityManager identityManager, IChannelFactory channelFactory)
        {
            string publicId = identityManager.PublicId;
            friendDirectory = new FriendDirectory(publicId, channelFactory);
            friendDirectory.OnDiscoverResult += (sender, friend) => this.OnDiscoverResult.RaiseEvent(sender, friend);

            messagesManager = new MessagesManager(friendDirectory, channelFactory, publicId);
            messagesManager.OnAcknowledged += (sender, ack) => this.OnAcknowledged.RaiseEvent(sender, ack);
            messagesManager.OnReceived += (sender, message) => this.OnReceived.RaiseEvent(sender, message);
        }

        public void Discover(FriendRequest request)
        {
            friendDirectory.Discover(request);
        }

        public event EventHandler<Friend> OnDiscoverResult;

        public List<Friend> GetAll()
        {
            return friendDirectory.GetAll();
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
            this.messagesManager.Send(message);
        }

        public void AcknowledgeRead(Ack ack)
        {
            this.messagesManager.AcknowledgeRead(ack);
        }

        public event EventHandler<Message> OnReceived;
        public event EventHandler<Ack> OnAcknowledged;
        public List<Message> GetAllMessage(DateTime since)
        {
            //not implemented yet!
            return new List<Message>();
        }
    }
}
