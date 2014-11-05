namespace FlickerBox.Identity
{
    public interface IIdentityManager
    {
        string PublicId { get; }
        string PrivateId { get; }
    }
}
