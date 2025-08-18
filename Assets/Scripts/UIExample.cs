using UnityEngine;
using UnityEngine.UI;
using GGumtles.UI;

/// <summary>
/// UI 컴포넌트 사용 예시
/// ScrollView와 LayoutGroup의 실제 사용법을 보여줌
/// </summary>
public class UIExample : MonoBehaviour
{
    [Header("UI 예시")]
    [SerializeField] private Transform uiParent;

    // UI 컴포넌트들
    private ReusableScrollView achievementScrollView;
    private ReusableLayoutGroup itemLayoutGroup;
    private ReusableButton addButton;
    private ReusableButton clearButton;

    private void Start()
    {
        CreateUIExample();
    }

    /// <summary>
    /// UI 예시 생성
    /// </summary>
    private void CreateUIExample()
    {
        // 1. 스크롤 뷰 생성 (업적 목록용)
        CreateAchievementScrollView();

        // 2. 레이아웃 그룹 생성 (아이템 그리드용)
        CreateItemLayoutGroup();

        // 3. 컨트롤 버튼들 생성
        CreateControlButtons();
    }

    /// <summary>
    /// 업적 스크롤 뷰 생성
    /// </summary>
    private void CreateAchievementScrollView()
    {
        // 스크롤 뷰 생성
        GameObject scrollViewObj = UIPrefabManager.Instance.CreateUIElement("ScrollView", uiParent);
        achievementScrollView = scrollViewObj.GetComponent<ReusableScrollView>();

        // 스크롤 뷰 설정
        achievementScrollView.SetVerticalScrollEnabled(true);
        achievementScrollView.SetHorizontalScrollEnabled(false);
        achievementScrollView.SetSpacing(10f);
        achievementScrollView.SetPadding(new RectOffset(15, 15, 15, 15));
        achievementScrollView.SetScrollbarVisibility(true, false);

        // 이벤트 연결
        achievementScrollView.OnContentItemAddedEvent += OnAchievementAdded;
        achievementScrollView.OnScrollValueChangedEvent += OnAchievementScrollChanged;

        // 샘플 업적 아이템들 추가
        AddSampleAchievements();
    }

    /// <summary>
    /// 아이템 레이아웃 그룹 생성
    /// </summary>
    private void CreateItemLayoutGroup()
    {
        // 레이아웃 그룹 생성
        GameObject layoutGroupObj = UIPrefabManager.Instance.CreateUIElement("LayoutGroup", uiParent);
        itemLayoutGroup = layoutGroupObj.GetComponent<ReusableLayoutGroup>();

        // 그리드 레이아웃으로 설정
        itemLayoutGroup.SetLayoutType(ReusableLayoutGroup.LayoutType.Grid);
        itemLayoutGroup.SetCellSize(new Vector2(80f, 80f));
        itemLayoutGroup.SetGridSpacing(new Vector2(5f, 5f));
        itemLayoutGroup.SetGridConstraint(GridLayoutGroup.Constraint.FixedColumnCount, 4);
        itemLayoutGroup.SetPadding(new RectOffset(10, 10, 10, 10));

        // 이벤트 연결
        itemLayoutGroup.OnLayoutItemAddedEvent += OnItemAdded;
        itemLayoutGroup.OnLayoutChangedEvent += OnLayoutChanged;

        // 샘플 아이템들 추가
        AddSampleItems();
    }

    /// <summary>
    /// 컨트롤 버튼들 생성
    /// </summary>
    private void CreateControlButtons()
    {
        // 추가 버튼
        GameObject addButtonObj = UIPrefabManager.Instance.CreateUIElement("Button", uiParent);
        addButton = addButtonObj.GetComponent<ReusableButton>();
        addButton.SetText("아이템 추가");
        addButton.SetButtonStyle(ReusableButton.ButtonStyle.Primary);
        addButton.OnButtonClickedEvent += OnAddButtonClicked;

        // 클리어 버튼
        GameObject clearButtonObj = UIPrefabManager.Instance.CreateUIElement("Button", uiParent);
        clearButton = clearButtonObj.GetComponent<ReusableButton>();
        clearButton.SetText("모두 지우기");
        clearButton.SetButtonStyle(ReusableButton.ButtonStyle.Danger);
        clearButton.OnButtonClickedEvent += OnClearButtonClicked;
    }

    /// <summary>
    /// 샘플 업적 추가
    /// </summary>
    private void AddSampleAchievements()
    {
        // 업적 아이템 프리팹 (실제로는 Resources에서 로드)
        GameObject achievementItemPrefab = Resources.Load<GameObject>("Prefabs/UI/AchievementItem");
        
        if (achievementItemPrefab != null)
        {
            for (int i = 1; i <= 10; i++)
            {
                GameObject achievementItem = achievementScrollView.AddItem(achievementItemPrefab);
                
                // 업적 아이템 설정 (실제 구현에서는 AchievementItemUI 컴포넌트 사용)
                var achievementUI = achievementItem.GetComponent<AchievementButtonUI>();
                if (achievementUI != null)
                {
                                    // 업적 데이터와 아이콘 가져오기 (임시 구현)
                var achievementData = AchievementManager.Instance?.GetAchievementData($"Ach_{i:D2}");
                var iconSprite = SpriteManager.Instance?.GetAchievementSprite($"Ach_{i:D2}");
                var status = AchievementManager.Instance?.GetStatusById($"Ach_{i:D2}") ?? new AchievementStatus($"Ach_{i:D2}");
                    
                    if (achievementData != null)
                    {
                        achievementUI.Set(achievementData, iconSprite, i - 1, status);
                    }
                }
            }
        }
    }

