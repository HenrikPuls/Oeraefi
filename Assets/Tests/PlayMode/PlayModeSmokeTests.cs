using System.Collections;
using NUnit.Framework;
using Oerfi;
using UnityEngine;
using UnityEngine.TestTools;

namespace Oerfi.Tests.PlayMode
{
    /// <summary>
    /// M0 smoke test: prove that PlayMode tests execute in batchmode without
    /// any scene, camera, or rendering assumptions. Headless-safe by design.
    /// </summary>
    public class PlayModeSmokeTests
    {
        [UnityTest]
        public IEnumerator GameObject_SurvivesOneFrame()
        {
            var go = new GameObject("PlayModeSmokeTest");
            yield return null;
            Assert.That(go, Is.Not.Null);
            Object.Destroy(go);
        }

        [Test]
        public void AsmdefWiring_RuntimeAssemblyIsReferenced()
        {
            Assert.That(GameConstants.SAVE_DATA_VERSION, Is.GreaterThan(0));
        }
    }
}
