using System;
using Rimbara.Player;
using UnityEngine;

namespace Rimbara.Mission
{
    [Serializable]
    public sealed class SavedProgressData
    {
        public int version = 1;
        public string sceneName;
        public bool active;
        public float elapsedSeconds;
        public float playerX;
        public float playerY;
        public float playerZ;
        public float playerYaw;

        public int disasterStage;
        public bool hasEquipment;
        public string[] extinguishedFires;
        public string[] clearedDebris;

        public int recoveryStage;
        public string[] collectedSeeds;
        public string[] dugBeds;
        public string[] plantedBeds;
        public string[] wateredBeds;
        public string[] cleanedWaste;
        public string[] restoredHabitats;

        public int restoredStage;
        public bool finalHasSeedling;
        public bool finalPlanted;
        public bool finalWatered;
        public bool viewpointVisited;
        public bool endingSkipped;

        public string[] legacySeedlings;
        public string[] legacyTrees;
        public string[] legacyFires;
    }

    /// <summary>
    /// Serializes and restores active mission progress and player position across exits and menu returns.
    /// </summary>
    public static class GameSaveSystem
    {
        public const string DefaultPrefKey = "Rimbara_SavedGame_v1";

        public static bool HasSaveForScene(string sceneName, string prefKey = DefaultPrefKey)
        {
            return TryLoadForScene(sceneName, out _, prefKey);
        }

        public static SavedProgressData Capture(
            string sceneName,
            float elapsedSeconds,
            GameState state,
            MissionSystem missions,
            VolunteerRig rig)
        {
            var data = new SavedProgressData
            {
                sceneName = sceneName ?? string.Empty,
                active = true,
                elapsedSeconds = Mathf.Max(0f, elapsedSeconds)
            };

            if (rig != null)
            {
                Vector3 pos = rig.transform.position;
                data.playerX = pos.x;
                data.playerY = pos.y;
                data.playerZ = pos.z;
                data.playerYaw = rig.transform.eulerAngles.y;
            }

            if (state != null && state.Progress != null)
            {
                data.legacySeedlings = state.Progress.ExportSeedlings();
                data.legacyTrees = state.Progress.ExportTrees();
                data.legacyFires = state.Progress.ExportFires();
            }

            DisasterChapter disaster = missions != null ? missions.DisasterChapter : null;
            if (disaster != null && disaster.Progress != null)
            {
                data.disasterStage = (int)disaster.Progress.Stage;
                data.hasEquipment = disaster.Progress.HasEquipment;
                data.extinguishedFires = disaster.Progress.ExportExtinguishedFires();
                data.clearedDebris = disaster.Progress.ExportClearedDebris();

                RecoveryChapter recovery = disaster.Recovery;
                if (recovery != null && recovery.Progress != null)
                {
                    data.recoveryStage = (int)recovery.Progress.Stage;
                    data.collectedSeeds = recovery.Progress.ExportCollected();
                    data.dugBeds = recovery.Progress.ExportDug();
                    data.plantedBeds = recovery.Progress.ExportPlanted();
                    data.wateredBeds = recovery.Progress.ExportWatered();
                    data.cleanedWaste = recovery.Progress.ExportWaste();
                    data.restoredHabitats = recovery.Progress.ExportHabitats();

                    RestoredChapter restored = recovery.Restored;
                    if (restored != null && restored.Progress != null)
                    {
                        data.restoredStage = (int)restored.Progress.Stage;
                        data.finalHasSeedling = restored.Progress.HasSeedling;
                        data.finalPlanted = restored.Progress.Planted;
                        data.finalWatered = restored.Progress.Watered;
                        data.viewpointVisited = restored.Progress.ViewpointVisited;
                        data.endingSkipped = restored.Progress.EndingSkipped;
                    }
                }
            }

            return data;
        }

        public static string Serialize(SavedProgressData data) => JsonUtility.ToJson(data);

