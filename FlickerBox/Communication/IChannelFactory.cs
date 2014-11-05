namespace FlickerBox.Communication
{
    public interface IChannelFactory
    {
        IChannel GetNew(string subject);
    }
}
