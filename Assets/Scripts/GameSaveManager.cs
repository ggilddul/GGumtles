using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;

public class GameSaveManager : MonoBehaviour
{
    public static GameSaveManager Instance { get; private set; }

    public GameSaveData currentSaveData;
    private string saveFilePath;

    public delegate void OnGameSaveDataLoaded();
    public event OnGameSaveDataLoaded OnGameSaveDataLoadedEvent;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            saveFilePath = Path.Combine(Application.persistentDataPath, "gamesave.json");
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
        if (File.Exists(saveFilePath))
        {
            string json = File.ReadAllText(saveFilePath);
            currentSaveData = JsonUtility.FromJson<GameSaveData>(json);
            Debug.Log("[GameSaveManager] 기존 저장 데이터 로드 완료");
        }
        else
        {
            Debug.Log("[GameSaveManager] 저장 데이터 없음. 새로 생성합니다.");
            currentSaveData = new GameSaveData();
            InitializeNewSaveData();
        }

        // 업적 상태 초기화
        if (AchievementManager.Instance != null && currentSaveData.unlockedAchIds != null)
        {
            List<AchievementStatus> statusList = new List<AchievementStatus>();
            var definitions = AchievementManager.Instance.GetAllDefinitions();

            foreach (var def in definitions)
            {
                bool unlocked = currentSaveData.unlockedAchIds.Contains(def.ach_id);
                statusList.Add(new AchievementStatus
                {
                    ach_id = def.ach_id,
                    isUnlocked = unlocked
                });
            }
            AchievementManager.Instance.Initialize(statusList);
        }
        else
        {
            Debug.LogWarning("[GameSaveManager] AchievementManager 인스턴스가 없거나 unlockedAchIds가 null입니다.");
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

        currentSaveData.sfxOption = 2;
        currentSaveData.bgmOption = 2;

        // 초기 아이템 추가
        ItemManager.Instance.AddAndEquipItem("100");
        ItemManager.Instance.AddAndEquipItem("200");
        ItemManager.Instance.AddAndEquipItem("300");

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
        }

        string json = JsonUtility.ToJson(currentSaveData, true);

        try
        {
            File.WriteAllText(saveFilePath, json);
            Debug.Log("[GameSaveManager] 저장 완료");
        }
        catch (Exception ex)
        {
            Debug.LogError("[GameSaveManager] 저장 중 오류: " + ex.Message);
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
            if (AchievementManager.Instance.IsUnlocked(def.ach_id))
            {
                unlockedIds.Add(def.ach_id);
            }
        }

        return unlockedIds;
    }
}
