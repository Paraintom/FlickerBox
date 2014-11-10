namespace FlickerBox.Messages
{
    public class FriendRequest
    {
        public FriendRequest()
        {
            this.Type = this.GetType().Name;
        }
        public string Type { get; set; }
        public string Name { get; set; }
        public string Passphrase { get; set; }
    }
}
