using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GGumtles.UI;

/// <summary>
/// 게임의 메인 UI를 관리하는 클래스
/// TopBar, MainUI, TabBar를 체계적으로 배치하고 관리
/// </summary>
public class MainUI : MonoBehaviour
{
    [Header("UI 레이아웃")]
    [SerializeField] private Transform topBarParent;
    [SerializeField] private Transform mainUIParent;
    [SerializeField] private Transform tabBarParent;
    [SerializeField] private Transform popupParent;

    [Header("UI 설정")]
    [SerializeField] private bool enableAutoLayout = true;
    [SerializeField] private float buttonSpacing = 10f;
    [SerializeField] private Vector2 buttonSize = new Vector2(120f, 40f);

    [Header("디버그")]
    [SerializeField] private bool enableDebugLogs = false;

    // UI 요소들
    private ReusableButton collectButton;
    private ReusableButton drawButton;
    
    // TopBar UI 요소들
    private TMP_Text gameTimeText;
    private TMP_Text currentWormNameText;
    
    // TabBar UI 요소들
    private ReusableButton wormTabButton;
    private ReusableButton itemTabButton;
    private ReusableButton achievementTabButton;
    private ReusableButton settingsTabButton;



    // 상태 관리
    private bool isInitialized = false;

    private void Start()
    {
        InitializeMainUI();
    }

