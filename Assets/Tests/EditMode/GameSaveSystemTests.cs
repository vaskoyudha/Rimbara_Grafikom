using NUnit.Framework;
using Rimbara.Mission;

namespace Rimbara.Tests
{
    public sealed class GameSaveSystemTests
    {
        [Test]
        public void DisasterProgress_RestoreState_PreservesIdsAndStage()
        {
            var progress = new DisasterProgress(5, 3);
            progress.RestoreState(DisasterStage.CleanTheWasteland, true, new[] { "fire-1", "fire-2" }, new[] { "debris-1" });

            Assert.That(progress.Stage, Is.EqualTo(DisasterStage.CleanTheWasteland));
            Assert.That(progress.HasEquipment, Is.True);
            Assert.That(progress.Fires, Is.EqualTo(2));
            Assert.That(progress.Debris, Is.EqualTo(1));
            Assert.That(progress.IsFireExtinguished("fire-1"), Is.True);
            Assert.That(progress.IsDebrisCleared("debris-1"), Is.True);
        }

        [Test]
        public void RecoveryAndRestoredProgress_RestoreState_PreservesTargets()
        {
            var recovery = new RecoveryProgress(
                new[] { "s1", "s2" },
                new[] { "p1", "p2" },
                new[] { "w1", "w2" },
                new[] { "h1" });
            recovery.RestoreState(
                RecoveryStage.Ecosystem,
                new[] { "s1", "s2" },
                new[] { "p1", "p2" },
                new[] { "p1", "p2" },
                new[] { "p1", "p2" },
                new[] { "w1" },
                new[] { "h1" });

            Assert.That(recovery.Stage, Is.EqualTo(RecoveryStage.Ecosystem));
            Assert.That(recovery.Watered, Is.EqualTo(2));
            Assert.That(recovery.Waste, Is.EqualTo(1));
            Assert.That(recovery.Habitats, Is.EqualTo(1));
            Assert.That(recovery.IsWasteCleaned("w1"), Is.True);

            var restored = new RestoredProgress("final-bed", "viewpoint");
            restored.RestoreState(RestoredStage.Viewpoint, false, true, true, false, false);
            Assert.That(restored.Stage, Is.EqualTo(RestoredStage.Viewpoint));
            Assert.That(restored.Planted, Is.True);
            Assert.That(restored.Watered, Is.True);
        }

        [Test]
        public void SerializeAndDeserialize_RoundTripsActiveSave_RejectsCompletedSave()
        {
            var active = new SavedProgressData
            {
                sceneName = "FromAshes",
                active = true,
                elapsedSeconds = 42.5f,
                playerX = 3f,
                playerY = 0.2f,
                playerZ = -8f,
                playerYaw = 135f,
                disasterStage = (int)DisasterStage.StopTheFlames,
                hasEquipment = true,
                extinguishedFires = new[] { "fire-0", "fire-3" },
                clearedDebris = new string[0],
                restoredStage = (int)RestoredStage.Dormant
            };

            string json = GameSaveSystem.Serialize(active);
            bool loaded = GameSaveSystem.TryDeserialize(json, "FromAshes", out SavedProgressData parsed);

            Assert.That(loaded, Is.True);
            Assert.That(parsed.elapsedSeconds, Is.EqualTo(42.5f).Within(0.01f));
            Assert.That(parsed.playerYaw, Is.EqualTo(135f).Within(0.01f));
            Assert.That(parsed.extinguishedFires, Is.EquivalentTo(new[] { "fire-0", "fire-3" }));

            bool wrongScene = GameSaveSystem.TryDeserialize(json, "OtherScene", out _);
            Assert.That(wrongScene, Is.False);

            active.restoredStage = (int)RestoredStage.Complete;
            string completedJson = GameSaveSystem.Serialize(active);
            Assert.That(GameSaveSystem.TryDeserialize(completedJson, "FromAshes", out _), Is.False);
        }
    }
}
