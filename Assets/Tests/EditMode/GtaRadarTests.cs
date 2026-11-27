using NUnit.Framework;
using Rimbara.UI;
using UnityEngine;

namespace Rimbara.Tests
{
    public sealed class GtaRadarTests
    {
        [Test]
        public void WorldToNormalizedMapsExactTerrainCorners()
        {
            var min = new Vector2(-60f, -55f);
            var size = new Vector2(120f, 120f);

            Assert.That(GtaRadarMath.WorldToNormalized(new Vector3(-60f, 0f, -55f), min, size), Is.EqualTo(new Vector2(0f, 0f)));
            Assert.That(GtaRadarMath.WorldToNormalized(new Vector3(0f, 0f, 5f), min, size), Is.EqualTo(new Vector2(0.5f, 0.5f)));
            Assert.That(GtaRadarMath.WorldToNormalized(new Vector3(60f, 0f, 65f), min, size), Is.EqualTo(new Vector2(1f, 1f)));
        }

        [Test]
        public void RotateDeltaPointsCameraForwardStraightUp()
        {
            // Camera facing East (+X, yaw=90): target straight East (+10, 0) must sit straight UP (0, +10)
            Vector2 east = GtaRadarMath.RotateDelta(new Vector2(10f, 0f), 90f);
            Assert.That(east.x, Is.EqualTo(0f).Within(0.001f));
            Assert.That(east.y, Is.EqualTo(10f).Within(0.001f));

            // North (0, +10) while facing East (yaw=90) must sit straight LEFT (-10, 0)
            Vector2 north = GtaRadarMath.RotateDelta(new Vector2(0f, 10f), 90f);
            Assert.That(north.x, Is.EqualTo(-10f).Within(0.001f));
            Assert.That(north.y, Is.EqualTo(0f).Within(0.001f));

            // South (0, -10) while facing South (yaw=180) must sit straight UP (0, +10)
            Vector2 south = GtaRadarMath.RotateDelta(new Vector2(0f, -10f), 180f);
            Assert.That(south.x, Is.EqualTo(0f).Within(0.001f));
            Assert.That(south.y, Is.EqualTo(10f).Within(0.001f));
        }

        [Test]
        public void ClampToHalfSizePreservesDirectionAtPerimeter()
        {
            Vector2 inside = GtaRadarMath.ClampToHalfSize(new Vector2(40f, -20f), 90f, 90f, out bool wasClampedIn);
            Assert.That(wasClampedIn, Is.False);
            Assert.That(inside, Is.EqualTo(new Vector2(40f, -20f)));

            Vector2 outside = GtaRadarMath.ClampToHalfSize(new Vector2(180f, 90f), 90f, 90f, out bool wasClampedOut);
            Assert.That(wasClampedOut, Is.True);
            Assert.That(outside.x, Is.EqualTo(90f).Within(0.001f));
            Assert.That(outside.y, Is.EqualTo(45f).Within(0.001f));
        }
    }
}
