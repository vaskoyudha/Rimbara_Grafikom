using System;
using NUnit.Framework;
using Rimbara.Mission;

namespace Rimbara.Tests
{
    public sealed class RestoredProgressTests
    {
        private static RestoredProgress Create() => new RestoredProgress("final-bed", "viewpoint");
        private static RestoredProgress Briefing()
        { var p = Create(); p.Begin(true, true); p.CompleteTransition(true); return p; }
        private static RestoredProgress Ending()
        { var p = Briefing(); p.AcceptBriefing(true); p.Plant("final-bed", true); p.Water("final-bed", true); p.Visit("viewpoint", true); return p; }
        [Test] public void StartsDormantWithNoFinalSupply()
        {
            var p = Create(); Assert.That(p.Stage, Is.EqualTo(RestoredStage.Dormant)); Assert.That(p.HasSeedling, Is.False);
            Assert.That(p.AcceptBriefing(true), Is.False); Assert.That(p.Plant("final-bed", true), Is.False); Assert.That(p.Visit("viewpoint", true), Is.False);
        }
        [TestCase(false, true)] [TestCase(true, false)] [TestCase(false, false)]
        public void BeginRequiresRecoveryCompleteAndRunning(bool complete, bool running) => Assert.That(Create().Begin(complete, running), Is.False);
        [Test] public void TransitionCannotBeBypassedOrRepeated()
        {
            var p = Create(); Assert.That(p.CompleteTransition(true), Is.False); Assert.That(p.Begin(true, true), Is.True);
            Assert.That(p.Begin(true, true), Is.False); Assert.That(p.AcceptBriefing(true), Is.False);
            Assert.That(p.CompleteTransition(false), Is.False); Assert.That(p.CompleteTransition(true), Is.True); Assert.That(p.CompleteTransition(true), Is.False);
        }
        [Test] public void ElderGrantsOneDedicatedSeedlingOnlyOnce()
        {
            var p = Briefing(); Assert.That(p.AcceptBriefing(false), Is.False); Assert.That(p.HasSeedling, Is.False);
            Assert.That(p.AcceptBriefing(true), Is.True); Assert.That(p.HasSeedling, Is.True); Assert.That(p.AcceptBriefing(true), Is.False);
        }
        [TestCase(null)] [TestCase("")] [TestCase("wrong")]
        public void OnlyRegisteredFinalTreeCanBePlanted(string id)
        { var p = Briefing(); p.AcceptBriefing(true); Assert.That(p.Plant(id, true), Is.False); Assert.That(p.HasSeedling, Is.True); }
        [Test] public void PlantingConsumesExactlyOneAndRequiresRunning()
        {
            var p = Briefing(); p.AcceptBriefing(true); Assert.That(p.Plant("final-bed", false), Is.False);
            Assert.That(p.Plant("final-bed", true), Is.True); Assert.That(p.HasSeedling, Is.False); Assert.That(p.Planted, Is.True);
            Assert.That(p.Plant("final-bed", true), Is.False); Assert.That(p.Stage, Is.EqualTo(RestoredStage.FinalWatering));
        }
        [Test] public void WaterRequiresPlantAndRejectsPauseAndWrongId()
        {
            var p = Briefing(); p.AcceptBriefing(true); Assert.That(p.Water("final-bed", true), Is.False); p.Plant("final-bed", true);
            Assert.That(p.Water("final-bed", false), Is.False); Assert.That(p.Water("wrong", true), Is.False);
            Assert.That(p.Water("final-bed", true), Is.True); Assert.That(p.Water("final-bed", true), Is.False);
        }
        [Test] public void ViewpointRequiresWaterAndRegisteredId()
        {
            var p = Briefing(); p.AcceptBriefing(true); p.Plant("final-bed", true); Assert.That(p.Visit("viewpoint", true), Is.False);
            p.Water("final-bed", true); Assert.That(p.Visit("wrong", true), Is.False); Assert.That(p.Visit("viewpoint", false), Is.False);
            Assert.That(p.Visit("viewpoint", true), Is.True); Assert.That(p.Visit("viewpoint", true), Is.False);
        }
        [TestCase(false)] [TestCase(true)]
        public void OnlyStartedEndingCanFinishOrSkip(bool skip)
        {
            var p = Briefing(); Assert.That(p.FinishEnding(true, skip), Is.False);
            p = Ending(); Assert.That(p.FinishEnding(false, skip), Is.False); Assert.That(p.Stage, Is.EqualTo(RestoredStage.Ending));
            Assert.That(p.FinishEnding(true, skip), Is.True); Assert.That(p.EndingSkipped, Is.EqualTo(skip)); Assert.That(p.Stage, Is.EqualTo(RestoredStage.Complete));
            Assert.That(p.FinishEnding(true, skip), Is.False);
        }
        [Test] public void EndingIsNotVictoryUntilFinished()
        { var p = Ending(); Assert.That(p.Planted && p.Watered && p.ViewpointVisited, Is.True); Assert.That(p.Stage, Is.Not.EqualTo(RestoredStage.Complete)); }
        [Test] public void ResetClearsAllFinalFlagsAndSupply()
        {
            var p = Ending(); p.FinishEnding(true, true); p.Reset();
            Assert.That(p.Stage, Is.EqualTo(RestoredStage.Dormant)); Assert.That(p.HasSeedling || p.Planted || p.Watered || p.ViewpointVisited || p.EndingSkipped, Is.False);
            Assert.That(p.Begin(true, true), Is.True);
        }
        [Test] public void InvalidOrDuplicateTargetIdsFailEarly()
        {
            Assert.Throws<ArgumentException>(() => new RestoredProgress("", "v")); Assert.Throws<ArgumentException>(() => new RestoredProgress("b", null));
            Assert.Throws<ArgumentException>(() => new RestoredProgress("same", "same"));
        }
        [Test] public void FinalMissionDoesNotConsumeRecoveryInventoryOrAlterItsTargets()
        {
            var r = new RecoveryProgress(new[] { "s" }, new[] { "b" }, new[] { "w" }, new[] { "h" });
            r.Begin(true, true); r.AcceptBriefing(true); r.Collect("s", true); r.Dig("b", true); r.Plant("b", true); r.Water("b", true);
            r.AcceptPlantingDebrief(true); r.CleanWaste("w", true); r.RestoreHabitat("h", true); r.AcceptEcosystemDebrief(true);
            var p = Briefing(); p.AcceptBriefing(true); p.Plant("final-bed", true);
            Assert.That(r.Carried, Is.Zero); Assert.That(r.Planted, Is.EqualTo(1)); Assert.That(r.Stage, Is.EqualTo(RecoveryStage.Complete));
        }
    }
}