    /// <summary>
    /// MainUI 초기화
    /// </summary>
    private void InitializeMainUI()
    {
        try
        {
            // 자동 컴포넌트 찾기
            AutoFindParents();
            
            // UI 요소들 생성
            CreateTopBarUI();
            CreateMainUI();
            CreateTabBarUI();
            
            // 이벤트 연결
            ConnectEvents();
            
            isInitialized = true;
            LogDebug("[MainUI] 초기화 완료");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[MainUI] 초기화 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 부모 Transform 자동 찾기
    /// </summary>
    private void AutoFindParents()
    {
        if (topBarParent == null)
            topBarParent = transform.Find("TopBar") ?? transform;
        
        if (mainUIParent == null)
            mainUIParent = transform.Find("MainUI") ?? transform;
        
        if (tabBarParent == null)
            tabBarParent = transform.Find("TabBar") ?? transform;
        
        if (popupParent == null)
            popupParent = transform.Find("PopupLayer") ?? transform;
    }

    /// <summary>
    /// 상단 바 UI 생성 (게임 시간 + 웜 이름)
    /// </summary>
    private void CreateTopBarUI()
    {
        // 게임 시간 텍스트
        GameObject timeTextObj = UIPrefabManager.Instance.CreateUIElement("Text", topBarParent);
        gameTimeText = timeTextObj.GetComponent<TMP_Text>();
        gameTimeText.text = "00:00";
        gameTimeText.fontSize = 16f;
        gameTimeText.color = Color.white;

        // 현재 웜 이름 텍스트
        GameObject wormNameObj = UIPrefabManager.Instance.CreateUIElement("Text", topBarParent);
        currentWormNameText = wormNameObj.GetComponent<TMP_Text>();
        currentWormNameText.text = "현재 웜: 없음";
        currentWormNameText.fontSize = 16f;
        currentWormNameText.color = Color.white;

        // 레이아웃 조정
        if (enableAutoLayout)
        {
            ArrangeTextsHorizontally(topBarParent, buttonSpacing);
        }
    }

    /// <summary>
    /// 메인 UI 버튼들 생성
    /// </summary>
    private void CreateMainUI()
    {
        // 수집 버튼 (나무 흔들기)
        GameObject collectObj = UIPrefabManager.Instance.CreateUIElement("Button", mainUIParent);
        collectButton = collectObj.GetComponent<ReusableButton>();
        collectButton.SetText("나무 흔들기");
        collectButton.SetButtonStyle(ReusableButton.ButtonStyle.Primary);
        collectButton.SetIcon(GetTreeIcon());

        // 뽑기 버튼
        GameObject drawObj = UIPrefabManager.Instance.CreateUIElement("Button", mainUIParent);
        drawButton = drawObj.GetComponent<ReusableButton>();
        drawButton.SetText("아이템 뽑기");
        drawButton.SetButtonStyle(ReusableButton.ButtonStyle.Success);
        drawButton.SetIcon(GetDrawIcon());

        // 레이아웃 조정
        if (enableAutoLayout)
        {
            ArrangeButtonsVertically(mainUIParent, buttonSpacing);
        }
    }

    /// <summary>
    /// 탭 바 UI 생성
    /// </summary>
    private void CreateTabBarUI()
    {
        // 웜 탭 버튼
        GameObject wormTabObj = UIPrefabManager.Instance.CreateUIElement("Button", tabBarParent);
        wormTabButton = wormTabObj.GetComponent<ReusableButton>();
        wormTabButton.SetText("웜");
        wormTabButton.SetButtonStyle(ReusableButton.ButtonStyle.Normal);
        wormTabButton.SetIcon(GetWormIcon());

        // 아이템 탭 버튼
        GameObject itemTabObj = UIPrefabManager.Instance.CreateUIElement("Button", tabBarParent);
        itemTabButton = itemTabObj.GetComponent<ReusableButton>();
        itemTabButton.SetText("아이템");
        itemTabButton.SetButtonStyle(ReusableButton.ButtonStyle.Normal);
        itemTabButton.SetIcon(GetItemIcon());

        // 업적 탭 버튼
        GameObject achievementTabObj = UIPrefabManager.Instance.CreateUIElement("Button", tabBarParent);
        achievementTabButton = achievementTabObj.GetComponent<ReusableButton>();
        achievementTabButton.SetText("업적");
        achievementTabButton.SetButtonStyle(ReusableButton.ButtonStyle.Normal);
        achievementTabButton.SetIcon(GetAchievementIcon());

        // 설정 탭 버튼
        GameObject settingsTabObj = UIPrefabManager.Instance.CreateUIElement("Button", tabBarParent);
        settingsTabButton = settingsTabObj.GetComponent<ReusableButton>();
        settingsTabButton.SetText("설정");
        settingsTabButton.SetButtonStyle(ReusableButton.ButtonStyle.Normal);
        settingsTabButton.SetIcon(GetSettingsIcon());

        // 레이아웃 조정
        if (enableAutoLayout)
        {
            ArrangeButtonsHorizontally(tabBarParent, buttonSpacing);
        }
    }

    /// <summary>
    /// 이벤트 연결
    /// </summary>
    private void ConnectEvents()
    {
        // 수집 버튼
        collectButton.OnButtonClickedEvent += OnCollectButtonClicked;
        
        // 뽑기 버튼
        drawButton.OnButtonClickedEvent += OnDrawButtonClicked;
        
        // 탭 버튼들
        wormTabButton.OnButtonClickedEvent += OnWormTabClicked;
        itemTabButton.OnButtonClickedEvent += OnItemTabClicked;
        achievementTabButton.OnButtonClickedEvent += OnAchievementTabClicked;
        settingsTabButton.OnButtonClickedEvent += OnSettingsTabClicked;
    }

    /// <summary>
    /// 수집 버튼 클릭 이벤트
    /// </summary>
    private void OnCollectButtonClicked(ReusableButton button)
    {
        LogDebug("[MainUI] 수집 버튼 클릭됨");
        
        // 나무 흔들기
        FindFirstObjectByType<TreeController>()?.ShakeTree();
        
        // 버튼 비활성화 (쿨다운)
        button.SetInteractable(false);
        StartCoroutine(EnableButtonAfterDelay(button, 2f));
    }

    /// <summary>
    /// 웜 탭 클릭 이벤트
    /// </summary>
    private void OnWormTabClicked(ReusableButton button)
    {
        LogDebug("[MainUI] 웜 탭 클릭됨");
        SwitchToWormTab();
    }

    /// <summary>
    /// 아이템 탭 클릭 이벤트
    /// </summary>
    private void OnItemTabClicked(ReusableButton button)
    {
        LogDebug("[MainUI] 아이템 탭 클릭됨");
        SwitchToItemTab();
    }

    /// <summary>
    /// 업적 탭 클릭 이벤트
    /// </summary>
    private void OnAchievementTabClicked(ReusableButton button)
    {
        LogDebug("[MainUI] 업적 탭 클릭됨");
        SwitchToAchievementTab();
    }

    /// <summary>
    /// 설정 탭 클릭 이벤트
    /// </summary>
    private void OnSettingsTabClicked(ReusableButton button)
    {
        LogDebug("[MainUI] 설정 탭 클릭됨");
        SwitchToSettingsTab();
    }

    /// <summary>
    /// 웜 탭으로 전환
    /// </summary>
    private void SwitchToWormTab()
    {
        SetAllTabsInactive();
        wormTabButton?.SetButtonStyle(ReusableButton.ButtonStyle.Primary);
        LogDebug("[MainUI] 웜 탭으로 전환됨");
    }

    /// <summary>
    /// 아이템 탭으로 전환
    /// </summary>
    private void SwitchToItemTab()
    {
        SetAllTabsInactive();
        itemTabButton?.SetButtonStyle(ReusableButton.ButtonStyle.Primary);
        LogDebug("[MainUI] 아이템 탭으로 전환됨");
    }

    /// <summary>
    /// 업적 탭으로 전환
    /// </summary>
    private void SwitchToAchievementTab()
    {
        SetAllTabsInactive();
        achievementTabButton?.SetButtonStyle(ReusableButton.ButtonStyle.Primary);
        LogDebug("[MainUI] 업적 탭으로 전환됨");
    }

    /// <summary>
    /// 설정 탭으로 전환
    /// </summary>
    private void SwitchToSettingsTab()
    {
        SetAllTabsInactive();
        settingsTabButton?.SetButtonStyle(ReusableButton.ButtonStyle.Primary);
        LogDebug("[MainUI] 설정 탭으로 전환됨");
    }

    /// <summary>
    /// 모든 탭 비활성화
    /// </summary>
    private void SetAllTabsInactive()
    {
        wormTabButton?.SetButtonStyle(ReusableButton.ButtonStyle.Normal);
        itemTabButton?.SetButtonStyle(ReusableButton.ButtonStyle.Normal);
        achievementTabButton?.SetButtonStyle(ReusableButton.ButtonStyle.Normal);
        settingsTabButton?.SetButtonStyle(ReusableButton.ButtonStyle.Normal);
    }

    /// <summary>
    /// 뽑기 버튼 클릭 이벤트
    /// </summary>
    private void OnDrawButtonClicked(ReusableButton button)
    {
        LogDebug("[MainUI] 뽑기 버튼 클릭됨");
        
        // 뽑기 확인 팝업 열기
        PopupManager.Instance?.OpenPopup(PopupManager.PopupType.DrawConfirm, PopupManager.PopupPriority.Normal);
    }

    /// <summary>
    /// 버튼을 일정 시간 후 활성화
    /// </summary>
    private System.Collections.IEnumerator EnableButtonAfterDelay(ReusableButton button, float delay)
    {
        yield return new WaitForSeconds(delay);
        button.SetInteractable(true);
    }

    /// <summary>
    /// 텍스트들을 수평으로 배치
    /// </summary>
    private void ArrangeTextsHorizontally(Transform parent, float spacing)
    {
        TMP_Text[] texts = parent.GetComponentsInChildren<TMP_Text>();
        float currentX = 0f;

        foreach (var text in texts)
        {
            RectTransform rectTransform = text.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.anchoredPosition = new Vector2(currentX, 0f);
                currentX += rectTransform.sizeDelta.x + spacing;
            }
        }
    }

    /// <summary>
    /// 버튼들을 수평으로 배치
    /// </summary>
    private void ArrangeButtonsHorizontally(Transform parent, float spacing)
    {
        ReusableButton[] buttons = parent.GetComponentsInChildren<ReusableButton>();
        float currentX = 0f;

        foreach (var button in buttons)
        {
            RectTransform rectTransform = button.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.anchoredPosition = new Vector2(currentX, 0f);
                rectTransform.sizeDelta = buttonSize;
                currentX += buttonSize.x + spacing;
            }
        }
    }

