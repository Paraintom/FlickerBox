using System;
using System.Threading;
using System.Threading.Tasks;
using FlickerBox.Directory;
using FlickerBox.Messages;
using NUnit.Framework;

namespace UnitTests
{
    [TestFixture]
    public class FriendDirectoryTests : BaseTest
    {
        private string Id = "MyID";

        [SetUp]
        [TearDown]
        public override void Init()
        {
            base.Init();
            Write("Cleaning all the friends.");
            FriendDirectory.ResetAll();
        }
        [Test]
        public void TestAll()
        {
            string tick = DateTime.UtcNow.Ticks.ToString();
            Friend result = null;
            string friendName = "Name" + tick;
            string friendPublicId = "FId" + tick;
            string passphrase = "Passphrase" + tick;
            //Strating the test
            IFriendDirectory toTest = new FriendDirectory(Id,FakeChannelFactory);
            //We should not have any friends at the starts
            Assert.AreEqual(0,toTest.GetAll().Count);
            toTest.OnDiscoverResult += (sender, friend) => result = friend;
            AckHandShake(friendPublicId, passphrase);
            toTest.Discover(new FriendRequest() { Name = friendName, Passphrase = passphrase });
            Thread.Sleep(4000);
            //Then we should have received the confirmation
            Assert.IsNotNull(result);
            Assert.AreEqual(friendName, result.Name);
            Assert.AreEqual(friendPublicId, result.PublicId);
            Assert.AreEqual(passphrase, result.Passphrase);
            Assert.AreEqual(1, toTest.GetAll().Count);
            Assert.AreEqual(result, toTest.GetAll()[0]);

            //Test persistence...
            IFriendDirectory toTest2 = new FriendDirectory(Id, FakeChannelFactory);
            Assert.AreEqual(1, toTest2.GetAll().Count);
            Assert.IsNotNull(toTest2.Get(friendName));
            
        }

        [Test]
        [ExpectedException(typeof(ApplicationException))]
        public void TestUnknownFriend()
        {
            IFriendDirectory toTest = new FriendDirectory(Id, FakeChannelFactory);
            toTest.Get("UnknownFriend");
        }

        public void AckHandShake(string friendId, string passphrase)
        {
            new Task(() =>
            {
                var ack = new HandShakeManager(FakeChannelFactory);
                var result = ack.ExchangeIdentity(friendId, passphrase);
                //Just for the sake of it...
                Assert.AreEqual(Id, result);
            }).Start();
        }
    }
}
