using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class WormDiePopupUI : MonoBehaviour
{
    [SerializeField] private Image wormImage;
    [SerializeField] private TMP_Text ageText;

    private WormData currentWorm;

    public void OpenPopup(WormData worm)
    {
        currentWorm = worm;
        gameObject.SetActive(true);

        ageText.text = $"{FormatAge(worm.age)}";

        var costumeItem = ItemManager.Instance.GetItemById(worm.costumeItemId);
        wormImage.sprite = costumeItem != null ? costumeItem.sprite : null;
    }

    public static string FormatAge(int ageInMinutes)
    {
        int days = ageInMinutes / 1440;           // 1일 = 1440분
        int hours = (ageInMinutes % 1440) / 60;    // 나머지에서 시간 추출
        int minutes = ageInMinutes % 60;           // 나머지에서 분 추출

        List<string> parts = new List<string>();
        if (days > 0) parts.Add($"{days}일");
        if (hours > 0) parts.Add($"{hours}시간");
        if (minutes > 0 || parts.Count == 0) parts.Add($"{minutes}분");

        return string.Join(" ", parts);
    }

}