    /// <summary>
    /// 버튼들을 수직으로 배치
    /// </summary>
    private void ArrangeButtonsVertically(Transform parent, float spacing)
    {
        ReusableButton[] buttons = parent.GetComponentsInChildren<ReusableButton>();
        float currentY = 0f;

        foreach (var button in buttons)
        {
            RectTransform rectTransform = button.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.anchoredPosition = new Vector2(0f, currentY);
                rectTransform.sizeDelta = buttonSize;
                currentY -= buttonSize.y + spacing;
            }
        }
    }

    /// <summary>
    /// 아이콘 가져오기 (임시 구현)
    /// </summary>
    private Sprite GetTreeIcon() => Resources.Load<Sprite>("Images/tree");
    private Sprite GetSettingsIcon() => Resources.Load<Sprite>("Images/icons8-check-icon-48");
    private Sprite GetAchievementIcon() => Resources.Load<Sprite>("Images/achieveMedal");
    private Sprite GetWormIcon() => Resources.Load<Sprite>("Images/Worms/1st");
    private Sprite GetItemIcon() => Resources.Load<Sprite>("Images/icons8-itemBack-96");
    private Sprite GetDrawIcon() => Resources.Load<Sprite>("Images/ShuffleImage");

    /// <summary>
    /// UI 새로고침
    /// </summary>
    public void RefreshUI()
    {
        UpdateButtonStates();
        UpdateTopBarInfo();
        LogDebug("[MainUI] UI 새로고침 완료");
    }

    /// <summary>
    /// TopBar 정보 업데이트
    /// </summary>
    private void UpdateTopBarInfo()
    {
        if (gameTimeText != null)
        {
            gameTimeText.text = "00:00";
        }

        if (currentWormNameText != null)
        {
            var currentWorm = WormManager.Instance?.GetCurrentWorm();
            currentWormNameText.text = currentWorm != null ? currentWorm.name : "없음";
        }
    }

    /// <summary>
    /// 버튼 상태 업데이트
    /// </summary>
    private void UpdateButtonStates()
    {
        if (drawButton != null)
        {
            bool canDraw = GameManager.Instance?.diamondCount >= 10;
            drawButton.SetInteractable(canDraw);
        }
    }

    /// <summary>
    /// 특정 버튼 활성화/비활성화
    /// </summary>
    public void SetButtonInteractable(string buttonName, bool interactable)
    {
        switch (buttonName.ToLower())
        {
            case "collect":
                collectButton?.SetInteractable(interactable);
                break;
            case "draw":
                drawButton?.SetInteractable(interactable);
                break;
            case "wormtab":
                wormTabButton?.SetInteractable(interactable);
                break;
            case "itemtab":
                itemTabButton?.SetInteractable(interactable);
                break;
            case "achievementtab":
                achievementTabButton?.SetInteractable(interactable);
                break;
            case "settingstab":
                settingsTabButton?.SetInteractable(interactable);
                break;
        }
    }

    /// <summary>
    /// 모든 UI 요소 숨기기
    /// </summary>
    public void HideAllUI()
    {
        if (topBarParent != null) topBarParent.gameObject.SetActive(false);
        if (mainUIParent != null) mainUIParent.gameObject.SetActive(false);
        if (tabBarParent != null) tabBarParent.gameObject.SetActive(false);
    }

    /// <summary>
    /// 모든 UI 요소 보이기
    /// </summary>
    public void ShowAllUI()
    {
        if (topBarParent != null) topBarParent.gameObject.SetActive(true);
        if (mainUIParent != null) mainUIParent.gameObject.SetActive(true);
        if (tabBarParent != null) tabBarParent.gameObject.SetActive(true);
    }

    /// <summary>
    /// MainUI 정보 반환
    /// </summary>
    public string GetMainUIInfo()
    {
        var info = new System.Text.StringBuilder();
        info.AppendLine($"[MainUI 정보]");
        info.AppendLine($"초기화됨: {isInitialized}");
        info.AppendLine($"자동 레이아웃: {(enableAutoLayout ? "활성화" : "비활성화")}");
        info.AppendLine($"버튼 간격: {buttonSpacing}");
        info.AppendLine($"버튼 크기: {buttonSize}");
        info.AppendLine($"수집 버튼: {(collectButton != null ? "생성됨" : "없음")}");
        info.AppendLine($"뽑기 버튼: {(drawButton != null ? "생성됨" : "없음")}");
        info.AppendLine($"웜 탭 버튼: {(wormTabButton != null ? "생성됨" : "없음")}");
        info.AppendLine($"아이템 탭 버튼: {(itemTabButton != null ? "생성됨" : "없음")}");
        info.AppendLine($"업적 탭 버튼: {(achievementTabButton != null ? "생성됨" : "없음")}");
        info.AppendLine($"설정 탭 버튼: {(settingsTabButton != null ? "생성됨" : "없음")}");

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
        // 이벤트 구독 해제
        if (collectButton != null)
            collectButton.OnButtonClickedEvent -= OnCollectButtonClicked;
        if (drawButton != null)
            drawButton.OnButtonClickedEvent -= OnDrawButtonClicked;
        if (wormTabButton != null)
            wormTabButton.OnButtonClickedEvent -= OnWormTabClicked;
        if (itemTabButton != null)
            itemTabButton.OnButtonClickedEvent -= OnItemTabClicked;
        if (achievementTabButton != null)
            achievementTabButton.OnButtonClickedEvent -= OnAchievementTabClicked;
        if (settingsTabButton != null)
            settingsTabButton.OnButtonClickedEvent -= OnSettingsTabClicked;
    }
}
