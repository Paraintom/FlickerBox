using System;
using System.Collections.Generic;
using FlickerBox.Messages;

namespace FlickerBox.Directory
{
    public interface IFriendDirectory
    {
        void Discover(FriendRequest request);
        event EventHandler<Friend> OnDiscoverResult;

        List<Friend> GetAll();
        Friend Get(string friendName);
    }
}
