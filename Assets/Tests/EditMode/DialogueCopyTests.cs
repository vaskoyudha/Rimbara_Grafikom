using NUnit.Framework;
using Rimbara.UI;

namespace Rimbara.Tests
{
    public sealed class DialogueCopyTests
    {
        [Test]
        public void EveryPageIsNonEmptyAndPunctuated()
        {
            foreach (string page in DialogueCopy.AllPages(10))
            {
                Assert.That(page, Is.Not.Empty);
                Assert.That(page.Trim(), Does.EndWith(".").Or.EndWith("!").Or.EndWith("?"));
            }
        }

        [Test]
        public void SpeakerNamesAreIndonesian()
        {
            Assert.That(DialogueCopy.CoordinatorName, Is.EqualTo("BIMA PRATAMA"));
            Assert.That(DialogueCopy.CoordinatorSpeaker, Is.EqualTo("KOORDINATOR MISI"));
            Assert.That(DialogueCopy.CoordinatorHeader, Does.Contain("BIMA PRATAMA").And.Contain("KOORDINATOR MISI"));
            Assert.That(DialogueCopy.RangerSpeaker, Is.EqualTo("PETUGAS HUTAN"));
            Assert.That(DialogueCopy.ElderSpeaker, Is.EqualTo("TETUA DESA"));
        }

        [Test]
        public void OpeningDialogueHasFourIndonesianShotsWithSpeakers()
        {
            Assert.That(DialogueCopy.NarratorSpeaker, Is.EqualTo("NARATOR"));
            Assert.That(DialogueCopy.VillagerSpeaker, Is.EqualTo("WARGA DESA"));
            Assert.That(DialogueCopy.OpeningSpeakers.Length, Is.EqualTo(4));
            Assert.That(DialogueCopy.OpeningLines.Length, Is.EqualTo(4));
            for (int i = 0; i < 4; i++)
            {
                string formatted = DialogueCopy.FormatOpeningSubtitle(i);
                Assert.That(formatted, Does.Contain(DialogueCopy.OpeningSpeakers[i]));
                Assert.That(formatted, Does.Contain(DialogueCopy.OpeningLines[i]));
            }
            Assert.That(IndonesianText.Labels["Skip opening cinematic label"], Is.EqualTo("LEWATI SINEMATIK"));
        }

        [Test]
        public void OpeningShotTimingMapsAllFourBeats()
        {
            Assert.That(OpeningDirector.ShotForTime(0f, out float p0), Is.EqualTo(0));
            Assert.That(p0, Is.EqualTo(0f).Within(0.001f));
            Assert.That(OpeningDirector.ShotForTime(8.0f, out float p1), Is.EqualTo(1));
            Assert.That(p1, Is.EqualTo(0f).Within(0.001f));
            Assert.That(OpeningDirector.ShotForTime(15.0f, out float p2), Is.EqualTo(2));
            Assert.That(p2, Is.EqualTo(0f).Within(0.001f));
            Assert.That(OpeningDirector.ShotForTime(22.0f, out float p3), Is.EqualTo(3));
            Assert.That(p3, Is.EqualTo(0f).Within(0.001f));
            Assert.That(OpeningDirector.ShotForTime(30.0f, out float pEnd), Is.EqualTo(3));
            Assert.That(pEnd, Is.EqualTo(1f).Within(0.001f));
        }

        [Test]
        public void InteractiveDialogueOptionsMatchBriefingPages()
        {
            Assert.That(DialogueCopy.CoordinatorBriefingOptions.Length, Is.EqualTo(DialogueCopy.CoordinatorBriefing.Length));
            Assert.That(DialogueCopy.CoordinatorDebriefOptions.Length, Is.EqualTo(DialogueCopy.CoordinatorDebrief.Length));
            Assert.That(DialogueCopy.RangerBriefingOptions.Length, Is.EqualTo(DialogueCopy.RangerBriefing.Length));
            Assert.That(DialogueCopy.ElderBriefingOptions.Length, Is.EqualTo(DialogueCopy.ElderBriefing.Length));
        }
    }
}
