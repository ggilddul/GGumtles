using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WormNodeUI : MonoBehaviour
{
    public Image hatImage;
    public Image faceImage;
    public Image costumeImage;

    public TMP_Text genText;
    public TMP_Text nameText;
    public TMP_Text ageText;

    private WormData data;

    public void SetData(WormData newData)
    {
        data = newData;

        hatImage.sprite = GetItemSprite(data.hatItemId);
        faceImage.sprite = GetItemSprite(data.faceItemId);
        costumeImage.sprite = GetItemSprite(data.costumeItemId);

        genText.text = $"{newData.gen}세대";
        nameText.text = newData.name;
        ageText.text = FormatAge(newData.age);
    }

    private Sprite GetItemSprite(string itemId)
    {
        var item = ItemManager.Instance.GetItemById(itemId);
        return item != null ? item.sprite : null;
    }

    public WormData GetCurrentData()
    {
        return data;
    }

    private string FormatAge(int age)
    {
        int days = age / 1440;      // 1440분 = 1일
        int hours = (age % 1440) / 60;

        if (days > 0)
            return $"{days}일 {hours}시간";
        else if (hours > 0)
            return $"{hours}시간";
        else
            return $"{age}분";
    }
}
