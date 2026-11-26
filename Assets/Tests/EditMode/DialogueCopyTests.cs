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
            Assert.That(DialogueCopy.CoordinatorSpeaker, Is.EqualTo("KOORDINATOR MISI"));
            Assert.That(DialogueCopy.RangerSpeaker, Is.EqualTo("PETUGAS HUTAN"));
            Assert.That(DialogueCopy.ElderSpeaker, Is.EqualTo("TETUA DESA"));
        }
    }
}
