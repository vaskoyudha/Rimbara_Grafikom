using System;
using NUnit.Framework;
using Rimbara.Mission;

namespace Rimbara.Tests
{
    public sealed class RecoveryProgressTests
    {
        private static RecoveryProgress Create() => new RecoveryProgress(new[] { "s0", "s1" }, new[] { "b0", "b1" }, new[] { "w0", "w1" }, new[] { "h0" });
        private static RecoveryProgress Planting()
        {
            RecoveryProgress p = Create(); p.Begin(true, true); p.AcceptBriefing(true); return p;
        }
        private static RecoveryProgress Ecosystem()
        {
            RecoveryProgress p = Planting();
            for (int i = 0; i < 2; i++) { p.Collect("s" + i, true); p.Dig("b" + i, true); p.Plant("b" + i, true); p.Water("b" + i, true); }
            p.AcceptPlantingDebrief(true); return p;
        }

        [Test] public void StartsDormantWithNoInventoryOrRecovery()
        {
            var p = Create(); Assert.That(p.Stage, Is.EqualTo(RecoveryStage.Dormant)); Assert.That(p.Carried, Is.Zero); Assert.That(p.Fraction, Is.Zero);
            Assert.That(p.Collect("s0", true), Is.False); Assert.That(p.Dig("b0", true), Is.False);
        }
        [TestCase(false, true)] [TestCase(true, false)] [TestCase(false, false)]
        public void BeginRequiresCompletedDisasterAndRunning(bool ready, bool running) => Assert.That(Create().Begin(ready, running), Is.False);
        [Test] public void BriefingMustBeReadAndOnlyAcceptedOnce()
        {
            var p = Create(); Assert.That(p.AcceptBriefing(true), Is.False); Assert.That(p.Begin(true, true), Is.True);
            Assert.That(p.Collect("s0", true), Is.False); Assert.That(p.AcceptBriefing(false), Is.False);
            Assert.That(p.AcceptBriefing(true), Is.True); Assert.That(p.AcceptBriefing(true), Is.False); Assert.That(p.Begin(true, true), Is.False);
        }
        [TestCase(null)] [TestCase("")] [TestCase("unknown")]
        public void UnregisteredTargetsCannotEarnProgress(string id)
        {
            var p = Planting(); Assert.That(p.Collect(id, true), Is.False); Assert.That(p.Dig(id, true), Is.False);
            Assert.That(p.Plant(id, true), Is.False); Assert.That(p.Water(id, true), Is.False);
        }
        [Test] public void PlantRequiresBothDiggingAndCarriedSeedling()
        {
            var p = Planting(); Assert.That(p.Plant("b0", true), Is.False); Assert.That(p.Dig("b0", true), Is.True);
            Assert.That(p.Plant("b0", true), Is.False); Assert.That(p.Collect("s0", true), Is.True);
            Assert.That(p.Plant("b1", true), Is.False); Assert.That(p.Plant("b0", true), Is.True); Assert.That(p.Carried, Is.Zero);
        }
        [Test] public void WaterRequiresSpecificPlantedTree()
        {
            var p = Planting(); p.Collect("s0", true); p.Dig("b0", true); p.Plant("b0", true);
            Assert.That(p.Water("b1", true), Is.False); Assert.That(p.Water("b0", true), Is.True); Assert.That(p.Watered, Is.EqualTo(1));
            Assert.That(p.Stage, Is.EqualTo(RecoveryStage.Planting)); Assert.That(p.IsWatered("b0"), Is.True);
        }
        [Test] public void DuplicateActionsDoNotFarmProgress()
        {
            var p = Planting(); p.Collect("s0", true); p.Dig("b0", true); p.Plant("b0", true); p.Water("b0", true);
            Assert.That(p.Collect("s0", true), Is.False); Assert.That(p.Dig("b0", true), Is.False);
            Assert.That(p.Plant("b0", true), Is.False); Assert.That(p.Water("b0", true), Is.False); Assert.That(p.Carried, Is.Zero);
        }
        [Test] public void PlantingAllTreesWithoutWaterDoesNotUnlockEcosystem()
        {
            var p = Planting();
            for (int i = 0; i < 2; i++) { p.Collect("s" + i, true); p.Dig("b" + i, true); p.Plant("b" + i, true); }
            Assert.That(p.Stage, Is.EqualTo(RecoveryStage.Planting)); Assert.That(p.AcceptPlantingDebrief(true), Is.False);
            Assert.That(p.CleanWaste("w0", true), Is.False); Assert.That(p.RestoreHabitat("h0", true), Is.False);
            p.Water("b0", true); p.Water("b1", true);
            Assert.That(p.Stage, Is.EqualTo(RecoveryStage.PlantingDebrief)); Assert.That(p.CleanWaste("w0", true), Is.False);
        }
        [Test] public void PauseBlocksEveryPlantingMutation()
        {
            var p = Planting(); Assert.That(p.Collect("s0", false), Is.False); Assert.That(p.Dig("b0", false), Is.False);
            p.Collect("s0", true); p.Dig("b0", true); Assert.That(p.Plant("b0", false), Is.False);
            p.Plant("b0", true); Assert.That(p.Water("b0", false), Is.False);
        }
        [Test] public void WaterCleanupAndHabitatAreBothRequired()
        {
            var p = Ecosystem(); p.CleanWaste("w0", true); Assert.That(p.WaterClean, Is.False);
            p.CleanWaste("w1", true); Assert.That(p.WaterClean, Is.True); Assert.That(p.Stage, Is.EqualTo(RecoveryStage.Ecosystem));
            Assert.That(p.AcceptEcosystemDebrief(true), Is.False); p.RestoreHabitat("h0", true);
            Assert.That(p.Fraction, Is.EqualTo(1f)); Assert.That(p.Stage, Is.EqualTo(RecoveryStage.EcosystemDebrief));
            Assert.That(p.AcceptEcosystemDebrief(false), Is.False); Assert.That(p.AcceptEcosystemDebrief(true), Is.True);
            Assert.That(p.Stage, Is.EqualTo(RecoveryStage.Complete));
        }
        [Test] public void HabitatFirstDoesNotCleanWaterAutomatically()
        {
            var p = Ecosystem(); p.RestoreHabitat("h0", true); Assert.That(p.WaterClean, Is.False);
            Assert.That(p.Stage, Is.EqualTo(RecoveryStage.Ecosystem));
        }
        [Test] public void EcosystemRejectsPauseUnknownAndDuplicateTargets()
        {
            var p = Ecosystem(); Assert.That(p.CleanWaste("w0", false), Is.False); Assert.That(p.RestoreHabitat("h0", false), Is.False);
            Assert.That(p.CleanWaste("unknown", true), Is.False); Assert.That(p.RestoreHabitat("unknown", true), Is.False);
            p.CleanWaste("w0", true); p.RestoreHabitat("h0", true);
            Assert.That(p.CleanWaste("w0", true), Is.False); Assert.That(p.RestoreHabitat("h0", true), Is.False);
        }
        [Test] public void ResetClearsAllProgressAndAllowsFreshSession()
        {
            var p = Ecosystem(); p.CleanWaste("w0", true); p.RestoreHabitat("h0", true); p.Reset();
            Assert.That(p.Stage, Is.EqualTo(RecoveryStage.Dormant)); Assert.That(p.Fraction, Is.Zero);
            Assert.That(p.Collected + p.Dug + p.Planted + p.Watered + p.Waste + p.Habitats, Is.Zero);
            Assert.That(p.Begin(true, true), Is.True); Assert.That(p.AcceptBriefing(true), Is.True); Assert.That(p.Collect("s0", true), Is.True);
        }
        [Test] public void InvalidSupplyFailsEarly()
        {
            Assert.Throws<ArgumentException>(() => new RecoveryProgress(new[] { "s0" }, new[] { "b0", "b1" }, new[] { "w" }, new[] { "h" }));
            Assert.Throws<ArgumentException>(() => new RecoveryProgress(new[] { "s0", "s0" }, new[] { "b0" }, new[] { "w" }, new[] { "h" }));
            Assert.Throws<ArgumentException>(() => new RecoveryProgress(new[] { "s0" }, new[] { "b0" }, new string[0], new[] { "h" }));
            Assert.Throws<ArgumentException>(() => new RecoveryProgress(new[] { "s0" }, new[] { "b0" }, new[] { "w" }, new[] { "" }));
        }
    }
}