    /// <summary>
    /// 샘플 아이템 추가
    /// </summary>
    private void AddSampleItems()
    {
        // 아이템 프리팹 (실제로는 Resources에서 로드)
        GameObject itemPrefab = Resources.Load<GameObject>("Prefabs/UI/ItemSlot");
        
        if (itemPrefab != null)
        {
            for (int i = 0; i < 8; i++)
            {
                GameObject itemSlot = itemLayoutGroup.AddItem(itemPrefab);
                
                // 아이템 슬롯 설정 (실제 구현에서는 ItemSlotUI 컴포넌트 사용)
                var itemUI = itemSlot.GetComponent<ItemSlotUI>();
                if (itemUI != null)
                {
                    // 랜덤 아이템 데이터 설정
                    var randomItem = GetRandomItemData();
                    itemUI.Initialize(randomItem, randomItem.type);
                }
            }
        }
    }

    /// <summary>
    /// 랜덤 아이템 데이터 생성
    /// </summary>
    private ItemData GetRandomItemData()
    {
        // 실제로는 ItemManager에서 가져오거나 랜덤 생성
        var itemData = ScriptableObject.CreateInstance<ItemData>();
        itemData.itemId = Random.Range(100, 200).ToString();
        itemData.itemName = $"아이템 {itemData.itemId}";
        itemData.type = ItemData.ItemType.Hat;
        itemData.rarity = (ItemData.ItemRarity)Random.Range(0, 4);
        return itemData;
    }

    /// <summary>
    /// 추가 버튼 클릭 이벤트
    /// </summary>
    private void OnAddButtonClicked(ReusableButton button)
    {
        Debug.Log("[UIExample] 아이템 추가 버튼 클릭됨");
        
        // 새로운 아이템 추가
        GameObject itemPrefab = Resources.Load<GameObject>("Prefabs/UI/ItemSlot");
        if (itemPrefab != null)
        {
            GameObject newItem = itemLayoutGroup.AddItem(itemPrefab);
            
            // 새 아이템 설정
            var itemUI = newItem.GetComponent<ItemSlotUI>();
                            if (itemUI != null)
                {
                    var randomItem = GetRandomItemData();
                    itemUI.Initialize(randomItem, randomItem.type);
                }
        }
    }

    /// <summary>
    /// 클리어 버튼 클릭 이벤트
    /// </summary>
    private void OnClearButtonClicked(ReusableButton button)
    {
        Debug.Log("[UIExample] 모두 지우기 버튼 클릭됨");
        
        // 모든 아이템 제거
        itemLayoutGroup.ClearAllItems();
        achievementScrollView.ClearAllItems();
    }

    /// <summary>
    /// 업적 추가 이벤트
    /// </summary>
    private void OnAchievementAdded(GameObject achievement)
    {
        Debug.Log($"[UIExample] 업적 추가됨: {achievement.name}");
    }

    /// <summary>
    /// 업적 스크롤 변경 이벤트
    /// </summary>
    private void OnAchievementScrollChanged(Vector2 scrollValue)
    {
        Debug.Log($"[UIExample] 업적 스크롤 위치: {scrollValue}");
    }

    /// <summary>
    /// 아이템 추가 이벤트
    /// </summary>
    private void OnItemAdded(GameObject item)
    {
        Debug.Log($"[UIExample] 아이템 추가됨: {item.name}");
    }

    /// <summary>
    /// 레이아웃 변경 이벤트
    /// </summary>
    private void OnLayoutChanged()
    {
        Debug.Log("[UIExample] 레이아웃 변경됨");
    }

    /// <summary>
    /// 스크롤 뷰 제어 메서드들
    /// </summary>
    public void ScrollToTop()
    {
        achievementScrollView?.ScrollToTop();
    }

    public void ScrollToBottom()
    {
        achievementScrollView?.ScrollToBottom();
    }

    public void ScrollToItem(int index)
    {
        achievementScrollView?.ScrollToItem(index);
    }

    /// <summary>
    /// 레이아웃 그룹 제어 메서드들
    /// </summary>
    public void SwitchToVerticalLayout()
    {
        itemLayoutGroup?.SetLayoutType(ReusableLayoutGroup.LayoutType.Vertical);
    }

    public void SwitchToHorizontalLayout()
    {
        itemLayoutGroup?.SetLayoutType(ReusableLayoutGroup.LayoutType.Horizontal);
    }

    public void SwitchToGridLayout()
    {
        itemLayoutGroup?.SetLayoutType(ReusableLayoutGroup.LayoutType.Grid);
    }

    public void RefreshLayout()
    {
        itemLayoutGroup?.RefreshLayout();
    }
}
