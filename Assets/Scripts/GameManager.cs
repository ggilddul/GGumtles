using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Time")]
    public float timeScale = 60f; // 현실 시간의 60배속
    public float gameTime;

    private int hour;
    private int hour24;
    private int minute;
    private string AMPM;
    private int lastMinute = -1;

    [Header("Resources")]
    public int acornCount;
    public int diamondCount;

    [Header("References")]
    public MapManager mapManager;

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

    public void Initialize()
    {
        var saveData = GameSaveManager.Instance.currentSaveData;

        gameTime = saveData.totalPlayTime;
        acornCount = saveData.acornCount;
        diamondCount = saveData.diamondCount;

        if (saveData.wormList.Count == 0)
        {
            WormManager.Instance?.CreateNewWorm(1);
        }
        else
        {
            WormManager.LoadCurrentWorm(saveData.wormList[^1]); // 가장 마지막 웜 로드
        }

        UpdateCurrentWormNameUI();
        mapManager.SetMapIndex(saveData.selectedMapIndex);
        ItemTabUI.Instance?.RefreshAllPreviews();
        TabManager.Instance?.OpenTab(2);

        // 로딩 종료 2초 뒤 실행
        Invoke(nameof(EndLoading), 2f);
    }

    private void EndLoading()
    {
        LoadingManager.Instance.HideLoading(LoadingManager.LoadingType.Logo);
    }

    private void Update()
    {
        UpdateGameTime();
    }

    public void UseAcorn()
    {
        if (acornCount > 0)
        {
            acornCount--;
        }
        else
        {
            PopupManager.Instance?.OpenPopup(18);
        }
    }

    public void PickAcorn()
    {
        acornCount++;
    }

    public void PickDiamond()
    {
        diamondCount++;
    }

    public void EarnItem(int index)
    {
        // 아이템 획득 처리 (추후 구현)
    }

    private void UpdateGameTime()
    {
        float deltaGameTime = Time.deltaTime * timeScale;
        gameTime += deltaGameTime;

        float secondsInDay = 86400f;
        float currentSeconds = gameTime % secondsInDay;

        hour24 = (int)(currentSeconds / 3600) % 24;
        minute = (int)((currentSeconds % 3600) / 60);

        // AM/PM 및 12시간제 변환
        if (hour24 == 0)
        {
            hour = 12;
            AMPM = "AM";
        }
        else if (hour24 < 12)
        {
            hour = hour24;
            AMPM = "AM";
        }
        else if (hour24 == 12)
        {
            hour = 12;
            AMPM = "PM";
        }
        else
        {
            hour = hour24 - 12;
            AMPM = "PM";
        }

        if (minute != lastMinute)
        {
            lastMinute = minute;

            TopBarManager.Instance?.UpdateTime(hour, minute, AMPM);
            UpdateMapBackground();

            WormData currentWorm = WormManager.Instance.GetCurrentWorm();
            if (currentWorm != null)
            {
                currentWorm.age += 1;
                WormManager.Instance.EvolveCurrentWorm();
            }

            Debug.Log($"[GameTime] minute={minute}, age={currentWorm?.age}, stage={currentWorm?.lifeStage}");
        }
    }

    private void UpdateMapBackground()
    {
        SpriteManager.MapPhase phase;
        if (IsDaytime(hour24))
            phase = SpriteManager.MapPhase.Day;
        else if (IsSunset(hour24))
            phase = SpriteManager.MapPhase.Sunset;
        else
            phase = SpriteManager.MapPhase.Night;

        SpriteManager.MapType mapType = mapManager.GetCurrentMapType();
        Sprite mapSprite = SpriteManager.Instance.GetMapSprite(mapType, phase);
        mapManager?.UpdateMapBackground(mapSprite);
    }

    public void ForceUpdateMapBackground()
    {
        UpdateMapBackground();
    }

    private bool IsDaytime(int hour)
    {
        return hour >= 7 && hour < 17;
    }

    private bool IsSunset(int hour)
    {
        return (hour >= 5 && hour < 7) || (hour >= 17 && hour < 19);
    }

    private void UpdateCurrentWormNameUI()
    {
        WormData currentWorm = WormManager.Instance.GetCurrentWorm();
        if (currentWorm != null)
        {
            TopBarManager.Instance?.UpdateCurrentWormName(currentWorm.name);
        }
    }

    private void OnApplicationQuit()
    {
        GameSaveManager.Instance.SaveGame();
    }
}
