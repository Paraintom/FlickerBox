using System;
using System.Collections.Generic;
using FlickerBox;
using FlickerBox.ClientInteraction;
using FlickerBox.Directory;
using FlickerBox.Identity;
using FlickerBox.Messages;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;

namespace UnitTests
{
    [TestFixture]
    public class CommandListenerTests : BaseTest
    {
        #region Messages
        [Test]
        public void GetAllMessagesTriggerGoodEvent()
        {
            FakeChannel clientSideCommandChannel;
            var toTest = InitCommandListener(out clientSideCommandChannel);
            GetAllMessagesRequest result = null;
            DateTime utcNow = DateTime.UtcNow;
            //Truncate the Milliseconds...
            DateTime dateTimeExpected = utcNow.AddTicks(-(utcNow.Ticks % TimeSpan.TicksPerSecond));
            string now = dateTimeExpected.JavascriptTicks();

            string incomingCommand = "{" +
                                     "\"Type\":\"" + "GetAllMessagesRequest" + "\"," +
                                     "\"FromTime\":\"" + now + "\"}";
            toTest.OnGetAllMessagesFromReceived += (sender, time) => { result = time; Write("Event raised! :" + time); };
            Assert.IsNull(result);
            clientSideCommandChannel.SendMessage(incomingCommand);
            Assert.IsNotNull(result);
            Assert.AreEqual(dateTimeExpected, result.FromTime.FromJavascriptTicks());
        }
        [Test]
        public void OnMessageToSendReceivedTriggerGoodEvent()
        {
            FakeChannel clientSideCommandChannel;
            var toTest = InitCommandListener(out clientSideCommandChannel);
            Message result = null;
            string content = "content 123 !\"£$%^&*()_";
            string id = "e2pjpjewpe0";
            string friendName = "Nelson";
            string utcCreationTime = DateTime.UtcNow.JavascriptTicks();

            var toSend = GetMessage(content, id, friendName, utcCreationTime);

            string incomingCommand = JsonConvert.SerializeObject(toSend);
            toTest.OnMessageToSendReceived += (sender, time) => { result = time; Write("Event raised! :" + time); };
            Assert.IsNull(result);
            clientSideCommandChannel.SendMessage(incomingCommand);
            Assert.IsNotNull(result);
            Assert.AreEqual(result.Content, content);
            Assert.IsNotNullOrEmpty(result.FromPublicId);
            Assert.AreEqual(result.Id, id);
            Assert.AreEqual(result.ToFriendName, friendName);
            Assert.AreEqual(result.UtcCreationTime, utcCreationTime);
        }

        private static Message GetMessage(string content, string id, string friendName, string utcCreationTime)
        {
            Message toSend = new Message()
            {
                Content = content,
                Id = id,
                ToFriendName = friendName,
                FromFriendName = "Thomas",
                UtcCreationTime = utcCreationTime
            };
            return toSend;
        }

        [Test]
        public void OnFlagMessageReadTriggerGoodEvent()
        {
            FakeChannel clientSideCommandChannel;
            var toTest = InitCommandListener(out clientSideCommandChannel);
            Ack result = null;
            string id = "e2pjpjewpe0";
            //Not yet, in the futur?
            //string utcCreationTime = DateTime.UtcNow.JavascriptTicks();

            Ack toSend = new Ack()
            {
                Id = id,
                State = AckStates.Read.ToString()
            };

            string incomingCommand = JsonConvert.SerializeObject(toSend);
            toTest.OnFlagMessageRead += (sender, time) => { result = time; Write("Event raised! :" + time); };
            Assert.IsNull(result);
            clientSideCommandChannel.SendMessage(incomingCommand);
            Assert.IsNotNull(result);
            Assert.AreEqual(result.Id, id);
        }
        [Test]
        public void SendAckIsWellParsed()
        {
            string received = null;
            FakeChannel clientSideCommandChannel;
            ICommandListener toTest = InitCommandListener(out clientSideCommandChannel);
            clientSideCommandChannel.OnMessageReceived += (sender, s) =>
            {
                received = s;
                Write("Message received raised! :" + s);
            };
            string expectedId = "IDweq32";
            Ack ack = new Ack()
            {
                Id = expectedId,
                State = AckStates.Read.ToString()
            };
            //Creating message
            string expectedMessage = "{\"Type\":\"Ack\",\"Id\":\"" + expectedId + "\",\"State\":\"" + AckStates.Read.ToString() + "\"}";
            Assert.IsNull(received);
            toTest.SendStateChanged(ack);
            Assert.IsNotNull(received);
            Assert.AreEqual(expectedMessage, received);
        }

