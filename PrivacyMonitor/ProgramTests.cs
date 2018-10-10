using Moq;
using NUnit.Framework;
using PrivacyMonitor;

namespace PrivacyMonitor
{
    [TestFixture]
    public class ProgramTests
    {
        private MockRepository mockRepository;


        [SetUp]
        public void SetUp()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);
        }

        [TearDown]
        public void TearDown()
        {
            this.mockRepository.VerifyAll();
        }

        [Test]
        public void TestMethod1()
        {
            // Arrange


            // Act
            Program program = this.CreateProgram();


            // Assert

        }

        private Program CreateProgram()
        {
            return new Program();
        }
    }
}
