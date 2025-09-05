using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;
using System.Collections;

public class GameSaveManager : MonoBehaviour
{
    public static GameSaveManager Instance { get; private set; }

    public GameSaveData currentSaveData;
    private string saveFilePath;
    private string backupFilePath;
    private const int MAX_BACKUP_COUNT = 3;

    public delegate void OnGameSaveDataLoaded();
    public event OnGameSaveDataLoaded OnGameSaveDataLoadedEvent;

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
            saveFilePath = Path.Combine(Application.persistentDataPath, "gamesave.json");
            backupFilePath = Path.Combine(Application.persistentDataPath, "gamesave_backup.json");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 저장 데이터 초기화 및 로드
    /// </summary>
    public void Initialize()
    {
        LoadGame();
        OnGameSaveDataLoadedEvent?.Invoke();
    }

    /// <summary>
    /// 저장 데이터 로드 또는 신규 생성
    /// </summary>
    private void LoadGame()
    {
        try
        {
            if (File.Exists(saveFilePath))
            {
                string json = File.ReadAllText(saveFilePath);
                currentSaveData = JsonUtility.FromJson<GameSaveData>(json);
                
                // 데이터 검증
                if (ValidateSaveData(currentSaveData))
                {
                    Debug.Log("[GameSaveManager] 기존 저장 데이터 로드 완료");
                }
                else
                {
                    Debug.LogWarning("[GameSaveManager] 저장 데이터 검증 실패. 백업에서 복원 시도");
                    if (!TryLoadFromBackup())
                    {
                        CreateNewSaveData();
                    }
                }
            }
            else
            {
                Debug.Log("[GameSaveManager] 저장 데이터 없음. 새로 생성합니다.");
                CreateNewSaveData();
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[GameSaveManager] 로드 중 오류: {ex.Message}");
            if (!TryLoadFromBackup())
            {
                CreateNewSaveData();
            }
        }

        InitializeAchievementSystem();
    }

    /// <summary>
    /// 저장 데이터 유효성 검사
    /// </summary>
    private bool ValidateSaveData(GameSaveData data)
    {
        if (data == null) return false;
        
        // 기본 데이터 검증
        if (data.acornCount < 0 || data.diamondCount < 0) return false;
        if (data.selectedMapIndex < 0) return false;
        if (!System.Enum.IsDefined(typeof(GameSaveData.AudioOption), data.sfxOption)) return false;
        if (!System.Enum.IsDefined(typeof(GameSaveData.AudioOption), data.bgmOption)) return false;
        
        // 리스트 null 체크
        if (data.wormList == null || data.ownedItemIds == null || data.unlockedAchIds == null) return false;
        
        return true;
    }

    /// <summary>
    /// 백업에서 데이터 복원 시도
    /// </summary>
    private bool TryLoadFromBackup()
    {
        try
        {
            if (File.Exists(backupFilePath))
            {
                string json = File.ReadAllText(backupFilePath);
                currentSaveData = JsonUtility.FromJson<GameSaveData>(json);
                
                if (ValidateSaveData(currentSaveData))
                {
                    Debug.Log("[GameSaveManager] 백업에서 데이터 복원 성공");
                    return true;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[GameSaveManager] 백업 복원 실패: {ex.Message}");
        }
        
        return false;
    }

    /// <summary>
    /// 새로운 저장 데이터 생성
    /// </summary>
    private void CreateNewSaveData()
    {
        currentSaveData = new GameSaveData();
        InitializeNewSaveData();
    }

    /// <summary>
    /// 업적 시스템 초기화
    /// </summary>
    private void InitializeAchievementSystem()
    {
        if (AchievementManager.Instance != null)
        {
            // AchievementManager가 초기화되지 않은 경우 먼저 초기화
            if (!AchievementManager.Instance.IsInitialized)
            {
                AchievementManager.Instance.Initialize();
            }
            
            if (currentSaveData.unlockedAchIds != null)
            {
                AchievementManager.Instance.Initialize(currentSaveData.unlockedAchIds);
                
                // 달성 웜 ID 로드
                if (currentSaveData.achievementWormIds != null)
                {
                    LoadAchievementWormIds();
                }
            }
        }
        else
        {
            Debug.LogWarning("[GameSaveManager] AchievementManager 인스턴스가 없습니다.");
        }
    }

    /// <summary>
    /// 달성 웜 ID 로드
    /// </summary>
    private void LoadAchievementWormIds()
    {
        try
        {
            var wormIdsDict = new Dictionary<string, int>();
            foreach (var wormData in currentSaveData.achievementWormIds)
            {
                if (!string.IsNullOrEmpty(wormData.achievementId))
                {
                    wormIdsDict[wormData.achievementId] = wormData.wormId;
                }
            }
            
            AchievementManager.Instance.SetAchievementWormIds(wormIdsDict);
            Debug.Log($"[GameSaveManager] 달성 웜 ID 로드 완료: {currentSaveData.achievementWormIds.Count}개");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[GameSaveManager] 달성 웜 ID 로드 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 신규 저장 데이터 기본값 설정
    /// </summary>
    private void InitializeNewSaveData()
    {
        currentSaveData.totalPlayTime = 0f;
        currentSaveData.acornCount = 0;
        currentSaveData.diamondCount = 0;
        currentSaveData.wormList = new List<WormData>();
        currentSaveData.ownedItemIds = new List<string>();
        currentSaveData.selectedMapIndex = 0;

        // 업적 아이디 리스트 초기화 (빈 리스트)
        currentSaveData.unlockedAchIds = new List<string>();
        currentSaveData.achievementWormIds = new List<AchievementWormData>();

        currentSaveData.sfxOption = GameSaveData.AudioOption.High;
        currentSaveData.bgmOption = GameSaveData.AudioOption.High;

        // 초기 아이템 추가
        if (ItemManager.Instance != null)
        {
            ItemManager.Instance.AddItem("100", 1, false);
            ItemManager.Instance.AddItem("200", 1, false);
            ItemManager.Instance.AddItem("300", 1, false);
        }

        SaveGame();
    }

    /// <summary>
    /// 현재 저장 데이터를 파일로 저장
    /// </summary>
    public void SaveGame()
    {
        if (currentSaveData == null)
        {
            Debug.LogWarning("[GameSaveManager] 저장 실패: currentSaveData가 null입니다.");
            return;
        }

        // 업적 상태 갱신
        if (AchievementManager.Instance != null)
        {
            currentSaveData.unlockedAchIds = GetUnlockedAchIds();
            currentSaveData.achievementWormIds = GetAchievementWormIds();
        }

        StartCoroutine(SaveGameAsync());
    }

    /// <summary>
    /// 비동기 저장 처리
    /// </summary>
    private IEnumerator SaveGameAsync()
    {
        yield return new WaitForEndOfFrame(); // UI 블로킹 방지

        try
        {
            // 백업 생성
            if (File.Exists(saveFilePath))
            {
                File.Copy(saveFilePath, backupFilePath, true);
            }

            string json = JsonUtility.ToJson(currentSaveData, true);
            File.WriteAllText(saveFilePath, json);
            
            Debug.Log("[GameSaveManager] 저장 완료");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[GameSaveManager] 저장 중 오류: {ex.Message}");
            
            // 백업에서 복원 시도
            if (File.Exists(backupFilePath))
            {
                try
                {
                    File.Copy(backupFilePath, saveFilePath, true);
                    Debug.Log("[GameSaveManager] 백업에서 복원 완료");
                }
                catch (Exception restoreEx)
                {
                    Debug.LogError($"[GameSaveManager] 백업 복원 실패: {restoreEx.Message}");
                }
            }
        }
    }

    /// <summary>
    /// 현재 해금된 업적 ID 리스트 반환
    /// </summary>
    public List<string> GetUnlockedAchIds()
    {
        List<string> unlockedIds = new List<string>();

        if (AchievementManager.Instance == null)
        {
            Debug.LogWarning("[GameSaveManager] AchievementManager 인스턴스가 null입니다.");
            return unlockedIds;
        }

        var definitions = AchievementManager.Instance.GetAllDefinitions();
        foreach (var def in definitions)
        {
            if (AchievementManager.Instance.IsUnlocked(def.achievementId))
            {
                unlockedIds.Add(def.achievementId);
            }
        }

        return unlockedIds;
    }

    /// <summary>
    /// 달성 웜 ID 리스트 반환
    /// </summary>
    public List<AchievementWormData> GetAchievementWormIds()
    {
        List<AchievementWormData> wormIds = new List<AchievementWormData>();

        if (AchievementManager.Instance == null)
        {
            Debug.LogWarning("[GameSaveManager] AchievementManager 인스턴스가 null입니다.");
            return wormIds;
        }

        try
        {
            var wormIdsDict = AchievementManager.Instance.GetAchievementWormIds();
            foreach (var kvp in wormIdsDict)
            {
                wormIds.Add(new AchievementWormData(kvp.Key, kvp.Value));
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[GameSaveManager] 달성 웜 ID 가져오기 중 오류: {ex.Message}");
        }

        return wormIds;
    }

    /// <summary>
    /// 저장된 벌레 데이터 반환
    /// </summary>
    public List<WormData> GetWormData()
    {
        return currentSaveData?.wormList ?? new List<WormData>();
    }

    /// <summary>
    /// 저장 데이터 백업 생성
    /// </summary>
    public void CreateBackup()
    {
        try
        {
            if (File.Exists(saveFilePath))
            {
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string backupPath = Path.Combine(Application.persistentDataPath, $"gamesave_backup_{timestamp}.json");
                File.Copy(saveFilePath, backupPath, true);
                Debug.Log($"[GameSaveManager] 백업 생성 완료: {backupPath}");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[GameSaveManager] 백업 생성 실패: {ex.Message}");
        }
    }
}
