using System.IO;
using UnityEditor;
using UnityEngine;

namespace Rimbara.Editor
{
    /// <summary>
    /// Bakes a high-resolution orthographic top-down map of the active terrain
    /// and generates crisp radar overlay sprites (player chevron, FOV wedge, blips).
    /// </summary>
    public static class TopDownMapBaker
    {
        public const string Folder = "Assets/Art/UI/Map";
        public const string MapPath = Folder + "/ArunikaTopDownMap.png";
        public const string ChevronPath = Folder + "/RadarPlayerChevron.png";
        public const string FovPath = Folder + "/RadarFovCone.png";
        public const string DiamondPath = Folder + "/RadarBlipDiamond.png";
        public const string DotPath = Folder + "/RadarBlipDot.png";
        public const string BezelPath = Folder + "/RadarBezelFrame.png";

        [MenuItem("RIMBARA/UI/Bake Arunika Top-Down Map")]
        public static void BakeToProject()
        {
            Directory.CreateDirectory(Folder);
            BakeWorldTexture(out Vector2 worldMin, out Vector2 worldSize);
            CreateRadarSprites();
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

            ImportSprite(MapPath, mipmaps: true, Vector4.zero, FilterMode.Bilinear);
            ImportSprite(ChevronPath, mipmaps: false, Vector4.zero, FilterMode.Bilinear);
            ImportSprite(FovPath, mipmaps: false, Vector4.zero, FilterMode.Bilinear);
            ImportSprite(DiamondPath, mipmaps: false, Vector4.zero, FilterMode.Bilinear);
            ImportSprite(DotPath, mipmaps: false, Vector4.zero, FilterMode.Bilinear);
            ImportSprite(BezelPath, mipmaps: false, new Vector4(16f, 16f, 16f, 16f), FilterMode.Bilinear);

            Debug.Log($"TopDownMapBaker complete: bounds min={worldMin}, size={worldSize}");
        }

        public static void BakeWorldTexture(out Vector2 worldMin, out Vector2 worldSize)
        {
            Terrain terrain = Terrain.activeTerrain;
            if (terrain != null && terrain.terrainData != null)
            {
                Vector3 tPos = terrain.transform.position;
                Vector3 tSize = terrain.terrainData.size;
                worldMin = new Vector2(tPos.x, tPos.z);
                float maxSpan = Mathf.Max(tSize.x, tSize.z);
                worldSize = new Vector2(maxSpan, maxSpan);
            }
            else
            {
                worldMin = new Vector2(-60f, -55f);
                worldSize = new Vector2(120f, 120f);
            }

            Vector2 center = worldMin + worldSize * 0.5f;
            float baseY = terrain != null ? terrain.transform.position.y : 0f;

            const int size = 1024;
            var rt = new RenderTexture(size, size, 24, RenderTextureFormat.ARGB32);
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);

            var camGo = new GameObject("TempMapBakeCamera");
            var cam = camGo.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = worldSize.x * 0.5f;
            cam.nearClipPlane = 1f;
            cam.farClipPlane = 180f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.11f, 0.09f, 0.07f, 1f);
            cam.cullingMask = ~(1 << 5); // Exclude UI layer
            cam.transform.position = new Vector3(center.x, baseY + 110f, center.y);
            cam.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            cam.targetTexture = rt;

