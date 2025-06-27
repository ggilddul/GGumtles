using UnityEngine;
using TMPro;

public class TopBarManager : MonoBehaviour
{
    public static TopBarManager Instance { get; private set; }

    [Header("UI References")]
    public TextMeshProUGUI AMPMText;
    public TextMeshProUGUI GameTimeText;
    public TextMeshProUGUI CurrentWormNameText;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 시간 UI를 갱신합니다 (12시간제).
    /// </summary>
    public void UpdateTime(int hour, int minute, string ampm)
    {
        GameTimeText?.SetText($"{hour}:{minute:D2}");
        AMPMText?.SetText(ampm);
    }

    /// <summary>
    /// 현재 선택된 웜 이름 UI를 갱신합니다.
    /// </summary>
    public void UpdateCurrentWormName(string wormName)
    {
        CurrentWormNameText?.SetText(wormName);
    }

}
