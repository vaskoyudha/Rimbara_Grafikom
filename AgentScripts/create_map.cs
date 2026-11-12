using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

var sb = new StringBuilder();
var src = SceneManager.GetActiveScene();
if (src.name != "VillageRestoration") throw new System.Exception("Open VillageRestoration first, got " + src.name);

const string targetPath = "Assets/Scenes/Arunika.unity";
EditorSceneManager.SaveScene(src, targetPath);
var scene = SceneManager.GetSceneByPath(targetPath);
sb.AppendLine("created " + targetPath + " valid=" + scene.IsValid() + " roots=" + scene.rootCount);

// Remove everything that belongs to the copied village demo.
string[] villageRoots = {
    "Bushes", "Clifs_MDG", "Fence_Wall_01", "Environments", "Buildings", "Fences", "Props",
    "rest", "Flats", "Ground", "Grasses", "Plants", "Rocks", "Terrain", "treeses", "Restoration Ground"
};
foreach (var name in villageRoots)
{
    var go = scene.GetRootGameObjects().FirstOrDefault(x => x.name == name);
    if (go != null) { Object.DestroyImmediate(go); sb.AppendLine("removed " + name); }
}

// Terrain layers from the stylized EmaceArt textures.
const string texDir = "Assets/EmaceArt/Slavic World Free/Texture/";
System.IO.Directory.CreateDirectory("Assets/Art/Terrain");

TerrainLayer MakeLayer(string assetPath, string textureName, Vector2 tile, string normalName = null)
{
    var layer = new TerrainLayer();
    layer.diffuseTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(texDir + textureName);
    if (!string.IsNullOrEmpty(normalName)) layer.normalMapTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(texDir + normalName);
    layer.tileSize = tile;
    layer.metallic = 0f;
    layer.smoothness = 0.1f;
    AssetDatabase.DeleteAsset(assetPath);
    AssetDatabase.CreateAsset(layer, assetPath);
    return layer;
}

var grassLayer = MakeLayer("Assets/Art/Terrain/Grass.terrainlayer", "Grass_Texture.png", new Vector2(6f, 6f));
var flowerLayer = MakeLayer("Assets/Art/Terrain/Flowers.terrainlayer", "Flower_Grass.png", new Vector2(6f, 6f), "Flower_Grass_NM.png");
var dirtLayer = MakeLayer("Assets/Art/Terrain/Dirt.terrainlayer", "Pobbles_Sand_Dirt.png", new Vector2(5f, 5f));
AssetDatabase.SaveAssets();
sb.AppendLine("layers ready");

// Terrain object.
var terrainGo = Terrain.CreateTerrainGameObject(new TerrainData());
terrainGo.name = "Arunika Terrain";
terrainGo.transform.position = new Vector3(-90f, 0f, -90f);
var terrain = terrainGo.GetComponent<Terrain>();
var data = terrain.terrainData;
data.heightmapResolution = 513;
data.alphamapResolution = 512;
data.baseMapResolution = 1024;
data.size = new Vector3(180f, 55f, 180f);
data.terrainLayers = new[] { grassLayer, flowerLayer, dirtLayer };

int res = data.heightmapResolution;
var heights = new float[res, res];
float Height01(float x, float z)
{
    float r = Mathf.Sqrt(x * x + z * z);
    float n = Mathf.PerlinNoise(x * 0.025f + 11.3f, z * 0.025f + 5.7f);
    float n2 = Mathf.PerlinNoise(x * 0.07f + 31.1f, z * 0.07f + 17.9f);
    float rim = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(34f, 88f, r));
    float h = rim * (16f + 9f * n + 5f * n2);
    h += (n2 - 0.5f) * 1.1f * Mathf.InverseLerp(12f, 40f, r);
    h += (n - 0.5f) * 0.5f;
    if (r < 30f) h *= Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(22f, 30f, r));
    return Mathf.Clamp01((Mathf.Max(0f, h) + 0.05f) / data.size.y);
}

