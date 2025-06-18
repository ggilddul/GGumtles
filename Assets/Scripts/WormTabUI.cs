using UnityEngine;
using System.Collections;
using TMPro;
public class WormTabUI : MonoBehaviour
{
    public static WormTabUI Instance { get; private set; }

    public OverViewRenderer overViewRenderer;

    public TMP_Text AgeText;
    public TMP_Text StageText;
    public TMP_Text GenText;
    public TMP_Text NameText;

    public void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        StartCoroutine(AutoRefreshRoutine());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    private IEnumerator AutoRefreshRoutine()
    {
        while (true)
        {
            UpdateUI();  
            yield return new WaitForSeconds(1f); 
        }
    }

    public void UpdateUI()
    {
        var worm = WormManager.Instance.GetCurrentWorm();
        if (worm == null)
        {
            AgeText.text = "-";
            StageText.text = "-";
            GenText.text = "-";
            NameText.text = "-";
            return;
        }

        AgeText.text = FormatAge(worm.age);
        StageText.text = GetLifeStageName(worm.lifeStage);
        GenText.text = $"{worm.gen}세대";
        NameText.text = worm.name;
    }

    private string FormatAge(int ageInMinutes)
    {
        int days = ageInMinutes / 1440;
        int hours = (ageInMinutes % 1440) / 60;
        int minutes = ageInMinutes % 60;

        string result = "";
        if (days > 0) result += $"{days}일 ";
        if (hours > 0) result += $"{hours}시간 ";
        if (minutes > 0 || result == "") result += $"{minutes}분";

        return result.Trim();
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
}
