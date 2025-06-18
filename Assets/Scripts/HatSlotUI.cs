using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HatSlotUI : MonoBehaviour
{
    public Image iconImage;
    public TMP_Text nameText;
    public Button selectButton; 

    private ItemData itemData;

    public void Initialize(ItemData item)
    {
        itemData = item;

        // 직접 itemData의 sprite 사용
        iconImage.sprite = item.sprite;

        nameText.text = item.itemName; // 'item.name'이 아닌 itemName

        selectButton.onClick.RemoveAllListeners();
        selectButton.onClick.AddListener(OnSelect);
    }


    void OnSelect()
    {
        // 아이템 장착 처리
        ItemManager.Instance.EquipItem(itemData.itemId);
        
        // 팝업 갱신 (필요하면)
        ItemTabUI.Instance.RefreshAllPreviews();
    }
}
