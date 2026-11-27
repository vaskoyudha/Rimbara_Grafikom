using UnityEngine;

namespace Rimbara.Mission
{
    public sealed class RestoredEnvironment : MonoBehaviour
    {
        [SerializeField] private RestoredChapter chapter;
        [SerializeField] private GameObject restoredVisuals;
        [SerializeField] private Renderer[] charredRenderers;
        [SerializeField] private GameObject[] youngTrees;
        [SerializeField] private Terrain terrain;
        [SerializeField] private TerrainLayer restoredLayer;
        [SerializeField] private Renderer river;
        [SerializeField] private Material riverMaterial;
        [SerializeField] private Material sky;
        [SerializeField] private Light sun;
        [SerializeField] private ParticleSystem smoke;
        private TerrainData originalTerrain;
        private TerrainData runtimeTerrain;
        private Material originalSky;
        public bool Applied { get; private set; }
        private void Awake()
        {
            if (chapter == null || restoredVisuals == null || charredRenderers == null || youngTrees == null || terrain == null
                || restoredLayer == null || river == null || riverMaterial == null || sky == null || sun == null || smoke == null)
                throw new System.InvalidOperationException("Restored environment requires its complete authored visual and terrain references.");
        }
        public void Configure(RestoredChapter owner, GameObject visuals, Renderer[] charred, GameObject[] saplings, Terrain ground,
            TerrainLayer greenSoil, Renderer water, Material cleanWater, Material cleanSky, Light light, ParticleSystem particles)
        { chapter = owner; restoredVisuals = visuals; charredRenderers = charred; youngTrees = saplings; terrain = ground; restoredLayer = greenSoil; river = water; riverMaterial = cleanWater; sky = cleanSky; sun = light; smoke = particles; }
        public void Apply()
        {
            if (Applied || chapter.Progress.Stage != RestoredStage.TimeTransition) return;
            ApplyInternal();
        }

        /// <summary>Applies the lush restored environment immediately when loading a saved post-transition run.</summary>
        public void ApplyFromSave()
        {
            if (Applied || chapter == null || chapter.Progress.Stage < RestoredStage.TimeTransition) return;
            ApplyInternal();
        }

        private void ApplyInternal()
        {
            Applied = true;
            originalSky = RenderSettings.skybox; RenderSettings.skybox = sky;
            RenderSettings.fog = true; RenderSettings.fogDensity = 0.0015f; RenderSettings.fogColor = new Color(0.66f, 0.78f, 0.72f);
            RenderSettings.ambientSkyColor = new Color(0.54f, 0.67f, 0.62f);
            RenderSettings.ambientEquatorColor = new Color(0.42f, 0.5f, 0.3f);
            sun.intensity = 1.65f; sun.color = new Color(1f, 0.96f, 0.83f);
            smoke.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            foreach (Renderer r in charredRenderers) if (r != null) r.enabled = false;
            foreach (GameObject tree in youngTrees) if (tree != null) tree.SetActive(false);
            restoredVisuals.SetActive(true);
            foreach (Transform t in FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (t.name.StartsWith("Beacon - ") || t.name.StartsWith("Recovery beacon - ")) t.gameObject.SetActive(false);
            river.sharedMaterial = riverMaterial;
            river.SetPropertyBlock(null);
            originalTerrain = terrain.terrainData;
            runtimeTerrain = Instantiate(originalTerrain);
            TerrainLayer[] layers = runtimeTerrain.terrainLayers;
            runtimeTerrain.terrainLayers = new[] { layers[0], layers[1], restoredLayer };
            int resolution = runtimeTerrain.alphamapResolution;
            float[,,] map = new float[resolution, resolution, 3];
            for (int z = 0; z < resolution; z++) for (int x = 0; x < resolution; x++)
            {
                float wx = terrain.transform.position.x + x * runtimeTerrain.size.x / (resolution - 1);
                float wz = terrain.transform.position.z + z * runtimeTerrain.size.z / (resolution - 1);
                float path = Mathf.Abs(wx) < 2.8f && wz < 18f ? 0.75f : 0.12f;
                map[z, x, 0] = path; map[z, x, 1] = 0f; map[z, x, 2] = 1f - path;
            }
            runtimeTerrain.SetAlphamaps(0, 0, map);
            terrain.terrainData = runtimeTerrain; terrain.GetComponent<TerrainCollider>().terrainData = runtimeTerrain;
        }
        private void OnDestroy()
        {
            if (!Applied) return;
            if (terrain != null && originalTerrain != null)
            { terrain.terrainData = originalTerrain; terrain.GetComponent<TerrainCollider>().terrainData = originalTerrain; }
            if (RenderSettings.skybox == sky) RenderSettings.skybox = originalSky;
            if (runtimeTerrain != null) Destroy(runtimeTerrain);
        }
    }
}
