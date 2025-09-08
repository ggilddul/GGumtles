using System.Collections.Generic;
using UnityEngine;
using GGumtles.Data;
using System.Linq;
using System.Collections;
using GGumtles.Managers;

namespace GGumtles.Managers
{
    public class ItemManager : MonoBehaviour
{
    public static ItemManager Instance { get; private set; }

    [Header("아이템 설정")]
    [SerializeField] private bool autoSaveOnChange = true;
    [SerializeField] private float saveDelay = 1f;
    
    [Header("디버그 설정")]
    [SerializeField] private bool enableDebugLogs = true;
    [SerializeField] private bool showItemNotifications = true;

    // 아이템 데이터 관리
    private Dictionary<string, ItemData> allItemDataDict;
    private Dictionary<string, OwnedItemInfo> ownedItemsDict;
    private List<ItemData> ownedItemsList;

    // 장착 아이템 관리
    [System.Serializable]
    public class EquippedItems
    {
        public string hatId = "";
        public string faceId = "";
        public string costumeId = "";
        
        public EquippedItems Clone()
        {
            return new EquippedItems
            {
                hatId = this.hatId,
                faceId = this.faceId,
                costumeId = this.costumeId
            };
        }
    }

    private EquippedItems equippedItems;

    // 아이템 정보 클래스
    [System.Serializable]
    public class OwnedItemInfo
    {
        public string itemId;
        public ItemData itemData;
        public int quantity = 1;
        public bool isEquipped = false;
        public float obtainedTime;
        public int useCount = 0;
        public float lastUsedTime;
        
        public OwnedItemInfo(string id, ItemData data)
        {
            itemId = id;
            itemData = data;
            obtainedTime = Time.time;
        }
    }



    // 이벤트 정의
    public delegate void OnItemAdded(string itemId, ItemData itemData, int quantity);
    public event OnItemAdded OnItemAddedEvent;

    public delegate void OnItemEquipped(string itemId, ItemData.ItemType itemType);
    public event OnItemEquipped OnItemEquippedEvent;

    public delegate void OnItemUnequipped(string itemId, ItemData.ItemType itemType);
    public event OnItemUnequipped OnItemUnequippedEvent;

    public delegate void OnItemUsed(string itemId, int useCount);
    public event OnItemUsed OnItemUsedEvent;



    // 상태 관리
    private bool isInitialized = false;
    private bool isSaving = false;
    private Coroutine saveCoroutine;

    // 프로퍼티
    public int TotalOwnedItems => ownedItemsDict?.Count ?? 0;
    public int TotalEquippedItems => GetEquippedItemsCount();
    public bool HasItems => TotalOwnedItems > 0;
    public bool IsInitialized => isInitialized;

    private void Awake()
    {
        InitializeSingleton();
    }

