using NUnit.Framework;
using Oerfi;

namespace Oerfi.Tests.EditMode
{
    /// <summary>
    /// M0 smoke tests: verify the verification pipeline itself (batchmode run,
    /// asmdef wiring, test framework integration). Real system tests follow
    /// with their respective milestones.
    /// </summary>
    public class EditModeSmokeTests
    {
        [Test]
        public void Sanity_AssertionsWork()
        {
            Assert.That(2 + 2, Is.EqualTo(4));
        }

        [Test]
        public void AsmdefWiring_RuntimeAssemblyIsReferenced()
        {
            // If the Oerfi.Tests.EditMode -> Oerfi.Runtime asmdef reference is
            // broken, this file does not compile and the whole gate fails loudly.
            Assert.That(GameConstants.SAVE_DATA_VERSION, Is.GreaterThan(0));
        }
    }
}
