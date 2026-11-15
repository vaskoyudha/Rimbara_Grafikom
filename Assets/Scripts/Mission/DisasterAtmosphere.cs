using UnityEngine;

namespace Rimbara.Mission
{
    // A stylized chapter cue, not weather or a physical smoke simulation.
    public sealed class DisasterAtmosphere : MonoBehaviour
    {
        [SerializeField] private DisasterChapter chapter;
        [SerializeField] private Light sun;
        [SerializeField] private ParticleSystem overheadSmoke;
        public void ConfigureSmoke(ParticleSystem smoke) { overheadSmoke = smoke; }
        private Color originalAmbient;
        private Color originalSunColor;
        private float originalSunIntensity;
        private bool originalFog;
        private FogMode originalFogMode;
        private Color originalFogColor;
        private float originalFogDensity;

        public void Configure(DisasterChapter owner, Light directionalLight) { chapter = owner; sun = directionalLight; }

        private void Start()
        {
            originalAmbient = RenderSettings.ambientLight;
            originalFog = RenderSettings.fog;
            originalFogMode = RenderSettings.fogMode;
            originalFogColor = RenderSettings.fogColor;
            originalFogDensity = RenderSettings.fogDensity;
            if (sun != null) { originalSunColor = sun.color; originalSunIntensity = sun.intensity; }
            chapter.Changed += Refresh;
            Refresh();
        }

        private void Refresh()
        {
            bool contained = chapter.Progress.Stage >= DisasterStage.CleanTheWasteland;
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = contained ? new Color(0.48f, 0.52f, 0.46f) : new Color(0.29f, 0.27f, 0.23f);
            RenderSettings.fogDensity = contained ? 0.008f : 0.018f;
            RenderSettings.ambientLight = originalAmbient * (contained ? 0.85f : 0.65f);
            if (overheadSmoke != null)
            {
                var emission = overheadSmoke.emission;
                emission.rateOverTime = contained ? 0.4f : 2f;
            }
            if (sun != null)
            {
                sun.intensity = originalSunIntensity * (contained ? 0.85f : 0.6f);
                sun.color = contained ? originalSunColor : new Color(0.85f, 0.70f, 0.53f);
            }
        }

        private void OnDestroy()
        {
            if (chapter != null) chapter.Changed -= Refresh;
            RenderSettings.ambientLight = originalAmbient;
            RenderSettings.fog = originalFog;
            RenderSettings.fogMode = originalFogMode;
            RenderSettings.fogColor = originalFogColor;
            RenderSettings.fogDensity = originalFogDensity;
            if (sun != null) { sun.color = originalSunColor; sun.intensity = originalSunIntensity; }
        }
    }
}