        public static bool TryDeserialize(string json, string expectedSceneName, out SavedProgressData data)
        {
            data = null;
            if (string.IsNullOrWhiteSpace(json)) return false;
            try
            {
                SavedProgressData parsed = JsonUtility.FromJson<SavedProgressData>(json);
                if (parsed == null || !parsed.active) return false;
                if (!string.IsNullOrEmpty(expectedSceneName) && parsed.sceneName != expectedSceneName) return false;
                if (parsed.restoredStage >= (int)RestoredStage.Complete) return false;
                data = parsed;
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static void Save(
            string sceneName,
            float elapsedSeconds,
            GameState state,
            MissionSystem missions,
            VolunteerRig rig,
            string prefKey = DefaultPrefKey)
        {
            SavedProgressData data = Capture(sceneName, elapsedSeconds, state, missions, rig);
            PlayerPrefs.SetString(prefKey, Serialize(data));
            PlayerPrefs.Save();
        }

        public static bool TryLoadForScene(string sceneName, out SavedProgressData data, string prefKey = DefaultPrefKey)
        {
            string json = PlayerPrefs.GetString(prefKey, string.Empty);
            return TryDeserialize(json, sceneName, out data);
        }

        public static void Clear(string prefKey = DefaultPrefKey)
        {
            if (PlayerPrefs.HasKey(prefKey))
            {
                PlayerPrefs.DeleteKey(prefKey);
                PlayerPrefs.Save();
            }
        }

        public static void Apply(SavedProgressData data, GameState state, MissionSystem missions, VolunteerRig rig)
        {
            if (data == null) return;

            if (state != null && state.Progress != null)
                state.Progress.RestoreState(data.legacySeedlings, data.legacyTrees, data.legacyFires);

            DisasterChapter disaster = missions != null ? missions.DisasterChapter : null;
            if (disaster != null && disaster.Progress != null)
            {
                DisasterStage dStage = Enum.IsDefined(typeof(DisasterStage), data.disasterStage)
                    ? (DisasterStage)data.disasterStage
                    : DisasterStage.Briefing;
                disaster.Progress.RestoreState(dStage, data.hasEquipment, data.extinguishedFires, data.clearedDebris);
                disaster.SyncVisualsFromProgress();

                RecoveryChapter recovery = disaster.Recovery;
                if (recovery != null && recovery.Progress != null)
                {
                    RecoveryStage rStage = Enum.IsDefined(typeof(RecoveryStage), data.recoveryStage)
                        ? (RecoveryStage)data.recoveryStage
                        : RecoveryStage.Dormant;
                    recovery.Progress.RestoreState(
                        rStage,
                        data.collectedSeeds,
                        data.dugBeds,
                        data.plantedBeds,
                        data.wateredBeds,
                        data.cleanedWaste,
                        data.restoredHabitats);

                    RestoredChapter restored = recovery.Restored;
                    if (restored != null && restored.Progress != null)
                    {
                        RestoredStage resStage = Enum.IsDefined(typeof(RestoredStage), data.restoredStage)
                            ? (RestoredStage)data.restoredStage
                            : RestoredStage.Dormant;
                        // If player exited during a transient sequence, resume cleanly at the interactive stage.
                        if (resStage == RestoredStage.TimeTransition) resStage = RestoredStage.ElderBriefing;
                        if (resStage == RestoredStage.Ending) resStage = RestoredStage.Viewpoint;
                        restored.Progress.RestoreState(
                            resStage,
                            data.finalHasSeedling,
                            data.finalPlanted,
                            data.finalWatered,
                            data.viewpointVisited,
                            data.endingSkipped);
                        restored.SyncVisualsFromProgress();
                    }
                }
            }

            if (rig != null)
            {
                Vector3 savedPos = new Vector3(data.playerX, data.playerY, data.playerZ);
                if (savedPos.sqrMagnitude > 0.001f)
                {
                    rig.Teleport(savedPos);
                    rig.transform.rotation = Quaternion.Euler(0f, data.playerYaw, 0f);
                    rig.SetCameraOrbit(data.playerYaw, 15f);
                }
            }
        }
    }
}
