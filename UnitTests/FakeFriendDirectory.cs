using System;
using System.Collections.Generic;
using FlickerBox.Directory;

namespace UnitTests
{
    public class FakeFriendDirectory : IFriendDirectory
    {
        public void Discover(string name, string passphrase){}

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
    }
}
