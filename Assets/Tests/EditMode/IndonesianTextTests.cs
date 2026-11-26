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
        [TestCase("Menu scope", "5 BAB  /  3 ZONA  /  TANPA BATAS WAKTU")]
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

        [Test]
        public void EveryLabelIsNonEmptyAndConsistent()
        {
            Assert.That(IndonesianText.Validate(), Is.Empty);
        }

        [Test]
        public void ChapterCountIsSingleSourced()
        {
            Assert.That(GameScope.Chapters, Is.EqualTo(5));
            Assert.That(IndonesianText.Labels["Menu scope"], Does.Contain(GameScope.Chapters.ToString()));
            Assert.That(IndonesianText.Labels["Briefing steps"], Does.Contain("05"));
        }

        [Test]
        public void NoPlaceholderOrEnglishLeak()
        {
            var banned = new[] { "TODO", "TBD", "Lorem", "Collect native", "FIRST STEPS", "SEEDS OF HOPE" };
            foreach (var pair in IndonesianText.Labels)
                foreach (var word in banned)
                    Assert.That(pair.Value, Does.Not.Contain(word), $"{pair.Key} leaks '{word}'");
        }
    }
}
