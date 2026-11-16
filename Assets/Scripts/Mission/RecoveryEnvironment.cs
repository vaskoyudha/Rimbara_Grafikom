using UnityEngine;

namespace Rimbara.Mission
{
    // Modest local recovery, not instant mature forest growth.
    public sealed class RecoveryEnvironment : MonoBehaviour
    {
        [SerializeField] private RecoveryChapter chapter;
        [SerializeField] private Renderer water;
        [SerializeField] private Material affectedWater;
        [SerializeField] private Material cleanerWater;
        [SerializeField] private GameObject wildlifeSigns;
        [SerializeField] private ParticleSystem smoke;
        [SerializeField] private Light sun;
        private Material recoverySky;
        private Material initialSky;
        public bool RecoverySkyActive => recoverySky != null && RenderSettings.skybox == recoverySky;
        public bool WildlifeReturned => chapter.Progress != null && chapter.Progress.WaterClean && chapter.Progress.Habitats == chapter.Progress.HabitatGoal;
        public void ConfigureWater(Renderer renderer, Material affected, Material cleaner)
        { water = renderer; affectedWater = affected; cleanerWater = cleaner; }
        public void Configure(RecoveryChapter owner, Renderer pool, Material before, Material after, GameObject wildlife, ParticleSystem overhead, Light light)
        { chapter = owner; water = pool; affectedWater = before; cleanerWater = after; wildlifeSigns = wildlife; smoke = overhead; sun = light; }
        private void LateUpdate()
        {
            if (chapter.Restored != null && chapter.Restored.WorldApplied) return;
            wildlifeSigns.SetActive(WildlifeReturned);
            water.sharedMaterial = chapter.Progress.WaterClean ? cleanerWater : affectedWater;
            if (!chapter.Active) return;
            if (recoverySky == null && RenderSettings.skybox != null)
            {
                initialSky = RenderSettings.skybox;
                recoverySky = new Material(initialSky);
                recoverySky.SetColor("_SkyTint", new Color(0.43f, 0.48f, 0.48f));
                recoverySky.SetFloat("_AtmosphereThickness", 1.45f);
                recoverySky.SetFloat("_Exposure", 0.8f);
                RenderSettings.skybox = recoverySky;
            }
            RenderSettings.fogColor = new Color(0.51f, 0.56f, 0.48f);
            RenderSettings.fogDensity = Mathf.Lerp(0.006f, 0.0035f, chapter.Progress.Fraction);
            if (sun != null)
            {
                sun.intensity = Mathf.Lerp(0.95f, 1.2f, chapter.Progress.Fraction);
                sun.color = new Color(0.88f, 0.85f, 0.7f);
            }
            if (smoke != null) { var emission = smoke.emission; emission.rateOverTime = 0.15f; }
        }
        private void OnDestroy()
        {
            if (recoverySky == null) return;
            if (RenderSettings.skybox == recoverySky) RenderSettings.skybox = initialSky;
            Destroy(recoverySky);
        }
    }
}
