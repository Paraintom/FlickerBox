using System.IO;
using System.Threading;
using FlickerBox.Identity;
using NUnit.Framework;

namespace UnitTests
{

    [TestFixture]
    public class IdentityTests : BaseTest
    {
        [TearDown]
        public void Cleanup()
        {
            File.Delete(IdentityManager.FileName);
        }
        [Test]
        public void TestAll()
        {
            Cleanup();
            Write("New installation, should generate the Id and write it in the file.");
            IIdentityManager toTest = new IdentityManager();
            AssertFieldsNotEmpty(toTest);
            Write("If file exist, should read and keep the same values");
            IIdentityManager toTest2 = new IdentityManager();
            AssertFieldsNotEmpty(toTest2);
            Assert.AreEqual(toTest.PublicId, toTest2.PublicId);
            Assert.AreEqual(toTest.PrivateId, toTest2.PrivateId);
            Write("Removing the file should reset the Id");
            Cleanup();
            Thread.Sleep(1);
            IIdentityManager toTest3 = new IdentityManager();
            AssertFieldsNotEmpty(toTest3);
            Assert.AreNotEqual(toTest.PublicId, toTest3.PublicId);
        }

        private static void AssertFieldsNotEmpty(IIdentityManager toTest)
        {
            Assert.IsNotNullOrEmpty(toTest.PublicId);
            Assert.IsNotNullOrEmpty(toTest.PrivateId);
        }
    }
}
