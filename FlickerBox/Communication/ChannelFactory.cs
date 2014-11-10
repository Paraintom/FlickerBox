using System;
using FlickerBox.Configuration;
using NLog;

namespace FlickerBox.Communication
{
    class ChannelFactory : IChannelFactory
    {
        Logger logger = LogManager.GetCurrentClassLogger();
        public IChannel GetNew(string subject)
        {
            string url = MyConfiguration.GetString(ConfigurationKeys.FastFlickerUrl.ToString(), "ws://localhost:8099/");
            logger.Info(String.Format("Url found in configuration {0}", url));
            return new FastFlickerClient(url, subject);
        }
    }
}
