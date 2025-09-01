using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using GGumtles.UI;

public class AchievementManager : MonoBehaviour
{
    public static AchievementManager Instance { get; private set; }

    [Header("업적 설정")]
    [SerializeField] private List<AchievementData> achievementDefinitions;
    
    private Dictionary<string, bool> achievementStates = new(); // 단순화: 해금 여부만 저장
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

    public void Initialize()
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
            .GroupBy(x => x.achievementId)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        if (duplicateIds.Count > 0)
        {
            Debug.LogError($"[AchievementManager] 중복된 업적 ID 발견: {string.Join(", ", duplicateIds)}");
        }

        foreach (var def in achievementDefinitions)
        {
            if (string.IsNullOrEmpty(def.achievementId))
            {
                Debug.LogError("[AchievementManager] 업적 ID가 비어있습니다.");
            }
            if (string.IsNullOrEmpty(def.achievementTitle))
            {
                Debug.LogWarning($"[AchievementManager] 업적 '{def.achievementId}'의 제목이 비어있습니다.");
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
                if (!string.IsNullOrEmpty(def.achievementId))
                {
                    achievementDefinitionsMap[def.achievementId] = def;
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
                if (!string.IsNullOrEmpty(def.achievementId) && !achievementStates.ContainsKey(def.achievementId))
                {
                    achievementStates.Add(def.achievementId, false); // 기본값: 해금되지 않음
                }
            }
        }
    }

