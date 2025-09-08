using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using GGumtles.Data;
using GGumtles.Managers;

namespace GGumtles.Managers
{
    public class TopBarManager : MonoBehaviour
{
    public static TopBarManager Instance { get; private set; }

    [Header("TopBar 타입")]
    [SerializeField] private TopBarType currentTopBarType = TopBarType.NonGameState;
    // [SerializeField] private bool enableAutoSwitch = true; // 사용되지 않음

    [Header("NonGameState TopBar")]
    [SerializeField] private Transform nonGameStateTopBar;
    [SerializeField] private Button gameTimePopupButton;
    [SerializeField] private TextMeshProUGUI ampmText;
    [SerializeField] private TextMeshProUGUI gameTimeText;
    [SerializeField] private Button wormNamePopupButton;
    [SerializeField] private TextMeshProUGUI wormNameText;
    
    [Header("Count Popup Buttons")]
    [SerializeField] private Button acornCountPopupButton;
    [SerializeField] private Button diamondCountPopupButton;
    [SerializeField] private Button medalCountPopupButton;
    
    [Header("Count Texts")]
    [SerializeField] private TextMeshProUGUI acornCountText;
    [SerializeField] private TextMeshProUGUI diamondCountText;
    [SerializeField] private TextMeshProUGUI medalCountText;

    [Header("GameState TopBar")]
    [SerializeField] private Transform gameStateTopBar;
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private TextMeshProUGUI gameNameText;
    [SerializeField] private TextMeshProUGUI scoreText;
    

    
    [Header("UI 설정")]
    [SerializeField] private bool showSeconds = false;
    [SerializeField] private bool use24HourFormat = false;
    [SerializeField] private string currencyFormat = "N0";

    // TopBar 타입 열거형
    public enum TopBarType
    {
        NonGameState,    // 게임 외 상태 (메뉴, 탭 등)
        GameState        // 게임 내 상태 (실제 게임 플레이)
    }

    // 탭 타입 열거형
    public enum TabType
    {
        PlayTab,
        WormTab,
        HomeTab,
        ItemTab,
        AchievementTab
    }



    // 상태 관리
    private int currentAcornCount = 0;
    private int currentDiamondCount = 0;
    private int currentMedalCount = 0;
    private int currentScore = 0;
    private TopBarType previousTopBarType = TopBarType.NonGameState;
    private TabType currentTabType = TabType.HomeTab;
    private bool isInitialized = false;

    // 프로퍼티
    public int CurrentAcornCount => currentAcornCount;
    public int CurrentDiamondCount => currentDiamondCount;
    public int CurrentMedalCount => currentMedalCount;
    public int CurrentScore => currentScore;
    public TopBarType CurrentTopBarType => currentTopBarType;
    public TabType CurrentTabType => currentTabType;
    public bool IsInitialized => isInitialized;

    private void Awake()
    {
        InitializeSingleton();
    }

    public void Initialize()
    {
        InitializeTopBarSystem();
    }

    private void InitializeSingleton()
    {
        if (Instance == null)
        {
            Instance = this;
            // UI 요소는 Canvas 하위에 있어야 하므로 Canvas를 DontDestroyOnLoad로 설정
            Canvas parentCanvas = GetComponentInParent<Canvas>();
            if (parentCanvas != null)
            {
                Debug.Log("[TopBarManager] Canvas를 DontDestroyOnLoad로 설정합니다.");
                DontDestroyOnLoad(parentCanvas.gameObject);
            }
            else
            {
                Debug.LogWarning("[TopBarManager] Canvas를 찾을 수 없습니다. 직접 DontDestroyOnLoad를 설정합니다.");
                DontDestroyOnLoad(gameObject);
            }
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
            
            // 명시적으로 NonGameState로 설정
            currentTopBarType = TopBarType.NonGameState;
            ShowCurrentTopBar();
            
            Debug.Log($"[TopBarManager] 초기 TopBar 타입 설정: {currentTopBarType}");
            
            // TabManager가 아직 초기화되지 않았을 수 있으므로 나중에 구독
            StartCoroutine(SubscribeToTabManagerWhenReady());
            
            isInitialized = true;
            Debug.Log("[TopBarManager] TopBar 시스템 초기화 완료");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[TopBarManager] 초기화 중 오류: {ex.Message}");
        }
    }

