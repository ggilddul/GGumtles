using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CostumeSlotUI : MonoBehaviour
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
        // 의상 아이템 장착
        ItemManager.Instance.EquipItem(itemData.itemId);

        // UI 갱신
        ItemTabUI.Instance.RefreshAllPreviews();
    }
}
