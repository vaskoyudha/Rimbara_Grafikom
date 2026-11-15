using NUnit.Framework;
using Rimbara.UI;

namespace Rimbara.Tests
{
    public sealed class IndonesianTextTests
    {
        [TestCase("Start restoration label", "MULAI PEMULIHAN")]
        [TestCase("Help title", "CARA BERMAIN")]
        [TestCase("Pause title", "PERMAINAN DIJEDA")]
        [TestCase("Victory title", "AWAL YANG BERSIH")]
        public void PrimaryScreensUseIndonesian(string label, string expected)
        {
            Assert.That(IndonesianText.Labels[label], Is.EqualTo(expected));
        }

        [TestCase("MISSION COORDINATOR", "KOORDINATOR MISI")]
        [TestCase("REMAINING FIRE", "TITIK API")]
        [TestCase("DESIGNATED DEBRIS", "PUING BERTANDA")]
        [TestCase("EMERGENCY MISSION POST", "POSKO DARURAT")]
        public void WorldLabelsTranslateWithoutChangingHierarchyNames(string source, string expected)
        {
            Assert.That(IndonesianText.WorldLabel(source), Is.EqualTo(expected));
            Assert.That(IndonesianText.WorldLabel(expected), Is.EqualTo(expected));
        }

        [Test]
        public void StoryAndControlsRetainScopeAndBindings()
        {
            Assert.That(IndonesianText.Labels["Control list"], Does.Contain("WASD").And.Contain("SPASI").And.Contain("KLIK KIRI").And.Contain("ESC"));
            Assert.That(IndonesianText.Labels["Mission results"], Does.Contain(IndonesianText.ChapterPending));
        }
    }
}
