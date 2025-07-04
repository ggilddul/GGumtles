using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class AchievementManager : MonoBehaviour
{
    public static AchievementManager Instance { get; private set; }

    [SerializeField] private List<AchievementData> achievementDefinitions; // SO 목록
    private Dictionary<string, AchievementStatus> achievementStates = new();

    public int Count => achievementDefinitions?.Count ?? 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeDefaultStates();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeDefaultStates()
    {
        achievementStates.Clear();
        foreach (var def in achievementDefinitions)
        {
            if (!achievementStates.ContainsKey(def.ach_id))
            {
                achievementStates.Add(def.ach_id, new AchievementStatus
                {
                    ach_id = def.ach_id,
                    isUnlocked = false
                });
            }
        }
    }

    // 저장된 데이터로 초기화
    public void Initialize(List<AchievementStatus> savedStatuses)
    {
        InitializeDefaultStates();

        if (savedStatuses != null)
        {
            foreach (var saved in savedStatuses)
            {
                if (achievementStates.ContainsKey(saved.ach_id))
                {
                    achievementStates[saved.ach_id].isUnlocked = saved.isUnlocked;
                }
            }
        }
    }

    // 업적 달성
    public void CheckAchievement(string ach_id)
    {
        if (!achievementStates.ContainsKey(ach_id)) return;

        var status = achievementStates[ach_id];
        if (!status.isUnlocked)
        {
            status.isUnlocked = true;
            int index = achievementDefinitions.FindIndex(a => a.ach_id == ach_id);
            if (index >= 0)
                PopupManager.Instance?.ShowAchievementPopup(index);
            GameSaveManager.Instance.SaveGame();
        }
    }

    public AchievementStatus GetStatusById(string ach_id)
    {
        if (achievementStates == null)
            return null;

        // Find 대신 TryGetValue 사용
        if (achievementStates.TryGetValue(ach_id, out var status))
            return status;

        return null;
    }

    public List<AchievementData> GetAllDefinitions() => achievementDefinitions;

    public List<AchievementStatus> GetAllStatuses() => achievementStates.Values.ToList();

    public bool IsUnlocked(string ach_id)
    {
        return achievementStates.TryGetValue(ach_id, out var status) && status.isUnlocked;
    }

    public List<string> GetUnlockedIds()
    {
        return achievementStates.Where(p => p.Value.isUnlocked).Select(p => p.Key).ToList();
    }
}
