using Rimbara.Audio;
using Rimbara.Mission;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Rimbara.UI
{
    /// <summary>
    /// Drives interactive state for the Zone Selection and Settings menu screens
    /// (zone progress bars, locked states, graphics quality selector, and volume sync).
    /// </summary>
    public sealed class MenuScreensController : MonoBehaviour
    {
        private const string QualityPrefKey = "Rimbara_QualityTier";

        [SerializeField] private Slider settingsVolumeSlider;
        [SerializeField] private TMP_Text settingsVolumeValue;
        [SerializeField] private Button[] qualityButtons;
        [SerializeField] private Image zoneAProgressFill;
        [SerializeField] private TMP_Text zoneAProgressText;

        private GameAudio audioSystem;
        private Slider pauseVolumeSlider;
        private int selectedQuality = 2;

        public void Configure(
            Slider settingsVolume,
            TMP_Text volumeValueText,
            Button[] qualityTierButtons,
            Image progressFillA,
            TMP_Text progressTextA)
        {
            settingsVolumeSlider = settingsVolume;
            settingsVolumeValue = volumeValueText;
            qualityButtons = qualityTierButtons;
            zoneAProgressFill = progressFillA;
            zoneAProgressText = progressTextA;
        }

        public void Bind(GameAudio audio, Slider pauseSlider)
        {
            audioSystem = audio;
            pauseVolumeSlider = pauseSlider;

            if (settingsVolumeSlider != null && audioSystem != null)
            {
                settingsVolumeSlider.SetValueWithoutNotify(audioSystem.Volume);
                UpdateVolumeLabel(audioSystem.Volume);
                settingsVolumeSlider.onValueChanged.RemoveListener(OnSettingsVolumeChanged);
                settingsVolumeSlider.onValueChanged.AddListener(OnSettingsVolumeChanged);
            }

            if (qualityButtons != null)
            {
                for (int i = 0; i < qualityButtons.Length; i++)
                {
                    if (qualityButtons[i] == null) continue;
                    int tier = i;
                    qualityButtons[i].onClick.RemoveAllListeners();
                    qualityButtons[i].onClick.AddListener(() => SetQualityTier(tier));
                }
                int saved = PlayerPrefs.GetInt(QualityPrefKey, Mathf.Clamp(QualitySettings.GetQualityLevel() > 0 ? 2 : 1, 0, 2));
                ApplyQualityVisuals(saved);
            }
        }

        public void RefreshZoneProgress(float zoneAFraction)
        {
            float clamped = Mathf.Clamp01(zoneAFraction);
            if (zoneAProgressFill != null) zoneAProgressFill.fillAmount = clamped;
            if (zoneAProgressText != null)
                zoneAProgressText.text = "PROGRES  " + Mathf.RoundToInt(clamped * 100f) + "%";
        }

        public void SyncVolumeFromPause(float value)
        {
            if (settingsVolumeSlider != null)
                settingsVolumeSlider.SetValueWithoutNotify(value);
            UpdateVolumeLabel(value);
        }

        private void OnSettingsVolumeChanged(float value)
        {
            if (audioSystem != null) audioSystem.SetVolume(value);
            if (pauseVolumeSlider != null) pauseVolumeSlider.SetValueWithoutNotify(value);
            UpdateVolumeLabel(value);
        }

        private void UpdateVolumeLabel(float value)
        {
            if (settingsVolumeValue != null)
                settingsVolumeValue.text = Mathf.RoundToInt(Mathf.Clamp01(value) * 100f) + "%";
        }

        public void SetQualityTier(int tier)
        {
            selectedQuality = Mathf.Clamp(tier, 0, 2);
            int count = QualitySettings.names != null ? QualitySettings.names.Length : 1;
            if (count > 1)
            {
                int mapped = selectedQuality == 0 ? 0
                    : selectedQuality == 1 ? count / 2
                    : count - 1;
                QualitySettings.SetQualityLevel(mapped, true);
            }
            PlayerPrefs.SetInt(QualityPrefKey, selectedQuality);
            PlayerPrefs.Save();
            ApplyQualityVisuals(selectedQuality);
        }

        private void ApplyQualityVisuals(int activeTier)
        {
            if (qualityButtons == null) return;
            for (int i = 0; i < qualityButtons.Length; i++)
            {
                Button btn = qualityButtons[i];
                if (btn == null) continue;
                Image img = btn.GetComponent<Image>();
                TMP_Text txt = btn.GetComponentInChildren<TMP_Text>(true);
                bool active = i == activeTier;
                if (img != null)
                    img.color = active ? new Color(1f, 0.92f, 0.72f, 1f) : new Color(0.62f, 0.56f, 0.48f, 0.9f);
                if (txt != null)
                    txt.color = active ? new Color32(0xFF, 0xF0, 0xCC, 0xFF) : new Color32(0xC8, 0xB8, 0x98, 0xFF);
            }
        }
    }
}
