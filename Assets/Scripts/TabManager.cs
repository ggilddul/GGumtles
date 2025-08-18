using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class TabManager : MonoBehaviour
{
    public static TabManager Instance { get; private set; }

    [Header("탭 설정")]
    [SerializeField] private List<GameObject> tabPanels;
    [SerializeField] private List<Button> tabButtons;
    [SerializeField] private List<TabData> tabDataList;
    
    [Header("애니메이션 설정")]
    [SerializeField] private float transitionDuration = 0.3f;
    [SerializeField] private AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private bool useAnimation = true;
    
    [Header("UI 설정")]
    [SerializeField] private float defaultHeight = 240f;
    [SerializeField] private float activeHeight = 320f;
    [SerializeField] private Color defaultTabColor = Color.white;
    [SerializeField] private Color activeTabColor = Color.yellow;
    [SerializeField] private Color defaultTextColor = Color.black;
    [SerializeField] private Color activeTextColor = Color.white;

    // 탭 타입 열거형
    public enum TabType
    {
        Main,
        Collection,
        Shop,
        Settings,
        Custom
    }

    // 탭 데이터 클래스
    [System.Serializable]
    public class TabData
    {
        public TabType type;
        public string tabName;
        public Sprite tabIcon;
        public bool isUnlocked = true;
        public int unlockRequirement = 0;
        public System.Action onTabOpen;
        public System.Action onTabClose;
        
        public TabData(TabType type, string name, Sprite icon = null)
        {
            this.type = type;
            this.tabName = name;
            this.tabIcon = icon;
        }
    }

    // 탭 정보 클래스
    [System.Serializable]
    public class TabInfo
    {
        public int index;
        public TabType type;
        public GameObject panel;
        public Button button;
        public bool isActive;
        public Coroutine animationCoroutine;
        
        public TabInfo(int index, TabType type, GameObject panel, Button button)
        {
            this.index = index;
            this.type = type;
            this.panel = panel;
            this.button = button;
            this.isActive = false;
        }
    }

    // 이벤트 정의
    public delegate void OnTabChanged(int fromIndex, int toIndex, TabType fromType, TabType toType);
    public event OnTabChanged OnTabChangedEvent;

    public delegate void OnTabAnimationComplete(int tabIndex, TabType tabType);
    public event OnTabAnimationComplete OnTabAnimationCompleteEvent;

    // 상태 관리
    private int currentTabIndex = -1;
    private TabInfo currentTabInfo;
    private List<TabInfo> tabInfoList = new List<TabInfo>();
    private bool isInitialized = false;
    private bool isTransitioning = false;

    // 프로퍼티
    public int CurrentTabIndex => currentTabIndex;
    public TabType CurrentTabType => currentTabInfo?.type ?? TabType.Main;
    public bool IsTransitioning => isTransitioning;
    public int TabCount => tabPanels?.Count ?? 0;

    private void Awake()
    {
        InitializeSingleton();
    }

    private void Start()
    {
        InitializeTabSystem();
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

    private void InitializeTabSystem()
    {
        try
        {
            ValidateComponents();
            InitializeTabInfo();
            SetupTabButtons();
            LoadTabState();
            isInitialized = true;
            
            Debug.Log($"[TabManager] 탭 시스템 초기화 완료 - 총 {TabCount}개 탭");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[TabManager] 초기화 중 오류: {ex.Message}");
        }
    }

    private void ValidateComponents()
    {
        if (tabPanels == null || tabPanels.Count == 0)
        {
            Debug.LogError("[TabManager] 탭 패널이 설정되지 않았습니다.");
        }
        
        if (tabButtons == null || tabButtons.Count == 0)
        {
            Debug.LogError("[TabManager] 탭 버튼이 설정되지 않았습니다.");
        }
        
        if (tabPanels.Count != tabButtons.Count)
        {
            Debug.LogError($"[TabManager] 탭 패널({tabPanels.Count})과 버튼({tabButtons.Count})의 개수가 일치하지 않습니다.");
        }
    }

    private void InitializeTabInfo()
    {
        tabInfoList.Clear();
        
        for (int i = 0; i < tabPanels.Count; i++)
        {
            TabType tabType = GetTabTypeByIndex(i);
            TabInfo tabInfo = new TabInfo(i, tabType, tabPanels[i], tabButtons[i]);
            tabInfoList.Add(tabInfo);
        }
    }

    private TabType GetTabTypeByIndex(int index)
    {
        if (tabDataList != null && index < tabDataList.Count)
        {
            return tabDataList[index].type;
        }
        
        // 기본 타입 반환
        return index switch
        {
            0 => TabType.Main,
            1 => TabType.Collection,
            2 => TabType.Shop,
            3 => TabType.Settings,
            _ => TabType.Custom
        };
    }

    private void SetupTabButtons()
    {
        for (int i = 0; i < tabButtons.Count; i++)
        {
            int tabIndex = i; // 클로저를 위한 로컬 변수
            tabButtons[i].onClick.AddListener(() => OpenTab(tabIndex));
            
            // 초기 상태 설정
            UpdateTabVisual(i, false);
        }
    }

    private void LoadTabState()
    {
        // 저장된 탭 상태 로드 (필요시 구현)
        // 기본적으로 첫 번째 탭 열기
        if (tabPanels.Count > 0)
        {
            OpenTab(0);
        }
    }

    /// <summary>
    /// 탭 열기
    /// </summary>
    public void OpenTab(int index)
    {
        if (!isInitialized)
        {
            Debug.LogWarning("[TabManager] 아직 초기화되지 않았습니다.");
            return;
        }

        if (index < 0 || index >= tabPanels.Count)
        {
            Debug.LogError($"[TabManager] 잘못된 탭 인덱스: {index}");
            return;
        }

        if (index == currentTabIndex)
        {
            Debug.Log($"[TabManager] 이미 열린 탭: {index}");
            return;
        }

        if (isTransitioning)
        {
            Debug.LogWarning("[TabManager] 탭 전환 중입니다.");
            return;
        }

        // 탭 잠금 확인
        if (!IsTabUnlocked(index))
        {
            Debug.LogWarning($"[TabManager] 잠긴 탭: {index}");
            ShowTabLockedMessage(index);
            return;
        }

        StartCoroutine(OpenTabCoroutine(index));
    }

    /// <summary>
    /// 탭 타입으로 열기
    /// </summary>
    public void OpenTab(TabType tabType)
    {
        int index = GetTabIndexByType(tabType);
        if (index >= 0)
        {
            OpenTab(index);
        }
        else
        {
            Debug.LogError($"[TabManager] 찾을 수 없는 탭 타입: {tabType}");
        }
    }

    private int GetTabIndexByType(TabType tabType)
    {
        for (int i = 0; i < tabInfoList.Count; i++)
        {
            if (tabInfoList[i].type == tabType)
            {
                return i;
            }
        }
        return -1;
    }

    private IEnumerator OpenTabCoroutine(int newIndex)
    {
        isTransitioning = true;
        int previousIndex = currentTabIndex;
        TabType previousType = currentTabInfo?.type ?? TabType.Main;
        TabType newType = tabInfoList[newIndex].type;

        // 이전 탭 닫기 콜백
        if (currentTabInfo != null && previousIndex >= 0)
        {
            var previousTabData = GetTabData(previousIndex);
            previousTabData?.onTabClose?.Invoke();
        }

        // 새 탭 열기 콜백
        var newTabData = GetTabData(newIndex);
        newTabData?.onTabOpen?.Invoke();

        // 애니메이션 실행
        if (useAnimation)
        {
            yield return StartCoroutine(AnimateTabTransition(previousIndex, newIndex));
        }
        else
        {
            // 즉시 전환
            CloseAllTabs();
            OpenTabImmediate(newIndex);
        }

        // 상태 업데이트
        currentTabIndex = newIndex;
        currentTabInfo = tabInfoList[newIndex];
        currentTabInfo.isActive = true;

        // 이벤트 발생
        OnTabChangedEvent?.Invoke(previousIndex, newIndex, previousType, newType);
        OnTabAnimationCompleteEvent?.Invoke(newIndex, newType);

        isTransitioning = false;
        
        Debug.Log($"[TabManager] 탭 전환 완료: {previousIndex} -> {newIndex} ({previousType} -> {newType})");
    }

    private IEnumerator AnimateTabTransition(int fromIndex, int toIndex)
    {
        // 페이드 아웃
        if (fromIndex >= 0 && fromIndex < tabPanels.Count)
        {
            yield return StartCoroutine(FadeOutTab(fromIndex));
        }

        // 탭 패널 전환
        CloseAllTabs();
        OpenTabImmediate(toIndex);

        // 페이드 인
        yield return StartCoroutine(FadeInTab(toIndex));
    }

    private IEnumerator FadeOutTab(int index)
    {
        GameObject panel = tabPanels[index];
        CanvasGroup canvasGroup = panel.GetComponent<CanvasGroup>();
        
        if (canvasGroup == null)
        {
            canvasGroup = panel.AddComponent<CanvasGroup>();
        }

        float elapsed = 0f;
        float startAlpha = canvasGroup.alpha;

        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            float progress = transitionCurve.Evaluate(elapsed / transitionDuration);
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, progress);
            yield return null;
        }

        canvasGroup.alpha = 0f;
    }

    private IEnumerator FadeInTab(int index)
    {
        GameObject panel = tabPanels[index];
        CanvasGroup canvasGroup = panel.GetComponent<CanvasGroup>();
        
        if (canvasGroup == null)
        {
            canvasGroup = panel.AddComponent<CanvasGroup>();
        }

        canvasGroup.alpha = 0f;
        float elapsed = 0f;

        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            float progress = transitionCurve.Evaluate(elapsed / transitionDuration);
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, progress);
            yield return null;
        }

        canvasGroup.alpha = 1f;
    }

    private void CloseAllTabs()
    {
        for (int i = 0; i < tabPanels.Count; i++)
        {
            tabPanels[i].SetActive(false);
            UpdateTabVisual(i, false);
            
            if (tabInfoList[i] != null)
            {
                tabInfoList[i].isActive = false;
            }
        }
    }

    private void OpenTabImmediate(int index)
    {
        tabPanels[index].SetActive(true);
        UpdateTabVisual(index, true);
        
        if (tabInfoList[index] != null)
        {
            tabInfoList[index].isActive = true;
        }
    }

    private void UpdateTabVisual(int index, bool isActive)
    {
        if (index < 0 || index >= tabButtons.Count) return;

        Button button = tabButtons[index];
        if (button == null) return;

        // 높이 변경
        LayoutElement layoutElement = button.GetComponent<LayoutElement>();
        if (layoutElement != null)
        {
            layoutElement.preferredHeight = isActive ? activeHeight : defaultHeight;
        }

        // 색상 변경
        Image buttonImage = button.GetComponent<Image>();
        if (buttonImage != null)
        {
            buttonImage.color = isActive ? activeTabColor : defaultTabColor;
        }

        // 텍스트 색상 변경
        TMP_Text buttonText = button.GetComponentInChildren<TMP_Text>();
        if (buttonText != null)
        {
            buttonText.color = isActive ? activeTextColor : defaultTextColor;
        }

        // 아이콘 변경 (있는 경우)
        var iconImage = button.transform.Find("Icon")?.GetComponent<Image>();
        if (iconImage != null && tabDataList != null && index < tabDataList.Count)
        {
            iconImage.sprite = tabDataList[index].tabIcon;
        }
    }

    private TabData GetTabData(int index)
    {
        if (tabDataList != null && index < tabDataList.Count)
        {
            return tabDataList[index];
        }
        return null;
    }

    private bool IsTabUnlocked(int index)
    {
        var tabData = GetTabData(index);
        if (tabData == null) return true; // 데이터가 없으면 기본적으로 잠금 해제
        
        return tabData.isUnlocked;
    }

    private void ShowTabLockedMessage(int index)
    {
        var tabData = GetTabData(index);
        if (tabData != null)
        {
            string message = $"이 탭을 열려면 {tabData.unlockRequirement}개가 필요합니다.";
            
            // 토스트 메시지 또는 팝업 표시
            if (PopupManager.Instance != null)
            {
                PopupManager.Instance.ShowToast(message, 2f);
            }
        }
    }

    /// <summary>
    /// 탭 잠금 해제
    /// </summary>
    public void UnlockTab(int index)
    {
        var tabData = GetTabData(index);
        if (tabData != null)
        {
            tabData.isUnlocked = true;
            Debug.Log($"[TabManager] 탭 잠금 해제: {index} ({tabData.tabName})");
        }
    }

    /// <summary>
    /// 탭 잠금
    /// </summary>
    public void LockTab(int index)
    {
        var tabData = GetTabData(index);
        if (tabData != null)
        {
            tabData.isUnlocked = false;
            
            // 현재 열린 탭이 잠기면 첫 번째 탭으로 이동
            if (currentTabIndex == index)
            {
                OpenTab(0);
            }
            
            Debug.Log($"[TabManager] 탭 잠금: {index} ({tabData.tabName})");
        }
    }

    /// <summary>
    /// 탭 정보 가져오기
    /// </summary>
    public TabInfo GetTabInfo(int index)
    {
        if (index >= 0 && index < tabInfoList.Count)
        {
            return tabInfoList[index];
        }
        return null;
    }

    /// <summary>
    /// 탭 활성화 여부 확인
    /// </summary>
    public bool IsTabActive(int index)
    {
        var tabInfo = GetTabInfo(index);
        return tabInfo?.isActive ?? false;
    }

    /// <summary>
    /// 탭 잠금 여부 확인
    /// </summary>
    public bool IsTabLocked(int index)
    {
        return !IsTabUnlocked(index);
    }

    /// <summary>
    /// 애니메이션 설정 변경
    /// </summary>
    public void SetAnimationEnabled(bool enabled)
    {
        useAnimation = enabled;
        Debug.Log($"[TabManager] 애니메이션 {(enabled ? "활성화" : "비활성화")}");
    }

    /// <summary>
    /// 전환 시간 설정
    /// </summary>
    public void SetTransitionDuration(float duration)
    {
        transitionDuration = Mathf.Max(0.1f, duration);
        Debug.Log($"[TabManager] 전환 시간 설정: {transitionDuration}초");
    }

    private void OnDestroy()
    {
        // 이벤트 구독 해제
        OnTabChangedEvent = null;
        OnTabAnimationCompleteEvent = null;
    }
}
