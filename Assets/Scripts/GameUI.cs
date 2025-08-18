using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GGumtles.UI;

/// <summary>
/// 게임의 모든 UI 요소를 관리하는 중앙 제어 클래스
/// 버튼, 패널, 팝업 등을 체계적으로 배치하고 관리
/// </summary>
public class GameUI : MonoBehaviour
{
    [Header("UI 레이아웃")]
    [SerializeField] private Transform topBarParent;
    [SerializeField] private Transform mainUIParent;
    [SerializeField] private Transform bottomBarParent;
    [SerializeField] private Transform popupParent;

    [Header("UI 설정")]
    [SerializeField] private bool enableAutoLayout = true;
    [SerializeField] private float buttonSpacing = 10f;
    [SerializeField] private Vector2 buttonSize = new Vector2(120f, 40f);

    [Header("디버그")]
    [SerializeField] private bool enableDebugLogs = false;

    // UI 요소들
    private ReusableButton collectButton;
    private ReusableButton settingsButton;
    private ReusableButton inventoryButton;
    private ReusableButton achievementButton;
    private ReusableButton drawButton;

    // UI 패널들
    private GameObject mainPanel;
    private GameObject inventoryPanel;
    private GameObject achievementPanel;

    // 상태 관리
    private bool isInitialized = false;

    private void Start()
    {
        InitializeGameUI();
    }

