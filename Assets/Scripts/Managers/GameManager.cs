using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // 상수 정의
    private const float DEFAULT_TIME_SCALE = 60f;
    private const float SECONDS_IN_DAY = 86400f;
    private const int DAYTIME_START = 7;
    private const int DAYTIME_END = 17;
    private const int SUNSET_START_MORNING = 5;
    private const int SUNSET_END_MORNING = 7;
    private const int SUNSET_START_EVENING = 17;
    private const int SUNSET_END_EVENING = 19;

    [Header("Game Time")]
    public float timeScale = DEFAULT_TIME_SCALE;
    public float gameTime;

    private int hour;
    private int hour24;
    private int minute;
    private string AMPM;
    private int lastMinute = -1;
    private bool isGameInitialized = false;

    [Header("Resources")]
    public int acornCount;
    public int diamondCount;

    [Header("References")]
    public MapManager mapManager;

    // 현재 시간 프로퍼티
    public int CurrentHour => hour;
    public int CurrentMinute => minute;
    public string CurrentAMPM => AMPM;

    // 이벤트 정의
    public delegate void OnGameTimeChanged(int hour, int minute, string ampm);
    public event OnGameTimeChanged OnGameTimeChangedEvent;

    public delegate void OnResourceChanged(int acornCount, int diamondCount);
    public event OnResourceChanged OnResourceChangedEvent;

    public delegate void OnGameInitialized();
    public event OnGameInitialized OnGameInitializedEvent;

    private void Awake()
    {
        InitializeSingleton();
    }

    private void InitializeSingleton()
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
        try
        {
            LoadGameData();
            InitializeWormSystem();
            InitializeUI();
            
            isGameInitialized = true;
            Debug.Log("[GameManager] 게임 초기화 완료");
            
            // 초기화 완료 이벤트 발생
            OnGameInitializedEvent?.Invoke();
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[GameManager] 초기화 중 오류: {ex.Message}");
        }
    }

    private void LoadGameData()
    {
        if (GameSaveManager.Instance?.currentSaveData == null)
        {
            Debug.LogError("[GameManager] 세이브 데이터를 로드할 수 없습니다.");
            return;
        }

        var saveData = GameSaveManager.Instance.currentSaveData;
        
        gameTime = saveData.totalPlayTime;
        acornCount = saveData.acornCount;
        diamondCount = saveData.diamondCount;

        Debug.Log($"[GameManager] 게임 데이터 로드 완료 - 도토리: {acornCount}, 다이아몬드: {diamondCount}");
    }

    private void InitializeWormSystem()
    {
        if (WormManager.Instance == null)
        {
            Debug.LogWarning("[GameManager] WormManager가 없습니다.");
            return;
        }

        var saveData = GameSaveManager.Instance.currentSaveData;
        
        if (saveData.wormList.Count == 0)
        {
            // 벌레가 없으면 WormManager에서 EggFound 팝업을 띄우도록 함
            // CreateNewWorm()은 사용자가 ConfirmEggFound 버튼을 눌렀을 때만 실행
            Debug.Log("[GameManager] 저장된 벌레가 없습니다. WormManager에서 EggFound 팝업을 처리합니다.");
        }
        else
        {
            WormManager.Instance.GetCurrentWorm();
            Debug.Log("[GameManager] 기존 벌레 로드");
        }
    }

    private void InitializeUI()
    {
        UpdateCurrentWormNameUI();
        StartCoroutine(UpdateTimeUIWhenReady()); // TopBarManager가 준비될 때까지 기다림
        
        if (mapManager != null)
        {
            var saveData = GameSaveManager.Instance.currentSaveData;
            mapManager.ChangeMapByIndex(saveData.selectedMapIndex);
        }

        ItemTabUI.Instance?.RefreshAllPreviews();
        TabManager.Instance?.OpenTab(2); // Home 탭으로 자동 전환
    }

    private IEnumerator UpdateTimeUIWhenReady()
    {
        yield return new WaitUntil(() => TopBarManager.Instance != null && TopBarManager.Instance.IsInitialized);
        UpdateTimeUI();
    }

    /// <summary>
    /// TopBarManager에서 호출할 수 있는 시간 업데이트 메서드
    /// </summary>
    public void ForceTimeUpdate()
    {
        OnGameTimeChangedEvent?.Invoke(hour, minute, AMPM);
    }



    private void Update()
    {
        if (isGameInitialized)
        {
            UpdateGameTime();
        }
    }

    public void UseAcorn()
    {
        if (acornCount > 0)
        {
            acornCount--;
            OnResourceChangedEvent?.Invoke(acornCount, diamondCount);
            Debug.Log($"[GameManager] 도토리 사용 - 남은 개수: {acornCount}");
        }
        else
        {
            PopupManager.Instance?.OpenPopup(PopupManager.PopupType.Option);
            Debug.LogWarning("[GameManager] 도토리가 부족합니다.");
        }
    }

    public void PickAcorn()
    {
        acornCount++;
        OnResourceChangedEvent?.Invoke(acornCount, diamondCount);
        Debug.Log($"[GameManager] 도토리 획득 - 총 개수: {acornCount}");
    }

    public void PickDiamond()
    {
        diamondCount++;
        OnResourceChangedEvent?.Invoke(acornCount, diamondCount);
        Debug.Log($"[GameManager] 다이아몬드 획득 - 총 개수: {diamondCount}");
    }

    public void EarnItem(int index)
    {
        // 아이템 획득 처리 (추후 구현)
        Debug.Log($"[GameManager] 아이템 획득 - 인덱스: {index}");
    }

    private void UpdateGameTime()
    {
        float deltaGameTime = Time.deltaTime * timeScale;
        gameTime += deltaGameTime;

        float currentSeconds = gameTime % SECONDS_IN_DAY;
        hour24 = (int)(currentSeconds / 3600) % 24;
        minute = (int)((currentSeconds % 3600) / 60);

        ConvertTo12HourFormat();

        if (minute != lastMinute)
        {
            lastMinute = minute;
            OnMinuteChanged();
        }
    }

    private void ConvertTo12HourFormat()
    {
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
    }

    private void OnMinuteChanged()
    {
        UpdateTimeUI();
        UpdateMapBackground();
        UpdateWormAge();
        
        OnGameTimeChangedEvent?.Invoke(hour, minute, AMPM);
        
        Debug.Log($"[GameTime] {hour}:{minute:D2} {AMPM}");
    }

    private void UpdateTimeUI()
    {
        TopBarManager.Instance?.UpdateTime(hour, minute, AMPM);
    }

    private void UpdateMapBackground()
    {
        if (mapManager == null) return;

        // 현재 시간에 따른 맵 페이즈 결정
        SpriteManager.MapPhase currentPhase = GetCurrentMapPhase();
        
        // 맵 페이즈가 변경되었으면 MapManager에 알림
        if (MapManager.Instance != null && MapManager.Instance.CurrentMapPhase != currentPhase)
        {
            MapManager.Instance.ChangeMapPhase(currentPhase);
        }
        else
        {
            // 페이즈가 변경되지 않았어도 배경 업데이트
            mapManager.UpdateMapBackground();
        }
    }

    private SpriteManager.MapPhase GetCurrentMapPhase()
    {
        if (IsDaytime(hour24))
            return SpriteManager.MapPhase.Day;
        else if (IsSunset(hour24))
            return SpriteManager.MapPhase.Sunset;
        else
            return SpriteManager.MapPhase.Night;
    }

    public void ForceUpdateMapBackground()
    {
        UpdateMapBackground();
    }

    private void UpdateWormAge()
    {
        if (WormManager.Instance == null) return;

        WormData currentWorm = WormManager.Instance.GetCurrentWorm();
        if (currentWorm != null)
        {
            // WormManager에서 이미 나이 증가를 처리하므로 여기서는 제거
            // currentWorm.age += 1; // 제거
            // WormManager.Instance.CheckEvolution(); // 제거
            Debug.Log($"[GameManager] 벌레 나이 증가 체크 - 나이: {currentWorm.age}, 단계: {currentWorm.lifeStage}");
        }
    }

    private bool IsDaytime(int hour)
    {
        return hour >= DAYTIME_START && hour < DAYTIME_END;
    }

    private bool IsSunset(int hour)
    {
        return (hour >= SUNSET_START_MORNING && hour < SUNSET_END_MORNING) || 
               (hour >= SUNSET_START_EVENING && hour < SUNSET_END_EVENING);
    }

    private void UpdateCurrentWormNameUI()
    {
        if (WormManager.Instance == null) return;

        WormData currentWorm = WormManager.Instance.GetCurrentWorm();
        if (currentWorm != null)
        {
            TopBarManager.Instance?.UpdateCurrentWormName(currentWorm.name);
        }
    }

    public void SaveGameData()
    {
        if (GameSaveManager.Instance?.currentSaveData == null) return;

        var saveData = GameSaveManager.Instance.currentSaveData;
        saveData.totalPlayTime = gameTime;
        saveData.acornCount = acornCount;
        saveData.diamondCount = diamondCount;

        GameSaveManager.Instance.SaveGame();
        Debug.Log("[GameManager] 게임 데이터 저장 완료");
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            SaveGameData();
        }
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
        {
            SaveGameData();
        }
    }

    private void OnApplicationQuit()
    {
        SaveGameData();
    }

    // 디버그용 메서드
    public void SetGameTime(float newTime)
    {
        gameTime = newTime;
        Debug.Log($"[GameManager] 게임 시간 설정: {newTime}");
    }

    public void SetTimeScale(float newScale)
    {
        timeScale = newScale;
        Debug.Log($"[GameManager] 시간 배율 설정: {newScale}");
    }
}
