using System;
using System.Runtime.InteropServices;
using FlickerBox;
using FlickerBox.Communication;
using FlickerBox.Messages;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NUnit.Framework;

namespace UnitTests
{
    [TestFixture]
    class MessagesManagerTests : BaseTest
    {
        private const string ForTestPublicId = "qwopjrrwhqp2346";
        #region Box send ...

        [Test]
        public void TestSendMessage()
        {
            string messageId = DateTime.UtcNow.ToShortDateString();
            string messageTo = "John";
            string messageFrom = "Tom";
            string messageCreationTime = DateTime.UtcNow.JavascriptTicks();
            string messageContent = "HEllo Woorld!";
            Message toSend = new Message()
            {
                ToFriendName = messageTo,
                FromFriendName = messageFrom,
                Id = messageId,
                Content = messageContent,
                UtcCreationTime = messageCreationTime
            };

            //Create a fake channel and test the values passed to
            var mockedChannel = new Mock<IChannel>();
            FakeChannelFactory.ToReturn = mockedChannel.Object;
            mockedChannel.Setup(o => o.SendMessage(It.IsAny<string>())).Callback<string>(s => Assert.AreEqual(
                "{" +
                "\"Type\":\"" + "Message" + "\"," +
                "\"ToFriendName\":\"" + messageTo + "\"," +
                "\"FromFriendName\":\"" + messageFrom + "\"," +
                "\"FromPublicId\":\"" + ForTestPublicId + "\"," +
                "\"Id\":\"" + messageId + "\"," +
                "\"UtcCreationTime\":\"" + messageCreationTime + "\"," +
                "\"Content\":\"" + messageContent + "\"}", s));

            //
            IMessagesManager toTest = new MessagesManager(FakeFriendDirectory,FakeChannelFactory, ForTestPublicId);
            toTest.Send(toSend);
            mockedChannel.Verify(o => o.SendMessage(It.IsAny<string>()), Times.Once);

            FakeChannelFactory.ToReturn = null;
        }

        [Test]
        public void TestSendAck()
        {
            string id = DateTime.UtcNow.ToShortDateString();
            string fromPublicId = "FromPublicId";
            string expectedState = AckStates.Read.ToString();

            //Create a fake channel and test the values passed to
            var mockedChannel = new Mock<IChannel>();
            FakeChannelFactory.ToReturn = mockedChannel.Object;
            mockedChannel.Setup(o => o.SendMessage(It.IsAny<string>())).Callback<string>(s => Assert.AreEqual(
                "{" +
                "\"Type\":\"" + "Ack" + "\"," +
                "\"Id\":\"" + id + "\"," +
                "\"State\":\"" + expectedState + "\"}", s));

            //
            IMessagesManager toTest = new MessagesManager(FakeFriendDirectory, FakeChannelFactory, fromPublicId);
            toTest.AcknowledgeRead(new Ack() { Id = id, State = AckStates.Read.ToString()});
            mockedChannel.Verify(o => o.SendMessage(It.IsAny<string>()), Times.Once);

            FakeChannelFactory.ToReturn = null;
        } 
        #endregion

        #region Box receive

        [Test]
        public void TestReceiveShit()
        {
            FakeChannel fakeChannel = new FakeChannel(ForTestPublicId);
            FakeChannelFactory.ToReturn = fakeChannel;
            int notifReceived = 0;
            IMessagesManager toTest = new MessagesManager(FakeFriendDirectory, FakeChannelFactory, ForTestPublicId);
            toTest.OnAcknowledged += (sender, ack) => notifReceived++;
            toTest.OnReceived += (sender, msg) => notifReceived++;
            fakeChannel.MessageReceived("ewiojqeoiwqheew");
            Assert.AreEqual(0, notifReceived);
        }

        [Test]
        public void TestBadMessageType()
        {
            FakeChannel fakeChannel = new FakeChannel(ForTestPublicId);
            FakeChannelFactory.ToReturn = fakeChannel;
            int notifReceived = 0;
            IMessagesManager toTest = new MessagesManager(FakeFriendDirectory, FakeChannelFactory, ForTestPublicId);
            toTest.OnAcknowledged += (sender, ack) => notifReceived++;
            toTest.OnReceived += (sender, msg) => notifReceived++;
            string badMessage = "{" +
                "\"Id\":\"" + "id" + "\"," +
                "\"State\":\"" + "badState" + "\"}";  
            fakeChannel.MessageReceived(badMessage);
            Assert.AreEqual(0, notifReceived);
        }

        #endregion
    }
}
