using Rimbara.Mission;
using UnityEngine;

namespace Rimbara.World
{
    public sealed class RecoveryRiver : MonoBehaviour
    {
        [SerializeField] private RecoveryChapter chapter;
        [SerializeField] private Renderer surface;
        [SerializeField] private GameObject recoveredBanks;
        [SerializeField] private AudioSource flow;
        private MaterialPropertyBlock properties;
        public float CleanupFraction => chapter.Progress == null ? 0f : (float)chapter.Progress.Waste / chapter.Progress.WasteGoal;
        public bool BanksRecovered => recoveredBanks.activeSelf;
        public bool FlowAudible => flow != null && flow.isPlaying && flow.volume > 0f;
        public void Configure(RecoveryChapter owner, Renderer water, GameObject vegetation, AudioSource sound)
        { chapter = owner; surface = water; recoveredBanks = vegetation; flow = sound; }
        private void Awake()
        {
            if (chapter == null || surface == null || recoveredBanks == null || flow == null || flow.clip == null)
                throw new System.InvalidOperationException("River needs its chapter, water surface, recovered banks and flow audio.");
            properties = new MaterialPropertyBlock();
        }
        private void LateUpdate()
        {
            if (chapter.Restored != null && chapter.Restored.WorldApplied) return;
            surface.GetPropertyBlock(properties);
            properties.SetFloat("_Recovery", CleanupFraction);
            surface.SetPropertyBlock(properties);
            recoveredBanks.SetActive(chapter.Progress.WaterClean);
            if (!flow.isPlaying) flow.Play();
            flow.volume = chapter.Active ? 0.22f : 0.08f;
            // Keep ambience subject to the same master volume as all game audio.
            flow.ignoreListenerPause = false;
        }
    }
}
