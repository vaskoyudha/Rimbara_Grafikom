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
        [SerializeField] private Button continueButton;
        [SerializeField] private Button newGameButton;
        [SerializeField] private Sprite primaryNormal;
        [SerializeField] private Sprite primaryHover;
        [SerializeField] private Sprite primaryPressed;
        [SerializeField] private Sprite primaryDisabled;
        [SerializeField] private Sprite secondaryNormal;
        [SerializeField] private Sprite secondaryHover;
        [SerializeField] private Sprite secondaryPressed;
        [SerializeField] private Color creamTextColor = new Color32(0xFF, 0xF0, 0xCC, 0xFF);
        [SerializeField] private Color parchmentTextColor = new Color32(0xE8, 0xDC, 0xC4, 0xFF);
        [SerializeField] private Image achievementSummaryFill;
        [SerializeField] private TMP_Text achievementSummaryText;
        [SerializeField] private Image[] achievementCardBgs;
        [SerializeField] private Image[] achievementSlotBgs;
        [SerializeField] private TMP_Text[] achievementStatusLabels;

        private static readonly string[] BadgeKeys =
        {
            "Rimbara_Badge_FirstStep",
            "Rimbara_Badge_FirstFire",
            "Rimbara_Badge_FireMaster",
            "Rimbara_Badge_Cleaner",
            "Rimbara_Badge_Planter",
            "Rimbara_Badge_Hero"
        };

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

        public void ConfigureMainMenu(Button continueBtn, Button startBtn, UiKit kit)
        {
            continueButton = continueBtn;
            newGameButton = startBtn;
            if (kit != null)
            {
                primaryNormal = kit.ButtonPrimaryNormal;
                primaryHover = kit.ButtonPrimaryHover;
                primaryPressed = kit.ButtonPrimaryPressed;
                primaryDisabled = kit.ButtonPrimaryDisabled;
                secondaryNormal = kit.CardInset != null ? kit.CardInset : kit.Tooltip;
                secondaryHover = kit.Tooltip != null ? kit.Tooltip : secondaryNormal;
                secondaryPressed = kit.Inset != null ? kit.Inset : secondaryNormal;
                creamTextColor = kit.Cream;
                parchmentTextColor = kit.Parchment;
            }
        }

        public void RefreshContinueState(bool canContinue)
        {
            if (continueButton != null)
            {
                continueButton.gameObject.SetActive(canContinue);
                if (canContinue)
                    SetMainMenuButtonBounds(continueButton, 58f, -222f, 374f, 58f);
            }
            if (newGameButton == null) return;

            var label = newGameButton.GetComponentInChildren<TMP_Text>(true);
            if (canContinue)
            {
                SetMainMenuButtonBounds(newGameButton, 446f, -222f, 216f, 58f);
                ApplyMenuButtonSprites(newGameButton, secondaryNormal, secondaryHover, secondaryPressed, secondaryNormal, parchmentTextColor);
                if (label != null) label.text = "MULAI BARU";
            }
            else
            {
                SetMainMenuButtonBounds(newGameButton, 58f, -222f, 604f, 58f);
                ApplyMenuButtonSprites(newGameButton, primaryNormal, primaryHover, primaryPressed, primaryDisabled, creamTextColor);
                if (label != null) label.text = IndonesianText.Labels["Start restoration label"];
            }
        }

        private static void SetMainMenuButtonBounds(Button btn, float left, float topY, float width, float height)
        {
            var rect = btn.GetComponent<RectTransform>();
            if (rect == null) return;
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(width, height);
            rect.anchoredPosition = new Vector2(left + width * 0.5f, topY - height * 0.5f);
            rect.localScale = Vector3.one;

            var label = btn.GetComponentInChildren<TMP_Text>(true);
            if (label != null)
            {
                var lRect = label.rectTransform;
                lRect.anchorMin = Vector2.zero;
                lRect.anchorMax = Vector2.one;
                lRect.pivot = new Vector2(0.5f, 0.5f);
                lRect.offsetMin = new Vector2(8f, 4f);
                lRect.offsetMax = new Vector2(-8f, -4f);
                label.alignment = TextAlignmentOptions.Center;
            }
        }

        private static void ApplyMenuButtonSprites(Button btn, Sprite normal, Sprite hover, Sprite pressed, Sprite disabled, Color textColor)
        {
            if (btn == null || normal == null) return;
            var img = btn.GetComponent<Image>();
            if (img != null)
            {
                img.sprite = normal;
                img.type = Image.Type.Sliced;
                img.color = Color.white;
            }
            btn.transition = Selectable.Transition.SpriteSwap;
            btn.spriteState = new SpriteState
            {
                highlightedSprite = hover != null ? hover : normal,
                pressedSprite = pressed != null ? pressed : normal,
                selectedSprite = hover != null ? hover : normal,
                disabledSprite = disabled != null ? disabled : normal
            };
            var label = btn.GetComponentInChildren<TMP_Text>(true);
            if (label != null) label.color = textColor;
        }

        public void ConfigureAchievements(
            Image summaryFill,
            TMP_Text summaryText,
            Image[] cardBgs,
            Image[] slotBgs,
            TMP_Text[] statusLabels)
        {
            achievementSummaryFill = summaryFill;
            achievementSummaryText = summaryText;
            achievementCardBgs = cardBgs;
            achievementSlotBgs = slotBgs;
            achievementStatusLabels = statusLabels;
        }

        public void UnlockBadge(int index)
        {
            if (index < 0 || index >= BadgeKeys.Length) return;
            if (PlayerPrefs.GetInt(BadgeKeys[index], 0) == 1) return;
            PlayerPrefs.SetInt(BadgeKeys[index], 1);
            PlayerPrefs.Save();
        }

        public void SyncGameplayBadges(int firesExtinguished, int fireGoal, int treesPlanted, bool startedMission, bool victoryReached)
        {
            if (startedMission) UnlockBadge(0);
            if (firesExtinguished >= 1) UnlockBadge(1);
            if (fireGoal > 0 && firesExtinguished >= fireGoal) UnlockBadge(2);
            if (treesPlanted >= 1) UnlockBadge(4);
            if (victoryReached)
            {
                UnlockBadge(3);
                UnlockBadge(5);
            }
        }

        public void RefreshAchievements()
        {
            int unlockedCount = 0;
            for (int i = 0; i < BadgeKeys.Length; i++)
            {
                bool unlocked = PlayerPrefs.GetInt(BadgeKeys[i], 0) == 1;
                if (unlocked) unlockedCount++;

                if (achievementCardBgs != null && i < achievementCardBgs.Length && achievementCardBgs[i] != null)
                    achievementCardBgs[i].color = unlocked
                        ? new Color(1f, 1f, 1f, 0.98f)
                        : new Color(0.66f, 0.62f, 0.56f, 0.88f);

                if (achievementSlotBgs != null && i < achievementSlotBgs.Length && achievementSlotBgs[i] != null)
                    achievementSlotBgs[i].color = unlocked
                        ? new Color(1f, 0.94f, 0.76f, 1f)
                        : new Color(0.50f, 0.46f, 0.42f, 0.85f);

                if (achievementStatusLabels != null && i < achievementStatusLabels.Length && achievementStatusLabels[i] != null)
                {
                    achievementStatusLabels[i].text = unlocked
                        ? IndonesianText.Labels["Badge unlocked label"]
                        : IndonesianText.Labels["Badge locked label"];
                    achievementStatusLabels[i].color = unlocked
                        ? new Color32(0xC9, 0x9A, 0x3F, 0xFF)
                        : new Color32(0xA8, 0x94, 0x72, 0xFF);
                }
            }

            if (achievementSummaryFill != null)
                achievementSummaryFill.fillAmount = (float)unlockedCount / BadgeKeys.Length;
            if (achievementSummaryText != null)
                achievementSummaryText.text = "LENCANA TERBUKA  " + unlockedCount + " / " + BadgeKeys.Length;
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
