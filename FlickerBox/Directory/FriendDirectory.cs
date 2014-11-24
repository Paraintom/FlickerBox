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
using NLog;

namespace FlickerBox.Directory
{
    public class FriendDirectory : IFriendDirectory
    {
        private readonly Logger log = LogManager.GetCurrentClassLogger();
        private readonly IChannelFactory channelFactory;
        private readonly string publicId;
        private readonly object internalLock = new object();
        readonly Dictionary<string, Friend> allFriends;

        public FriendDirectory(string publicId, IChannelFactory channelFactory)
        {
            this.publicId = publicId;
            this.channelFactory = channelFactory;
            allFriends = LoadFriends();
        }

        public void Discover(FriendRequest request)
        {
            var name = request.Name;
            var passphrase = request.Passphrase;
            log.Info("Discover Query received for {0}", name);
            lock (internalLock)
            {
                if (allFriends.ContainsKey(name))
                {
                    throw new ApplicationException("You already added " + name);
                }
            }
            var discoverBackground = new Task(() => DiscoverSync(name, passphrase));
            discoverBackground.Start();
        }

        public event EventHandler<Friend> OnDiscoverResult;
        public List<Friend> GetAll()
        {
            lock (internalLock)
            {
                return allFriends.Values.ToList();
            }
        }

        public Friend Get(string friendName)
        {
            lock (internalLock)
            {
                if (allFriends.ContainsKey(friendName))
                {
                    return allFriends[friendName];
                }
                string problem = String.Format("No friend with name {0} has been found", friendName);
                log.Warn(problem);
                throw new ApplicationException(problem);
            }
        }

        public Friend GetFromPublicId(string fromPublicId)
        {
            lock (internalLock)
            {
                var result = allFriends.Select(o => o.Value).FirstOrDefault(o => o.PublicId == fromPublicId);
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
                lock (internalLock)
                {
                    if (!allFriends.ContainsKey(name))
                    {
                        newFriend.PublicId = friendPublicId;
                        log.Info("Adding new friend : {0}.", newFriend);
                        allFriends.Add(name, newFriend);
                        SaveFriends();
                    }
                    else
                    {
                        log.Warn(string.Format("We already registered {0}, ignoring result.", name));
                        newFriend = allFriends[name];
                    }
                }
            }
            else
            {
                log.Warn("Unable to discover friend's publicId!");
            }
            OnDiscoverResult.RaiseEvent(this, newFriend);
        }

        private void SaveFriends()
        {
            log.Info("Saving friends");
            var listFriends = allFriends.Values.ToList();
            var serializer = new XmlSerializer(listFriends.GetType());
            using (var writer = XmlWriter.Create("friends.xml"))
            {
                serializer.Serialize(writer, listFriends);
            }
        }

        private Dictionary<string, Friend> LoadFriends()
        {
            log.Info("Loading friends");
            var serializer = new XmlSerializer(typeof(List<Friend>));
            try
            {
                using (var reader = XmlReader.Create("friends.xml"))
                {
                    var result = (List<Friend>)serializer.Deserialize(reader);
                    return result.ToDictionary(o => o.Name, o => o);
                }


            }
            catch (FileNotFoundException)
            {
                log.Warn("File friends not found ...");
            }
            catch (Exception e)
            {
                log.Error("Error while loading friends : " + e);
            }

            return new Dictionary<string, Friend>();
        }

        /// <summary>
        /// Should be used for test only, erase all friends.
        /// </summary>
        public static void ResetAll()
        {
            File.Delete("friends.xml");
        }
    }
}
