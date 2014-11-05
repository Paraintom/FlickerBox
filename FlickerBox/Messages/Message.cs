namespace FlickerBox.Messages
{
    public class Message
    {
        public Message()
        {
            this.Type = this.GetType().Name;
        }
        public string Type { get; set; }
        public string ToFriendName { get; set; }
        public string FromPublicId { get; set; }
        public string Id { get; set; }
        public string UtcCreationTime { get; set; }
        public string Content { get; set; }
    }
}