for (int y = 0; y < res; y++)
    for (int x = 0; x < res; x++)
    {
        float wx = (float)x / (res - 1) * 180f - 90f;
        float wz = (float)y / (res - 1) * 180f - 90f;
        heights[y, x] = Height01(wx, wz);
    }
data.SetHeights(0, 0, heights);

// Terrain paint: grass base, flower patches, dirt path and burnt ring.
Vector2[] path =
{
    new Vector2(0f, -40f), new Vector2(0f, -20f), new Vector2(0f, -8f),
    new Vector2(5f, -3f), new Vector2(12f, 3f)
};
Vector2[] sidePaths =
{
    new Vector2(0f, -8f), new Vector2(-8f, -12f), new Vector2(-12f, -8f)
};

float DistanceToPath(float x, float z, Vector2[] pts)
{
    float best = float.MaxValue;
    for (int i = 0; i < pts.Length - 1; i++)
    {
        Vector2 a = pts[i], b = pts[i + 1];
        Vector2 ab = b - a, ap = new Vector2(x, z) - a;
        float t = Mathf.Clamp01(Vector2.Dot(ap, ab) / Mathf.Max(0.001f, ab.sqrMagnitude));
        best = Mathf.Min(best, Vector2.Distance(new Vector2(x, z), a + ab * t));
    }
    return best;
}

int ares = data.alphamapResolution;

var splat = new float[ares, ares, 3];
for (int y = 0; y < ares; y++)
    for (int x = 0; x < ares; x++)
    {
        float wx = (float)x / (ares - 1) * 180f - 90f;
        float wz = (float)y / (ares - 1) * 180f - 90f;
        float pathDist = Mathf.Min(DistanceToPath(wx, wz, path), DistanceToPath(wx, wz, sidePaths));
        // world-space slope
        float hx = Height01(wx + 1f, wz) - Height01(wx - 1f, wz);
        float hz = Height01(wx, wz + 1f) - Height01(wx, wz - 1f);
        float slope = Mathf.Sqrt(hx * hx + hz * hz) * data.size.y * 0.5f;

        float dirt = pathDist < 3.2f ? 1f : 0f;
        dirt = Mathf.Max(dirt, Mathf.InverseLerp(0.55f, 1.4f, slope));
        float flower = Mathf.InverseLerp(0.55f, 0.75f, Mathf.PerlinNoise(wx * 0.09f + 61f, wz * 0.09f + 23f));
        flower *= 1f - dirt;
        float grass = Mathf.Clamp01(1f - dirt - flower * 0.55f);
        float total = grass + flower + dirt;
        splat[y, x, 0] = grass / total;
        splat[y, x, 1] = flower / total;
        splat[y, x, 2] = dirt / total;
    }
data.SetAlphamaps(0, 0, splat);

// Lighting and atmosphere for the burnt valley.
RenderSettings.fog = true;
RenderSettings.fogMode = FogMode.ExponentialSquared;
RenderSettings.fogDensity = 0.0045f;
RenderSettings.fogColor = new Color(0.62f, 0.66f, 0.60f);
RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
RenderSettings.ambientSkyColor = new Color(0.62f, 0.68f, 0.70f);
RenderSettings.ambientEquatorColor = new Color(0.42f, 0.45f, 0.38f);
RenderSettings.ambientGroundColor = new Color(0.20f, 0.19f, 0.15f);

var sun = scene.GetRootGameObjects().Select(x => x.GetComponent<Light>()).FirstOrDefault(l => l != null && l.type == LightType.Directional);
if (sun != null)
{
    sun.transform.rotation = Quaternion.Euler(42f, -35f, 0f);
    sun.intensity = 1.15f;
    sun.color = new Color(1f, 0.95f, 0.85f);
    sun.shadows = LightShadows.Soft;
    sb.AppendLine("sun tuned");
}

EditorSceneManager.MarkSceneDirty(scene);
EditorSceneManager.SaveScene(scene);
sb.AppendLine("SAVED " + scene.path);
return sb.ToString();
