using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

var sb = new StringBuilder();
var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
if (scene.name != "Arunika") throw new System.Exception("wrong scene " + scene.name);

var terrain = Object.FindFirstObjectByType<Terrain>();
var data = terrain.terrainData;

var grassLayer = AssetDatabase.LoadAssetAtPath<TerrainLayer>("Assets/Art/Terrain/Grass.terrainlayer");
var dirtLayer = AssetDatabase.LoadAssetAtPath<TerrainLayer>("Assets/Art/Terrain/Dirt.terrainlayer");
if (grassLayer == null || dirtLayer == null) throw new System.Exception("layer assets missing");

grassLayer.diffuseTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/EmaceArt/Slavic World Free/Texture/Flower_Grass.png");
grassLayer.normalMapTexture = null;
grassLayer.tileSize = new Vector2(9f, 9f);
dirtLayer.tileSize = new Vector2(7f, 7f);
EditorUtility.SetDirty(grassLayer);
EditorUtility.SetDirty(dirtLayer);
AssetDatabase.SaveAssets();

data.terrainLayers = new[] { grassLayer, dirtLayer };

Vector2[] path =
{
    new Vector2(0f, -40f), new Vector2(0f, -20f), new Vector2(0f, -8f),
    new Vector2(5f, -3f), new Vector2(12f, 3f)
};
Vector2[] sidePath = { new Vector2(0f, -8f), new Vector2(-8f, -12f), new Vector2(-12f, -8f) };
Vector2[] firePath = { new Vector2(0f, -8f), new Vector2(-4f, 4f), new Vector2(-5f, 12f) };

float Dist(float x, float z, Vector2[] pts)
{
    float best = float.MaxValue;
    for (int i = 0; i < pts.Length - 1; i++)
    {
        Vector2 a = pts[i], b = pts[i + 1];
        Vector2 ab = b - a, ap = new Vector2(x, z) - a;
        float t = Mathf.Clamp01(Vector2.Dot(ap, ab) / Mathf.Max(0.0001f, ab.sqrMagnitude));
        best = Mathf.Min(best, Vector2.Distance(new Vector2(x, z), a + ab * t));
    }
    return best;
}

float H(float x, float z)
{
    float r = Mathf.Sqrt(x * x + z * z);
    float n = Mathf.PerlinNoise(x * 0.025f + 11.3f, z * 0.025f + 5.7f);
    float n2 = Mathf.PerlinNoise(x * 0.07f + 31.1f, z * 0.07f + 17.9f);
    float rim = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(34f, 88f, r));
    float h = rim * (16f + 9f * n + 5f * n2);
    h += (n2 - 0.5f) * 1.1f * Mathf.InverseLerp(12f, 40f, r);
    h += (n - 0.5f) * 0.5f;
    if (r < 30f) h *= Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(22f, 30f, r));
    return Mathf.Max(0f, h);
}

int ares = data.alphamapResolution;
var splat = new float[ares, ares, 2];
for (int y = 0; y < ares; y++)
    for (int x = 0; x < ares; x++)
    {
        float wx = (float)x / (ares - 1) * 180f - 90f;
        float wz = (float)y / (ares - 1) * 180f - 90f;
        float pd = Mathf.Min(Mathf.Min(Dist(wx, wz, path), Dist(wx, wz, sidePath)), Dist(wx, wz, firePath));
        float hx = H(wx + 1f, wz) - H(wx - 1f, wz);
        float hz = H(wx, wz + 1f) - H(wx, wz - 1f);
        float slope = Mathf.Sqrt(hx * hx + hz * hz) * 0.5f;
        float dirt = pd < 3.2f ? 1f : Mathf.InverseLerp(0.55f, 1.3f, slope);
        // wider worn soil around the burnt fire clearing
        dirt = Mathf.Max(dirt, Mathf.InverseLerp(11f, 4f, Vector2.Distance(new Vector2(wx, wz), new Vector2(-2f, 15f))) * 0.85f);
        dirt = Mathf.Max(dirt, Mathf.InverseLerp(9f, 2f, Vector2.Distance(new Vector2(wx, wz), new Vector2(0f, -31f))) * 0.6f);
        splat[y, x, 0] = 1f - dirt;
        splat[y, x, 1] = dirt;
    }
data.SetAlphamaps(0, 0, splat);

EditorSceneManager.MarkSceneDirty(scene);
EditorSceneManager.SaveScene(scene);
sb.AppendLine("layers=" + data.terrainLayers.Length + " saved");
return sb.ToString();
