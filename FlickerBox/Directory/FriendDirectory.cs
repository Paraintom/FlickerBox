using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using FlickerBox.Communication;
using FlickerBox.Events;
using FlickerBox.Messages;
using FlickerBox.Persistence;
using NLog;

namespace FlickerBox.Directory
{
    public class FriendDirectory : BasePersister<Friend>, IFriendDirectory
    {
        private readonly Logger log = LogManager.GetCurrentClassLogger();
        private readonly IChannelFactory channelFactory;
        private readonly string publicId;

        public FriendDirectory(string publicId, IChannelFactory channelFactory) 
            : base(o=>o.Name)
        {
            this.publicId = publicId;
            this.channelFactory = channelFactory;
        }

        public void Discover(FriendRequest request)
        {
            var name = request.Name;
            var passphrase = request.Passphrase;
            log.Info("Discover Query received for {0}", name);
            lock (InternalLock)
            {
                if (AllManaged.ContainsKey(name))
                {
                    throw new ApplicationException("You already added " + name);
                }
            }
            var discoverBackground = new Task(() => DiscoverSync(name, passphrase));
            discoverBackground.Start();
        }

        public event EventHandler<Friend> OnDiscoverResult;
        
        public Friend Get(string friendName)
        {
            lock (InternalLock)
            {
                if (AllManaged.ContainsKey(friendName))
                {
                    return AllManaged[friendName];
                }
                string problem = String.Format("No friend with name {0} has been found", friendName);
                log.Warn(problem);
                throw new ApplicationException(problem);
            }
        }

        public Friend GetFromPublicId(string fromPublicId)
        {
            lock (InternalLock)
            {
                var result = AllManaged.Select(o => o.Value).FirstOrDefault(o => o.PublicId == fromPublicId);
                if (result != null)
                {
                    return result;
                }
                string problem = String.Format("No friend with publicId {0} has been found", fromPublicId);
                log.Warn(problem);
                throw new ApplicationException(problem);
            }
        }

        public void DiscoverSync(string name, string passphrase)
        {
            var newFriend = new Friend { Name = name, Passphrase = passphrase };

            log.Info("Looking for {0} with passphrase [{1}].", name, passphrase);
            var handShakeManager = new HandShakeManager(channelFactory);
            var friendPublicId = handShakeManager.ExchangeIdentity(publicId, passphrase);

            log.Debug("PublicId found : {0}", friendPublicId);
            if (!string.IsNullOrEmpty(friendPublicId))
            {
                lock (InternalLock)
                {
                    if (!AllManaged.ContainsKey(name))
                    {
                        newFriend.PublicId = friendPublicId;
                        log.Info("Adding new friend : {0}.", newFriend);
                        Persist(newFriend);
                    }
                    else
                    {
                        log.Warn(string.Format("We already registered {0}, ignoring result.", name));
                        newFriend = AllManaged[name];
                    }
                }
                OnDiscoverResult.RaiseEvent(this, newFriend);
            }
            else
            {
                log.Warn("Unable to discover friend's publicId!");
            }
        }

    }
}
