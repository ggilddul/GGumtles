using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class WormEvolvePopupUI : MonoBehaviour
{
    public OverViewRenderer overviewRenderer;
    [SerializeField] private TMP_Text beforeText;
    [SerializeField] private TMP_Text afterText;
    [SerializeField] private TMP_Text ageText;

    private WormData currentWorm;

    public void OpenPopup(WormData worm)
    {
        currentWorm = worm;
        gameObject.SetActive(true);

        beforeText.text = GetLifeStageName(worm.lifeStage - 1);
        afterText.text = GetLifeStageName(worm.lifeStage);
        ageText.text = $"{FormatAge(worm.age)}";

        overviewRenderer.RefreshOverview();
    }

    private string GetLifeStageName(int stage)
    {
        return stage switch
        {
            0 => "알",
            1 => "제 1 유충기",
            2 => "제 2 유충기",
            3 => "제 3 유충기",
            4 => "제 4 유충기",
            5 => "성체",
            6 => "영혼",
            _ => "?"
        };
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
