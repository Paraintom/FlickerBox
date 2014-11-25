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
        public string FromTime { get; set; }
    }
}
