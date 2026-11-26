using NUnit.Framework;
using Rimbara.UI;
using UnityEditor;
using UnityEngine;

namespace Rimbara.Tests
{
    public sealed class UiKitTests
    {
        private static UiKit Load() => AssetDatabase.LoadAssetAtPath<UiKit>("Assets/UI/KingsforgeUiKit.asset");

        [Test]
        public void KingsforgeKitIsComplete()
        {
            UiKit kit = Load();
            if (kit == null) Assert.Ignore("Kingsforge kit not imported. Run RIMBARA/UI/Build Kingsforge theme.");
            Assert.That(kit.IsComplete, Is.True, string.Join(", ", kit.MissingRoles()));
            Assert.That(kit.Window.border, Is.Not.EqualTo(Vector4.zero), "Window must be 9-sliced.");
            Assert.That(kit.ButtonPrimaryNormal.border, Is.Not.EqualTo(Vector4.zero), "Button must be 9-sliced.");
        }

        [Test]
        public void EveryRequiredRoleIsAssigned()
        {
            UiKit kit = Load();
            if (kit == null) Assert.Ignore("Kingsforge kit not imported.");
            Assert.That(kit.MissingRoles(), Is.Empty);
        }
    }
}
