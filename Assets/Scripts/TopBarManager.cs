using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class TopBarManager : MonoBehaviour
{
    public static TopBarManager Instance { get; private set; }

    [Header("시간 UI")]
    [SerializeField] private TextMeshProUGUI AMPMText;
    [SerializeField] private TextMeshProUGUI GameTimeText;
    
    [Header("벌레 정보")]
    [SerializeField] private TextMeshProUGUI CurrentWormNameText;
    [SerializeField] private Image WormIconImage;
    [SerializeField] private Slider WormHealthSlider;
    
    [Header("재화 UI")]
    [SerializeField] private TextMeshProUGUI AcornCountText;
    [SerializeField] private TextMeshProUGUI DiamondCountText;
    [SerializeField] private Button AcornButton;
    [SerializeField] private Button DiamondButton;
    
    [Header("설정 및 알림")]
    [SerializeField] private Button SettingsButton;
    [SerializeField] private Button NotificationButton;
    [SerializeField] private GameObject NotificationBadge;
    [SerializeField] private TextMeshProUGUI NotificationCountText;
    
    [Header("애니메이션 설정")]
    [SerializeField] private float numberChangeDuration = 0.5f;
    [SerializeField] private AnimationCurve numberChangeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private Color highlightColor = Color.yellow;
    [SerializeField] private float highlightDuration = 0.3f;
    
    [Header("UI 설정")]
    [SerializeField] private bool showSeconds = false;
    [SerializeField] private bool use24HourFormat = false;
    // [SerializeField] private string timeFormat = "HH:mm";  // 미사용
    [SerializeField] private string currencyFormat = "N0";

    // 이벤트 정의
    public delegate void OnResourceClicked(string resourceType);
    public event OnResourceClicked OnResourceClickedEvent;

    public delegate void OnSettingsClicked();
    public event OnSettingsClicked OnSettingsClickedEvent;

    public delegate void OnNotificationClicked();
    public event OnNotificationClicked OnNotificationClickedEvent;

    // 상태 관리
    private int currentAcornCount = 0;
    private int currentDiamondCount = 0;
    private int notificationCount = 0;
    private bool isInitialized = false;
    private Dictionary<TextMeshProUGUI, Coroutine> animationCoroutines = new Dictionary<TextMeshProUGUI, Coroutine>();

    // 프로퍼티
    public int CurrentAcornCount => currentAcornCount;
    public int CurrentDiamondCount => currentDiamondCount;
    public int NotificationCount => notificationCount;
    public bool IsInitialized => isInitialized;

    private void Awake()
    {
        InitializeSingleton();
    }

    private void Start()
    {
        InitializeTopBarSystem();
    }

    private void InitializeSingleton()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeTopBarSystem()
    {
        try
        {
            ValidateComponents();
            SetupButtons();
            InitializeUI();
            SubscribeToEvents();
            isInitialized = true;
            
            Debug.Log("[TopBarManager] 상단바 시스템 초기화 완료");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[TopBarManager] 초기화 중 오류: {ex.Message}");
        }
    }

    private void ValidateComponents()
    {
        if (GameTimeText == null)
        {
            Debug.LogError("[TopBarManager] GameTimeText가 설정되지 않았습니다.");
        }
        
        if (AMPMText == null)
        {
            Debug.LogWarning("[TopBarManager] AMPMText가 설정되지 않았습니다.");
        }
        
        if (CurrentWormNameText == null)
        {
            Debug.LogWarning("[TopBarManager] CurrentWormNameText가 설정되지 않았습니다.");
        }
    }

    private void SetupButtons()
    {
        // 재화 버튼 설정
        if (AcornButton != null)
        {
            AcornButton.onClick.AddListener(() => OnResourceClickedEvent?.Invoke("acorn"));
        }
        
        if (DiamondButton != null)
        {
            DiamondButton.onClick.AddListener(() => OnResourceClickedEvent?.Invoke("diamond"));
        }
        
        // 설정 버튼 설정
        if (SettingsButton != null)
        {
            SettingsButton.onClick.AddListener(() => OnSettingsClickedEvent?.Invoke());
        }
        
        // 알림 버튼 설정
        if (NotificationButton != null)
        {
            NotificationButton.onClick.AddListener(() => OnNotificationClickedEvent?.Invoke());
        }
    }

    private void InitializeUI()
    {
        // 초기 값 설정
        UpdateAcornCount(0, false);
        UpdateDiamondCount(0, false);
        UpdateNotificationCount(0);
        
        // 알림 배지 숨기기
        if (NotificationBadge != null)
        {
            NotificationBadge.SetActive(false);
        }
    }

    private void SubscribeToEvents()
    {
        // GameManager 이벤트 구독
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnResourceChangedEvent += OnResourceChanged;
        }
    }

    private void OnResourceChanged(int acornCount, int diamondCount)
    {
        UpdateAcornCount(acornCount, true);
        UpdateDiamondCount(diamondCount, true);
    }

    /// <summary>
    /// 시간 UI를 갱신합니다
    /// </summary>
    public void UpdateTime(int hour, int minute, string ampm)
    {
        if (!isInitialized) return;

        try
        {
            if (use24HourFormat)
            {
                // 24시간 형식
                if (GameTimeText != null)
                {
                    GameTimeText.SetText($"{hour:D2}:{minute:D2}");
                }
                
                if (AMPMText != null)
                {
                    AMPMText.gameObject.SetActive(false);
                }
            }
            else
            {
                // 12시간 형식
                if (GameTimeText != null)
                {
                    GameTimeText.SetText($"{hour}:{minute:D2}");
                }
                
                if (AMPMText != null)
                {
                    AMPMText.SetText(ampm);
                    AMPMText.gameObject.SetActive(true);
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[TopBarManager] 시간 업데이트 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 초 단위까지 표시하는 시간 업데이트
    /// </summary>
    public void UpdateTimeWithSeconds(int hour, int minute, int second)
    {
        if (!isInitialized) return;

        if (GameTimeText != null)
        {
            if (showSeconds)
            {
                GameTimeText.SetText($"{hour:D2}:{minute:D2}:{second:D2}");
            }
            else
            {
                GameTimeText.SetText($"{hour:D2}:{minute:D2}");
            }
        }
    }

    /// <summary>
    /// 현재 선택된 벌레 이름 UI를 갱신합니다
    /// </summary>
    public void UpdateCurrentWormName(string wormName)
    {
        if (!isInitialized) return;

        if (CurrentWormNameText != null)
        {
            CurrentWormNameText.SetText(wormName ?? "이름 없음");
        }
    }

    /// <summary>
    /// 벌레 아이콘 업데이트
    /// </summary>
    public void UpdateWormIcon(Sprite wormIcon)
    {
        if (!isInitialized || WormIconImage == null) return;

        WormIconImage.sprite = wormIcon;
    }

    /// <summary>
    /// 벌레 체력 업데이트
    /// </summary>
    public void UpdateWormHealth(float healthPercent)
    {
        if (!isInitialized || WormHealthSlider == null) return;

        healthPercent = Mathf.Clamp01(healthPercent);
        WormHealthSlider.value = healthPercent;
        
        // 체력에 따른 색상 변경
        var fillImage = WormHealthSlider.fillRect?.GetComponent<Image>();
        if (fillImage != null)
        {
            if (healthPercent > 0.6f)
                fillImage.color = Color.green;
            else if (healthPercent > 0.3f)
                fillImage.color = Color.yellow;
            else
                fillImage.color = Color.red;
        }
    }

    /// <summary>
    /// 도토리 개수 업데이트
    /// </summary>
    public void UpdateAcornCount(int newCount, bool animate = true)
    {
        if (!isInitialized || AcornCountText == null) return;

        if (animate)
        {
            StartCoroutine(AnimateNumberChange(AcornCountText, currentAcornCount, newCount));
        }
        else
        {
            AcornCountText.SetText(newCount.ToString(currencyFormat));
        }
        
        currentAcornCount = newCount;
    }

    /// <summary>
    /// 다이아몬드 개수 업데이트
    /// </summary>
    public void UpdateDiamondCount(int newCount, bool animate = true)
    {
        if (!isInitialized || DiamondCountText == null) return;

        if (animate)
        {
            StartCoroutine(AnimateNumberChange(DiamondCountText, currentDiamondCount, newCount));
        }
        else
        {
            DiamondCountText.SetText(newCount.ToString(currencyFormat));
        }
        
        currentDiamondCount = newCount;
    }

    /// <summary>
    /// 알림 개수 업데이트
    /// </summary>
    public void UpdateNotificationCount(int newCount)
    {
        if (!isInitialized) return;

        notificationCount = newCount;
        
        if (NotificationBadge != null)
        {
            NotificationBadge.SetActive(newCount > 0);
        }
        
        if (NotificationCountText != null)
        {
            if (newCount > 99)
            {
                NotificationCountText.SetText("99+");
            }
            else
            {
                NotificationCountText.SetText(newCount.ToString());
            }
        }
    }

    /// <summary>
    /// 알림 추가
    /// </summary>
    public void AddNotification()
    {
        UpdateNotificationCount(notificationCount + 1);
    }

    /// <summary>
    /// 알림 제거
    /// </summary>
    public void RemoveNotification()
    {
        if (notificationCount > 0)
        {
            UpdateNotificationCount(notificationCount - 1);
        }
    }

    /// <summary>
    /// 알림 초기화
    /// </summary>
    public void ClearNotifications()
    {
        UpdateNotificationCount(0);
    }

    /// <summary>
    /// 숫자 변경 애니메이션
    /// </summary>
    private IEnumerator AnimateNumberChange(TextMeshProUGUI textComponent, int fromValue, int toValue)
    {
        if (textComponent == null) yield break;

        // 이전 애니메이션 중지
        if (animationCoroutines.ContainsKey(textComponent))
        {
            StopCoroutine(animationCoroutines[textComponent]);
        }

        float elapsed = 0f;
        int currentValue = fromValue;

        while (elapsed < numberChangeDuration)
        {
            elapsed += Time.deltaTime;
            float progress = numberChangeCurve.Evaluate(elapsed / numberChangeDuration);
            currentValue = Mathf.RoundToInt(Mathf.Lerp(fromValue, toValue, progress));
            
            textComponent.SetText(currentValue.ToString(currencyFormat));
            yield return null;
        }

        textComponent.SetText(toValue.ToString(currencyFormat));
        
        // 하이라이트 효과
        yield return StartCoroutine(HighlightText(textComponent));
        
        // 애니메이션 완료
        animationCoroutines.Remove(textComponent);
    }

    /// <summary>
    /// 텍스트 하이라이트 효과
    /// </summary>
    private IEnumerator HighlightText(TextMeshProUGUI textComponent)
    {
        if (textComponent == null) yield break;

        Color originalColor = textComponent.color;
        float elapsed = 0f;

        while (elapsed < highlightDuration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Sin(elapsed / highlightDuration * Mathf.PI);
            textComponent.color = Color.Lerp(originalColor, highlightColor, progress);
            yield return null;
        }

        textComponent.color = originalColor;
    }

    /// <summary>
    /// UI 표시/숨김 설정
    /// </summary>
    public void SetUIElementVisible(string elementName, bool visible)
    {
        switch (elementName.ToLower())
        {
            case "time":
                if (GameTimeText != null) GameTimeText.gameObject.SetActive(visible);
                if (AMPMText != null) AMPMText.gameObject.SetActive(visible);
                break;
            case "worm":
                if (CurrentWormNameText != null) CurrentWormNameText.gameObject.SetActive(visible);
                if (WormIconImage != null) WormIconImage.gameObject.SetActive(visible);
                if (WormHealthSlider != null) WormHealthSlider.gameObject.SetActive(visible);
                break;
            case "resources":
                if (AcornCountText != null) AcornCountText.gameObject.SetActive(visible);
                if (DiamondCountText != null) DiamondCountText.gameObject.SetActive(visible);
                break;
            case "notifications":
                if (NotificationButton != null) NotificationButton.gameObject.SetActive(visible);
                break;
        }
    }

    /// <summary>
    /// 전체 UI 숨김/표시
    /// </summary>
    public void SetTopBarVisible(bool visible)
    {
        gameObject.SetActive(visible);
    }

    /// <summary>
    /// 시간 형식 설정
    /// </summary>
    public void SetTimeFormat(bool use24Hour)
    {
        use24HourFormat = use24Hour;
    }

    /// <summary>
    /// 초 표시 설정
    /// </summary>
    public void SetShowSeconds(bool show)
    {
        showSeconds = show;
    }

    /// <summary>
    /// 애니메이션 설정
    /// </summary>
    public void SetAnimationEnabled(bool enabled)
    {
        numberChangeDuration = enabled ? 0.5f : 0f;
    }

    /// <summary>
    /// 현재 UI 상태 정보 반환
    /// </summary>
    public string GetUIStatus()
    {
        return $"도토리: {currentAcornCount}, 다이아몬드: {currentDiamondCount}, 알림: {notificationCount}";
    }

    private void OnDestroy()
    {
        // 이벤트 구독 해제
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnResourceChangedEvent -= OnResourceChanged;
        }
        
        // 이벤트 초기화
        OnResourceClickedEvent = null;
        OnSettingsClickedEvent = null;
        OnNotificationClickedEvent = null;
    }
}
