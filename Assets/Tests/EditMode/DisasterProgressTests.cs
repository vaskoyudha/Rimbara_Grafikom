using System;
using NUnit.Framework;
using Rimbara.Mission;

namespace Rimbara.Tests
{
    public sealed class DisasterProgressTests
    {
        [TestCase(0, 4)]
        [TestCase(2, 0)]
        [TestCase(-1, 4)]
        public void RejectsMissingChapterSupply(int fires, int debris)
        {
            Assert.That(() => new DisasterProgress(fires, debris), Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void RequiresBriefingThenEquipmentBeforeFire()
        {
            var model = new DisasterProgress(10, 4);
            Assert.That(model.CollectEquipment(true), Is.False);
            Assert.That(model.Extinguish("fire", true), Is.False);
            Assert.That(model.ClearDebris("debris", true), Is.False);
            Assert.That(model.AcceptBriefing(true), Is.True);
            Assert.That(model.AcceptBriefing(true), Is.False);
            Assert.That(model.Extinguish("fire", true), Is.False);
            Assert.That(model.CollectEquipment(true), Is.True);
            Assert.That(model.CollectEquipment(true), Is.False);
            Assert.That(model.Stage, Is.EqualTo(DisasterStage.StopTheFlames));
        }

        [Test]
        public void CleanupCannotStartUntilEveryFireIsContained()
        {
            var model = Ready();
            for (int i = 1; i < 10; i++) model.Extinguish("fire-" + i, true);
            Assert.That(model.ClearDebris("pile-1", true), Is.False);
            Assert.That(model.Stage, Is.EqualTo(DisasterStage.StopTheFlames));
            model.Extinguish("fire-10", true);
            Assert.That(model.Stage, Is.EqualTo(DisasterStage.CleanTheWasteland));
            Assert.That(model.ClearDebris("pile-1", true), Is.True);
        }

        [Test]
        public void UniqueActionsCompleteChapterButNotWholeStory()
        {
            var model = Ready();
            int notifications = 0;
            model.Changed += () => notifications++;
            for (int i = 1; i <= 10; i++) Assert.That(model.Extinguish("fire-" + i, true), Is.True);
            Assert.That(model.Extinguish("fire-1", true), Is.False);
            Assert.That(model.Extinguish("extra", true), Is.False);
            for (int i = 0; i < 4; i++)
            {
                Assert.That(model.ClearDebris("pile-" + i, true), Is.True);
                Assert.That(model.ClearDebris("pile-" + i, true), Is.False);
            }
            Assert.That(model.ClearDebris("extra", true), Is.False);
            Assert.That(model.Stage, Is.EqualTo(DisasterStage.RecoveryReady));
            Assert.That(model.ChapterFraction, Is.EqualTo(1f));
            Assert.That(notifications, Is.EqualTo(14));
        }

        [Test]
        public void PauseBlocksAllChapterMutations()
        {
            var model = new DisasterProgress(10, 4);
            Assert.That(model.AcceptBriefing(false), Is.False);
            model.AcceptBriefing(true);
            Assert.That(model.CollectEquipment(false), Is.False);
            model.CollectEquipment(true);
            Assert.That(model.Extinguish("fire-1", false), Is.False);
            for (int i = 1; i <= 10; i++) model.Extinguish("fire-" + i, true);
            Assert.That(model.ClearDebris("pile", false), Is.False);
            Assert.That(model.Debris, Is.Zero);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public void InvalidIdsNeverEarnProgress(string id)
        {
            var model = Ready();
            Assert.That(model.Extinguish(id, true), Is.False);
            for (int i = 1; i <= 10; i++) model.Extinguish("fire-" + i, true);
            Assert.That(model.ClearDebris(id, true), Is.False);
        }

        [Test]
        public void ResetClearsEquipmentAndUniqueIds()
        {
            var model = Ready();
            for (int i = 1; i <= 10; i++) model.Extinguish("fire-" + i, true);
            model.ClearDebris("pile", true);
            model.Reset();
            Assert.That(model.Stage, Is.EqualTo(DisasterStage.Briefing));
            Assert.That(model.HasEquipment, Is.False);
            Assert.That(model.ChapterFraction, Is.Zero);
            model.AcceptBriefing(true); model.CollectEquipment(true);
            for (int i = 1; i <= 10; i++) Assert.That(model.Extinguish("fire-" + i, true), Is.True);
            Assert.That(model.ClearDebris("pile", true), Is.True);
        }

        private static DisasterProgress Ready()
        {
            var model = new DisasterProgress(10, 4);
            model.AcceptBriefing(true); model.CollectEquipment(true);
            return model;
        }
    }
}