            // Hide volumetric smoke/fire particles and 3D text beacons so ground roads, river & buildings read cleanly.
            var disabledParticles = new System.Collections.Generic.List<ParticleSystemRenderer>();
            foreach (ParticleSystemRenderer psr in Object.FindObjectsByType<ParticleSystemRenderer>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
            {
                if (!psr.enabled) continue;
                psr.enabled = false;
                disabledParticles.Add(psr);
            }

            var disabledTexts = new System.Collections.Generic.List<TMPro.TextMeshPro>();
            foreach (TMPro.TextMeshPro tmp in Object.FindObjectsByType<TMPro.TextMeshPro>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
            {
                if (!tmp.enabled) continue;
                tmp.enabled = false;
                disabledTexts.Add(tmp);
            }

            bool prevFog = RenderSettings.fog;
            Color prevAmbient = RenderSettings.ambientLight;
            RenderSettings.fog = false;
            RenderSettings.ambientLight = new Color(0.76f, 0.72f, 0.65f, 1f);

            RenderTexture prevActive = RenderTexture.active;
            try
            {
                cam.Render();
                RenderTexture.active = rt;
                tex.ReadPixels(new Rect(0, 0, size, size), 0, 0);
                tex.Apply();
                ApplyCartographicContrast(tex);
                File.WriteAllBytes(MapPath, tex.EncodeToPNG());
            }
            finally
            {
                RenderSettings.fog = prevFog;
                RenderSettings.ambientLight = prevAmbient;
                foreach (ParticleSystemRenderer psr in disabledParticles)
                    if (psr != null) psr.enabled = true;
                foreach (TMPro.TextMeshPro tmp in disabledTexts)
                    if (tmp != null) tmp.enabled = true;
                RenderTexture.active = prevActive;
                cam.targetTexture = null;
                Object.DestroyImmediate(camGo);
                Object.DestroyImmediate(rt);
                Object.DestroyImmediate(tex);
            }
        }

        private static void ApplyCartographicContrast(Texture2D tex)
        {
            Color32[] pixels = tex.GetPixels32();
            int w = tex.width;
            int h = tex.height;
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    int idx = y * w + x;
                    Color c = pixels[idx];
                    // Slight warm-tactical boost so paths, roofs, trees & river contrast cleanly under amber/cream blips.
                    float lum = c.r * 0.299f + c.g * 0.587f + c.b * 0.114f;
                    c.r = Mathf.Clamp01(Mathf.Lerp(lum * 1.10f, c.r * 1.18f, 0.72f) + 0.03f);
                    c.g = Mathf.Clamp01(Mathf.Lerp(lum * 1.04f, c.g * 1.15f, 0.72f) + 0.025f);
                    c.b = Mathf.Clamp01(Mathf.Lerp(lum * 0.94f, c.b * 1.08f, 0.72f) + 0.015f);
                    // Subtle tactical grid lines every 128px (~15m)
                    if ((x % 128 == 0 || y % 128 == 0) && x > 0 && y > 0)
                        c = Color.Lerp(c, new Color(0.85f, 0.72f, 0.42f, 1f), 0.10f);
                    c.a = 1f;
                    pixels[idx] = c;
                }
            }
            tex.SetPixels32(pixels);
            tex.Apply();
        }

        private static void CreateRadarSprites()
        {
            WriteChevron(ChevronPath, 64);
            WriteFovCone(FovPath, 128);
            WriteDiamond(DiamondPath, 48);
            WriteDot(DotPath, 32);
            WriteBezel(BezelPath, 96);
        }

        private static void WriteBezel(string path, int s)
        {
            var tex = new Texture2D(s, s, TextureFormat.RGBA32, false);
            var pixels = new Color32[s * s];
            for (int py = 0; py < s; py++)
            {
                for (int px = 0; px < s; px++)
                {
                    int edge = Mathf.Min(Mathf.Min(px, s - 1 - px), Mathf.Min(py, s - 1 - py));
                    if (edge <= 1)
                        pixels[py * s + px] = new Color32(18, 12, 8, 235);
                    else if (edge == 2)
                        pixels[py * s + px] = new Color32(201, 154, 63, 240);
                    else if (edge == 3)
                        pixels[py * s + px] = new Color32(36, 22, 14, 190);
                    else if (edge <= 10)
                    {
                        float alpha = (11f - edge) / 8f * 0.28f;
                        pixels[py * s + px] = new Color(0.05f, 0.03f, 0.02f, alpha);
                    }
                    else
                        pixels[py * s + px] = new Color32(0, 0, 0, 0);
                }
            }
            tex.SetPixels32(pixels);
            tex.Apply();
            File.WriteAllBytes(path, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
        }

        private static void WriteChevron(string path, int s)
        {
            var tex = new Texture2D(s, s, TextureFormat.RGBA32, false);
            var clear = new Color32(0, 0, 0, 0);
            var pixels = new Color32[s * s];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = clear;

            for (int py = 0; py < s; py++)
            {
                for (int px = 0; px < s; px++)
                {
                    float u = (px + 0.5f) / s * 2f - 1f;
                    float v = (py + 0.5f) / s * 2f - 1f;
                    // Sharp GTA-style arrowhead pointing +V (up), notched bottom
                    bool insideOuter = v <= 0.76f - Mathf.Abs(u) * 1.38f && v >= -0.62f + Mathf.Abs(u) * 0.45f && v >= -0.68f;
                    bool insideInner = v <= 0.58f - Mathf.Abs(u) * 1.38f && v >= -0.44f + Mathf.Abs(u) * 0.45f && v >= -0.50f;
                    if (insideInner)
                        pixels[py * s + px] = new Color32(255, 244, 210, 255);
                    else if (insideOuter)
                        pixels[py * s + px] = new Color32(24, 16, 10, 245);
                }
            }
            tex.SetPixels32(pixels);
            tex.Apply();
            File.WriteAllBytes(path, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
        }

        private static void WriteFovCone(string path, int s)
        {
            var tex = new Texture2D(s, s, TextureFormat.RGBA32, false);
            var pixels = new Color32[s * s];
            for (int py = 0; py < s; py++)
            {
                for (int px = 0; px < s; px++)
                {
                    float u = (px + 0.5f) / s * 2f - 1f;
                    float v = (py + 0.5f) / s; // 0 at bottom-center origin, 1 at top
                    if (v <= 0.02f) { pixels[py * s + px] = new Color32(0, 0, 0, 0); continue; }
                    float angle = Mathf.Abs(Mathf.Atan2(u, v)) * Mathf.Rad2Deg;
                    float dist = Mathf.Sqrt(u * u + v * v);
                    float cone = 1f - Mathf.SmoothStep(22f, 36f, angle);
                    float radial = 1f - Mathf.SmoothStep(0.08f, 0.96f, dist);
                    float alpha = cone * radial * 0.34f;
                    pixels[py * s + px] = new Color(1f, 0.92f, 0.70f, alpha);
                }
            }
            tex.SetPixels32(pixels);
            tex.Apply();
            File.WriteAllBytes(path, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
        }

        private static void WriteDiamond(string path, int s)
        {
            var tex = new Texture2D(s, s, TextureFormat.RGBA32, false);
            var pixels = new Color32[s * s];
            for (int py = 0; py < s; py++)
            {
                for (int px = 0; px < s; px++)
                {
                    float d = Mathf.Abs((px + 0.5f) / s * 2f - 1f) + Mathf.Abs((py + 0.5f) / s * 2f - 1f);
                    if (d <= 0.58f)
                        pixels[py * s + px] = new Color32(255, 255, 255, 255);
                    else if (d <= 0.84f)
                        pixels[py * s + px] = new Color32(20, 14, 10, 240);
                    else
                        pixels[py * s + px] = new Color32(0, 0, 0, 0);
                }
            }
            tex.SetPixels32(pixels);
            tex.Apply();
            File.WriteAllBytes(path, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
        }

        private static void WriteDot(string path, int s)
        {
            var tex = new Texture2D(s, s, TextureFormat.RGBA32, false);
            var pixels = new Color32[s * s];
            for (int py = 0; py < s; py++)
            {
                for (int px = 0; px < s; px++)
                {
                    float u = (px + 0.5f) / s * 2f - 1f;
                    float v = (py + 0.5f) / s * 2f - 1f;
                    float d = Mathf.Sqrt(u * u + v * v);
                    if (d <= 0.56f)
                        pixels[py * s + px] = new Color32(255, 255, 255, 255);
                    else if (d <= 0.84f)
                        pixels[py * s + px] = new Color32(20, 14, 10, 240);
                    else
                        pixels[py * s + px] = new Color32(0, 0, 0, 0);
                }
            }
            tex.SetPixels32(pixels);
            tex.Apply();
            File.WriteAllBytes(path, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
        }

        private static void ImportSprite(string path, bool mipmaps, Vector4 border, FilterMode filter)
        {
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) return;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spriteBorder = border;
            importer.mipmapEnabled = mipmaps;
            importer.alphaIsTransparency = true;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.filterMode = filter;
            importer.maxTextureSize = 1024;
            importer.SaveAndReimport();
        }
    }
}
