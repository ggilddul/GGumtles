using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AchievementButtonUI : MonoBehaviour
{
    [Header("UI 컴포넌트")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private Button button;

    [Header("설정")]
    [SerializeField] private bool enableSound = true;
    [SerializeField] private bool enableDebugLogs = false;

    // 데이터
    private int achievementIndex;
    private AchievementData achievementData;
    private AchievementStatus achievementStatus;

    // 프로퍼티
    public int AchievementIndex => achievementIndex;
    public AchievementData AchievementData => achievementData;
    public AchievementStatus AchievementStatus => achievementStatus;

    // 이벤트 정의
    public delegate void OnAchievementButtonClicked(AchievementButtonUI button, int index);
    public event OnAchievementButtonClicked OnAchievementButtonClickedEvent;

    private void Awake()
    {
        InitializeComponents();
    }

    private void InitializeComponents()
    {
        try
        {
            // 자동으로 컴포넌트 찾기
            if (iconImage == null)
                iconImage = GetComponentInChildren<Image>();
            
            if (titleText == null)
                titleText = GetComponentInChildren<TMP_Text>();
            
            if (button == null)
                button = GetComponent<Button>();

            if (button == null)
                button = gameObject.AddComponent<Button>();

            LogDebug("[AchievementButtonUI] 컴포넌트 초기화 완료");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[AchievementButtonUI] 컴포넌트 초기화 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 업적 버튼 설정
    /// </summary>
    public void Set(AchievementData definition, Sprite iconSprite, int index, AchievementStatus status)
    {
        try
        {
            achievementIndex = index;
            achievementData = definition;
            achievementStatus = status;

            // UI 업데이트
            UpdateUI(definition, iconSprite, status);

            // 버튼 이벤트 설정
            SetupButtonEvents();

            LogDebug($"[AchievementButtonUI] 업적 버튼 설정 완료: {definition?.ach_title}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[AchievementButtonUI] 업적 버튼 설정 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// UI 업데이트
    /// </summary>
    private void UpdateUI(AchievementData definition, Sprite iconSprite, AchievementStatus status)
    {
        if (titleText != null)
        {
            titleText.text = definition?.ach_title ?? "알 수 없는 업적";
        }

        if (iconImage != null)
        {
            iconImage.sprite = iconSprite;
            iconImage.color = status.isUnlocked ? Color.white : new Color(0.8f, 0.8f, 0.8f, 1f);
        }
    }

    /// <summary>
    /// 버튼 이벤트 설정
    /// </summary>
    private void SetupButtonEvents()
    {
        if (button == null) return;

        try
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnButtonClicked);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[AchievementButtonUI] 버튼 이벤트 설정 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 버튼 클릭 이벤트
    /// </summary>
    private void OnButtonClicked()
    {
        try
        {
            // 사운드 재생
            if (enableSound)
            {
                AudioManager.Instance?.PlaySFX(AudioManager.SFXType.Button);
            }

            // 팝업 표시
            PopupManager.Instance?.OpenPopup(PopupManager.PopupType.Achievement, PopupManager.PopupPriority.Normal, achievementIndex);

            // 이벤트 발생
            OnAchievementButtonClickedEvent?.Invoke(this, achievementIndex);

            LogDebug($"[AchievementButtonUI] 업적 버튼 클릭: {achievementData?.ach_title}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[AchievementButtonUI] 버튼 클릭 처리 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 업적 상태 업데이트
    /// </summary>
    public void UpdateStatus(AchievementStatus newStatus)
    {
        try
        {
            achievementStatus = newStatus;
            
            if (iconImage != null)
            {
                iconImage.color = newStatus.isUnlocked ? Color.white : new Color(0.8f, 0.8f, 0.8f, 1f);
            }

            LogDebug($"[AchievementButtonUI] 업적 상태 업데이트: {achievementData?.ach_title} - {newStatus.isUnlocked}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[AchievementButtonUI] 상태 업데이트 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 버튼 활성화/비활성화
    /// </summary>
    public void SetInteractable(bool interactable)
    {
        if (button != null)
        {
            button.interactable = interactable;
        }
    }

    /// <summary>
    /// 사운드 활성화/비활성화
    /// </summary>
    public void SetSoundEnabled(bool enabled)
    {
        enableSound = enabled;
        LogDebug($"[AchievementButtonUI] 사운드 {(enabled ? "활성화" : "비활성화")}");
    }

    /// <summary>
    /// 버튼 정보 반환
    /// </summary>
    public string GetButtonInfo()
    {
        var info = new System.Text.StringBuilder();
        info.AppendLine($"[AchievementButtonUI 정보]");
        info.AppendLine($"업적: {achievementData?.ach_title ?? "없음"}");
        info.AppendLine($"인덱스: {achievementIndex}");
        info.AppendLine($"해금됨: {achievementStatus.isUnlocked}");
        info.AppendLine($"사운드: {(enableSound ? "활성화" : "비활성화")}");

        return info.ToString();
    }

    private void LogDebug(string message)
    {
        if (enableDebugLogs)
        {
            Debug.Log(message);
        }
    }

    private void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
        }
    }
}
