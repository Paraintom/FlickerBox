using System.Threading;
using System.Threading.Tasks;
using FlickerBox;
using FlickerBox.Communication;
using FlickerBox.Directory;
using NUnit.Framework;

namespace UnitTests
{
    [TestFixture]
    class HandShakeManagerTests : BaseTest
    {
        [Test]
        public void TestAlone()
        {
            IHandShakeManager alone = new HandShakeManager(FakeChannelFactory);
            var result = alone.ExchangeIdentity("useless", "idem");
            Assert.IsNullOrEmpty(result);
        }

        [Test]
        public void TestBasic()
        {
            IHandShakeManager first = new HandShakeManager(FakeChannelFactory);
            IHandShakeManager second = new HandShakeManager(FakeChannelFactory);

            string passphrase = "This is a test";
            string id1 = "A";
            string id2 = "B";
            Task<string> t1 = new Task<string>(() => first.ExchangeIdentity(id1, passphrase));
            Task<string> t2 = new Task<string>(() => second.ExchangeIdentity(id2, passphrase));
            t1.Start();
            Thread.Sleep(2000);
            t2.Start();
            //Identity have been exchanged
            Assert.AreEqual(id2, t1.Result);
            Assert.AreEqual(id1, t2.Result);
        }
    }
}
