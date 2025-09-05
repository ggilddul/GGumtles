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
    
    [Header("UI 설정")]
    [SerializeField] private float defaultHeight = 240f;
    [SerializeField] private float activeHeight = 320f;

    // 탭 타입 열거형
    public enum TabType
    {
        Play,       // 0: 플레이 탭
        Worm,       // 1: 웜 탭
        Home,       // 2: 홈 탭
        Item,       // 3: 아이템 탭
        Achievement // 4: 업적 탭
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



    // 상태 관리
    private int currentTabIndex = -1;
    private TabInfo currentTabInfo;
    private List<TabInfo> tabInfoList = new List<TabInfo>();
    private bool isInitialized = false;

    // 프로퍼티
    public int CurrentTabIndex => currentTabIndex;
    public TabType CurrentTabType => currentTabInfo?.type ?? TabType.Play;
    public int TabCount => tabPanels?.Count ?? 0;

    private void Awake()
    {
        InitializeSingleton();
    }

    public void Initialize()
    {
        InitializeTabSystem();
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
                Debug.Log("[TabManager] Canvas를 DontDestroyOnLoad로 설정합니다.");
                DontDestroyOnLoad(parentCanvas.gameObject);
            }
            else
            {
                Debug.LogWarning("[TabManager] Canvas를 찾을 수 없습니다. 직접 DontDestroyOnLoad를 설정합니다.");
                DontDestroyOnLoad(gameObject);
            }
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
            InitializeTabs();
            isInitialized = true;
            Debug.Log("[TabManager] 탭 시스템 초기화 완료");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[TabManager] 초기화 중 오류: {ex.Message}");
        }
    }

    private void ValidateComponents()
    {
        if (tabPanels == null || tabButtons == null || tabPanels.Count != tabButtons.Count)
        {
            Debug.LogError("[TabManager] 탭 패널과 버튼의 개수가 일치하지 않습니다.");
        }
    }

    private void InitializeTabs()
    {
        if (tabPanels == null || tabButtons == null) return;

        // 탭 정보 리스트 초기화
        tabInfoList.Clear();
        for (int i = 0; i < tabPanels.Count; i++)
        {
            var tabInfo = new TabInfo(i, GetTabTypeFromIndex(i), tabPanels[i], tabButtons[i]);
            tabInfoList.Add(tabInfo);

            // 버튼 이벤트 설정
            int tabIndex = i; // 클로저를 위한 변수 캡처
            tabButtons[i].onClick.AddListener(() => SwitchToTab(tabIndex));
        }

        // 모든 탭 비활성화 후 기본 탭 설정
        DeactivateAllTabs();
        if (tabPanels.Count > 0)
        {
            SwitchToTab(0);
        }
    }

    private TabType GetTabTypeFromIndex(int index)
    {
        if (tabDataList != null && index < tabDataList.Count)
        {
            return tabDataList[index].type;
        }

        // 기본 탭 타입 매핑
        return index switch
        {
            0 => TabType.Play,
            1 => TabType.Worm,
            2 => TabType.Home,
            3 => TabType.Item,
            4 => TabType.Achievement,
            _ => TabType.Home // 기본값
        };
    }

    /// <summary>
    /// 탭 전환
    /// </summary>
    public void SwitchToTab(int tabIndex)
    {
        if (tabIndex < 0 || tabIndex >= tabInfoList.Count) return;

        if (currentTabIndex == tabIndex) return;

        int previousIndex = currentTabIndex;
        TabType previousType = currentTabInfo?.type ?? TabType.Play;

        // 모든 탭 비활성화
        DeactivateAllTabs();

        // 새 탭 활성화
        currentTabIndex = tabIndex;
        currentTabInfo = tabInfoList[tabIndex];
        ActivateTab(currentTabInfo);

        // 이벤트 발생
        OnTabChangedEvent?.Invoke(previousIndex, currentTabIndex, previousType, currentTabInfo.type);
        Debug.Log($"[TabManager] 탭 전환: {previousIndex} → {currentTabIndex} ({previousType} → {currentTabInfo.type})");
    }

    /// <summary>
    /// 탭 열기 (SwitchToTab의 별칭)
    /// </summary>
    public void OpenTab(int tabIndex)
    {
        SwitchToTab(tabIndex);
        
        // 특정 탭에서 카운트 버튼 활성화
        ActivateCountButtonForTab(tabIndex);
    }

    /// <summary>
    /// 탭 타입으로 열기
    /// </summary>
    public void OpenTab(TabType tabType)
    {
        for (int i = 0; i < tabInfoList.Count; i++)
        {
            if (tabInfoList[i].type == tabType)
            {
                SwitchToTab(i);
                
                // 특정 탭에서 카운트 버튼 활성화
                ActivateCountButtonForTab(i);
                return;
            }
        }
        Debug.LogWarning($"[TabManager] 탭 타입 '{tabType}'을 찾을 수 없습니다.");
    }

    /// <summary>
    /// 모든 탭 비활성화
    /// </summary>
    private void DeactivateAllTabs()
    {
        foreach (var tabInfo in tabInfoList)
        {
            DeactivateTab(tabInfo);
        }
        Debug.Log("[TabManager] 모든 탭 비활성화 완료");
    }

    /// <summary>
    /// 탭 활성화
    /// </summary>
    private void ActivateTab(TabInfo tabInfo)
    {
        if (tabInfo == null) return;

        tabInfo.isActive = true;

        // 패널 활성화
        if (tabInfo.panel != null)
        {
            tabInfo.panel.SetActive(true);
        }

        // 버튼 높이 조절 (활성 탭은 더 크게)
        if (tabInfo.button != null)
        {
            var rectTransform = tabInfo.button.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, activeHeight);
            }
        }

        // 탭 데이터의 onTabOpen 콜백 실행
        if (tabDataList != null && tabInfo.index < tabDataList.Count)
        {
            tabDataList[tabInfo.index].onTabOpen?.Invoke();
        }
    }

    /// <summary>
    /// 탭 비활성화
    /// </summary>
    private void DeactivateTab(TabInfo tabInfo)
    {
        if (tabInfo == null) return;

        tabInfo.isActive = false;

        // 패널 비활성화
        if (tabInfo.panel != null)
        {
            tabInfo.panel.SetActive(false);
        }

        // 버튼 높이 조절 (비활성 탭은 기본 크기로)
        if (tabInfo.button != null)
        {
            var rectTransform = tabInfo.button.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, defaultHeight);
            }
        }

        // 탭 데이터의 onTabClose 콜백 실행
        if (tabDataList != null && tabInfo.index < tabDataList.Count)
        {
            tabDataList[tabInfo.index].onTabClose?.Invoke();
        }
    }

    /// <summary>
    /// 특정 탭에 맞는 카운트 버튼 활성화
    /// </summary>
    private void ActivateCountButtonForTab(int tabIndex)
    {
        // 0, 1, 2 탭 간의 전환에서는 실행하지 않음 (이미 AcornCount가 활성화되어 있음)
        if (tabIndex >= 0 && tabIndex <= 2)
        {
            Debug.Log($"[TabManager] 탭 {tabIndex} (Play/Worm/Home) - AcornCount 유지");
            return;
        }
        
        // TopBarManager가 있으면 해당 탭에 맞는 카운트 버튼 활성화
        if (TopBarManager.Instance != null)
        {
            switch (tabIndex)
            {
                case 3: // Item 탭
                    TopBarManager.Instance.ActivateDiamondCountButton();
                    Debug.Log("[TabManager] Item 탭 - DiamondCount 활성화");
                    break;
                case 4: // Achievement 탭
                    TopBarManager.Instance.ActivateMedalCountButton();
                    Debug.Log("[TabManager] Achievement 탭 - MedalCount 활성화");
                    break;
                default:
                    Debug.LogWarning($"[TabManager] 알 수 없는 탭 인덱스: {tabIndex}");
                    break;
            }
        }
    }



    /// <summary>
    /// 탭 잠금/해제 설정
    /// </summary>
    public void SetTabLocked(int tabIndex, bool locked)
    {
        if (tabIndex < 0 || tabIndex >= tabInfoList.Count) return;

        var tabInfo = tabInfoList[tabIndex];
        if (tabInfo.button != null)
        {
            tabInfo.button.interactable = !locked;
        }

        if (tabDataList != null && tabIndex < tabDataList.Count)
        {
            tabDataList[tabIndex].isUnlocked = !locked;
        }
    }

    /// <summary>
    /// 탭 추가
    /// </summary>
    public void AddTab(TabData tabData, GameObject panel, Button button)
    {
        int newIndex = tabPanels.Count;
        
        tabPanels.Add(panel);
        tabButtons.Add(button);
        tabDataList.Add(tabData);

        var tabInfo = new TabInfo(newIndex, tabData.type, panel, button);
        tabInfoList.Add(tabInfo);

        // 버튼 이벤트 설정
        int tabIndex = newIndex;
        button.onClick.AddListener(() => SwitchToTab(tabIndex));

        Debug.Log($"[TabManager] 탭 추가: {tabData.tabName} (인덱스: {newIndex})");
    }

    /// <summary>
    /// 탭 제거
    /// </summary>
    public void RemoveTab(int tabIndex)
    {
        if (tabIndex < 0 || tabIndex >= tabInfoList.Count) return;

        // 현재 탭이 제거되는 탭이면 첫 번째 탭으로 전환
        if (currentTabIndex == tabIndex)
        {
            SwitchToTab(0);
        }

        // 리스트에서 제거
        tabPanels.RemoveAt(tabIndex);
        tabButtons.RemoveAt(tabIndex);
        tabDataList.RemoveAt(tabIndex);
        tabInfoList.RemoveAt(tabIndex);

        // 인덱스 재정렬
        for (int i = tabIndex; i < tabInfoList.Count; i++)
        {
            tabInfoList[i].index = i;
        }

        Debug.Log($"[TabManager] 탭 제거: 인덱스 {tabIndex}");
    }

    /// <summary>
    /// TabManager 정보 반환
    /// </summary>
    public string GetTabInfo()
    {
        var info = new System.Text.StringBuilder();
        info.AppendLine($"[Tab 정보]");
        info.AppendLine($"초기화됨: {isInitialized}");
        info.AppendLine($"현재 탭 인덱스: {currentTabIndex}");
        info.AppendLine($"현재 탭 타입: {CurrentTabType}");
        info.AppendLine($"탭 개수: {TabCount}");

        return info.ToString();
    }

    private void OnDestroy()
    {
        // 이벤트 초기화
        OnTabChangedEvent = null;
    }
}
