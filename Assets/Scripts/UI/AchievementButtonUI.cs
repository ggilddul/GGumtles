using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AchievementButtonUI : MonoBehaviour
{
    [Header("UI 컴포넌트")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private Button button;
    
    // 자동 찾기용 필드
    private bool componentsInitialized = false;

    [Header("디버그")]
    [SerializeField] private bool enableDebugLogs = false;

    // 데이터
    private int achievementIndex;
    private AchievementData achievementData;
    private bool isUnlocked = false;

    // 프로퍼티
    public int AchievementIndex => achievementIndex;
    public AchievementData AchievementData => achievementData;
    public bool IsUnlocked => isUnlocked;

    /// <summary>
    /// 업적 버튼 초기화
    /// </summary>
    public void Initialize(AchievementData definition, int index)
    {
        try
        {
            // 컴포넌트 자동 찾기 (Instantiate된 경우)
            InitializeComponents();
            
            achievementIndex = index;
            achievementData = definition;
            
            // AchievementManager에서 해금 여부 가져오기
            var achievementManager = AchievementManager.Instance;
            if (achievementManager != null)
            {
                isUnlocked = achievementManager.IsUnlocked(definition.achievementId);
            }
            
            // UI 업데이트
            UpdateUI();
            
            // 버튼 이벤트 설정
            SetupButtonEvents();
            
            LogDebug($"[AchievementButtonUI] 업적 버튼 초기화: {definition?.achievementTitle}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[AchievementButtonUI] 업적 버튼 초기화 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 컴포넌트 자동 찾기
    /// </summary>
    private void InitializeComponents()
    {
        if (componentsInitialized) return;
        
        try
        {
            // titleText 자동 찾기
            if (titleText == null)
            {
                titleText = GetComponentInChildren<TMP_Text>();
                if (titleText == null)
                {
                    // "TitleText" 또는 "NameText" 이름으로 찾기
                    Transform titleTransform = transform.Find("TitleText");
                    if (titleTransform == null) titleTransform = transform.Find("NameText");
                    if (titleTransform == null) titleTransform = transform.Find("AchievementNameText");
                    if (titleTransform != null)
                    {
                        titleText = titleTransform.GetComponent<TMP_Text>();
                    }
                }
            }
            
            // iconImage 자동 찾기
            if (iconImage == null)
            {
                iconImage = GetComponentInChildren<Image>();
                if (iconImage == null)
                {
                    // "IconImage" 또는 "Icon" 이름으로 찾기
                    Transform iconTransform = transform.Find("IconImage");
                    if (iconTransform == null) iconTransform = transform.Find("Icon");
                    if (iconTransform != null)
                    {
                        iconImage = iconTransform.GetComponent<Image>();
                    }
                }
            }
            
            // button 자동 찾기
            if (button == null)
            {
                button = GetComponent<Button>();
                if (button == null)
                {
                    button = gameObject.AddComponent<Button>();
                }
            }
            
            componentsInitialized = true;
            LogDebug($"[AchievementButtonUI] 컴포넌트 자동 찾기 완료 - titleText: {titleText != null}, iconImage: {iconImage != null}, button: {button != null}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[AchievementButtonUI] 컴포넌트 자동 찾기 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// UI 업데이트
    /// </summary>
    private void UpdateUI()
    {
        if (achievementData == null) return;

        // 제목 텍스트
        if (titleText != null)
        {
            titleText.text = achievementData.achievementTitle;
            LogDebug($"[AchievementButtonUI] 제목 텍스트 설정: {achievementData.achievementTitle}");
        }
        else
        {
            Debug.LogWarning($"[AchievementButtonUI] titleText가 null입니다. Inspector에서 할당하거나 자식 오브젝트에 TMP_Text를 추가하세요.");
        }

        // 아이콘 이미지
        if (iconImage != null)
        {
            // SpriteManager에서 아이콘 가져오기
            Sprite iconSprite = null;
            if (SpriteManager.Instance != null)
            {
                iconSprite = SpriteManager.Instance.GetAchievementSprite(achievementData.achievementId);
            }
            
            iconImage.sprite = iconSprite;
            iconImage.color = isUnlocked ? Color.white : new Color(0.8f, 0.8f, 0.8f, 1f);
            LogDebug($"[AchievementButtonUI] 아이콘 이미지 설정: {iconSprite != null}");
        }
        else
        {
            Debug.LogWarning($"[AchievementButtonUI] iconImage가 null입니다. Inspector에서 할당하거나 자식 오브젝트에 Image를 추가하세요.");
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
            AudioManager.Instance?.PlayButtonSound(0);

            // 업적 팝업 열기 (해금 상태에 따라 구분)
            if (achievementData != null)
            {
                if (isUnlocked)
                {
                    // 해금된 업적: Achievement1 팝업
                    PopupManager.Instance?.OpenPopup(PopupManager.PopupType.Achievement1, PopupManager.PopupPriority.Normal, achievementIndex);
                }
                else
                {
                    // 잠금된 업적: Achievement2 팝업
                    PopupManager.Instance?.OpenPopup(PopupManager.PopupType.Achievement2, PopupManager.PopupPriority.Normal, achievementIndex);
                }
            }

            LogDebug($"[AchievementButtonUI] 업적 버튼 클릭: {achievementData?.achievementTitle}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[AchievementButtonUI] 버튼 클릭 처리 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 업적 상태 업데이트
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