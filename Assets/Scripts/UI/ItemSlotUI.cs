using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GGumtles.Data;
using System.Collections.Generic;
using System.Linq;
using GGumtles.Managers;

namespace GGumtles.UI
{
    public class ItemSlotUI : MonoBehaviour
{
    [Header("Content")]
    [SerializeField] private Transform contentTransform;  // 아이템 버튼들이 생성될 부모 Transform
    
    [Header("ItemButton 프리팹")]
    [SerializeField] private GameObject itemButtonPrefab;  // ItemButton 프리팹
    
    [Header("현재 착용 아이템 설명")]
    [SerializeField] private GameObject itemDescObject;   // 현재 착용 아이템 설명 오브젝트
    [SerializeField] private TMP_Text itemDescNameText;    // 착용 아이템 이름 텍스트
    [SerializeField] private TMP_Text itemDescText;        // 착용 아이템 설명 텍스트
    
    [Header("디버그")]
    [SerializeField] private bool enableDebugLogs = false;

    // 데이터 및 상태
    private ItemData.ItemType currentItemType;            // 현재 표시 중인 아이템 타입
    private List<ItemButton> itemButtons;                 // 생성된 아이템 버튼들
    private ItemButton selectedItemButton;               // 현재 선택된 아이템 버튼

    // 이벤트 정의
    public delegate void OnItemSelected(ItemData itemData, ItemData.ItemType itemType);
    public event OnItemSelected OnItemSelectedEvent;

    private void Awake()
    {
        InitializeComponents();
    }