        [Test]
        public void SendMessagesIsWellParsed()
        {
            string received = null;
            FakeChannel clientSideCommandChannel;
            ICommandListener toTest = InitCommandListener(out clientSideCommandChannel);
            clientSideCommandChannel.OnMessageReceived += (sender, s) =>
            {
                received = s;
                Write("Message received raised! :" + s);
            };
            string utcCreationTime = DateTime.UtcNow.JavascriptTicks();
            List<Message> allMessages = new List<Message>();
            allMessages.Add(GetMessage("Cont1", "id1", "Simon", utcCreationTime));
            allMessages.Add(GetMessage("Cont2", "id2", "Dam", utcCreationTime));
            //Creating message
            string expectedMessage = "[" +
                                     "{\"Type\":\"Message\",\"ToFriendName\":\"Simon\",\"FromFriendName\":\"Thomas\",\"FromPublicId\":null," +
                                     "\"Id\":\"id1\",\"UtcCreationTime\":\""+utcCreationTime+"\",\"Content\":\"Cont1\"}," +
                                     "{\"Type\":\"Message\",\"ToFriendName\":\"Dam\",\"FromFriendName\":\"Thomas\",\"FromPublicId\":null," +
                                     "\"Id\":\"id2\",\"UtcCreationTime\":\""+utcCreationTime+"\",\"Content\":\"Cont2\"}]";
            Assert.IsNull(received);
            toTest.SendMessages(allMessages);
            Assert.IsNotNull(received);
            Assert.AreEqual(expectedMessage, received);
        }
        #endregion

        #region Friends
        [Test]
        public void FriendRequestTriggerGoodEvent()
        {
            FakeChannel clientSideCommandChannel;
            ICommandListener toTest = InitCommandListener(out clientSideCommandChannel);
            FriendRequest result = null;
            //Creating message
            string name = "Romain";
            string passphrase = "bps";
            string incomingMessage = "{" +
                                     "\"Type\":\"" + "FriendRequest" + "\"," +
                                     "\"Name\":\"" + name + "\"," +
                                     "\"Passphrase\":\"" + passphrase + "\"}";
            toTest.OnFriendRequestReceived += (sender, request) => { result = request; Write("Event raised! :" + request); };
            Assert.IsNull(result);
            clientSideCommandChannel.SendMessage(incomingMessage);
            Assert.IsNotNull(result);
            Assert.AreEqual(name, result.Name);
            Assert.AreEqual(passphrase, result.Passphrase);
        }
        [Test]
        public void GetAllFriendRequestTriggerGoodEvent()
        {
            FakeChannel clientSideCommandChannel;
            ICommandListener toTest = InitCommandListener(out clientSideCommandChannel);
            GetAllFriendRequest result = null;
            //Creating message
            string incomingMessage = "{" +
                                     "\"Type\":\"" + "GetAllFriendRequest" +
                                     "\"}";
            toTest.OnGetAllFriendsReceived += (sender, request) => { result = request; Write("Event raised! :" + request); };
            Assert.IsNull(result);
            clientSideCommandChannel.SendMessage(incomingMessage);
            Assert.IsNotNull(result);
        }
        [Test]
        public void SendFriendsIsWellParsed()
        {
            string received = null;
            FakeChannel clientSideCommandChannel;
            ICommandListener toTest = InitCommandListener(out clientSideCommandChannel);
            clientSideCommandChannel.OnMessageReceived += (sender, s) =>
            {
                received = s;
                Write("Message received raised! :" + s);
            };
            string name1 = "a1";
            string name2 = "a2";
            List<Friend> allFriends = new List<Friend>()
            {
                new Friend(){Name = name1, Passphrase = "secret!", PublicId = "NotImportant..."},
                new Friend(){Name = name2, Passphrase = "secret!", PublicId = "NotImportant..."}
            };
            //Creating message
            string expectedMessage = "{\"Type\":\"FriendList\",\"Result\":[" +
                                           "{\"Name\":\"" + name1 + "\"}," +
                                           "{\"Name\":\"" + name2 + "\"}" +
                                           "]}";
            Assert.IsNull(received);
            toTest.SendFriends(allFriends);
            Assert.IsNotNull(received);
            Assert.AreEqual(expectedMessage, received);
        }
        #endregion

        #region Helpers
        private ICommandListener InitCommandListener(out FakeChannel clientSideCommandChannel)
        {
            IIdentityManager fakeIdManager = GetAnyIdManager();
            clientSideCommandChannel = new FakeChannel(fakeIdManager.PublicId);
            var serverSideCommandChannel = new FakeChannel(fakeIdManager.PublicId);
            FakeChannelFactory.ToReturn = serverSideCommandChannel;
            ICommandListener toTest = new CommandListener(fakeIdManager, FakeChannelFactory);
            return toTest;
        }
        private IIdentityManager GetAnyIdManager()
        {
            var mock = new Mock<IIdentityManager>();
            mock.Setup(o => o.PublicId).Returns("bla2" + DateTime.UtcNow.Ticks);
            mock.Setup(o => o.PrivateId).Returns("bla" + DateTime.UtcNow.Ticks);
            return mock.Object;
        }
        #endregion
    }
}
