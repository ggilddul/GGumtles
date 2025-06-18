using UnityEngine;
using System.Collections.Generic;

public class CostumePopupUI : MonoBehaviour
{
    public Transform slotParent;            // 슬롯 붙을 부모 (그리드 레이아웃)
    public GameObject costumeSlotPrefab;    // CostumeSlotUI 프리팹

    private List<CostumeSlotUI> slotList = new List<CostumeSlotUI>();

    private void OnEnable()
    {
        RefreshSlots();
    }

    public void RefreshSlots()
    {
        ClearSlots();

        var costumeItems = ItemManager.Instance.GetItemsByType(ItemData.ItemType.Costume);
        foreach (var item in costumeItems)
        {
            GameObject go = Instantiate(costumeSlotPrefab, slotParent);
            CostumeSlotUI slot = go.GetComponent<CostumeSlotUI>();
            slot.Initialize(item);
            slotList.Add(slot);
        }
    }

    void ClearSlots()
    {
        foreach (var slot in slotList)
        {
            if (slot != null)
                Destroy(slot.gameObject);
        }
        slotList.Clear();
    }

    public void ClosePopup()
    {
        gameObject.SetActive(false);
    }
}
