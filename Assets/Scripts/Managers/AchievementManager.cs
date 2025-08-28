using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class AchievementManager : MonoBehaviour
{
    public static AchievementManager Instance { get; private set; }

    [Header("업적 설정")]
    [SerializeField] private List<AchievementData> achievementDefinitions;
    
    private Dictionary<string, AchievementStatus> achievementStates = new();
    private Dictionary<string, AchievementData> achievementDefinitionsMap = new();
    private bool isInitialized = false;

    // 이벤트 정의
    public delegate void OnAchievementUnlocked(string achievementId, AchievementData achievementData);
    public event OnAchievementUnlocked OnAchievementUnlockedEvent;

    public delegate void OnAchievementProgressChanged(string achievementId, float progress);
    public event OnAchievementProgressChanged OnAchievementProgressChangedEvent;

    public int Count => achievementDefinitions?.Count ?? 0;
    public bool IsInitialized => isInitialized;

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

    private void Start()
    {
        InitializeAchievementSystem();
    }

    private void InitializeAchievementSystem()
    {
        try
        {
            ValidateAchievementDefinitions();
            BuildAchievementMaps();
            InitializeDefaultStates();
            isInitialized = true;
            
            Debug.Log($"[AchievementManager] 업적 시스템 초기화 완료 - 총 {Count}개 업적");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[AchievementManager] 초기화 중 오류: {ex.Message}");
        }
    }

    private void ValidateAchievementDefinitions()
    {
        if (achievementDefinitions == null || achievementDefinitions.Count == 0)
        {
            Debug.LogWarning("[AchievementManager] 업적 정의가 없습니다.");
            return;
        }

        var duplicateIds = achievementDefinitions
            .GroupBy(x => x.ach_id)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        if (duplicateIds.Count > 0)
        {
            Debug.LogError($"[AchievementManager] 중복된 업적 ID 발견: {string.Join(", ", duplicateIds)}");
        }

        foreach (var def in achievementDefinitions)
        {
            if (string.IsNullOrEmpty(def.ach_id))
            {
                Debug.LogError("[AchievementManager] 업적 ID가 비어있습니다.");
            }
            if (string.IsNullOrEmpty(def.ach_title))
            {
                Debug.LogWarning($"[AchievementManager] 업적 '{def.ach_id}'의 제목이 비어있습니다.");
            }
        }
    }

    private void BuildAchievementMaps()
    {
        achievementDefinitionsMap.Clear();
        
        if (achievementDefinitions != null)
        {
            foreach (var def in achievementDefinitions)
            {
                if (!string.IsNullOrEmpty(def.ach_id))
                {
                    achievementDefinitionsMap[def.ach_id] = def;
                }
            }
        }
    }

    private void InitializeDefaultStates()
    {
        achievementStates.Clear();
        
        if (achievementDefinitions != null)
        {
            foreach (var def in achievementDefinitions)
            {
                if (!string.IsNullOrEmpty(def.ach_id) && !achievementStates.ContainsKey(def.ach_id))
                {
                    achievementStates.Add(def.ach_id, new AchievementStatus
                    {
                        ach_id = def.ach_id,
                        isUnlocked = false
                    });
                }
            }
        }
    }

    /// <summary>
    /// 저장된 데이터로 초기화
    /// </summary>
    public void Initialize(List<AchievementStatus> savedStatuses)
    {
        if (!isInitialized)
        {
            Debug.LogWarning("[AchievementManager] 아직 초기화되지 않았습니다.");
            return;
        }

        try
        {
            InitializeDefaultStates();

            if (savedStatuses != null)
            {
                foreach (var saved in savedStatuses)
                {
                    if (!string.IsNullOrEmpty(saved.ach_id) && achievementStates.ContainsKey(saved.ach_id))
                    {
                        achievementStates[saved.ach_id].isUnlocked = saved.isUnlocked;
                    }
                }
            }

            Debug.Log($"[AchievementManager] 저장된 업적 상태 로드 완료 - 해금된 업적: {GetUnlockedCount()}개");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[AchievementManager] 초기화 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 업적 달성 체크
    /// </summary>
    public void CheckAchievement(string ach_id)
    {
        if (!isInitialized)
        {
            Debug.LogWarning("[AchievementManager] 아직 초기화되지 않았습니다.");
            return;
        }

        if (string.IsNullOrEmpty(ach_id))
        {
            Debug.LogWarning("[AchievementManager] 업적 ID가 비어있습니다.");
            return;
        }

        try
        {
            if (!achievementStates.ContainsKey(ach_id))
            {
                Debug.LogWarning($"[AchievementManager] 존재하지 않는 업적 ID: {ach_id}");
                return;
            }

            var status = achievementStates[ach_id];
            if (!status.isUnlocked)
            {
                UnlockAchievement(ach_id);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[AchievementManager] 업적 체크 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 업적 해금
    /// </summary>
    private void UnlockAchievement(string ach_id)
    {
        var status = achievementStates[ach_id];
        status.isUnlocked = true;

        var achievementData = GetAchievementData(ach_id);
        
        // 이벤트 발생
        OnAchievementUnlockedEvent?.Invoke(ach_id, achievementData);

        // 팝업 표시
        if (achievementData != null)
        {
            int index = achievementDefinitions.IndexOf(achievementData);
            if (index >= 0)
            {
                PopupManager.Instance?.ShowAchievementPopup(index);
            }
        }

        // 자동 저장
        if (GameSaveManager.Instance != null)
        {
            GameSaveManager.Instance.SaveGame();
        }

        Debug.Log($"[AchievementManager] 업적 해금: {achievementData?.ach_title ?? ach_id}");
    }

    /// <summary>
    /// 업적 진행도 업데이트
    /// </summary>
    public void UpdateProgress(string ach_id, float progress)
    {
        if (!isInitialized || string.IsNullOrEmpty(ach_id)) return;

        OnAchievementProgressChangedEvent?.Invoke(ach_id, progress);
    }

    /// <summary>
    /// 업적 상태 조회
    /// </summary>
    public AchievementStatus GetStatusById(string ach_id)
    {
        if (!isInitialized || string.IsNullOrEmpty(ach_id))
            return null;

        return achievementStates.TryGetValue(ach_id, out var status) ? status : null;
    }

    /// <summary>
    /// 업적 데이터 조회
    /// </summary>
    public AchievementData GetAchievementData(string ach_id)
    {
        if (string.IsNullOrEmpty(ach_id))
            return null;

        return achievementDefinitionsMap.TryGetValue(ach_id, out var data) ? data : null;
    }

    /// <summary>
    /// 모든 업적 정의 반환
    /// </summary>
    public List<AchievementData> GetAllDefinitions()
    {
        return achievementDefinitions ?? new List<AchievementData>();
    }

    /// <summary>
    /// 모든 업적 상태 반환
    /// </summary>
    public List<AchievementStatus> GetAllStatuses()
    {
        return achievementStates.Values.ToList();
    }

    /// <summary>
    /// 업적 해금 여부 확인
    /// </summary>
    public bool IsUnlocked(string ach_id)
    {
        if (!isInitialized || string.IsNullOrEmpty(ach_id))
            return false;

        return achievementStates.TryGetValue(ach_id, out var status) && status.isUnlocked;
    }

    /// <summary>
    /// 해금된 업적 ID 목록 반환
    /// </summary>
    public List<string> GetUnlockedIds()
    {
        return achievementStates
            .Where(p => p.Value.isUnlocked)
            .Select(p => p.Key)
            .ToList();
    }

    /// <summary>
    /// 해금된 업적 개수 반환
    /// </summary>
    public int GetUnlockedCount()
    {
        return achievementStates.Count(p => p.Value.isUnlocked);
    }

    /// <summary>
    /// 업적 해금률 반환
    /// </summary>
    public float GetUnlockRate()
    {
        if (Count == 0) return 0f;
        return (float)GetUnlockedCount() / Count * 100f;
    }

    /// <summary>
    /// 조건에 따른 업적 체크 (확장 가능)
    /// </summary>
    public void CheckAchievementByCondition(string conditionType, object value)
    {
        if (!isInitialized) return;

        switch (conditionType.ToLower())
        {
            case "acorn_count":
                CheckAcornCountAchievements((int)value);
                break;
            case "diamond_count":
                CheckDiamondCountAchievements((int)value);
                break;
            case "worm_age":
                CheckWormAgeAchievements((int)value);
                break;
            case "play_time":
                CheckPlayTimeAchievements((float)value);
                break;
            default:
                Debug.LogWarning($"[AchievementManager] 알 수 없는 조건 타입: {conditionType}");
                break;
        }
    }

    private void CheckAcornCountAchievements(int acornCount)
    {
        // 도토리 개수 관련 업적 체크
        if (acornCount >= 5) CheckAchievement("Ach_01");
        if (acornCount >= 10) CheckAchievement("Ach_02");
        if (acornCount >= 50) CheckAchievement("Ach_03");
    }

    private void CheckDiamondCountAchievements(int diamondCount)
    {
        // 다이아몬드 개수 관련 업적 체크
        if (diamondCount >= 1) CheckAchievement("Ach_04");
        if (diamondCount >= 5) CheckAchievement("Ach_05");
    }

    private void CheckWormAgeAchievements(int wormAge)
    {
        // 벌레 나이 관련 업적 체크
        if (wormAge >= 10) CheckAchievement("Ach_06");
        if (wormAge >= 30) CheckAchievement("Ach_07");
    }

    private void CheckPlayTimeAchievements(float playTime)
    {
        // 플레이 시간 관련 업적 체크
        if (playTime >= 3600f) CheckAchievement("Ach_08"); // 1시간
        if (playTime >= 86400f) CheckAchievement("Ach_09"); // 24시간
    }

    /// <summary>
    /// 디버그용: 모든 업적 해금
    /// </summary>
    [ContextMenu("모든 업적 해금")]
    public void UnlockAllAchievements()
    {
        if (!isInitialized) return;

        foreach (var achId in achievementStates.Keys.ToList())
        {
            if (!IsUnlocked(achId))
            {
                UnlockAchievement(achId);
            }
        }

        Debug.Log("[AchievementManager] 모든 업적이 해금되었습니다.");
    }

    /// <summary>
    /// 디버그용: 모든 업적 리셋
    /// </summary>
    [ContextMenu("모든 업적 리셋")]
    public void ResetAllAchievements()
    {
        if (!isInitialized) return;

        foreach (var status in achievementStates.Values)
        {
            status.isUnlocked = false;
        }

        if (GameSaveManager.Instance != null)
        {
            GameSaveManager.Instance.SaveGame();
        }

        Debug.Log("[AchievementManager] 모든 업적이 리셋되었습니다.");
    }
}
