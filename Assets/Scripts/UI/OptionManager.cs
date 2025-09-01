using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OptionManager : MonoBehaviour
{
    [Header("오디오 설정")]
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private TMP_Text sfxValueText;
    [SerializeField] private TMP_Text bgmValueText;
    [SerializeField] private Button sfxMuteButton;
    [SerializeField] private Button bgmMuteButton;

    [Header("게임 설정")]
    [SerializeField] private Toggle vibrationToggle;
    [SerializeField] private Toggle notificationsToggle;
    [SerializeField] private Toggle autoSaveToggle;
    [SerializeField] private Dropdown languageDropdown;
    [SerializeField] private Dropdown qualityDropdown;

    [Header("설정")]
    [SerializeField] private bool enableDebugLogs = false;
    [SerializeField] private bool autoSaveSettings = true;
    [SerializeField] private float settingsSaveDelay = 1f;

    [Header("기본값")]
    [SerializeField] private float defaultSFXVolume = 0.7f;
    [SerializeField] private float defaultBGMVolume = 0.5f;
    [SerializeField] private bool defaultVibration = true;
    [SerializeField] private bool defaultNotifications = true;
    [SerializeField] private bool defaultAutoSave = true;

    // 상태 관리
    private Coroutine saveSettingsCoroutine;

    // 이벤트 정의
    public delegate void OnOptionChanged(OptionType type, float value);
    public delegate void OnSettingsSaved();
    public delegate void OnSettingsLoaded();
    public event OnOptionChanged OnOptionChangedEvent;
    public event OnSettingsSaved OnSettingsSavedEvent;
    public event OnSettingsLoaded OnSettingsLoadedEvent;

    public enum OptionType
    {
        SFX,
        BGM,
        Vibration,
        Notifications,
        AutoSave,
        Language,
        Quality
    }

    // 프로퍼티
    public float SFXVolume => sfxSlider != null ? sfxSlider.value : defaultSFXVolume;
    public float BGMVolume => bgmSlider != null ? bgmSlider.value : defaultBGMVolume;
    public bool IsVibrationEnabled => vibrationToggle != null ? vibrationToggle.isOn : defaultVibration;
    public bool IsNotificationsEnabled => notificationsToggle != null ? notificationsToggle.isOn : defaultNotifications;
    public bool IsAutoSaveEnabled => autoSaveToggle != null ? autoSaveToggle.isOn : defaultAutoSave;

    private void Awake()
    {
        InitializeOptionManager();
    }

    private void Start()
    {
        LoadSettings();
        SetupUI();
    }

    private void InitializeOptionManager()
    {
        try
        {
            // 자동으로 컴포넌트 찾기
            if (sfxSlider == null)
                sfxSlider = GetComponentInChildren<Slider>();

            if (bgmSlider == null)
                bgmSlider = GetComponentInChildren<Slider>();

            LogDebug("[OptionManager] 옵션 매니저 초기화 완료");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[OptionManager] 초기화 중 오류: {ex.Message}");
        }
    }

    private void SetupUI()
    {
        try
        {
            // SFX 슬라이더 설정
            if (sfxSlider != null)
            {
                sfxSlider.wholeNumbers = false;
                sfxSlider.minValue = 0f;
                sfxSlider.maxValue = 1f;
                sfxSlider.value = defaultSFXVolume;
                sfxSlider.onValueChanged.RemoveAllListeners();
                sfxSlider.onValueChanged.AddListener((value) =>
                {
                    AdjustOption(OptionType.SFX, value);
                });
            }

            // BGM 슬라이더 설정
            if (bgmSlider != null)
            {
                bgmSlider.wholeNumbers = false;
                bgmSlider.minValue = 0f;
                bgmSlider.maxValue = 1f;
                bgmSlider.value = defaultBGMVolume;
                bgmSlider.onValueChanged.RemoveAllListeners();
                bgmSlider.onValueChanged.AddListener((value) =>
                {
                    AdjustOption(OptionType.BGM, value);
                });
            }

            // 음소거 버튼 설정
            if (sfxMuteButton != null)
            {
                sfxMuteButton.onClick.RemoveAllListeners();
                sfxMuteButton.onClick.AddListener(() => ToggleMute(OptionType.SFX));
            }

            if (bgmMuteButton != null)
            {
                bgmMuteButton.onClick.RemoveAllListeners();
                bgmMuteButton.onClick.AddListener(() => ToggleMute(OptionType.BGM));
            }

            // 토글 설정
            if (vibrationToggle != null)
            {
                vibrationToggle.isOn = defaultVibration;
                vibrationToggle.onValueChanged.RemoveAllListeners();
                vibrationToggle.onValueChanged.AddListener((value) =>
                {
                    AdjustOption(OptionType.Vibration, value ? 1f : 0f);
                });
            }

            if (notificationsToggle != null)
            {
                notificationsToggle.isOn = defaultNotifications;
                notificationsToggle.onValueChanged.RemoveAllListeners();
                notificationsToggle.onValueChanged.AddListener((value) =>
                {
                    AdjustOption(OptionType.Notifications, value ? 1f : 0f);
                });
            }

            if (autoSaveToggle != null)
            {
                autoSaveToggle.isOn = defaultAutoSave;
                autoSaveToggle.onValueChanged.RemoveAllListeners();
                autoSaveToggle.onValueChanged.AddListener((value) =>
                {
                    AdjustOption(OptionType.AutoSave, value ? 1f : 0f);
                });
            }

            // 드롭다운 설정
            SetupLanguageDropdown();
            SetupQualityDropdown();

            LogDebug("[OptionManager] UI 설정 완료");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[OptionManager] UI 설정 중 오류: {ex.Message}");
        }
    }

    private void SetupLanguageDropdown()
    {
        if (languageDropdown == null) return;

        try
        {
            languageDropdown.ClearOptions();
            languageDropdown.AddOptions(new System.Collections.Generic.List<string>
            {
                "한국어",
                "English",
                "日本語",
                "中文"
            });

            languageDropdown.value = 0; // 기본값: 한국어
            languageDropdown.onValueChanged.RemoveAllListeners();
            languageDropdown.onValueChanged.AddListener((index) =>
            {
                AdjustOption(OptionType.Language, index);
            });
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[OptionManager] 언어 드롭다운 설정 중 오류: {ex.Message}");
        }
    }

    private void SetupQualityDropdown()
    {
        if (qualityDropdown == null) return;

        try
        {
            qualityDropdown.ClearOptions();
            qualityDropdown.AddOptions(new System.Collections.Generic.List<string>
            {
                "낮음",
                "보통",
                "높음",
                "최고"
            });

            qualityDropdown.value = 1; // 기본값: 보통
            qualityDropdown.onValueChanged.RemoveAllListeners();
            qualityDropdown.onValueChanged.AddListener((index) =>
            {
                AdjustOption(OptionType.Quality, index);
            });
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[OptionManager] 품질 드롭다운 설정 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 옵션 조정
    /// </summary>
    public void AdjustOption(OptionType type, float value)
    {
        try
        {
            switch (type)
            {
                case OptionType.SFX:
                    AudioManager.Instance?.SetSFXVolume(value);
                    UpdateSFXUI(value);
                    break;

                case OptionType.BGM:
                    AudioManager.Instance?.SetBGMVolume(value);
                    UpdateBGMUI(value);
                    break;

                case OptionType.Vibration:
                    // 진동 설정 (모바일에서만)
                    #if UNITY_ANDROID || UNITY_IOS
                    Handheld.Vibrate();
                    #endif
                    break;

                case OptionType.Notifications:
                    // 알림 설정
                    break;

                case OptionType.AutoSave:
                    // 자동 저장 설정
                    break;

                case OptionType.Language:
                    // 언어 설정
                    break;

                case OptionType.Quality:
                    // 그래픽 품질 설정
                    QualitySettings.SetQualityLevel((int)value, true);
                    break;
            }

            // 이벤트 발생
            OnOptionChangedEvent?.Invoke(type, value);

            // 설정 자동 저장
            if (autoSaveSettings)
            {
                ScheduleSaveSettings();
            }

            LogDebug($"[OptionManager] 옵션 조정: {type} = {value}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[OptionManager] 옵션 조정 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 음소거 토글
    /// </summary>
    public void ToggleMute(OptionType type)
    {
        try
        {
            switch (type)
            {
                case OptionType.SFX:
                    if (sfxSlider != null)
                    {
                        float currentValue = sfxSlider.value;
                        float newValue = currentValue > 0f ? 0f : defaultSFXVolume;
                        sfxSlider.value = newValue;
                    }
                    break;

                case OptionType.BGM:
                    if (bgmSlider != null)
                    {
                        float currentValue = bgmSlider.value;
                        float newValue = currentValue > 0f ? 0f : defaultBGMVolume;
                        bgmSlider.value = newValue;
                    }
                    break;
            }

            LogDebug($"[OptionManager] 음소거 토글: {type}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[OptionManager] 음소거 토글 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// SFX UI 업데이트
    /// </summary>
    private void UpdateSFXUI(float value)
    {
        if (sfxValueText != null)
        {
            sfxValueText.text = $"{value:P0}";
        }

        if (sfxMuteButton != null)
        {
            // 음소거 버튼 아이콘 변경
            var buttonImage = sfxMuteButton.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.color = value > 0f ? Color.white : Color.gray;
            }
        }
    }

    /// <summary>
    /// BGM UI 업데이트
    /// </summary>
    private void UpdateBGMUI(float value)
    {
        if (bgmValueText != null)
        {
            bgmValueText.text = $"{value:P0}";
        }

        if (bgmMuteButton != null)
        {
            // 음소거 버튼 아이콘 변경
            var buttonImage = bgmMuteButton.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.color = value > 0f ? Color.white : Color.gray;
            }
        }
    }

    /// <summary>
    /// 설정 저장 예약
    /// </summary>
    private void ScheduleSaveSettings()
    {
        if (saveSettingsCoroutine != null)
        {
            StopCoroutine(saveSettingsCoroutine);
        }

        saveSettingsCoroutine = StartCoroutine(SaveSettingsDelayed());
    }

    /// <summary>
    /// 지연된 설정 저장
    /// </summary>
    private System.Collections.IEnumerator SaveSettingsDelayed()
    {
        yield return new WaitForSeconds(settingsSaveDelay);
        SaveSettings();
    }

    /// <summary>
    /// 설정 저장
    /// </summary>
    public void SaveSettings()
    {
        try
        {
            if (GameSaveManager.Instance?.currentSaveData != null)
            {
                var saveData = GameSaveManager.Instance.currentSaveData;

                // 오디오 설정 저장
                saveData.sfxOption = ConvertVolumeToOption(SFXVolume);
                saveData.bgmOption = ConvertVolumeToOption(BGMVolume);

                // 게임 설정 저장
                PlayerPrefs.SetInt("VibrationEnabled", IsVibrationEnabled ? 1 : 0);
                PlayerPrefs.SetInt("NotificationsEnabled", IsNotificationsEnabled ? 1 : 0);
                PlayerPrefs.SetInt("AutoSaveEnabled", IsAutoSaveEnabled ? 1 : 0);
                PlayerPrefs.SetInt("LanguageIndex", languageDropdown != null ? languageDropdown.value : 0);
                PlayerPrefs.SetInt("QualityIndex", qualityDropdown != null ? qualityDropdown.value : 1);

                PlayerPrefs.Save();
                GameSaveManager.Instance.SaveGame();

                OnSettingsSavedEvent?.Invoke();
                LogDebug("[OptionManager] 설정 저장 완료");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[OptionManager] 설정 저장 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 설정 로드
    /// </summary>
    public void LoadSettings()
    {
        try
        {
            if (GameSaveManager.Instance?.currentSaveData != null)
            {
                var saveData = GameSaveManager.Instance.currentSaveData;

                // 오디오 설정 로드
                if (sfxSlider != null)
                {
                    sfxSlider.value = ConvertOptionToVolume(saveData.sfxOption);
                }

                if (bgmSlider != null)
                {
                    bgmSlider.value = ConvertOptionToVolume(saveData.bgmOption);
                }

                // 게임 설정 로드
                if (vibrationToggle != null)
                {
                    vibrationToggle.isOn = PlayerPrefs.GetInt("VibrationEnabled", 1) == 1;
                }

                if (notificationsToggle != null)
                {
                    notificationsToggle.isOn = PlayerPrefs.GetInt("NotificationsEnabled", 1) == 1;
                }

                if (autoSaveToggle != null)
                {
                    autoSaveToggle.isOn = PlayerPrefs.GetInt("AutoSaveEnabled", 1) == 1;
                }

                if (languageDropdown != null)
                {
                    languageDropdown.value = PlayerPrefs.GetInt("LanguageIndex", 0);
                }

                if (qualityDropdown != null)
                {
                    qualityDropdown.value = PlayerPrefs.GetInt("QualityIndex", 1);
                }

                OnSettingsLoadedEvent?.Invoke();
                LogDebug("[OptionManager] 설정 로드 완료");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[OptionManager] 설정 로드 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 볼륨을 옵션으로 변환 (0~4)
    /// </summary>
    private GameSaveData.AudioOption ConvertVolumeToOption(float volume)
    {
        if (volume == 0f) return GameSaveData.AudioOption.Off;
        if (volume <= 0.25f) return GameSaveData.AudioOption.Low;
        if (volume <= 0.5f) return GameSaveData.AudioOption.Medium;
        if (volume <= 0.75f) return GameSaveData.AudioOption.High;
        return GameSaveData.AudioOption.Max;
    }

    /// <summary>
    /// 옵션을 볼륨으로 변환 (0~4)
    /// </summary>
    private float ConvertOptionToVolume(GameSaveData.AudioOption option)
    {
        return option switch
        {
            GameSaveData.AudioOption.Off => 0f,
            GameSaveData.AudioOption.Low => 0.25f,
            GameSaveData.AudioOption.Medium => 0.5f,
            GameSaveData.AudioOption.High => 0.75f,
            GameSaveData.AudioOption.Max => 1f,
            _ => defaultSFXVolume
        };
    }

    /// <summary>
    /// 기본값으로 초기화
    /// </summary>
    public void ResetToDefaults()
    {
        try
        {
            if (sfxSlider != null) sfxSlider.value = defaultSFXVolume;
            if (bgmSlider != null) bgmSlider.value = defaultBGMVolume;
            if (vibrationToggle != null) vibrationToggle.isOn = defaultVibration;
            if (notificationsToggle != null) notificationsToggle.isOn = defaultNotifications;
            if (autoSaveToggle != null) autoSaveToggle.isOn = defaultAutoSave;
            if (languageDropdown != null) languageDropdown.value = 0;
            if (qualityDropdown != null) qualityDropdown.value = 1;

            SaveSettings();
            LogDebug("[OptionManager] 기본값으로 초기화 완료");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[OptionManager] 기본값 초기화 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 옵션 매니저 정보 반환
    /// </summary>
    public string GetOptionInfo()
    {
        var info = new System.Text.StringBuilder();
        info.AppendLine($"[OptionManager 정보]");
        info.AppendLine($"SFX 볼륨: {SFXVolume:P0}");
        info.AppendLine($"BGM 볼륨: {BGMVolume:P0}");
        info.AppendLine($"진동: {(IsVibrationEnabled ? "활성화" : "비활성화")}");
        info.AppendLine($"알림: {(IsNotificationsEnabled ? "활성화" : "비활성화")}");
        info.AppendLine($"자동 저장: {(IsAutoSaveEnabled ? "활성화" : "비활성화")}");

        return info.ToString();
    }

    private void LogDebug(string message)
    {
        if (enableDebugLogs)
        {
            Debug.Log(message);
        }
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus && autoSaveSettings)
        {
            SaveSettings();
        }
    }

    private void OnApplicationQuit()
    {
        if (autoSaveSettings)
        {
            SaveSettings();
        }
    }

    private void OnDestroy()
    {
        if (saveSettingsCoroutine != null)
        {
            StopCoroutine(saveSettingsCoroutine);
        }

        // 이벤트 구독 해제
        OnOptionChangedEvent = null;
        OnSettingsSavedEvent = null;
        OnSettingsLoadedEvent = null;
    }
}