    /// <summary>
    /// GameUI 초기화
    /// </summary>
    private void InitializeGameUI()
    {
        try
        {
            // 자동 컴포넌트 찾기
            AutoFindParents();
            
            // UI 요소들 생성
            CreateTopBarUI();
            CreateMainUI();
            CreateBottomBarUI();
            
            // 이벤트 연결
            ConnectEvents();
            
            isInitialized = true;
            LogDebug("[GameUI] 초기화 완료");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[GameUI] 초기화 중 오류: {ex.Message}");
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
        
        if (bottomBarParent == null)
            bottomBarParent = transform.Find("BottomBar") ?? transform;
        
        if (popupParent == null)
            popupParent = transform.Find("PopupLayer") ?? transform;
    }

    /// <summary>
    /// 상단 바 UI 생성
    /// </summary>
    private void CreateTopBarUI()
    {
        // 설정 버튼
        GameObject settingsObj = UIPrefabManager.Instance.CreateUIElement("Button", topBarParent);
        settingsButton = settingsObj.GetComponent<ReusableButton>();
        settingsButton.SetText("설정");
        settingsButton.SetButtonStyle(ReusableButton.ButtonStyle.Secondary);
        settingsButton.SetIcon(GetSettingsIcon());

        // 업적 버튼
        GameObject achievementObj = UIPrefabManager.Instance.CreateUIElement("Button", topBarParent);
        achievementButton = achievementObj.GetComponent<ReusableButton>();
        achievementButton.SetText("업적");
        achievementButton.SetButtonStyle(ReusableButton.ButtonStyle.Secondary);
        achievementButton.SetIcon(GetAchievementIcon());

        // 레이아웃 조정
        if (enableAutoLayout)
        {
            ArrangeButtonsHorizontally(topBarParent, buttonSpacing);
        }
    }

    /// <summary>
    /// 메인 UI 생성
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
    /// 하단 바 UI 생성
    /// </summary>
    private void CreateBottomBarUI()
    {
        // 인벤토리 버튼
        GameObject inventoryObj = UIPrefabManager.Instance.CreateUIElement("Button", bottomBarParent);
        inventoryButton = inventoryObj.GetComponent<ReusableButton>();
        inventoryButton.SetText("인벤토리");
        inventoryButton.SetButtonStyle(ReusableButton.ButtonStyle.Normal);
        inventoryButton.SetIcon(GetInventoryIcon());

        // 레이아웃 조정
        if (enableAutoLayout)
        {
            ArrangeButtonsHorizontally(bottomBarParent, buttonSpacing);
        }
    }

    /// <summary>
    /// 이벤트 연결
    /// </summary>
    private void ConnectEvents()
    {
        // 수집 버튼
        collectButton.OnButtonClickedEvent += OnCollectButtonClicked;
        
        // 설정 버튼
        settingsButton.OnButtonClickedEvent += OnSettingsButtonClicked;
        
        // 업적 버튼
        achievementButton.OnButtonClickedEvent += OnAchievementButtonClicked;
        
        // 인벤토리 버튼
        inventoryButton.OnButtonClickedEvent += OnInventoryButtonClicked;
        
        // 뽑기 버튼
        drawButton.OnButtonClickedEvent += OnDrawButtonClicked;
    }

    /// <summary>
    /// 수집 버튼 클릭 이벤트
    /// </summary>
    private void OnCollectButtonClicked(ReusableButton button)
    {
        LogDebug("[GameUI] 수집 버튼 클릭됨");
        
        // 나무 흔들기
        FindFirstObjectByType<TreeController>()?.ShakeTree();
        
        // 버튼 비활성화 (쿨다운)
        button.SetInteractable(false);
        StartCoroutine(EnableButtonAfterDelay(button, 2f));
    }

    /// <summary>
    /// 설정 버튼 클릭 이벤트
    /// </summary>
    private void OnSettingsButtonClicked(ReusableButton button)
    {
        LogDebug("[GameUI] 설정 버튼 클릭됨");
        
        // 설정 팝업 열기 (임시로 Custom 사용)
        PopupManager.Instance?.OpenPopup(PopupManager.PopupType.Custom, PopupManager.PopupPriority.Normal);
    }

    /// <summary>
    /// 업적 버튼 클릭 이벤트
    /// </summary>
    private void OnAchievementButtonClicked(ReusableButton button)
    {
        LogDebug("[GameUI] 업적 버튼 클릭됨");
        
        // 업적 팝업 열기
        PopupManager.Instance?.OpenPopup(PopupManager.PopupType.Achievement, PopupManager.PopupPriority.Normal);
    }

    /// <summary>
    /// 인벤토리 버튼 클릭 이벤트
    /// </summary>
    private void OnInventoryButtonClicked(ReusableButton button)
    {
        LogDebug("[GameUI] 인벤토리 버튼 클릭됨");
        
        // 인벤토리 팝업 열기
        PopupManager.Instance?.OpenPopup(PopupManager.PopupType.ItemSelection, PopupManager.PopupPriority.Normal);
    }

    /// <summary>
    /// 뽑기 버튼 클릭 이벤트
    /// </summary>
    private void OnDrawButtonClicked(ReusableButton button)
    {
        LogDebug("[GameUI] 뽑기 버튼 클릭됨");
        
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
    private Sprite GetInventoryIcon() => Resources.Load<Sprite>("Images/icons8-itemBack-96");
    private Sprite GetDrawIcon() => Resources.Load<Sprite>("Images/ShuffleImage");

    /// <summary>
    /// UI 새로고침
    /// </summary>
    public void RefreshUI()
    {
        // 버튼 상태 업데이트
        UpdateButtonStates();
        
        LogDebug("[GameUI] UI 새로고침 완료");
    }

    /// <summary>
    /// 버튼 상태 업데이트
    /// </summary>
    private void UpdateButtonStates()
    {
        // 뽑기 버튼 활성화/비활성화 (다이아몬드 개수에 따라)
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
            case "settings":
                settingsButton?.SetInteractable(interactable);
                break;
            case "achievement":
                achievementButton?.SetInteractable(interactable);
                break;
            case "inventory":
                inventoryButton?.SetInteractable(interactable);
                break;
            case "draw":
                drawButton?.SetInteractable(interactable);
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
        if (bottomBarParent != null) bottomBarParent.gameObject.SetActive(false);
    }

    /// <summary>
    /// 모든 UI 요소 보이기
    /// </summary>
    public void ShowAllUI()
    {
        if (topBarParent != null) topBarParent.gameObject.SetActive(true);
        if (mainUIParent != null) mainUIParent.gameObject.SetActive(true);
        if (bottomBarParent != null) bottomBarParent.gameObject.SetActive(true);
    }

    /// <summary>
    /// GameUI 정보 반환
    /// </summary>
    public string GetGameUIInfo()
    {
        var info = new System.Text.StringBuilder();
        info.AppendLine($"[GameUI 정보]");
        info.AppendLine($"초기화됨: {isInitialized}");
        info.AppendLine($"자동 레이아웃: {(enableAutoLayout ? "활성화" : "비활성화")}");
        info.AppendLine($"버튼 간격: {buttonSpacing}");
        info.AppendLine($"버튼 크기: {buttonSize}");
        info.AppendLine($"수집 버튼: {(collectButton != null ? "생성됨" : "없음")}");
        info.AppendLine($"설정 버튼: {(settingsButton != null ? "생성됨" : "없음")}");
        info.AppendLine($"업적 버튼: {(achievementButton != null ? "생성됨" : "없음")}");
        info.AppendLine($"인벤토리 버튼: {(inventoryButton != null ? "생성됨" : "없음")}");
        info.AppendLine($"뽑기 버튼: {(drawButton != null ? "생성됨" : "없음")}");

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
        if (settingsButton != null)
            settingsButton.OnButtonClickedEvent -= OnSettingsButtonClicked;
        if (achievementButton != null)
            achievementButton.OnButtonClickedEvent -= OnAchievementButtonClicked;
        if (inventoryButton != null)
            inventoryButton.OnButtonClickedEvent -= OnInventoryButtonClicked;
        if (drawButton != null)
            drawButton.OnButtonClickedEvent -= OnDrawButtonClicked;
    }
}
