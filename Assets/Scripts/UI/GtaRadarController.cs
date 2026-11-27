using System;
using System.Collections.Generic;
using Rimbara.Interaction;
using Rimbara.Mission;
using Rimbara.Player;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Rimbara.UI
{
    /// <summary>
    /// Pure coordinate-conversion and edge-clamping helpers for the GTA-style radar.
    /// </summary>
    public static class GtaRadarMath
    {
        public static Vector2 WorldToNormalized(Vector3 worldPos, Vector2 worldMin, Vector2 worldSize)
        {
            float sx = Mathf.Max(1f, worldSize.x);
            float sz = Mathf.Max(1f, worldSize.y);
            return new Vector2(
                Mathf.Clamp01((worldPos.x - worldMin.x) / sx),
                Mathf.Clamp01((worldPos.z - worldMin.y) / sz));
        }

        /// <summary>
        /// Rotates a world XZ delta into camera-relative 2D radar space where +Y is camera forward.
        /// </summary>
        public static Vector2 RotateDelta(Vector2 worldDeltaXZ, float cameraYawDegrees)
        {
            float rad = cameraYawDegrees * Mathf.Deg2Rad;
            float cos = Mathf.Cos(rad);
            float sin = Mathf.Sin(rad);
            return new Vector2(
                worldDeltaXZ.x * cos - worldDeltaXZ.y * sin,
                worldDeltaXZ.x * sin + worldDeltaXZ.y * cos);
        }

        /// <summary>
        /// Clamps an offset radially onto the inner edge of a rectangular radar viewport.
        /// </summary>
        public static Vector2 ClampToHalfSize(Vector2 offset, float halfWidth, float halfHeight, out bool clamped)
        {
            float hw = Mathf.Max(1f, halfWidth);
            float hh = Mathf.Max(1f, halfHeight);
            float scale = Mathf.Max(Mathf.Abs(offset.x) / hw, Mathf.Abs(offset.y) / hh);
            if (scale <= 1f)
            {
                clamped = false;
                return offset;
            }
            clamped = true;
            return offset / scale;
        }

        public static float NormalizeAngle(float degrees)
        {
            float a = degrees % 360f;
            return a < 0f ? a + 360f : a;
        }
    }

    /// <summary>
    /// Drives the bottom-right GTA 5-style rotating/scrolling radar and the [M] expandable tactical map.
    /// </summary>
    [DefaultExecutionOrder(240)]
    public sealed class GtaRadarController : MonoBehaviour
    {
        [Header("World Bounds")]
        [SerializeField] private Vector2 worldMin = new Vector2(-60f, -55f);
        [SerializeField] private Vector2 worldSize = new Vector2(120f, 120f);
        [SerializeField] private float radarHalfMeters = 28f;

        [Header("Minimap References")]
        [SerializeField] private RectTransform radarViewport;
        [SerializeField] private RectTransform radarMapImage;
        [SerializeField] private RectTransform playerChevron;
        [SerializeField] private RectTransform fovCone;
        [SerializeField] private RectTransform targetMarker;
        [SerializeField] private RectTransform gpsLine;
        [SerializeField] private RectTransform northBadge;
        [SerializeField] private RectTransform secondaryLayer;

        [Header("Expanded Map References")]
        [SerializeField] private GameObject fullMapOverlay;
        [SerializeField] private RectTransform fullMapSurface;
        [SerializeField] private RectTransform fullPlayerChevron;
        [SerializeField] private RectTransform fullFovCone;
        [SerializeField] private RectTransform fullTargetMarker;
        [SerializeField] private RectTransform fullGpsLine;
        [SerializeField] private RectTransform fullSecondaryLayer;

        [Header("Blip Visuals")]
        [SerializeField] private Sprite dotSprite;

        private const int MaxSecondaryBlips = 28;
        private readonly List<Image> radarBlipPool = new List<Image>(MaxSecondaryBlips);
        private readonly List<Image> fullBlipPool = new List<Image>(MaxSecondaryBlips);
        private readonly List<(Vector3 pos, Color color, float size)> scratchBlips = new List<(Vector3, Color, float)>(MaxSecondaryBlips);

        private VolunteerRig rig;
        private GameFlow flow;
        private DisasterChapter chapter;
        private RecoveryChapter recovery;
        private RestoredChapter restored;
        private DisasterHud hud;

        private FirePoint[] fires;
        private DebrisPickup[] debris;
        private EquipmentPickup equipment;
        private RecoverySeedling[] seedlings;
        private RecoveryPlantingPoint[] beds;
        private RecoveryTask[] tasks;
        private bool actorsCached;

        private static readonly Color TargetColor = new Color(0.95f, 0.72f, 0.22f, 1f);
        private static readonly Color FireColor = new Color(0.92f, 0.34f, 0.22f, 0.96f);
        private static readonly Color DebrisColor = new Color(0.86f, 0.62f, 0.28f, 0.92f);
        private static readonly Color RecoveryColor = new Color(0.48f, 0.78f, 0.36f, 0.94f);
        private static readonly Color NpcColor = new Color(0.55f, 0.84f, 0.95f, 0.94f);

        public bool HasWorldMap => radarMapImage != null && radarMapImage.GetComponent<Image>()?.sprite != null;
        public bool IsExpanded => fullMapOverlay != null && fullMapOverlay.activeSelf;

        public void ConfigureMinimap(
            Vector2 min, Vector2 size, Sprite blipDot,
            RectTransform viewport, RectTransform mapImage,
            RectTransform chevron, RectTransform fov,
            RectTransform target, RectTransform gps,
            RectTransform north, RectTransform blips)
        {
            worldMin = min;
            worldSize = size;
            dotSprite = blipDot;
            radarViewport = viewport;
            radarMapImage = mapImage;
            playerChevron = chevron;
            fovCone = fov;
            targetMarker = target;
            gpsLine = gps;
            northBadge = north;
            secondaryLayer = blips;
        }

        public void ConfigureExpandedMap(
            GameObject overlay, RectTransform surface,
            RectTransform chevron, RectTransform fov,
            RectTransform target, RectTransform gps,
            RectTransform blips)
        {
            fullMapOverlay = overlay;
            fullMapSurface = surface;
            fullPlayerChevron = chevron;
            fullFovCone = fov;
            fullTargetMarker = target;
            fullGpsLine = gps;
            fullSecondaryLayer = blips;
        }

        public void ToggleExpanded()
        {
            if (fullMapOverlay == null) return;
            if (flow != null && flow.Mode != ScreenMode.Playing) return;
            if (hud != null && hud.DialogueOpen) return;
            fullMapOverlay.SetActive(!fullMapOverlay.activeSelf);
        }

        public void SetExpanded(bool open)
        {
            if (fullMapOverlay != null) fullMapOverlay.SetActive(open);
        }

        private void Awake()
        {
            EnsurePool(secondaryLayer, radarBlipPool);
            EnsurePool(fullSecondaryLayer, fullBlipPool);
            if (fullMapOverlay != null) fullMapOverlay.SetActive(false);
            var clickToggle = GetComponent<Button>();
            if (clickToggle != null) clickToggle.onClick.AddListener(ToggleExpanded);
        }

        private void Start()
        {
            ResolveRuntimeRefs();
            CacheWorldActors();
        }

        private void Update()
        {
            ResolveRuntimeRefs();
            if (flow == null) return;

            bool blocked = flow.Mode != ScreenMode.Playing || (hud != null && hud.DialogueOpen);
            if (blocked)
            {
                if (IsExpanded) SetExpanded(false);
                return;
            }

            if (Keyboard.current != null && Keyboard.current.mKey.wasPressedThisFrame)
                ToggleExpanded();
        }

        private void LateUpdate()
        {
            if (!HasWorldMap) return;
            ResolveRuntimeRefs();
            if (rig == null || flow == null) return;
            if (flow.Mode != ScreenMode.Playing && flow.Mode != ScreenMode.Paused) return;
            CacheWorldActors();

            Vector3 playerPos = rig.transform.position;
            float camYaw = rig.ViewCamera != null ? rig.ViewCamera.transform.eulerAngles.y : rig.transform.eulerAngles.y;
            float playerYaw = rig.transform.eulerAngles.y;
            Transform activeTarget = ResolveActiveTarget(playerPos);

            CollectSecondaryBlips(activeTarget);
            UpdateMinimap(playerPos, camYaw, playerYaw, activeTarget);
            if (IsExpanded) UpdateExpandedMap(playerPos, camYaw, playerYaw, activeTarget);
        }

        private void UpdateMinimap(Vector3 playerPos, float camYaw, float playerYaw, Transform activeTarget)
        {
            if (radarViewport == null || radarMapImage == null) return;

            float viewW = Mathf.Max(64f, radarViewport.rect.width);
            float viewH = Mathf.Max(64f, radarViewport.rect.height);
            float halfW = viewW * 0.5f;
            float halfH = viewH * 0.5f;
            float pxPerMeter = halfW / Mathf.Max(6f, radarHalfMeters);

            radarMapImage.anchorMin = radarMapImage.anchorMax = new Vector2(0.5f, 0.5f);
            radarMapImage.pivot = GtaRadarMath.WorldToNormalized(playerPos, worldMin, worldSize);
            radarMapImage.anchoredPosition = Vector2.zero;
            radarMapImage.sizeDelta = worldSize * pxPerMeter;
            radarMapImage.localEulerAngles = new Vector3(0f, 0f, camYaw);

            if (fovCone != null)
            {
                fovCone.anchoredPosition = Vector2.zero;
                fovCone.localEulerAngles = Vector3.zero;
            }

            if (playerChevron != null)
            {
                playerChevron.anchorMin = playerChevron.anchorMax = new Vector2(0.5f, 0.5f);
                playerChevron.pivot = new Vector2(0.5f, 0.5f);
                playerChevron.anchoredPosition = Vector2.zero;
                playerChevron.localEulerAngles = new Vector3(0f, 0f, GtaRadarMath.NormalizeAngle(camYaw - playerYaw));
            }

            if (northBadge != null)
            {
                Vector2 northDir = GtaRadarMath.RotateDelta(new Vector2(0f, 1000f), camYaw);
                northBadge.anchoredPosition = GtaRadarMath.ClampToHalfSize(northDir, halfW - 11f, halfH - 11f, out _);
            }

            if (targetMarker != null)
            {
                targetMarker.anchorMin = targetMarker.anchorMax = new Vector2(0.5f, 0.5f);
                targetMarker.pivot = new Vector2(0.5f, 0.5f);
                if (activeTarget != null)
                {
                    targetMarker.gameObject.SetActive(true);
                    Vector2 deltaXZ = new Vector2(activeTarget.position.x - playerPos.x, activeTarget.position.z - playerPos.z);
                    Vector2 radarOffset = GtaRadarMath.RotateDelta(deltaXZ, camYaw) * pxPerMeter;
                    Vector2 clamped = GtaRadarMath.ClampToHalfSize(radarOffset, halfW - 10f, halfH - 10f, out bool isClamped);
                    targetMarker.anchoredPosition = clamped;
                    float pulse = isClamped ? 1.0f : (1f + 0.14f * Mathf.Sin(Time.unscaledTime * 5f));
                    targetMarker.localScale = new Vector3(pulse, pulse, 1f);
                    UpdateGpsLine(gpsLine, Vector2.zero, clamped);
                }
                else
                {
                    targetMarker.gameObject.SetActive(false);
                    if (gpsLine != null) gpsLine.gameObject.SetActive(false);
                }
            }

            EnsurePool(secondaryLayer, radarBlipPool);
            int used = 0;
            for (int i = 0; i < scratchBlips.Count && used < radarBlipPool.Count; i++)
            {
                var blip = scratchBlips[i];
                Vector2 deltaXZ = new Vector2(blip.pos.x - playerPos.x, blip.pos.z - playerPos.z);
                Vector2 offset = GtaRadarMath.RotateDelta(deltaXZ, camYaw) * pxPerMeter;
                if (Mathf.Abs(offset.x) > halfW - 5f || Mathf.Abs(offset.y) > halfH - 5f) continue;

                Image img = radarBlipPool[used++];
                img.gameObject.SetActive(true);
                img.color = blip.color;
                img.rectTransform.anchoredPosition = offset;
                img.rectTransform.sizeDelta = new Vector2(blip.size, blip.size);
            }
            for (int i = used; i < radarBlipPool.Count; i++)
                radarBlipPool[i].gameObject.SetActive(false);
        }

        private void UpdateExpandedMap(Vector3 playerPos, float camYaw, float playerYaw, Transform activeTarget)
        {
            if (fullMapSurface == null) return;
            float w = Mathf.Max(64f, fullMapSurface.rect.width);
            float h = Mathf.Max(64f, fullMapSurface.rect.height);

            Vector2 WorldToMapPoint(Vector3 pos)
            {
                Vector2 uv = GtaRadarMath.WorldToNormalized(pos, worldMin, worldSize);
                return new Vector2((uv.x - 0.5f) * w, (uv.y - 0.5f) * h);
            }

            Vector2 pUi = WorldToMapPoint(playerPos);
            if (fullFovCone != null)
            {
                fullFovCone.anchoredPosition = pUi;
                fullFovCone.localEulerAngles = new Vector3(0f, 0f, -camYaw);
            }
            if (fullPlayerChevron != null)
            {
                fullPlayerChevron.anchoredPosition = pUi;
                fullPlayerChevron.localEulerAngles = new Vector3(0f, 0f, -playerYaw);
            }

            if (fullTargetMarker != null)
            {
                if (activeTarget != null)
                {
                    fullTargetMarker.gameObject.SetActive(true);
                    Vector2 tUi = WorldToMapPoint(activeTarget.position);
                    fullTargetMarker.anchoredPosition = tUi;
                    float pulse = 1f + 0.14f * Mathf.Sin(Time.unscaledTime * 5f);
                    fullTargetMarker.localScale = new Vector3(pulse, pulse, 1f);
                    UpdateGpsLine(fullGpsLine, pUi, tUi);
                }
                else
                {
                    fullTargetMarker.gameObject.SetActive(false);
                    if (fullGpsLine != null) fullGpsLine.gameObject.SetActive(false);
                }
            }

            EnsurePool(fullSecondaryLayer, fullBlipPool);
            int count = Mathf.Min(scratchBlips.Count, fullBlipPool.Count);
            for (int i = 0; i < count; i++)
            {
                var blip = scratchBlips[i];
                Image img = fullBlipPool[i];
                img.gameObject.SetActive(true);
                img.color = blip.color;
                img.rectTransform.anchoredPosition = WorldToMapPoint(blip.pos);
                img.rectTransform.sizeDelta = new Vector2(blip.size + 2f, blip.size + 2f);
            }
            for (int i = count; i < fullBlipPool.Count; i++)
                fullBlipPool[i].gameObject.SetActive(false);
        }

        private static void UpdateGpsLine(RectTransform line, Vector2 from, Vector2 to)
        {
            if (line == null) return;
            Vector2 diff = to - from;
            float dist = diff.magnitude;
            if (dist < 10f)
            {
                line.gameObject.SetActive(false);
                return;
            }
            line.gameObject.SetActive(true);
            line.anchorMin = line.anchorMax = new Vector2(0.5f, 0.5f);
            line.pivot = new Vector2(0f, 0.5f);
            line.anchoredPosition = from;
            line.sizeDelta = new Vector2(Mathf.Max(0f, dist - 6f), 2.5f);
            line.localEulerAngles = new Vector3(0f, 0f, Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg);
        }

        private Transform ResolveActiveTarget(Vector3 playerPos)
        {
            if (restored != null && restored.Active) return restored.Destination;
            if (recovery != null && recovery.Active) return recovery.Destination(playerPos);
            if (chapter != null) return chapter.Destination(playerPos);
            return null;
        }

        private void CollectSecondaryBlips(Transform activeTarget)
        {
            scratchBlips.Clear();
            bool recovering = recovery != null && recovery.Active;
            bool restoringFinal = restored != null && restored.Active;

            if (chapter != null && chapter.Coordinator != null && chapter.Coordinator != activeTarget)
                scratchBlips.Add((chapter.Coordinator.position, NpcColor, 8.5f));
            if (recovery != null && recovery.Ranger != null && recovery.Active && recovery.Ranger != activeTarget)
                scratchBlips.Add((recovery.Ranger.position, NpcColor, 8.5f));
            if (restored != null && restored.Elder != null && restoringFinal && restored.Elder != activeTarget)
                scratchBlips.Add((restored.Elder.position, NpcColor, 8.5f));

            if (!recovering && !restoringFinal)
            {
                if (equipment != null && equipment.transform != activeTarget && chapter != null && chapter.Progress != null && !chapter.Progress.HasEquipment)
                    scratchBlips.Add((equipment.transform.position, TargetColor, 9f));

                if (fires != null)
                {
                    for (int i = 0; i < fires.Length && scratchBlips.Count < MaxSecondaryBlips; i++)
                    {
                        FirePoint f = fires[i];
                        if (f != null && f.gameObject.activeInHierarchy && !f.Extinguished && f.transform != activeTarget)
                            scratchBlips.Add((f.transform.position, FireColor, 8f));
                    }
                }

                if (debris != null)
                {
                    for (int i = 0; i < debris.Length && scratchBlips.Count < MaxSecondaryBlips; i++)
                    {
                        DebrisPickup d = debris[i];
                        if (d != null && d.Available && d.transform != activeTarget)
                            scratchBlips.Add((d.transform.position, DebrisColor, 7.5f));
                    }
                }
            }
            else if (recovering && !restoringFinal && recovery.Progress != null)
            {
                if (recovery.Progress.Stage == RecoveryStage.Planting)
                {
                    if (seedlings != null)
                        for (int i = 0; i < seedlings.Length && scratchBlips.Count < MaxSecondaryBlips; i++)
                            if (seedlings[i] != null && !seedlings[i].Collected && seedlings[i].transform != activeTarget)
                                scratchBlips.Add((seedlings[i].transform.position, RecoveryColor, 8f));
                    if (beds != null)
                        for (int i = 0; i < beds.Length && scratchBlips.Count < MaxSecondaryBlips; i++)
                            if (beds[i] != null && !beds[i].Watered && beds[i].transform != activeTarget)
                                scratchBlips.Add((beds[i].transform.position, RecoveryColor, 8f));
                }
                else if (recovery.Progress.Stage == RecoveryStage.Ecosystem && tasks != null)
                {
                    for (int i = 0; i < tasks.Length && scratchBlips.Count < MaxSecondaryBlips; i++)
                        if (tasks[i] != null && !tasks[i].Done && tasks[i].transform != activeTarget)
                            scratchBlips.Add((tasks[i].transform.position, RecoveryColor, 8f));
                }
            }
        }

        private void EnsurePool(RectTransform container, List<Image> pool)
        {
            if (container == null) return;
            if (pool.Count == 0 && container.childCount > 0)
            {
                for (int i = 0; i < container.childCount; i++)
                {
                    Image existing = container.GetChild(i).GetComponent<Image>();
                    if (existing != null) pool.Add(existing);
                }
            }
            while (pool.Count < MaxSecondaryBlips)
            {
                var go = new GameObject("Radar blip " + pool.Count, typeof(RectTransform), typeof(Image));
                go.layer = 5;
                go.transform.SetParent(container, false);
                var rect = go.GetComponent<RectTransform>();
                rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                var img = go.GetComponent<Image>();
                img.sprite = dotSprite;
                img.raycastTarget = false;
                go.SetActive(false);
                pool.Add(img);
            }
        }

        private void ResolveRuntimeRefs()
        {
            if (rig == null) rig = FindFirstObjectByType<VolunteerRig>();
            if (flow == null) flow = FindFirstObjectByType<GameFlow>();
            if (chapter == null) chapter = FindFirstObjectByType<DisasterChapter>();
            if (recovery == null && chapter != null) recovery = chapter.Recovery;
            if (restored == null && recovery != null) restored = recovery.Restored;
            if (hud == null) hud = FindFirstObjectByType<DisasterHud>();
        }

        private void CacheWorldActors()
        {
            if (actorsCached) return;
            actorsCached = true;
            fires = FindObjectsByType<FirePoint>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            debris = FindObjectsByType<DebrisPickup>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            equipment = FindFirstObjectByType<EquipmentPickup>(FindObjectsInactive.Include);
            seedlings = FindObjectsByType<RecoverySeedling>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            beds = FindObjectsByType<RecoveryPlantingPoint>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            tasks = FindObjectsByType<RecoveryTask>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        }
    }
}
