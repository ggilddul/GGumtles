using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GGumtles.UI;

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
    private bool isUnlocked = false; // 단순화: 해금 여부만 저장

    // 프로퍼티
    public int AchievementIndex => achievementIndex;
    public AchievementData AchievementData => achievementData;
    public bool IsUnlocked => isUnlocked;

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
    /// 업적 버튼 설정 (단순화)
    /// </summary>
    public void Set(AchievementData definition, Sprite iconSprite, int index, bool unlocked)
    {
        try
        {
            achievementIndex = index;
            achievementData = definition;
            isUnlocked = unlocked;

            // UI 업데이트
            UpdateUI(definition, iconSprite, unlocked);

            // 버튼 이벤트 설정
            SetupButtonEvents();

            LogDebug($"[AchievementButtonUI] 업적 버튼 설정 완료: {definition?.achievementTitle}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[AchievementButtonUI] 업적 버튼 설정 중 오류: {ex.Message}");
        }
    }
    
    /// <summary>
    /// 업적 데이터로 간단 설정 (MainUI에서 사용)
    /// </summary>
    public void SetupAchievement(AchievementData definition)
    {
        try
        {
            achievementData = definition;
            
            // AchievementManager에서 해금 여부 가져오기
            var achievementManager = AchievementManager.Instance;
            if (achievementManager != null)
            {
                isUnlocked = achievementManager.IsUnlocked(definition.achievementId);
            }
            
            // 아이콘 가져오기
            Sprite iconSprite = null;
            if (SpriteManager.Instance != null)
            {
                iconSprite = SpriteManager.Instance.GetAchievementSprite(definition.achievementId);
            }
            
            // UI 업데이트
            UpdateUI(definition, iconSprite, isUnlocked);
            
            // 버튼 이벤트 설정
            SetupButtonEvents();
            
            LogDebug($"[AchievementButtonUI] 업적 설정 완료: {definition?.achievementTitle}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[AchievementButtonUI] 업적 설정 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// UI 업데이트 (단순화)
    /// </summary>
    private void UpdateUI(AchievementData definition, Sprite iconSprite, bool unlocked)
    {
        if (titleText != null)
        {
            titleText.text = definition?.achievementTitle ?? "알 수 없는 업적";
        }

        if (iconImage != null)
        {
            iconImage.sprite = iconSprite;
            iconImage.color = unlocked ? Color.white : new Color(0.8f, 0.8f, 0.8f, 1f);
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

            // 업적 UI 생성 - UIPrefabManager 삭제로 인해 직접 처리 불가
            Debug.Log($"[AchievementButtonUI] 업적 버튼 클릭: {achievementData?.achievementTitle}");
            
            // 업적 팝업 열기
            if (achievementData != null)
            {
                PopupManager.Instance?.OpenPopup(PopupManager.PopupType.Achievement1, PopupManager.PopupPriority.Normal, achievementIndex);
            }

            // 이벤트 발생
            OnAchievementButtonClickedEvent?.Invoke(this, achievementIndex);

            LogDebug($"[AchievementButtonUI] 업적 버튼 클릭: {achievementData?.achievementTitle}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[AchievementButtonUI] 버튼 클릭 처리 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 업적 상태 업데이트 (단순화)
    /// </summary>
    public void UpdateStatus(bool unlocked)
    {
        try
        {
            isUnlocked = unlocked;
            
            if (iconImage != null)
            {
                iconImage.color = unlocked ? Color.white : new Color(0.8f, 0.8f, 0.8f, 1f);
            }

            LogDebug($"[AchievementButtonUI] 업적 상태 업데이트: {achievementData?.achievementTitle} - {unlocked}");
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
        info.AppendLine($"업적: {achievementData?.achievementTitle ?? "없음"}");
        info.AppendLine($"인덱스: {achievementIndex}");
        info.AppendLine($"해금됨: {isUnlocked}");
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
