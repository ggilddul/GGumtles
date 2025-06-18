using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class HatPopupUI : MonoBehaviour
{
    public Transform slotParent;             // 슬롯들이 붙을 부모(그리드 레이아웃)
    public GameObject hatSlotPrefab;         // HatSlotUI 프리팹

    private List<HatSlotUI> slotList = new List<HatSlotUI>();

    private void OnEnable()
    {
        RefreshSlots();
    }

    public void RefreshSlots()
    {
        ClearSlots();

        var hatItems = ItemManager.Instance.GetItemsByType(ItemData.ItemType.Hat);
        foreach (var item in hatItems)
        {
            GameObject go = Instantiate(hatSlotPrefab, slotParent);
            HatSlotUI slot = go.GetComponent<HatSlotUI>();
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
