using NUnit.Framework;
using Rimbara.Mission;
using Rimbara.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Rimbara.Tests
{
    public sealed class RestorationScoreTests
    {
        private static ProgressSnapshot Snapshot(int seedlings, int trees, int fires)
        {
            return new ProgressSnapshot(seedlings, trees, fires, 5, 5, 2);
        }

        private static HudReferences References()
        {
            return new HudReferences
            {
                Seedlings = Text("seedlings"),
                Trees = Text("trees"),
                Fires = Text("fires"),
                Restoration = Text("restoration"),
                RestorationFill = Image("fill"),
                BriefingTargets = Text("targets"),
                MissionNumber = Text("mission-number"),
                MissionTitle = Text("mission-title"),
                MissionDescription = Text("mission-description"),
            };
        }

        private static TMP_Text Text(string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            return go.AddComponent<TextMeshProUGUI>();
        }

        private static Image Image(string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            return go.AddComponent<Image>();
        }

        [Test]
        public void FromMapsEveryCounter()
        {
            RestorationScore score = RestorationScore.From(Snapshot(3, 2, 1), 2);
            Assert.That(score.Seedlings, Is.EqualTo(3));
            Assert.That(score.Trees, Is.EqualTo(2));
            Assert.That(score.Fires, Is.EqualTo(1));
            Assert.That(score.SeedlingGoal, Is.EqualTo(5));
            Assert.That(score.TreeGoal, Is.EqualTo(5));
            Assert.That(score.FireGoal, Is.EqualTo(2));
            Assert.That(score.CarriedSeedlings, Is.EqualTo(1));
            Assert.That(score.Total, Is.EqualTo(3));
            Assert.That(score.Target, Is.EqualTo(7));
            Assert.That(score.Mission, Is.EqualTo(2));
        }

        [Test]
        public void ProgressIsTreesPlusFiresOverSeven()
        {
            RestorationScore score = RestorationScore.From(Snapshot(5, 4, 2), 3);
            Assert.That(score.Fraction, Is.EqualTo(6f / 7f).Within(0.00001f));
            Assert.That(score.Percent, Is.EqualTo(86));
        }

        [Test]
        public void PercentRoundsLikeTheHud()
        {
            RestorationScore score = RestorationScore.From(Snapshot(5, 2, 1), 2);
            Assert.That(score.Fraction, Is.EqualTo(3f / 7f).Within(0.00001f));
            Assert.That(score.Percent, Is.EqualTo(43));
        }

        [Test]
        public void MissionNumberIsClampedToTheFourMissionWindow()
        {
            Assert.That(RestorationScore.MissionNumberFor(MissionStage.FirstSteps), Is.EqualTo(1));
            Assert.That(RestorationScore.MissionNumberFor(MissionStage.SeedsOfHope), Is.EqualTo(2));
            Assert.That(RestorationScore.MissionNumberFor(MissionStage.StopTheFire), Is.EqualTo(3));
            Assert.That(RestorationScore.MissionNumberFor(MissionStage.ANewBeginning), Is.EqualTo(4));
            Assert.That(RestorationScore.MissionNumberFor(MissionStage.Complete), Is.EqualTo(4));
            Assert.That(RestorationScore.From(Snapshot(0, 0, 0), 99).Mission, Is.EqualTo(4));
            Assert.That(RestorationScore.From(Snapshot(0, 0, 0), -3).Mission, Is.EqualTo(1));
        }

        [Test]
        public void RestoredRequiresEveryGoal()
        {
            Assert.That(RestorationScore.From(Snapshot(5, 5, 2), 4).Restored, Is.True);
            Assert.That(RestorationScore.From(Snapshot(5, 5, 1), 4).Restored, Is.False);
            Assert.That(RestorationScore.From(Snapshot(4, 4, 2), 4).Restored, Is.False);
        }

        [Test]
        public void ViewCarriesAuthoredCopyAndClampsElapsed()
        {
            RestorationScoreView view = RestorationScoreView.From(Snapshot(5, 3, 1), MissionStage.StopTheFire,
                "PADAMKAN API", "Arahkan bidikan.", -12f);
            Assert.That(view.MissionNumber, Is.EqualTo(3));
            Assert.That(view.MissionTitle, Is.EqualTo("PADAMKAN API"));
            Assert.That(view.MissionInstruction, Is.EqualTo("Arahkan bidikan."));
            Assert.That(view.ElapsedSeconds, Is.Zero);
            Assert.That(view.ChapterCount, Is.EqualTo(GameScope.Chapters));
        }

        [Test]
        public void WriteCountersBindsScoreToHudReferences()
        {
            HudReferences view = References();
            RestorationScoreView model = RestorationScoreView.From(Snapshot(3, 2, 1), MissionStage.SeedsOfHope, "BENIH HARAPAN", "Tanam bibit.");

            RimbaraHud.WriteCounters(view, model);

            StringAssert.Contains("3 / 5", view.Seedlings.text);
            StringAssert.Contains("dibawa 1", view.Seedlings.text);
            StringAssert.Contains("2 / 5", view.Trees.text);
            StringAssert.Contains("1 / 2", view.Fires.text);
            Assert.That(view.Restoration.text, Is.EqualTo("43% DIPULIHKAN"));
            Assert.That(view.RestorationFill.fillAmount, Is.EqualTo(3f / 7f).Within(0.00001f));
            Assert.That(view.BriefingTargets.text, Is.EqualTo("5 BIBIT LOKAL   /   5 POHON   /   2 TITIK API"));
        }

        [Test]
        public void WriteMissionBindsBannerFromViewModel()
        {
            HudReferences view = References();
            RestorationScoreView model = RestorationScoreView.From(Snapshot(0, 0, 0), MissionStage.StopTheFire, "PADAMKAN API", "Dekati titik api.");

            RimbaraHud.WriteMission(view, model);

            Assert.That(view.MissionNumber.text, Is.EqualTo("MISI LAPANGAN    03 / 05"));
            Assert.That(view.MissionTitle.text, Is.EqualTo("PADAMKAN API"));
            Assert.That(view.MissionDescription.text, Is.EqualTo("Dekati titik api."));
        }
    }
}