    private IEnumerator SubscribeToTabManagerWhenReady()
    {
        yield return new WaitUntil(() => TabManager.Instance != null);
        
        // GameManager가 준비되면 현재 시간 설정
        yield return new WaitUntil(() => GameManager.Instance != null);
        if (GameManager.Instance != null)
        {
            // 현재 시간으로 초기화
            GameManager.Instance.OnGameTimeChangedEvent += OnGameTimeChanged;
            Debug.Log("[TopBarManager] GameManager 시간 이벤트 구독 완료");
            
            // 초기 AcornCount 활성화 (기본 탭이 Home이므로)
            SetCountButtonVisible(acornCountPopupButton, true);
            SetCountButtonVisible(diamondCountPopupButton, false);
            SetCountButtonVisible(medalCountPopupButton, false);
            UpdateAcornCount(currentAcornCount);
            Debug.Log("[TopBarManager] 초기 AcornCount 활성화 완료");
            
            // 초기 시간 강제 업데이트
            StartCoroutine(ForceInitialTimeUpdate());
        }
        
        // WormManager가 준비되면 현재 벌레 이름 설정
        yield return new WaitUntil(() => WormManager.Instance != null);
        if (WormManager.Instance != null)
        {
            // 벌레 변경 이벤트 구독
            WormManager.Instance.OnCurrentWormChangedEvent += OnCurrentWormChanged;
            Debug.Log("[TopBarManager] WormManager 벌레 변경 이벤트 구독 완료");
            
            // 현재 벌레 이름으로 초기화
            var currentWorm = WormManager.Instance.GetCurrentWorm();
            if (currentWorm != null)
            {
                UpdateCurrentWormName(currentWorm.name);
                Debug.Log($"[TopBarManager] 초기 벌레 이름 설정: {currentWorm.name}");
            }
            else
            {
                UpdateCurrentWormName("");
                Debug.Log("[TopBarManager] 초기 벌레가 없습니다.");
            }
        }
    }

    private IEnumerator ForceInitialTimeUpdate()
    {
        yield return new WaitForSeconds(0.1f); // 약간의 지연
        
        if (GameManager.Instance != null)
        {
            // GameManager의 ForceTimeUpdate 메서드 호출
            GameManager.Instance.ForceTimeUpdate();
            Debug.Log("[TopBarManager] 초기 시간 강제 업데이트 완료");
        }
    }

    private void ValidateComponents()
    {
        if (nonGameStateTopBar == null)
        {
            Debug.LogError("[TopBarManager] NonGameStateTopBar가 설정되지 않았습니다.");
        }
        
        if (gameStateTopBar == null)
        {
            Debug.LogError("[TopBarManager] GameStateTopBar가 설정되지 않았습니다.");
        }
        
        if (gameTimePopupButton == null)
        {
            Debug.LogWarning("[TopBarManager] GameTimePopupButton이 설정되지 않았습니다.");
        }
        
        if (wormNamePopupButton == null)
        {
            Debug.LogWarning("[TopBarManager] WormNamePopupButton이 설정되지 않았습니다.");
        }
        
        if (acornCountPopupButton == null)
        {
            Debug.LogWarning("[TopBarManager] AcornCountPopupButton이 설정되지 않았습니다.");
        }
        
        if (diamondCountPopupButton == null)
        {
            Debug.LogWarning("[TopBarManager] DiamondCountPopupButton이 설정되지 않았습니다.");
        }
        
        if (medalCountPopupButton == null)
        {
            Debug.LogWarning("[TopBarManager] MedalCountPopupButton이 설정되지 않았습니다.");
        }
    }

    private void SetupButtons()
    {
        // NonGameState TopBar 버튼 설정
        if (gameTimePopupButton != null)
        {
            gameTimePopupButton.onClick.AddListener(HandleGameTimePopupClicked);
        }
        
        if (wormNamePopupButton != null)
        {
            wormNamePopupButton.onClick.AddListener(HandleWormNamePopupClicked);
        }
        
        // Count Popup 버튼들 설정
        if (acornCountPopupButton != null)
        {
            acornCountPopupButton.onClick.AddListener(HandleAcornCountPopupClicked);
        }
        
        if (diamondCountPopupButton != null)
        {
            diamondCountPopupButton.onClick.AddListener(HandleDiamondCountPopupClicked);
        }
        
        if (medalCountPopupButton != null)
        {
            medalCountPopupButton.onClick.AddListener(HandleMedalCountPopupClicked);
        }
    }