    public void Initialize()
    {
        InitializeItemSystem();
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

    private void InitializeItemSystem()
    {
        try
        {
            InitializeDataStructures();
            CacheAllItems();
            LoadEquippedItems();
            isInitialized = true;
            
            LogDebug($"[ItemManager] 아이템 시스템 초기화 완료 - 총 아이템: {allItemDataDict?.Count ?? 0}개");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[ItemManager] 초기화 중 오류: {ex.Message}");
        }
    }

    private void InitializeDataStructures()
    {
        allItemDataDict = new Dictionary<string, ItemData>();
        ownedItemsDict = new Dictionary<string, OwnedItemInfo>();
        ownedItemsList = new List<ItemData>();
        equippedItems = new EquippedItems();
    }

    private void CacheAllItems()
    {
        var itemDataArray = Resources.LoadAll<ItemData>("ItemData");
        
        foreach (var item in itemDataArray)
        {
            if (item != null && !string.IsNullOrEmpty(item.itemId))
            {
                allItemDataDict[item.itemId] = item;
            }
        }
        
        LogDebug($"[ItemManager] {allItemDataDict.Count}개의 아이템 데이터 캐시 완료");
    }

    private void LoadEquippedItems()
    {
        // 저장된 장착 아이템 로드 (GameSaveManager에서)
        // 착용 아이템은 기본값으로 초기화 (저장 데이터에서 로드하지 않음)
        equippedItems.hatId = "";
        equippedItems.faceId = "";
        equippedItems.costumeId = "";
    }

    /// <summary>
    /// 저장된 아이템 목록으로 초기화
    /// </summary>
    public void Initialize(List<string> savedItemIds)
    {
        if (!isInitialized)
        {
            Debug.LogWarning("[ItemManager] 아직 초기화되지 않았습니다.");
            return;
        }

        ClearAllItems();

        foreach (var id in savedItemIds)
        {
            if (!string.IsNullOrEmpty(id))
            {
                AddItem(id, 1, false);
            }
        }

        ValidateEquippedItems();
        LogDebug($"[ItemManager] {savedItemIds.Count}개의 저장된 아이템 로드 완료");
    }

    /// <summary>
    /// 아이템 추가
    /// </summary>
    public void AddItem(string itemId, int quantity = 1, bool showNotification = true)
    {
        if (!isInitialized)
        {
            Debug.LogWarning("[ItemManager] 아직 초기화되지 않았습니다.");
            return;
        }

        if (string.IsNullOrEmpty(itemId))
        {
            Debug.LogError("[ItemManager] 아이템 ID가 비어있습니다.");
            return;
        }

        if (!allItemDataDict.TryGetValue(itemId, out var itemData))
        {
            Debug.LogError($"[ItemManager] 찾을 수 없는 아이템 ID: {itemId}");
            return;
        }

        if (quantity <= 0)
        {
            Debug.LogWarning($"[ItemManager] 잘못된 수량: {quantity}");
            return;
        }

        // 기존 아이템이 있는지 확인
        if (ownedItemsDict.TryGetValue(itemId, out var existingItem))
        {
            existingItem.quantity += quantity;
            LogDebug($"[ItemManager] 아이템 수량 증가: {itemData.itemName} +{quantity} (총 {existingItem.quantity}개)");
        }
        else
        {
            // 새 아이템 추가
            var newItemInfo = new OwnedItemInfo(itemId, itemData);
            newItemInfo.quantity = quantity;
            
            ownedItemsDict[itemId] = newItemInfo;
            ownedItemsList.Add(itemData);
            
            LogDebug($"[ItemManager] 새 아이템 획득: {itemData.itemName} x{quantity}");
        }

        // 이벤트 발생
        OnItemAddedEvent?.Invoke(itemId, itemData, quantity);

        // 알림 표시
        if (showNotification && this.showItemNotifications)
        {
            ShowItemNotification(itemData, quantity);
        }

        // 자동 저장
        if (autoSaveOnChange)
        {
            ScheduleSave();
        }
    }

    /// <summary>
    /// 아이템 장착
    /// </summary>
    public bool EquipItem(string itemId)
    {
        if (!isInitialized)
        {
            Debug.LogWarning("[ItemManager] 아직 초기화되지 않았습니다.");
            return false;
        }

        if (string.IsNullOrEmpty(itemId))
        {
            Debug.LogError("[ItemManager] 아이템 ID가 비어있습니다.");
            return false;
        }

        if (!ownedItemsDict.TryGetValue(itemId, out var itemInfo))
        {
            Debug.LogError($"[ItemManager] 소유하지 않은 아이템: {itemId}");
            return false;
        }

        var itemData = itemInfo.itemData;
        if (itemData == null)
        {
            Debug.LogError($"[ItemManager] 아이템 데이터가 null입니다: {itemId}");
            return false;
        }

        // 이전 장착 아이템 해제
        UnequipItemByType(itemData.itemType);

        // 새 아이템 장착
        switch (itemData.itemType)
        {
            case ItemData.ItemType.Hat:
                equippedItems.hatId = itemId;
                break;
            case ItemData.ItemType.Face:
                equippedItems.faceId = itemId;
                break;
            case ItemData.ItemType.Costume:
                equippedItems.costumeId = itemId;
                break;
            default:
                Debug.LogWarning($"[ItemManager] 지원하지 않는 아이템 타입: {itemData.itemType}");
                return false;
        }

        itemInfo.isEquipped = true;
        


        LogDebug($"[ItemManager] 아이템 장착: {itemData.itemName} ({itemData.itemType})");

        // 이벤트 발생
        OnItemEquippedEvent?.Invoke(itemId, itemData.itemType);

        // 자동 저장
        if (autoSaveOnChange)
        {
            ScheduleSave();
        }

        return true;
    }

    /// <summary>
    /// 아이템 장착 해제
    /// </summary>
    public bool UnequipItem(ItemData.ItemType itemType)
    {
        if (!isInitialized) return false;

        string itemId = GetEquippedItemId(itemType);
        if (string.IsNullOrEmpty(itemId)) return false;

        return UnequipItemById(itemId);
    }

    public bool UnequipItemById(string itemId)
    {
        if (!isInitialized) return false;

        if (!ownedItemsDict.TryGetValue(itemId, out var itemInfo))
        {
            Debug.LogWarning($"[ItemManager] 소유하지 않은 아이템 해제 시도: {itemId}");
            return false;
        }

        var itemData = itemInfo.itemData;
        if (itemData == null) return false;

        // 장착 해제
        switch (itemData.itemType)
        {
            case ItemData.ItemType.Hat:
                if (equippedItems.hatId == itemId)
                    equippedItems.hatId = "";
                break;
            case ItemData.ItemType.Face:
                if (equippedItems.faceId == itemId)
                    equippedItems.faceId = "";
                break;
            case ItemData.ItemType.Costume:
                if (equippedItems.costumeId == itemId)
                    equippedItems.costumeId = "";
                break;
        }

        itemInfo.isEquipped = false;



        LogDebug($"[ItemManager] 아이템 장착 해제: {itemData.itemName}");

        // 이벤트 발생
        OnItemUnequippedEvent?.Invoke(itemId, itemData.itemType);

        // 자동 저장
        if (autoSaveOnChange)
        {
            ScheduleSave();
        }

        return true;
    }

    /// <summary>
    /// 아이템 사용
    /// </summary>
    public bool UseItem(string itemId)
    {
        if (!isInitialized) return false;

        if (!ownedItemsDict.TryGetValue(itemId, out var itemInfo))
        {
            Debug.LogWarning($"[ItemManager] 소유하지 않은 아이템 사용 시도: {itemId}");
            return false;
        }

        if (itemInfo.quantity <= 0)
        {
            Debug.LogWarning($"[ItemManager] 아이템 수량 부족: {itemId}");
            return false;
        }

        // 아이템 사용 로직
        itemInfo.quantity--;
        itemInfo.useCount++;
        itemInfo.lastUsedTime = Time.time;

        LogDebug($"[ItemManager] 아이템 사용: {itemInfo.itemData.itemName} (남은 수량: {itemInfo.quantity})");

        // 수량이 0이 되면 제거
        if (itemInfo.quantity <= 0)
        {
            RemoveItem(itemId);
        }

        // 이벤트 발생
        OnItemUsedEvent?.Invoke(itemId, itemInfo.useCount);

        // 자동 저장
        if (autoSaveOnChange)
        {
            ScheduleSave();
        }

        return true;
    }

    /// <summary>
    /// 아이템 제거
    /// </summary>
    public bool RemoveItem(string itemId)
    {
        if (!isInitialized) return false;

        if (!ownedItemsDict.TryGetValue(itemId, out var itemInfo))
        {
            Debug.LogWarning($"[ItemManager] 소유하지 않은 아이템 제거 시도: {itemId}");
            return false;
        }

        // 장착된 아이템이면 해제
        if (itemInfo.isEquipped)
        {
            UnequipItemById(itemId);
        }

        // 아이템 제거
        ownedItemsDict.Remove(itemId);
        ownedItemsList.Remove(itemInfo.itemData);

        LogDebug($"[ItemManager] 아이템 제거: {itemInfo.itemData.itemName}");

        // 자동 저장
        if (autoSaveOnChange)
        {
            ScheduleSave();
        }

        return true;
    }



    /// <summary>
    /// 아이템 검증
    /// </summary>
    private void ValidateEquippedItems()
    {
        var hatId = equippedItems.hatId;
        var faceId = equippedItems.faceId;
        var costumeId = equippedItems.costumeId;

        // 장착된 아이템이 실제로 소유하고 있는지 확인
        if (!string.IsNullOrEmpty(hatId) && !ownedItemsDict.ContainsKey(hatId))
        {
            Debug.LogWarning($"[ItemManager] 소유하지 않은 모자 아이템 장착 해제: {hatId}");
            equippedItems.hatId = "";
        }

        if (!string.IsNullOrEmpty(faceId) && !ownedItemsDict.ContainsKey(faceId))
        {
            Debug.LogWarning($"[ItemManager] 소유하지 않은 얼굴 아이템 장착 해제: {faceId}");
            equippedItems.faceId = "";
        }

        if (!string.IsNullOrEmpty(costumeId) && !ownedItemsDict.ContainsKey(costumeId))
        {
            Debug.LogWarning($"[ItemManager] 소유하지 않은 의상 아이템 장착 해제: {costumeId}");
            equippedItems.costumeId = "";
        }
    }

    /// <summary>
    /// 아이템 알림 표시
    /// </summary>
    private void ShowItemNotification(ItemData itemData, int quantity)
    {
        if (PopupManager.Instance != null)
        {
            string message = quantity > 1 ? 
                $"{itemData.itemName} x{quantity} 획득!" : 
                $"{itemData.itemName} 획득!";
            
            PopupManager.Instance.ShowToast(message, 2f);
        }
    }

    /// <summary>
    /// 자동 저장 스케줄링
    /// </summary>
    private void ScheduleSave()
    {
        if (saveCoroutine != null)
        {
            StopCoroutine(saveCoroutine);
        }
        
        saveCoroutine = StartCoroutine(SaveDelayed());
    }

    private IEnumerator SaveDelayed()
    {
        yield return new WaitForSeconds(saveDelay);
        SaveItemData();
    }

    /// <summary>
    /// 아이템 데이터 저장
    /// </summary>
    public void SaveItemData()
    {
        if (isSaving) return;

        try
        {
            isSaving = true;

            // GameSaveManager에 아이템 데이터 저장
            if (GameSaveManager.Instance?.currentSaveData != null)
            {
                var saveData = GameSaveManager.Instance.currentSaveData;
                
                // 소유 아이템 목록 저장
                saveData.ownedItemIds = ownedItemsDict.Keys.ToList();
                
                // 장착 아이템 저장 (GameSaveData에 equippedItems 필드가 없으므로 주석 처리)
                // saveData.equippedItems = equippedItems.Clone();
                
                GameSaveManager.Instance.SaveGame();
                
                LogDebug("[ItemManager] 아이템 데이터 저장 완료");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[ItemManager] 아이템 데이터 저장 중 오류: {ex.Message}");
        }
        finally
        {
            isSaving = false;
        }
    }

    // 유틸리티 메서드들
    public ItemData GetItemById(string itemId)
    {
        if (string.IsNullOrEmpty(itemId)) return null;
        return allItemDataDict.TryGetValue(itemId, out var item) ? item : null;
    }

    public OwnedItemInfo GetOwnedItemInfo(string itemId)
    {
        if (string.IsNullOrEmpty(itemId)) return null;
        return ownedItemsDict.TryGetValue(itemId, out var info) ? info : null;
    }

    public List<ItemData> GetOwnedItems() => new List<ItemData>(ownedItemsList);

    /// <summary>
    /// 모든 아이템 데이터 가져오기 (보유 여부와 관계없이)
    /// </summary>
    public List<ItemData> GetAllItems()
    {
        if (allItemDataDict == null) return new List<ItemData>();
        return new List<ItemData>(allItemDataDict.Values);
    }

    /// <summary>
    /// 특정 타입의 모든 아이템 데이터 가져오기
    /// </summary>
    public List<ItemData> GetItemsByType(ItemData.ItemType itemType)
    {
        if (allItemDataDict == null) return new List<ItemData>();
        
        return allItemDataDict.Values
            .Where(item => item.itemType == itemType)
            .ToList();
    }

    public List<OwnedItemInfo> GetOwnedItemInfosByType(ItemData.ItemType type)
    {
        return ownedItemsDict.Values.Where(i => i.itemData.itemType == type).ToList();
    }

    public string GetEquippedItemId(ItemData.ItemType type)
    {
        switch (type)
        {
            case ItemData.ItemType.Hat: return equippedItems.hatId;
            case ItemData.ItemType.Face: return equippedItems.faceId;
            case ItemData.ItemType.Costume: return equippedItems.costumeId;
            default: return "";
        }
    }

    public ItemData GetEquippedItem(ItemData.ItemType type)
    {
        string itemId = GetEquippedItemId(type);
        return GetItemById(itemId);
    }

    public EquippedItems GetEquippedItems() => equippedItems.Clone();

    public bool IsItemEquipped(string itemId)
    {
        return ownedItemsDict.TryGetValue(itemId, out var info) && info.isEquipped;
    }

    public bool HasItem(string itemId)
    {
        return ownedItemsDict.ContainsKey(itemId);
    }

    public int GetItemQuantity(string itemId)
    {
        return ownedItemsDict.TryGetValue(itemId, out var info) ? info.quantity : 0;
    }

    public int GetItemsCountByType(ItemData.ItemType type)
    {
        return ownedItemsDict.Values.Count(i => i.itemData.itemType == type);
    }

    public int GetEquippedItemsCount()
    {
        int count = 0;
        if (!string.IsNullOrEmpty(equippedItems.hatId)) count++;
        if (!string.IsNullOrEmpty(equippedItems.faceId)) count++;
        if (!string.IsNullOrEmpty(equippedItems.costumeId)) count++;
        return count;
    }

    public void ClearAllItems()
    {
        ownedItemsDict.Clear();
        ownedItemsList.Clear();
        
        // 장착 아이템도 해제
        equippedItems.hatId = "";
        equippedItems.faceId = "";
        equippedItems.costumeId = "";
        
        LogDebug("[ItemManager] 모든 아이템 제거");
    }

    private void UnequipItemByType(ItemData.ItemType type)
    {
        string currentItemId = GetEquippedItemId(type);
        if (!string.IsNullOrEmpty(currentItemId))
        {
            UnequipItemById(currentItemId);
        }
    }

    private void LogDebug(string message)
    {
        if (enableDebugLogs)
        {
            Debug.Log(message);
        }
    }

    /// <summary>
    /// 아이템 통계 정보 반환
    /// </summary>
    public string GetItemStatistics()
    {
        if (!isInitialized) return "아이템 시스템이 초기화되지 않았습니다.";

        var stats = new System.Text.StringBuilder();
        stats.AppendLine($"[아이템 통계]");
        stats.AppendLine($"총 소유 아이템: {TotalOwnedItems}개");
        stats.AppendLine($"총 장착 아이템: {TotalEquippedItems}개");
        stats.AppendLine($"모자: {GetItemsCountByType(ItemData.ItemType.Hat)}개");
        stats.AppendLine($"얼굴: {GetItemsCountByType(ItemData.ItemType.Face)}개");
        stats.AppendLine($"의상: {GetItemsCountByType(ItemData.ItemType.Costume)}개");

        return stats.ToString();
    }

    private void OnDestroy()
    {
        // 이벤트 초기화
        OnItemAddedEvent = null;
        OnItemEquippedEvent = null;
        OnItemUnequippedEvent = null;
        OnItemUsedEvent = null;

    }
    }
}
