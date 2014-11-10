using System.Diagnostics;
using FlickerBox.Communication;
using FlickerBox.Directory;
using NLog;
using NLog.Config;
using NLog.Targets;
using NUnit.Framework;

namespace UnitTests
{
    public class BaseTest
    {
        //Create a fake identity
        /*var mockIdentityManager = new Mock<IIdentityManager>();
        mockIdentityManager.Setup(o => o.PublicId).Returns(ForTestPublicId);
        mockIdentityManager.Setup(o => o.PrivateId).Throws(new AssertionException("MessagesManager doesn't need to know about this property"));*/

        protected static FakeChannelFactory FakeChannelFactory = new FakeChannelFactory();
        protected static FakeFriendDirectory FakeFriendDirectory = new FakeFriendDirectory();
        public BaseTest()
        {
            ConsoleTarget target = new ConsoleTarget();
            target.Layout = "${time}|${level:uppercase=true}|${callsite:className=true:includeSourcePath=false:methodName=false}|${message}";

            SimpleConfigurator.ConfigureForTargetLogging(target, LogLevel.Trace);

            Logger logger = LogManager.GetLogger("ee");
            logger.Debug("another log message");

        }

        [SetUp]
        public virtual void Init()
        {
            FakeChannelFactory.ResetAll();
        }

        public void Write(string message)
        {
            Debug.WriteLine(message);
        }
    }
}
