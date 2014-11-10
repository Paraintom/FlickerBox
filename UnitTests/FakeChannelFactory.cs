using FlickerBox.Communication;
using NLog;

namespace UnitTests
{
    public class FakeChannelFactory : IChannelFactory
    {
        Logger logger = LogManager.GetCurrentClassLogger();
        public IChannel ToReturn { get; set; }
        public IChannel GetNew(string subject)
        {
            if (ToReturn != null)
            {
                logger.Warn("Using forced result toReturn");
                return ToReturn;
            }
            else
            {
                return new FakeChannel(subject);
            }
        }

        public void ResetAll()
        {
            this.ToReturn = null;
        }
    }
}