    /// <summary>
    /// 저장된 데이터로 초기화 (단순화)
    /// </summary>
    public void Initialize(List<string> unlockedAchievementIds)
    {
        if (!isInitialized)
        {
            Debug.LogWarning("[AchievementManager] 아직 초기화되지 않았습니다.");
            return;
        }

        try
        {
            InitializeDefaultStates();

            if (unlockedAchievementIds != null)
            {
                foreach (var achievementId in unlockedAchievementIds)
                {
                    if (!string.IsNullOrEmpty(achievementId) && achievementStates.ContainsKey(achievementId))
                    {
                        achievementStates[achievementId] = true; // 해금됨
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
    public void CheckAchievement(string achievementId)
    {
        if (!isInitialized)
        {
            Debug.LogWarning("[AchievementManager] 아직 초기화되지 않았습니다.");
            return;
        }

        if (string.IsNullOrEmpty(achievementId))
        {
            Debug.LogWarning("[AchievementManager] 업적 ID가 비어있습니다.");
            return;
        }

        try
        {
            if (!achievementStates.ContainsKey(achievementId))
            {
                Debug.LogWarning($"[AchievementManager] 존재하지 않는 업적 ID: {achievementId}");
                return;
            }

            bool isUnlocked = achievementStates[achievementId];
            if (!isUnlocked)
            {
                UnlockAchievement(achievementId);
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
    private void UnlockAchievement(string achievementId)
    {
        achievementStates[achievementId] = true; // 해금됨

        var achievementData = GetAchievementData(achievementId);
        
        // 달성 웜 ID 설정 (Achievement2 전용)
        if (achievementData != null && achievementData.achieveWormId == -1)
        {
            SetAchievementWormId(achievementData);
        }
        
        // 이벤트 발생
        OnAchievementUnlockedEvent?.Invoke(achievementId, achievementData);

        // 팝업 표시
        if (achievementData != null)
        {
            int index = achievementDefinitions.IndexOf(achievementData);
            if (index >= 0)
            {
                // UIPrefabManager 삭제로 인해 AchieveTabUI에서 직접 처리
                Debug.Log($"[AchievementManager] 업적 해금 알림: {achievementData.achievementTitle}");
            }
        }

        // 자동 저장
        if (GameSaveManager.Instance != null)
        {
            GameSaveManager.Instance.SaveGame();
        }

        Debug.Log($"[AchievementManager] 업적 해금: {achievementData?.achievementTitle ?? achievementId}");
    }

    /// <summary>
    /// 업적 달성 시 웜 ID 설정 (Achievement2 전용)
    /// </summary>
    private void SetAchievementWormId(AchievementData achievementData)
    {
        try
        {
            if (WormManager.Instance == null)
            {
                Debug.LogWarning("[AchievementManager] WormManager 인스턴스가 없습니다.");
                return;
            }

            // 현재 활성 웜을 달성 웜으로 설정
            var currentWorm = WormManager.Instance.CurrentWorm;
            if (currentWorm != null)
            {
                achievementData.achieveWormId = currentWorm.wormId;
                Debug.Log($"[AchievementManager] 업적 달성 웜 ID 설정: {achievementData.achievementTitle} -> 웜 ID: {currentWorm.wormId} ({currentWorm.name})");
            }
            else
            {
                Debug.LogWarning("[AchievementManager] 현재 활성 웜이 없어 달성 웜 ID를 설정할 수 없습니다.");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[AchievementManager] 달성 웜 ID 설정 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 달성 웜 ID 목록 가져오기 (저장용)
    /// </summary>
    public Dictionary<string, int> GetAchievementWormIds()
    {
        var wormIds = new Dictionary<string, int>();
        
        try
        {
            foreach (var kvp in achievementStates)
            {
                string achievementId = kvp.Key;
                bool isUnlocked = kvp.Value;
                
                if (isUnlocked)
                {
                    var achievementData = GetAchievementData(achievementId);
                    if (achievementData != null && achievementData.achieveWormId >= 0)
                    {
                        wormIds[achievementId] = achievementData.achieveWormId;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[AchievementManager] 달성 웜 ID 목록 가져오기 중 오류: {ex.Message}");
        }
        
        return wormIds;
    }

    /// <summary>
    /// 달성 웜 ID 설정 (로드용)
    /// </summary>
    public void SetAchievementWormIds(Dictionary<string, int> wormIds)
    {
        try
        {
            if (wormIds == null) return;
            
            foreach (var kvp in wormIds)
            {
                string achievementId = kvp.Key;
                int wormId = kvp.Value;
                
                var achievementData = GetAchievementData(achievementId);
                if (achievementData != null)
                {
                    achievementData.achieveWormId = wormId;
                    Debug.Log($"[AchievementManager] 달성 웜 ID 로드: {achievementData.achievementTitle} -> 웜 ID: {wormId}");
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[AchievementManager] 달성 웜 ID 설정 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 업적 진행도 업데이트
    /// </summary>
    public void UpdateProgress(string achievementId, float progress)
    {
        if (!isInitialized || string.IsNullOrEmpty(achievementId)) return;

        OnAchievementProgressChangedEvent?.Invoke(achievementId, progress);
    }

    /// <summary>
    /// 업적 해금 여부 조회 (단순화)
    /// </summary>
    public bool IsUnlocked(string achievementId)
    {
        if (!isInitialized || string.IsNullOrEmpty(achievementId))
            return false;

        return achievementStates.TryGetValue(achievementId, out var isUnlocked) ? isUnlocked : false;
    }

    /// <summary>
    /// 업적 데이터 조회
    /// </summary>
    public AchievementData GetAchievementData(string achievementId)
    {
        if (string.IsNullOrEmpty(achievementId))
            return null;

        return achievementDefinitionsMap.TryGetValue(achievementId, out var data) ? data : null;
    }

    /// <summary>
    /// 모든 업적 정의 반환
    /// </summary>
    public List<AchievementData> GetAllDefinitions()
    {
        return achievementDefinitions ?? new List<AchievementData>();
    }

    /// <summary>
    /// 해금된 업적 ID 목록 반환 (단순화)
    /// </summary>
    public List<string> GetUnlockedIds()
    {
        return achievementStates
            .Where(p => p.Value)
            .Select(p => p.Key)
            .ToList();
    }

    /// <summary>
    /// 해금된 업적 개수 반환
    /// </summary>
    public int GetUnlockedCount()
    {
        return achievementStates.Count(p => p.Value);
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

        foreach (var achId in achievementStates.Keys.ToList())
        {
            achievementStates[achId] = false;
        }

        if (GameSaveManager.Instance != null)
        {
            GameSaveManager.Instance.SaveGame();
        }

        Debug.Log("[AchievementManager] 모든 업적이 리셋되었습니다.");
    }
}
