using System;

namespace FlickerBox.Messages
{
    public class GetAllMessagesRequest
    {
        public GetAllMessagesRequest()
        {
            this.Type = this.GetType().Name;
        }
        public string Type { get; set; }
        public DateTime From { get; set; }
    }
}
