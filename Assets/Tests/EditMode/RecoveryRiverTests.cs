using System.Linq;
using NUnit.Framework;
using Rimbara.Mission;
using Rimbara.World;

namespace Rimbara.Tests
{
    public sealed class RecoveryRiverTests
    {
        [Test]
        public void RiverHasTenDistinctDryBankTargets()
        {
            var points = Enumerable.Range(0, RecoveryRiverLayout.TrashCount).Select(RecoveryRiverLayout.TrashPosition).ToArray();
            Assert.That(points.Length, Is.EqualTo(10));
            Assert.That(points.Distinct().Count(), Is.EqualTo(10));
            Assert.That(points.All(p => UnityEngine.Mathf.Abs(p.x - RecoveryRiverLayout.CenterX(p.z)) > RecoveryRiverLayout.HalfWidth + 1f), Is.True);
            Assert.That(points.Count(p => p.x < RecoveryRiverLayout.CenterX(p.z)), Is.EqualTo(5));
            Assert.That(points.Count(p => p.x > RecoveryRiverLayout.CenterX(p.z)), Is.EqualTo(5));
        }
        [Test]
        public void NineTrashCleanupsAreNotEnough()
        {
            var p = new RecoveryProgress(new[] { "seed" }, new[] { "bed" }, Enumerable.Range(0, 10).Select(i => "w" + i), new[] { "habitat" });
            p.Begin(true, true); p.AcceptBriefing(true); p.Collect("seed", true); p.Dig("bed", true); p.Plant("bed", true); p.Water("bed", true); p.AcceptPlantingDebrief(true);
            for (int i = 0; i < 9; i++) Assert.That(p.CleanWaste("w" + i, true), Is.True);
            Assert.That(p.WaterClean, Is.False); Assert.That(p.Stage, Is.EqualTo(RecoveryStage.Ecosystem));
            Assert.That(p.CleanWaste("w8", true), Is.False); Assert.That(p.CleanWaste("w9", false), Is.False);
            Assert.That(p.CleanWaste("w9", true), Is.True); Assert.That(p.WaterClean, Is.True);
            Assert.That(p.Stage, Is.EqualTo(RecoveryStage.Ecosystem));
            p.RestoreHabitat("habitat", true); Assert.That(p.Fraction, Is.EqualTo(1f));
            p.Reset(); Assert.That(p.Waste, Is.Zero); Assert.That(p.WaterClean, Is.False);
        }
        [Test]
        public void ChannelLeavesOriginalPlantingClearingsOutsideBanks()
        {
            Assert.That(UnityEngine.Mathf.Abs(-8f - RecoveryRiverLayout.CenterX(27f)), Is.GreaterThan(2.35f));
            Assert.That(UnityEngine.Mathf.Abs(7f - RecoveryRiverLayout.CenterX(38f)), Is.GreaterThan(2.35f));
            Assert.That(RecoveryRiverLayout.EndZ - RecoveryRiverLayout.StartZ, Is.EqualTo(27f));
        }
    }
}
