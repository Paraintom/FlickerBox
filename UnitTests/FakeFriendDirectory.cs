using System;
using System.Collections.Generic;
using FlickerBox.Directory;
using FlickerBox.Messages;

namespace UnitTests
{
    public class FakeFriendDirectory : IFriendDirectory
    {
        public void Discover(FriendRequest request){}

        public event EventHandler<Friend> OnDiscoverResult;
        public List<Friend> GetAll()
        {
            return new List<Friend>();
        }

        public Friend Get(string friendName)
        {
            return new Friend()
            {
                Name = friendName,
                Passphrase = "PassPhrase" + friendName,
                PublicId = new Guid().ToString()
            };
        }

        public Friend GetFromPublicId(string publicId)
        {
            return new Friend()
            {
                Name = "friendName",
                Passphrase = "PassPhrase",
                PublicId = publicId
            };
        }
    }
}
