namespace FlickerBox.Messages
{
    public class GetAllFriendRequest
    {
        public GetAllFriendRequest()
        {
            this.Type = this.GetType().Name;
        }
        public string Type { get; set; }
    }
}
