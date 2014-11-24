using System;
using FlickerBox.Directory;
using FlickerBox.Events;
using FlickerBox.Messages;
using Newtonsoft.Json;
using NLog;

namespace FlickerBox.Communication
{

    /// <summary>
    /// Now This class send and received the messages in clear on publicId without encryption.
    /// In the future, we need to 
    ///      or encrypt the message with the passphrase and prefix with "senderPublicId:"
    ///      or Give a rendez-vous on another channel based on the passphrase one more time...
    /// </summary>
    public class MessagesManager : IMessagesManager
    {
        private readonly Logger log = LogManager.GetCurrentClassLogger();
        private readonly IChannelFactory channelFactory;
        private readonly IFriendDirectory friendDirectory;
        private readonly string publicId;
        public MessagesManager(IFriendDirectory friendDirectory, IChannelFactory channelFactory, string publicId)
        {
            this.friendDirectory = friendDirectory;
            this.channelFactory = channelFactory;
            this.publicId = publicId;
            IChannel publicChannel = channelFactory.GetNew(publicId);
            publicChannel.OnMessageReceived += OnMessageReceived;
        }

        private void OnMessageReceived(object sender, string s)
        {
            log.Info("string received from outside {0}", s);
            try
            {
                dynamic deserialized = JsonConvert.DeserializeObject(s);
                string type = deserialized.Type;
                if (type == typeof(Message).Name)
                {
                    var newMessage = JsonConvert.DeserializeObject<Message>(s);
                    newMessage.FromFriendName = this.friendDirectory.GetFromPublicId(newMessage.FromPublicId).Name;
                    OnReceived.RaiseEvent(this, newMessage);
                    Ack confirm = new Ack()
                    {
                        Id = newMessage.Id,
                        State = AckStates.Delivered.ToString()
                    };
                    //Send confirmation messageReceived
                    Send(confirm, newMessage.FromPublicId);
                }
                else
                {
                    if (type == typeof(Ack).Name)
                    {
                        var newAck = JsonConvert.DeserializeObject<Ack>(s);
                        OnAcknowledged.RaiseEvent(this, newAck);
                    }
                    else
                    {
                        log.Warn("Type not handled by protocole [{0}], ignoring message.",
                            type);
                    }
                }
            }
            catch (Exception e)
            {
                log.Error("error while parsing string :" + e);
            }
        }

        public void Send(Message message)
        {
            message.FromPublicId = publicId;
            //As soon as possible, we don't want to send the message in clear on the publicId
            //2 solutions : send it crypted with passphrase but how to know who sent it, witch passphrase using?
            // Else give a RDV to an other subject based on the passphrase...
            string subject = friendDirectory.Get(message.ToFriendName).PublicId;
            Send(message, subject);
        }

        public void AcknowledgeRead(Ack ack)
        {
            string subject = this.publicId;
            Send(ack, subject);
        }


        private void Send(Object toSend, string subject)
        {
            IChannel toSendTo = channelFactory.GetNew(subject);
            string jsonString = JsonConvert.SerializeObject(toSend);
            toSendTo.SendMessage(jsonString);
        }
        public event EventHandler<Message> OnReceived;

        public event EventHandler<Ack> OnAcknowledged;
    }
}