    private void InitializeUI()
    {
        // 초기 값 설정
        if (acornCountText != null)
            UpdateCountText(acornCountText, 0);
        if (diamondCountText != null)
            UpdateCountText(diamondCountText, 0);
        if (medalCountText != null)
            UpdateCountText(medalCountText, 0);
        UpdateScore(0);
    }

    private void SubscribeToEvents()
    {
        // GameManager 이벤트 구독 (리소스 변경만)
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnResourceChangedEvent += OnResourceChanged;
        }
        
        // WormManager 이벤트는 SubscribeToTabManagerWhenReady()에서 구독
    }

    private void OnResourceChanged(int acornCount, int diamondCount)
    {
        UpdateAcornCount(acornCount);
        UpdateDiamondCount(diamondCount);
    }

    private void OnCurrentWormChanged(WormData previousWorm, WormData newWorm)
    {
        if (newWorm != null)
        {
            UpdateCurrentWormName(newWorm.name);
            Debug.Log($"[TopBarManager] 벌레 이름 업데이트: {newWorm.name}");
        }
        else
        {
            UpdateCurrentWormName("벌레 없음");
            Debug.Log("[TopBarManager] 현재 벌레가 없습니다.");
        }
    }

    private void OnGameTimeChanged(int hour, int minute, string ampm)
    {
        UpdateTime(hour, minute, ampm);
    }

    private void OnTabChanged(int fromIndex, int toIndex, TabManager.TabType fromType, TabManager.TabType toType)
    {
        // TabManager의 TabType을 TopBarManager의 TabType으로 변환
        TabType newTabType = ConvertTabType(toType);
        SetCurrentTab(newTabType);
        
        Debug.Log($"[TopBarManager] 탭 변경 감지: {fromType} → {toType} (인덱스: {fromIndex} → {toIndex})");
        Debug.Log($"[TopBarManager] 변환된 탭 타입: {newTabType}");
        
        // 탭 변경 시 카운트 텍스트 업데이트
        UpdateNonGameStateCountTexts();
    }

    private TabType ConvertTabType(TabManager.TabType tabManagerType)
    {
        return tabManagerType switch
        {
            TabManager.TabType.Play => TabType.PlayTab,
            TabManager.TabType.Worm => TabType.WormTab,
            TabManager.TabType.Home => TabType.HomeTab,
            TabManager.TabType.Item => TabType.ItemTab,
            TabManager.TabType.Achievement => TabType.AchievementTab,
            _ => TabType.HomeTab
        };
    }

    /// <summary>
    /// TopBar 타입 전환
    /// </summary>
    public void SwitchToTopBar(TopBarType topBarType)
    {
        if (currentTopBarType == topBarType) return;

        previousTopBarType = currentTopBarType;
        currentTopBarType = topBarType;

        ShowCurrentTopBar();
        
        Debug.Log($"[TopBarManager] TopBar 전환: {previousTopBarType} → {currentTopBarType}");
    }

    /// <summary>
    /// 현재 TopBar 표시
    /// </summary>
    private void ShowCurrentTopBar()
    {
        if (nonGameStateTopBar != null)
        {
            nonGameStateTopBar.gameObject.SetActive(currentTopBarType == TopBarType.NonGameState);
        }
        
        if (gameStateTopBar != null)
        {
            gameStateTopBar.gameObject.SetActive(currentTopBarType == TopBarType.GameState);
        }

        if (currentTopBarType == TopBarType.NonGameState)
        {
            UpdateNonGameStateCountTexts();
        }
    }

    /// <summary>
    /// NonGameState TopBar의 CountText 업데이트 (탭에 따라)
    /// </summary>
    private void UpdateNonGameStateCountTexts()
    {
        Debug.Log($"[TopBarManager] UpdateNonGameStateCountTexts 호출됨 - 현재 탭: {currentTabType}");
        
        // 모든 CountText와 CountPopupButton 비활성화
        SetCountTextVisible(acornCountText, false);
        SetCountTextVisible(diamondCountText, false);
        SetCountTextVisible(medalCountText, false);
        
        SetCountButtonVisible(acornCountPopupButton, false);
        SetCountButtonVisible(diamondCountPopupButton, false);
        SetCountButtonVisible(medalCountPopupButton, false);

        // 현재 탭에 따라 해당 CountText와 CountPopupButton만 활성화
        switch (currentTabType)
        {
            case TabType.PlayTab:
            case TabType.WormTab:
            case TabType.HomeTab:
                SetCountTextVisible(acornCountText, true);
                SetCountButtonVisible(acornCountPopupButton, true);
                UpdateAcornCount(currentAcornCount);
                Debug.Log($"[TopBarManager] AcornCount 활성화 - 탭: {currentTabType}");
                break;
            case TabType.ItemTab:
                SetCountTextVisible(diamondCountText, true);
                SetCountButtonVisible(diamondCountPopupButton, true);
                UpdateDiamondCount(currentDiamondCount);
                Debug.Log($"[TopBarManager] DiamondCount 활성화 - 탭: {currentTabType}");
                break;
            case TabType.AchievementTab:
                SetCountTextVisible(medalCountText, true);
                SetCountButtonVisible(medalCountPopupButton, true);
                UpdateMedalCount(currentMedalCount);
                Debug.Log($"[TopBarManager] MedalCount 활성화 - 탭: {currentTabType}");
                break;
        }
    }

