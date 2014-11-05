namespace FlickerBox.Directory
{
    public interface IHandShakeManager
    {
        string ExchangeIdentity(string publicId, string passphrase);
    }
}
