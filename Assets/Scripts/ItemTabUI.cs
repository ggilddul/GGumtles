using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemTabUI : MonoBehaviour
{
    public static ItemTabUI Instance;
    public OverViewRenderer overViewRenderer;

    [Header("Hat UI")]
    public Image hatPreviewImage;
    public TMP_Text hatNameText;

    [Header("Face UI")]
    public Image facePreviewImage;
    public TMP_Text faceNameText;

    [Header("Costume UI")]
    public Image costumePreviewImage;
    public TMP_Text costumeNameText;

    private void Awake()
    {
        Instance = this;
    }

    public void RefreshAllPreviews()
    {
        RefreshPreview(ItemData.ItemType.Hat, hatPreviewImage, hatNameText);
        RefreshPreview(ItemData.ItemType.Face, facePreviewImage, faceNameText);
        RefreshPreview(ItemData.ItemType.Costume, costumePreviewImage, costumeNameText);
        overViewRenderer.RefreshOverview();
    }

    void RefreshPreview(ItemData.ItemType type, Image previewImage, TMP_Text nameText)
    {
        string itemId = type switch
        {
            ItemData.ItemType.Hat => ItemManager.Instance.GetCurrentHatId(),
            ItemData.ItemType.Face => ItemManager.Instance.GetCurrentFaceId(),
            ItemData.ItemType.Costume => ItemManager.Instance.GetCurrentCostumeId(),
            _ => ""
        };

        var item = ItemManager.Instance.GetItemById(itemId);

        if (item != null)
        {
            previewImage.sprite = item.sprite;
            nameText.text = item.itemName;
        }
        else
        {
            previewImage.sprite = null;
            nameText.text = "";
        }
    }

}
