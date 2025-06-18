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
    /// 외부에서 호출하는 저장 데이터 초기화 함수
    /// </summary>
    public void Initialize()
    {
        LoadGame();
        OnGameSaveDataLoadedEvent?.Invoke();
    }

    /// <summary>
    /// 저장된 게임 데이터를 불러오거나, 새로 생성
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

        // 업적 초기화 - 여기서 변환 후 전달
        if (AchievementManager.Instance != null && currentSaveData.unlockedAchievements != null)
        {
            List<AchievementStatus> statusList = new List<AchievementStatus>();

            var definitions = AchievementManager.Instance.GetAllDefinitions();
            for (int i = 0; i < definitions.Count; i++)
            {
                bool unlocked = false;
                if (i < currentSaveData.unlockedAchievements.Count)
                    unlocked = currentSaveData.unlockedAchievements[i] != 0; // 0이면 잠김, 1이면 해금

                statusList.Add(new AchievementStatus
                {
                    id = definitions[i].id,
                    isUnlocked = unlocked
                });
            }

            AchievementManager.Instance.Initialize(statusList);
        }
        else
        {
            Debug.LogWarning("[GameSaveManager] AchievementManager 인스턴스가 없거나 unlockedAchievements가 null입니다.");
        }
    }


    /// <summary>
    /// 신규 저장 데이터에 기본값 세팅
    /// </summary>

    private void InitializeNewSaveData()
    {
        currentSaveData.totalPlayTime = 0f;
        currentSaveData.acornCount = 0;
        currentSaveData.diamondCount = 0;
        currentSaveData.wormList = new List<WormData>();
        currentSaveData.ownedItems = new List<ItemData>();
        currentSaveData.selectedMapIndex = 0;

        int achievementCount = AchievementManager.Instance?.Count ?? 0;
        currentSaveData.unlockedAchievements = new List<int>();
        for (int i = 0; i < achievementCount; i++)
            currentSaveData.unlockedAchievements.Add(0);

        currentSaveData.sfxOption = 2;
        currentSaveData.bgmOption = 2;

        // 아이템 추가 (초기 아이템 보장)
        ItemManager.Instance.AddItem("100");
        ItemManager.Instance.EquipItem("100");
        ItemManager.Instance.AddItem("200");
        ItemManager.Instance.EquipItem("100");
        ItemManager.Instance.AddItem("300");
        ItemManager.Instance.EquipItem("100");

        SaveGame();
    }

    /// <summary>
    /// 현재 데이터를 파일로 저장
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
            currentSaveData.unlockedAchievements = GetUnlockedAchievementIndexes();
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



    public List<int> GetUnlockedAchievementIndexes()
{
    List<int> unlockedIndexes = new List<int>();

    if (AchievementManager.Instance == null)
    {
        Debug.LogWarning("[GameSaveManager] AchievementManager 인스턴스가 null입니다.");
        return unlockedIndexes;
    }

    var definitions = AchievementManager.Instance.GetAllDefinitions();

    for (int i = 0; i < definitions.Count; i++)
    {
        string id = definitions[i].id;
        if (AchievementManager.Instance.IsUnlocked(id))
        {
            unlockedIndexes.Add(i);
        }
    }

    return unlockedIndexes;
}

}
