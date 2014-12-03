using System;
using System.IO;
using System.Net;
using FlickerBox.Configuration;
using NLog;

namespace FlickerBox.Communication
{
    class ChannelFactory : IChannelFactory
    {
        Logger logger = LogManager.GetCurrentClassLogger();
        public IChannel GetNew(string subject)
        {
            string url = string.Empty;
            string configuredUrl = MyConfiguration.GetString(ConfigurationKeys.FastFlickerUrl.ToString(), "ws://localhost:8099/");
            if (configuredUrl.StartsWith("http://"))
            {
                //Dynamic retrieval!
                string urlRequest = string.Format("{0}?get=FastFlicker", configuredUrl);
                var result = WebRequest.Create(urlRequest).GetResponse().GetResponseStream();
                StreamReader stream = new StreamReader(result);
                String ContenuPageWeb = stream.ReadToEnd();
                logger.Info(string.Format("Response : {0}", ContenuPageWeb));
                url = String.Format("ws://{0}/", ContenuPageWeb.Trim());

                logger.Info(String.Format("Dynamic Url found: {0}", url));
            }
            else
            {
                if (configuredUrl.StartsWith("ws://"))
                {
                    url = configuredUrl;
                    logger.Info(String.Format("Url found in configuration {0}", url));
                }
                else
                {
                    throw new ApplicationException(string.Format("Invalid FastFlicker Url : {0}"));
                }
            }
            /**/
            return new FastFlickerClient(url, subject);
        }
    }
}
