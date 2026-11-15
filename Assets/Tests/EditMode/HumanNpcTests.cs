using System.Linq;
using NUnit.Framework;
using Rimbara.Interaction;
using Rimbara.Player;
using Rimbara.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Rimbara.Tests
{
    public sealed class HumanNpcTests
    {
        [OneTimeSetUp]
        public void LoadAuthoredScene() { EditorSceneManager.OpenScene("Assets/Scenes/FromAshes.unity", OpenSceneMode.Single); }

        [Test]
        public void TenVisualsHaveEightUniqueIdentities()
        {
            HumanNpcVisual[] visuals = Object.FindObjectsByType<HumanNpcVisual>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            Assert.That(visuals.Length, Is.EqualTo(10));
            Assert.That(visuals.Select(v => v.CharacterId).Distinct().Count(), Is.EqualTo(8));
        }

        [TestCase("Coordinator")]
        [TestCase("Ranger")]
        [TestCase("Elder")]
        [TestCase("Volunteer")]
        [TestCase("ResidentWoman")]
        [TestCase("VillagerMan")]
        [TestCase("VillagerWoman")]
        [TestCase("VillagerSenior")]
        public void GeneratedAssetHasValidHumanAvatar(string id)
        {
            GameObject model = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Art/Characters/HumanNPCs/" + id + ".fbx");
            Assert.That(model, Is.Not.Null);
            Animator animator = model.GetComponent<Animator>();
            Assert.That(animator, Is.Not.Null);
            Assert.That(animator.avatar.isValid && animator.avatar.isHuman, Is.True);
            Assert.That(model.GetComponentsInChildren<SkinnedMeshRenderer>().Length, Is.GreaterThanOrEqualTo(5));
            foreach (SkinnedMeshRenderer renderer in model.GetComponentsInChildren<SkinnedMeshRenderer>())
                Assert.That(renderer.sharedMaterials.All(m => m != null && m.shader.name == "Universal Render Pipeline/Lit"), Is.True, renderer.name);
        }

        [Test]
        public void NpcVisualsDoNotReplacePlayerOrInteractionRoots()
        {
            VolunteerRig player = Object.FindFirstObjectByType<VolunteerRig>();
            Assert.That(player, Is.Not.Null);
            Assert.That(player.GetComponentsInChildren<HumanNpcVisual>(true), Is.Empty);
            Assert.That(Object.FindObjectsByType<CoordinatorDialogue>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length, Is.EqualTo(1));
            Assert.That(Object.FindObjectsByType<RangerDialogue>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length, Is.EqualTo(1));
            Assert.That(Object.FindObjectsByType<ElderDialogue>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length, Is.EqualTo(1));
            foreach (HumanNpcVisual visual in Object.FindObjectsByType<HumanNpcVisual>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                Assert.That(visual.GetComponentsInChildren<Collider>(true), Is.Empty, visual.name);
                Assert.That(visual.GetComponentInParent<VolunteerRig>(), Is.Null);
            }
        }
    }
}
