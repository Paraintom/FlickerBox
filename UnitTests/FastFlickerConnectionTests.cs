using FlickerBox.Communication;
using NUnit.Framework;
using System;
using System.Threading;

namespace UnitTests
{
    [TestFixture]
    public class FastFlickerConnectionTests : BaseTest
    {
        private string subject = "TestSubject";
        private string url = "ws://localhost:8099/";
        [Ignore("Integration test, you need to start a fastfliker for testing it.")]
        [Test]
        public void IntegrationTestBasic()
        {
            Write("Starting the test...");
            string lastMessage = String.Empty;
            IChannel toTest = new FastFlickerClient(url, subject);
            toTest.OnMessageReceived += (s, m) => lastMessage = m;
            Assert.IsNullOrEmpty(lastMessage);
            toTest.SendMessage("Hello");
            Assert.IsNullOrEmpty(lastMessage);

            string incomingMessage = "I am here!";
            IChannel otherClient = new FastFlickerClient(url, subject);
            otherClient.SendMessage(incomingMessage);
            Thread.Sleep(500);
            Assert.AreEqual(incomingMessage, lastMessage);
        }
        [Ignore("Integration test, you need to start a fastfliker for testing it.")]
        [Test]
        public void IntegrationTestDisconnection()
        {
            Write("Starting the test...");
            string lastMessage = String.Empty;
            IChannel toTest = new FastFlickerClient(url, subject);
            IChannel listener = new FastFlickerClient(url, subject);
            listener.OnMessageReceived += (s, m) => lastMessage = m;
            Assert.IsNullOrEmpty(lastMessage);
            toTest.SendMessage("Hello");
            Write("Disconnect FastFlicker.");
            Thread.Sleep(10000);
            Write("Reconnect FastFlicker.");
            Thread.Sleep(10000);
            string incomingMessage = "It works?";
            toTest.SendMessage(incomingMessage);
            Thread.Sleep(1000);
            Assert.AreEqual(incomingMessage, lastMessage);
        }
    }
}
