using FlickerBox.Communication;

namespace UnitTests
{
    public class FakeChannelFactory : IChannelFactory
    {
        public IChannel ToReturn { get; set; }
        public IChannel GetNew(string subject)
        {
            return ToReturn ?? new FakeChannel(subject);
        }
    }
}