    /// <summary>
    /// 현재 탭 설정
    /// </summary>
    public void SetCurrentTab(TabType tabType)
    {
        currentTabType = tabType;
        if (currentTopBarType == TopBarType.NonGameState)
        {
            UpdateNonGameStateCountTexts();
        }
    }

    /// <summary>
    /// NonGameState TopBar 표시
    /// </summary>
    public void ShowNonGameStateTopBar()
    {
        SwitchToTopBar(TopBarType.NonGameState);
    }

    /// <summary>
    /// GameState TopBar 표시
    /// </summary>
    public void ShowGameStateTopBar()
    {
        SwitchToTopBar(TopBarType.GameState);
    }
    
    /// <summary>
    /// TopBar 타입 설정
    /// </summary>
    public void SetTopBarType(TopBarType topBarType)
    {
        SwitchToTopBar(topBarType);
    }

    /// <summary>
    /// 시간 UI를 갱신합니다 (NonGameState)
    /// </summary>
    public void UpdateTime(int hour, int minute, string ampm)
    {
        if (!isInitialized) return;

        try
        {
            if (currentTopBarType == TopBarType.NonGameState)
            {
                if (use24HourFormat)
                {
                    // 24시간 형식
                    if (gameTimeText != null)
                    {
                        gameTimeText.SetText($"{hour:D2}:{minute:D2}");
                    }
                }
                else
                {
                    // 12시간 형식
                    if (gameTimeText != null)
                    {
                        gameTimeText.SetText($"{hour}:{minute:D2}");
                    }
                    
                    if (ampmText != null)
                    {
                        ampmText.SetText(ampm);
                    }
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[TopBarManager] 시간 업데이트 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 초 단위까지 표시하는 시간 업데이트 (NonGameState)
    /// </summary>
    public void UpdateTimeWithSeconds(int hour, int minute, int second)
    {
        if (!isInitialized) return;

        if (currentTopBarType == TopBarType.NonGameState && gameTimeText != null)
        {
            if (showSeconds)
            {
                gameTimeText.SetText($"{hour:D2}:{minute:D2}:{second:D2}");
            }
            else
            {
                gameTimeText.SetText($"{hour:D2}:{minute:D2}");
            }
        }
    }

    /// <summary>
    /// 현재 선택된 벌레 이름 UI를 갱신합니다 (NonGameState)
    /// </summary>
    public void UpdateCurrentWormName(string wormName)
    {
        if (!isInitialized) return;

        if (currentTopBarType == TopBarType.NonGameState && wormNameText != null)
        {
            wormNameText.SetText(wormName ?? "이름 없음");
        }
    }

    /// <summary>
    /// 도토리 개수 업데이트
    /// </summary>
    public void UpdateAcornCount(int newCount)
    {
        if (!isInitialized) return;

        currentAcornCount = newCount;

        if (currentTopBarType == TopBarType.NonGameState && acornCountText != null)
        {
            UpdateCountText(acornCountText, newCount);
        }
    }

    /// <summary>
    /// 다이아몬드 개수 업데이트
    /// </summary>
    public void UpdateDiamondCount(int newCount)
    {
        if (!isInitialized) return;

        currentDiamondCount = newCount;

        if (diamondCountText != null && diamondCountText.gameObject.activeInHierarchy)
        {
            UpdateCountText(diamondCountText, newCount);
        }
    }

    /// <summary>
    /// 메달 개수 업데이트
    /// </summary>
    public void UpdateMedalCount(int newCount)
    {
        if (!isInitialized) return;

        currentMedalCount = newCount;

        if (medalCountText != null && medalCountText.gameObject.activeInHierarchy)
        {
            UpdateCountText(medalCountText, newCount);
        }
    }

    /// <summary>
    /// CountText 업데이트
    /// </summary>
    private void UpdateCountText(TextMeshProUGUI textComponent, int newCount)
    {
        if (textComponent == null) return;
        textComponent.SetText(newCount.ToString(currencyFormat));
    }

    /// <summary>
    /// CountText 표시/숨김 설정
    /// </summary>
    private void SetCountTextVisible(TextMeshProUGUI textComponent, bool visible)
    {
        if (textComponent != null)
        {
            textComponent.gameObject.SetActive(visible);
        }
    }

    /// <summary>
    /// CountPopupButton 표시/숨김 설정
    /// </summary>
    private void SetCountButtonVisible(Button button, bool visible)
    {
        if (button != null)
        {
            button.gameObject.SetActive(visible);
            Debug.Log($"[TopBarManager] CountButton 설정: {button.name} → {visible}");
        }
        else
        {
            Debug.LogError("[TopBarManager] CountButton이 null입니다!");
        }
    }

    /// <summary>
    /// 탭에 맞는 CountButton 활성화
    /// </summary>
    /// <param name="tabIndex">탭 인덱스 (0-4)</param>
    public void ActivateCountButtonForTab(int tabIndex)
    {
        // 모든 카운트 텍스트와 버튼 비활성화
        SetCountTextVisible(acornCountText, false);
        SetCountTextVisible(diamondCountText, false);
        SetCountTextVisible(medalCountText, false);
        
        SetCountButtonVisible(acornCountPopupButton, false);
        SetCountButtonVisible(diamondCountPopupButton, false);
        SetCountButtonVisible(medalCountPopupButton, false);
        
        // 탭에 맞는 CountText와 CountButton 활성화
        switch (tabIndex)
        {
            case 0: // Play 탭
            case 1: // Worm 탭
            case 2: // Home 탭
                SetCountTextVisible(acornCountText, true);
                SetCountButtonVisible(acornCountPopupButton, true);
                UpdateAcornCount(currentAcornCount);
                Debug.Log($"[TopBarManager] 탭 {tabIndex} - AcornCount 텍스트/버튼 활성화");
                break;
                
            case 3: // Item 탭
                SetCountTextVisible(diamondCountText, true);
                SetCountButtonVisible(diamondCountPopupButton, true);
                UpdateDiamondCount(currentDiamondCount);
                Debug.Log($"[TopBarManager] 탭 {tabIndex} - DiamondCount 텍스트/버튼 활성화");
                break;
                
            case 4: // Achievement 탭
                SetCountTextVisible(medalCountText, true);
                SetCountButtonVisible(medalCountPopupButton, true);
                UpdateMedalCount(currentMedalCount);
                Debug.Log($"[TopBarManager] 탭 {tabIndex} - MedalCount 텍스트/버튼 활성화");
                break;
                
            default:
                Debug.LogWarning($"[TopBarManager] 알 수 없는 탭 인덱스: {tabIndex}");
                break;
        }
    }

    /// <summary>
    /// AcornCount 버튼 활성화 (하위 호환용)
    /// </summary>
    public void ActivateAcornCountButton()
    {
        ActivateCountButtonForTab(0); // Play 탭으로 처리
    }

    /// <summary>
    /// DiamondCount 버튼 활성화 (하위 호환용)
    /// </summary>
    public void ActivateDiamondCountButton()
    {
        ActivateCountButtonForTab(3); // Item 탭으로 처리
    }

    /// <summary>
    /// MedalCount 버튼 활성화 (하위 호환용)
    /// </summary>
    public void ActivateMedalCountButton()
    {
        ActivateCountButtonForTab(4); // Achievement 탭으로 처리
    }

    /// <summary>
    /// 게임 시간 업데이트 (GameState - 스톱워치/제한시간)
    /// </summary>
    public void UpdateGameTime(int hour, int minute, int second = 0)
    {
        if (!isInitialized) return;

        if (currentTopBarType == TopBarType.GameState && timeText != null)
        {
            if (showSeconds)
            {
                timeText.SetText($"{hour:D2}:{minute:D2}:{second:D2}");
            }
            else
            {
                timeText.SetText($"{hour:D2}:{minute:D2}");
            }
        }
    }

    /// <summary>
    /// 게임 이름 업데이트 (GameState)
    /// </summary>
    public void UpdateGameName(string gameName)
    {
        if (!isInitialized) return;

        if (currentTopBarType == TopBarType.GameState && gameNameText != null)
        {
            gameNameText.SetText(gameName ?? "게임");
        }
    }

    /// <summary>
    /// 스코어 업데이트 (GameState)
    /// </summary>
    public void UpdateScore(int newScore)
    {
        if (!isInitialized) return;

        currentScore = newScore;

        if (currentTopBarType == TopBarType.GameState && scoreText != null)
        {
            scoreText.SetText(newScore.ToString(currencyFormat));
        }
    }



    /// <summary>
    /// 버튼 클릭 이벤트 핸들러들
    /// </summary>
    private void HandleGameTimePopupClicked()
    {
        Debug.Log("[TopBarManager] GameTimePopup 클릭됨");
    }

    private void HandleWormNamePopupClicked()
    {
        Debug.Log("[TopBarManager] WormNamePopup 클릭됨");
    }

    private void HandleAcornCountPopupClicked()
    {
        Debug.Log("[TopBarManager] AcornCountPopup 클릭됨");
    }

    private void HandleDiamondCountPopupClicked()
    {
        Debug.Log("[TopBarManager] DiamondCountPopup 클릭됨");
    }

    private void HandleMedalCountPopupClicked()
    {
        Debug.Log("[TopBarManager] MedalCountPopup 클릭됨");
    }

    /// <summary>
    /// UI 표시/숨김 설정
    /// </summary>
    public void SetUIElementVisible(string elementName, bool visible)
    {
        switch (elementName.ToLower())
        {
            case "gametimepopup":
                if (gameTimePopupButton != null) gameTimePopupButton.gameObject.SetActive(visible);
                break;
            case "wormnamepopup":
                if (wormNamePopupButton != null) wormNamePopupButton.gameObject.SetActive(visible);
                break;
            case "acorncountpopup":
                if (acornCountPopupButton != null) acornCountPopupButton.gameObject.SetActive(visible);
                break;
            case "diamondcountpopup":
                if (diamondCountPopupButton != null) diamondCountPopupButton.gameObject.SetActive(visible);
                break;
            case "medalcountpopup":
                if (medalCountPopupButton != null) medalCountPopupButton.gameObject.SetActive(visible);
                break;
            case "acorncounttext":
                if (acornCountText != null) acornCountText.gameObject.SetActive(visible);
                break;
            case "diamondcounttext":
                if (diamondCountText != null) diamondCountText.gameObject.SetActive(visible);
                break;
            case "medalcounttext":
                if (medalCountText != null) medalCountText.gameObject.SetActive(visible);
                break;
            case "timetext":
                if (timeText != null) timeText.gameObject.SetActive(visible);
                break;
            case "gamename":
                if (gameNameText != null) gameNameText.gameObject.SetActive(visible);
                break;
            case "score":
                if (scoreText != null) scoreText.gameObject.SetActive(visible);
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
    /// TopBar 정보 반환
    /// </summary>
    public string GetTopBarInfo()
    {
        var info = new System.Text.StringBuilder();
        info.AppendLine($"[TopBar 정보]");
        info.AppendLine($"초기화됨: {isInitialized}");
        info.AppendLine($"현재 TopBar 타입: {currentTopBarType}");
        info.AppendLine($"현재 탭 타입: {currentTabType}");
        info.AppendLine($"도토리 개수: {currentAcornCount}");
        info.AppendLine($"다이아몬드 개수: {currentDiamondCount}");
        info.AppendLine($"메달 개수: {currentMedalCount}");
        info.AppendLine($"현재 스코어: {currentScore}");
        info.AppendLine($"24시간 형식: {use24HourFormat}");
        info.AppendLine($"초 표시: {showSeconds}");

        return info.ToString();
    }

    /// <summary>
    /// 현재 UI 상태 정보 반환
    /// </summary>
    public string GetUIStatus()
    {
        return $"도토리: {currentAcornCount}, 다이아몬드: {currentDiamondCount}, 메달: {currentMedalCount}, 스코어: {currentScore}";
    }

    private void OnDestroy()
    {
        // 이벤트 구독 해제
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnResourceChangedEvent -= OnResourceChanged;
            GameManager.Instance.OnGameTimeChangedEvent -= OnGameTimeChanged;
        }
        
        if (WormManager.Instance != null)
        {
            WormManager.Instance.OnCurrentWormChangedEvent -= OnCurrentWormChanged;
        }
    }
    }
}
