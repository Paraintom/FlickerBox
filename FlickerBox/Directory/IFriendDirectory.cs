using System;
using System.Collections.Generic;

namespace FlickerBox.Directory
{
    public interface IFriendDirectory
    {
        void Discover(string name, string passphrase);
        event EventHandler<Friend> OnDiscoverResult;

        List<Friend> GetAll();
        Friend Get(string friendName);
    }
}
