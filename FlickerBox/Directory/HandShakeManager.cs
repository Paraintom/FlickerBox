using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using FlickerBox.Communication;
using FlickerBox.Encryption;
using NLog;

namespace FlickerBox.Directory
{
    public class HandShakeManager : IHandShakeManager
    {
        private readonly Logger log = LogManager.GetCurrentClassLogger();
        private readonly IChannelFactory channelFactory;
        private readonly object internalLock = new object();
        private string result;
        private IChannel channel;

        public HandShakeManager(IChannelFactory channelFactory)
        {
            this.channelFactory = channelFactory;
        }

        public string ExchangeIdentity(string publicId, string passphrase)
        {
            if (log.IsDebugEnabled)
                log.Debug("ExchangeId started, Id {0}, passphrase {1} ", publicId, passphrase);

            UpdateChannel(passphrase);
            Task broadCastIdInBackGround = new Task(() => BroadCastId(publicId, passphrase));
            broadCastIdInBackGround.Start();


            int maxTry = 12;
            int numberOfTry = 0;

            bool resultFound = false;
            while (!resultFound && numberOfTry < maxTry)
            {
                numberOfTry++;
                Thread.Sleep(2500);
                UpdateChannel(passphrase);

                lock (internalLock)
                {
                    resultFound = !String.IsNullOrEmpty(result);
                    if (resultFound)
                    {
                        log.Info("Result found : {0}", result);
                    }
                }
            }
            return result;
        }

        private void UpdateChannel(string passphrase)
        {
            var currentSubject = GetSubject(passphrase);
            if (channel == null || channel.Subject != currentSubject)
            {
                IChannel newChannel = channelFactory.GetNew(currentSubject);
                newChannel.OnMessageReceived += (sender, otherIdentity) =>
                {
                    if (String.IsNullOrEmpty(result))
                    {
                        lock (internalLock)
                        {
                            log.Info(String.Format("Received broadcasted message: {0} ", otherIdentity));
                            result = otherIdentity;
                        }
                    }
                };
                lock (internalLock)
                {
                    channel = newChannel;
                }
            }
        }

        private void BroadCastId(string id, string passphrase)
        {
            if (log.IsDebugEnabled)
                log.Debug("Broadcasting our identity with passphrase : " + passphrase);

            int maxTry = 6;
            int numberOfTry = 0;

            while (numberOfTry < maxTry)
            {
                channel.SendMessage(id);
                log.Info(String.Format("Broadcasted {0} ", id));
                numberOfTry++;
                Thread.Sleep(5000);
            }
        }

        private string GetSubject(string passphrase)
        {
            string salt = "DoYouHearMe?" + DateTime.UtcNow.Hour.ToString(CultureInfo.InvariantCulture) + DateTime.UtcNow.DayOfYear.ToString(CultureInfo.InvariantCulture);
            string toEncode = salt + passphrase;
            return toEncode.Encrypt();
        }
    }
}