    private void InitializeComponents()
    {
        try
        {
            // Content Transform 자동 찾기
            if (contentTransform == null)
                contentTransform = transform.Find("Content");

            if (contentTransform == null)
            {
                Debug.LogError("[ItemSlotUI] Content Transform을 찾을 수 없습니다.");
                return;
            }

            // ItemDescObject 자동 찾기
            if (itemDescObject == null)
                itemDescObject = transform.Find("ItemDescObject")?.gameObject;

            // ItemDesc 텍스트 자동 찾기
            if (itemDescNameText == null && itemDescObject != null)
                itemDescNameText = itemDescObject.transform.Find("ItemNameText")?.GetComponent<TMP_Text>();

            if (itemDescText == null && itemDescObject != null)
                itemDescText = itemDescObject.transform.Find("ItemDescText")?.GetComponent<TMP_Text>();

            // 리스트 초기화
            itemButtons = new List<ItemButton>();

            LogDebug("[ItemSlotUI] 컴포넌트 초기화 완료");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[ItemSlotUI] 컴포넌트 초기화 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 아이템 슬롯 초기화 (특정 타입의 아이템들 표시)
    /// </summary>
    public void Initialize(ItemData.ItemType itemType)
    {
        try
        {
            Debug.Log($"[ItemSlotUI] Initialize 호출됨 - ItemType: {itemType}");
            currentItemType = itemType;
            
            // 기존 아이템 버튼들 제거
            ClearItemButtons();
            
            // 해당 타입의 아이템들 가져오기
            var items = GetItemsByType(itemType);
            Debug.Log($"[ItemSlotUI] 가져온 아이템 수: {items.Count}");
            
            // 아이템 버튼들 생성
            CreateItemButtons(items);
            Debug.Log($"[ItemSlotUI] 생성된 ItemButton 수: {itemButtons.Count}");
            
            // 현재 착용 아이템 정보 업데이트
            UpdateEquippedItemInfo();
            
            Debug.Log($"[ItemSlotUI] 아이템 슬롯 초기화 완료: {itemType}, 아이템 수: {items.Count}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[ItemSlotUI] 아이템 슬롯 초기화 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 특정 타입의 아이템들 가져오기
    /// </summary>
    private List<ItemData> GetItemsByType(ItemData.ItemType itemType)
    {
        try
        {
            Debug.Log($"[ItemSlotUI] GetItemsByType 호출됨 - ItemType: {itemType}");
            
            if (ItemManager.Instance == null) 
            {
                Debug.LogError("[ItemSlotUI] ItemManager.Instance가 null입니다!");
                return new List<ItemData>();
            }

            // 소유한 아이템들 중 해당 타입만 필터링
            var allItems = ItemManager.Instance.GetOwnedItems();
            Debug.Log($"[ItemSlotUI] 전체 소유 아이템 수: {allItems.Count}");
            
            var filteredItems = new List<ItemData>();

            foreach (var item in allItems)
            {
                if (item.itemType == itemType)
                {
                    filteredItems.Add(item);
                    Debug.Log($"[ItemSlotUI] 필터링된 아이템: {item.itemName} (ID: {item.itemId})");
                }
            }

            Debug.Log($"[ItemSlotUI] {itemType} 타입 아이템 수: {filteredItems.Count}");
            return filteredItems;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[ItemSlotUI] 아이템 가져오기 중 오류: {ex.Message}");
            return new List<ItemData>();
        }
    }

    /// <summary>
    /// 아이템 버튼들 생성 (모든 아이템 생성, 보유/미보유 상태에 따른 시각적 구분)
    /// </summary>
    private void CreateItemButtons(List<ItemData> items)
    {
        try
        {
            if (itemButtonPrefab == null)
            {
                Debug.LogError("[ItemSlotUI] ItemButton 프리팹이 설정되지 않았습니다.");
                return;
            }

            // 모든 아이템 타입의 아이템들 가져오기 (보유 여부와 관계없이)
            var allItems = GetAllItemsByType(currentItemType);
            Debug.Log($"[ItemSlotUI] 전체 아이템 수: {allItems.Count}");

            foreach (var item in allItems)
            {
                // 프리팹 인스턴스 생성
                GameObject buttonObj = Instantiate(itemButtonPrefab, contentTransform);
                ItemButton itemButton = buttonObj.GetComponent<ItemButton>();

                if (itemButton != null)
                {
                    // 아이템 버튼 초기화
                    itemButton.Initialize(item, currentItemType);
                    
                    // 보유 상태 확인
                    bool isOwned = IsItemOwned(item.itemId);
                    
                    // 착용 상태 확인
                    bool isEquipped = IsItemEquipped(item.itemId);
                    
                    // 보유/미보유 상태에 따른 시각적 설정
                    SetItemButtonVisualState(itemButton, isOwned, isEquipped);
                    
                    // 클릭 이벤트 연결 (보유한 아이템만)
                    if (isOwned)
                    {
                        itemButton.OnItemClickedEvent += OnItemButtonClicked;
                    }
                    
                    // 리스트에 추가
                    itemButtons.Add(itemButton);
                }
                else
                {
                    Debug.LogError($"[ItemSlotUI] ItemButton 컴포넌트를 찾을 수 없습니다: {item.itemName}");
                    Destroy(buttonObj);
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[ItemSlotUI] 아이템 버튼 생성 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 특정 타입의 모든 아이템들 가져오기 (보유 여부와 관계없이)
    /// </summary>
    private List<ItemData> GetAllItemsByType(ItemData.ItemType itemType)
    {
        try
        {
            Debug.Log($"[ItemSlotUI] GetAllItemsByType 호출됨 - ItemType: {itemType}");
            
            if (ItemManager.Instance == null) 
            {
                Debug.LogError("[ItemSlotUI] ItemManager.Instance가 null입니다!");
                return new List<ItemData>();
            }

            // 모든 아이템들 중 해당 타입만 필터링 (보유 여부와 관계없이)
            var allItems = ItemManager.Instance.GetAllItems();
            Debug.Log($"[ItemSlotUI] 전체 아이템 수: {allItems.Count}");
            
            var filteredItems = new List<ItemData>();

            foreach (var item in allItems)
            {
                if (item.itemType == itemType)
                {
                    filteredItems.Add(item);
                    Debug.Log($"[ItemSlotUI] 필터링된 아이템: {item.itemName} (ID: {item.itemId})");
                }
            }

            Debug.Log($"[ItemSlotUI] 타입별 필터링 완료: {filteredItems.Count}개");
            return filteredItems;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[ItemSlotUI] 아이템 가져오기 중 오류: {ex.Message}");
            return new List<ItemData>();
        }
    }

    /// <summary>
    /// 아이템을 보유하고 있는지 확인
    /// </summary>
    private bool IsItemOwned(string itemId)
    {
        try
        {
            if (ItemManager.Instance == null) return false;
            
            var ownedItems = ItemManager.Instance.GetOwnedItems();
            return ownedItems.Any(item => item.itemId == itemId);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[ItemSlotUI] 아이템 보유 상태 확인 중 오류: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// 아이템이 착용되어 있는지 확인
    /// </summary>
    private bool IsItemEquipped(string itemId)
    {
        try
        {
            if (ItemManager.Instance == null) return false;
            return ItemManager.Instance.IsItemEquipped(itemId);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[ItemSlotUI] 착용 상태 확인 중 오류: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// 아이템 버튼의 시각적 상태 설정 (보유/미보유, 착용 상태)
    /// </summary>
    private void SetItemButtonVisualState(ItemButton itemButton, bool isOwned, bool isEquipped)
    {
        try
        {
            if (itemButton == null) return;

            // 보유 상태에 따른 색상 설정
            if (isOwned)
            {
                // 보유한 아이템: 정상 색상 (1f, 1f, 1f, 1f)
                itemButton.SetButtonColor(new Color(1f, 1f, 1f, 1f));
                itemButton.SetInteractable(true);
                LogDebug($"[ItemSlotUI] 보유 아이템 설정: {itemButton.ItemData.itemName} - 정상 색상");
            }
            else
            {
                // 미보유 아이템: 어두운 색상 (0.8f, 0.8f, 0.8f, 1f)
                itemButton.SetButtonColor(new Color(0.8f, 0.8f, 0.8f, 1f));
                itemButton.SetInteractable(false);
                LogDebug($"[ItemSlotUI] 미보유 아이템 설정: {itemButton.ItemData.itemName} - 어두운 색상");
            }

            // 착용 상태에 따른 EquippedMark 설정
            if (isEquipped)
            {
                itemButton.SetEquipped(true);
                LogDebug($"[ItemSlotUI] 착용 아이템 표시: {itemButton.ItemData.itemName}");
            }
            else
            {
                itemButton.SetEquipped(false);
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[ItemSlotUI] 아이템 버튼 시각적 상태 설정 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 아이템 버튼 클릭 이벤트 처리
    /// </summary>
    private void OnItemButtonClicked(ItemButton itemButton)
    {
        try
        {
            // 이전 선택 해제
            if (selectedItemButton != null && selectedItemButton != itemButton)
            {
                selectedItemButton.SetSelected(false);
            }

            // 새 선택 설정
            selectedItemButton = itemButton;
            itemButton.SetSelected(true);

            // 이벤트 발생
            OnItemSelectedEvent?.Invoke(itemButton.ItemData, currentItemType);

            // 아이템 교체 처리
            HandleItemEquip(itemButton);

            LogDebug($"[ItemSlotUI] 아이템 선택: {itemButton.ItemData.itemName}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[ItemSlotUI] 아이템 버튼 클릭 처리 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 아이템 착용/해제 처리
    /// </summary>
    private void HandleItemEquip(ItemButton itemButton)
    {
        try
        {
            if (ItemManager.Instance == null) return;

            string itemId = itemButton.ItemData.itemId;
            bool isCurrentlyEquipped = IsItemEquipped(itemId);

            if (isCurrentlyEquipped)
            {
                // 착용 해제
                ItemManager.Instance.UnequipItem(itemButton.ItemData.itemType);
                itemButton.SetEquipped(false);
                LogDebug($"[ItemSlotUI] 아이템 착용 해제: {itemButton.ItemData.itemName}");
            }
            else
            {
                // 착용
                ItemManager.Instance.EquipItem(itemId);
                itemButton.SetEquipped(true);
                LogDebug($"[ItemSlotUI] 아이템 착용: {itemButton.ItemData.itemName}");
            }

            // 모든 버튼의 착용 상태 업데이트
            UpdateAllButtonEquippedStates();
            
            // 현재 착용 아이템 정보 업데이트
            UpdateEquippedItemInfo();
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[ItemSlotUI] 아이템 착용 처리 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 모든 버튼의 착용 상태 업데이트
    /// </summary>
    private void UpdateAllButtonEquippedStates()
    {
        try
        {
            foreach (var button in itemButtons)
            {
                bool isEquipped = IsItemEquipped(button.ItemData.itemId);
                button.SetEquipped(isEquipped);
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[ItemSlotUI] 버튼 착용 상태 업데이트 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 현재 착용 아이템 정보 업데이트
    /// </summary>
    private void UpdateEquippedItemInfo()
    {
        try
        {
            if (itemDescObject == null) return;

            // 현재 착용된 아이템 가져오기
            ItemData equippedItem = ItemManager.Instance?.GetEquippedItem(currentItemType);

            if (equippedItem != null)
            {
                // 설명 오브젝트 활성화
                itemDescObject.SetActive(true);

                // 이름 텍스트 설정
                if (itemDescNameText != null)
                {
                    itemDescNameText.text = equippedItem.itemName;
                }

                // 설명 텍스트 설정
                if (itemDescText != null)
                {
                    itemDescText.text = GetItemDescription(equippedItem);
                }

                LogDebug($"[ItemSlotUI] 착용 아이템 정보 업데이트: {equippedItem.itemName}");
            }
            else
            {
                // 착용된 아이템이 없으면 설명 오브젝트 비활성화
                itemDescObject.SetActive(false);
                LogDebug("[ItemSlotUI] 착용된 아이템 없음");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[ItemSlotUI] 착용 아이템 정보 업데이트 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 아이템 설명 가져오기
    /// </summary>
    private string GetItemDescription(ItemData item)
    {
        // 간단한 설명 생성 (실제로는 ItemData에 description 필드가 있을 수 있음)
        return $"{item.itemName}의 상세한 설명입니다. 이 아이템은 {item.itemType} 타입입니다.";
    }

    /// <summary>
    /// 아이템 버튼들 제거
    /// </summary>
    private void ClearItemButtons()
    {
        try
        {
            foreach (var button in itemButtons)
            {
                if (button != null)
                {
                    button.OnItemClickedEvent -= OnItemButtonClicked;
                    Destroy(button.gameObject);
                }
            }

            itemButtons.Clear();
            selectedItemButton = null;

            LogDebug("[ItemSlotUI] 아이템 버튼들 제거 완료");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[ItemSlotUI] 아이템 버튼 제거 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 아이템 슬롯 새로고침
    /// </summary>
    public void Refresh()
    {
        Initialize(currentItemType);
    }

    /// <summary>
    /// 슬롯 정보 반환
    /// </summary>
    public string GetSlotInfo()
    {
        var info = new System.Text.StringBuilder();
        info.AppendLine($"[ItemSlotUI 정보]");
        info.AppendLine($"현재 타입: {currentItemType}");
        info.AppendLine($"아이템 버튼 수: {itemButtons.Count}");
        info.AppendLine($"선택된 버튼: {(selectedItemButton != null ? selectedItemButton.ItemData.itemName : "없음")}");
        info.AppendLine($"설명 오브젝트: {(itemDescObject != null ? "활성화" : "비활성화")}");

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
        foreach (var button in itemButtons)
        {
            if (button != null)
            {
                button.OnItemClickedEvent -= OnItemButtonClicked;
            }
        }

        // 이벤트 초기화
        OnItemSelectedEvent = null;
    }
    }
}
