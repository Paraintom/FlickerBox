using System;

namespace FlickerBox.Messages
{
    public class GetAllMessagesCommand
    {
        public GetAllMessagesCommand()
        {
            this.Type = this.GetType().Name;
        }
        public string Type { get; set; }
        public DateTime From { get; set; }
    }
}
