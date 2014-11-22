using System.Collections.Generic;
using FlickerBox.Directory;

namespace FlickerBox.Messages
{
    public class FriendList
    {
        public FriendList()
        {
            this.Type = this.GetType().Name;
        }
        public string Type { get; set; }
        public List<Friend> Result { get; set; }
    }
}
