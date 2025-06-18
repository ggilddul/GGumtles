using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ItemManager : MonoBehaviour
{
    public static ItemManager Instance { get; private set; }

    private Dictionary<string, ItemData> allItemDataDict;
    private List<ItemData> ownedItems = new();

    [System.Serializable]
    public class EquippedItems
    {
        public string hatId;
        public string faceId;
        public string costumeId;
    }

    private EquippedItems equipped = new();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            CacheAllItems();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void CacheAllItems()
    {
        allItemDataDict = Resources.LoadAll<ItemData>("ItemData")
            .ToDictionary(item => item.itemId);
    }

    public void Initialize(List<string> savedItemIds)
    {
        ownedItems.Clear();

        foreach (var id in savedItemIds)
        {
            if (allItemDataDict.TryGetValue(id, out var item))
            {
                ownedItems.Add(item);
            }
            else
            {
                Debug.LogWarning($"[ItemManager] ID {id}에 해당하는 ItemData를 찾을 수 없습니다.");
            }
        }

        equipped.hatId = GetFirstItemId(ItemData.ItemType.Hat);
        equipped.faceId = GetFirstItemId(ItemData.ItemType.Face);
        equipped.costumeId = GetFirstItemId(ItemData.ItemType.Costume);
    }

    private string GetFirstItemId(ItemData.ItemType type)
    {
        return ownedItems.FirstOrDefault(i => i.type == type)?.itemId ?? "";
    }

    public void EquipItem(string itemId)
    {
        var item = GetItemById(itemId);
        if (item == null || !ownedItems.Any(i => i.itemId == itemId)) return;

        switch (item.type)
        {
            case ItemData.ItemType.Hat: equipped.hatId = itemId; break;
            case ItemData.ItemType.Face: equipped.faceId = itemId; break;
            case ItemData.ItemType.Costume: equipped.costumeId = itemId; break;
        }
    }

    public void AddItem(string itemId)
    {
        if (!allItemDataDict.TryGetValue(itemId, out var newItem)) return;

        if (ownedItems.Any(i => i.itemId == itemId)) return;
        ownedItems.Add(newItem);
    }

    public ItemData GetItemById(string itemId)
    {
        if (string.IsNullOrEmpty(itemId)) return null;
        return allItemDataDict.TryGetValue(itemId, out var item) ? item : null;
    }

    public List<ItemData> GetOwnedItems() => new(ownedItems);

    public List<ItemData> GetItemsByType(ItemData.ItemType type)
    {
        return ownedItems.Where(i => i.type == type).ToList();
    }

    public int HatCount => GetItemsByType(ItemData.ItemType.Hat).Count;
    public int FaceCount => GetItemsByType(ItemData.ItemType.Face).Count;
    public int CostumeCount => GetItemsByType(ItemData.ItemType.Costume).Count;

    // 착용 상태 조회
    public string GetCurrentHatId() => equipped.hatId;
    public string GetCurrentFaceId() => equipped.faceId;
    public string GetCurrentCostumeId() => equipped.costumeId;

    public EquippedItems GetEquippedItems() => equipped;
}
