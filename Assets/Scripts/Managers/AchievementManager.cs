using System.Collections.Generic;
using UnityEngine;
using GGumtles.Data;
using System.Linq;
using System;
using GGumtles.UI;
using GGumtles.Managers;

namespace GGumtles.Managers
{
    public class AchievementManager : MonoBehaviour
{
    public static AchievementManager Instance { get; private set; }

    [Header("업적 설정")]
    [SerializeField] private List<AchievementData> achievementDefinitions;
    
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
            LoadAchievementAssets();
            BuildAchievementMaps();
            isInitialized = true;
            
            Debug.Log($"[AchievementManager] 업적 시스템 초기화 완료 - 총 {Count}개 업적");
            
            // 해금 수 기반 메달 카운트 UI 동기화
            if (TopBarManager.Instance != null)
            {
                TopBarManager.Instance.UpdateMedalCount(GetUnlockedCount());
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[AchievementManager] 초기화 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// Resources 폴더에서 AchievementData 에셋들을 로드
    /// </summary>
    private void LoadAchievementAssets()
    {
        // Resources/AchievementData 폴더에서 모든 AchievementData 에셋 로드
        AchievementData[] loadedAchievements = Resources.LoadAll<AchievementData>("AchievementData");
        
        if (loadedAchievements != null && loadedAchievements.Length > 0)
        {
            achievementDefinitions = new List<AchievementData>(loadedAchievements);
            Debug.Log($"[AchievementManager] Resources에서 {loadedAchievements.Length}개 업적 에셋 로드 완료");
        }
        else
        {
            Debug.LogWarning("[AchievementManager] Resources/AchievementData 폴더에서 업적 에셋을 찾을 수 없습니다.");
            achievementDefinitions = new List<AchievementData>();
        }
        
        ValidateAchievementDefinitions();
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


    /// <summary>
    /// 저장된 데이터로 초기화
    /// </summary>
    public void Initialize(List<string> unlockedAchievementIds, List<AchievementWormData> achievementWormIds)
    {
        if (!isInitialized)
        {
            Debug.LogWarning("[AchievementManager] 아직 초기화되지 않았습니다.");
            return;
        }

        try
        {
            // 모든 업적을 기본 상태(해금되지 않음)로 초기화
            if (achievementDefinitions != null)
            {
                foreach (var achievement in achievementDefinitions)
                {
                    achievement.isUnlocked = false;
                    achievement.achieveWormId = -1;
                }
            }

            // 저장된 해금된 업적들 복원
            if (unlockedAchievementIds != null)
            {
                foreach (var achievementId in unlockedAchievementIds)
                {
                    var achievement = GetAchievementData(achievementId);
                    if (achievement != null)
                    {
                        achievement.isUnlocked = true;
                    }
                }
            }

            // 저장된 달성 웜 ID들 복원
            if (achievementWormIds != null)
            {
                foreach (var wormData in achievementWormIds)
                {
                    var achievement = GetAchievementData(wormData.achievementId);
                    if (achievement != null)
                    {
                        achievement.achieveWormId = wormData.wormId;
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
            var achievement = GetAchievementData(achievementId);
            if (achievement == null)
            {
                Debug.LogWarning($"[AchievementManager] 존재하지 않는 업적 ID: {achievementId}");
                return;
            }

            if (!achievement.isUnlocked)
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
        var achievementData = GetAchievementData(achievementId);
        if (achievementData == null)
        {
            Debug.LogWarning($"[AchievementManager] 업적 데이터를 찾을 수 없습니다: {achievementId}");
            return;
        }

        achievementData.isUnlocked = true; // 해금됨
        
        // 달성 웜 ID 설정 (Achievement2 전용)
        if (achievementData.achieveWormId == -1)
        {
            SetAchievementWormId(achievementData);
        }
        
        // 리워드 지급
        GiveReward(achievementData);
        
        // 이벤트 발생
        OnAchievementUnlockedEvent?.Invoke(achievementId, achievementData);

        // 해금 수 기반 메달 카운트 UI 동기화
        if (TopBarManager.Instance != null)
        {
            TopBarManager.Instance.UpdateMedalCount(GetUnlockedCount());
        }

        // 팝업 표시
        Debug.Log($"[AchievementManager] 업적 해금 알림: {achievementData.achievementTitle}");
        
        // 자동 저장
        if (GameSaveManager.Instance != null)
        {
            GameSaveManager.Instance.SaveGame();
        }
        
        Debug.Log($"[AchievementManager] 업적 해금: {achievementData.achievementTitle}");
    }

    /// <summary>
    /// 업적 리워드 지급
    /// </summary>
    private void GiveReward(AchievementData achievementData)
    {
        try
        {
            switch (achievementData.rewardType)
            {
                case AchievementData.RewardType.Diamond:
                    if (GameManager.Instance != null)
                    {
                        // 다이아몬드 지급
                        for (int i = 0; i < achievementData.rewardAmount; i++)
                        {
                            GameManager.Instance.PickDiamond();
                        }
                        Debug.Log($"[AchievementManager] 업적 리워드 지급: 다이아몬드 {achievementData.rewardAmount}개");
                    }
                    else
                    {
                        Debug.LogWarning("[AchievementManager] GameManager가 없어 리워드를 지급할 수 없습니다.");
                    }
                    break;
                default:
                    Debug.LogWarning($"[AchievementManager] 알 수 없는 리워드 타입: {achievementData.rewardType}");
                    break;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[AchievementManager] 리워드 지급 중 오류: {ex.Message}");
        }
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
    public List<AchievementWormData> GetAchievementWormIds()
    {
        var wormIds = new List<AchievementWormData>();
        
        try
        {
            if (achievementDefinitions != null)
            {
                foreach (var achievement in achievementDefinitions)
                {
                    if (achievement.isUnlocked && achievement.achieveWormId >= 0)
                    {
                        wormIds.Add(new AchievementWormData(achievement.achievementId, achievement.achieveWormId));
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
    /// 업적 진행도 업데이트
    /// </summary>
    public void UpdateProgress(string achievementId, float progress)
    {
        if (!isInitialized || string.IsNullOrEmpty(achievementId)) return;

        OnAchievementProgressChangedEvent?.Invoke(achievementId, progress);
    }

    /// <summary>
    /// 업적 해금 여부 조회
    /// </summary>
    public bool IsUnlocked(string achievementId)
    {
        if (!isInitialized || string.IsNullOrEmpty(achievementId))
            return false;

        var achievement = GetAchievementData(achievementId);
        return achievement?.isUnlocked ?? false;
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
    /// 해금된 업적 ID 목록 반환
    /// </summary>
    public List<string> GetUnlockedIds()
    {
        if (achievementDefinitions == null) return new List<string>();
        
        return achievementDefinitions
            .Where(achievement => achievement.isUnlocked)
            .Select(achievement => achievement.achievementId)
            .ToList();
    }

    /// <summary>
    /// 해금된 업적 개수 반환
    /// </summary>
    public int GetUnlockedCount()
    {
        if (achievementDefinitions == null) return 0;
        
        return achievementDefinitions.Count(achievement => achievement.isUnlocked);
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
    /// 조건에 따른 업적 체크 (동적)
    /// </summary>
    public void CheckAchievementByCondition(string conditionType, object value)
    {
        if (!isInitialized) return;

        switch (conditionType.ToLower())
        {
            case "acorn_count":
                CheckAchievementsByType(AchievementData.AchievementConditionType.AcornCount, (int)value);
                break;
            case "diamond_count":
                CheckAchievementsByType(AchievementData.AchievementConditionType.DiamondCount, (int)value);
                break;
            case "worm_age":
                CheckAchievementsByType(AchievementData.AchievementConditionType.WormAge, (int)value);
                break;
            case "play_time":
                CheckAchievementsByType(AchievementData.AchievementConditionType.PlayTime, (float)value);
                break;
            case "item_count":
                CheckAchievementsByType(AchievementData.AchievementConditionType.ItemCount, (int)value);
                break;
            case "worm_count":
                CheckAchievementsByType(AchievementData.AchievementConditionType.WormCount, (int)value);
                break;
            default:
                Debug.LogWarning($"[AchievementManager] 알 수 없는 조건 타입: {conditionType}");
                break;
        }
    }

    /// <summary>
    /// 조건 타입별 업적 체크 (동적)
    /// </summary>
    private void CheckAchievementsByType(AchievementData.AchievementConditionType conditionType, float currentValue)
    {
        if (achievementDefinitions == null) return;

        try
        {
            var matchingAchievements = achievementDefinitions
                .Where(a => a.conditionType == conditionType)
                .Where(a => !a.isUnlocked && currentValue >= a.targetValue)
                .ToList();

            foreach (var achievement in matchingAchievements)
            {
                CheckAchievement(achievement.achievementId);
                Debug.Log($"[AchievementManager] 조건 달성: {achievement.achievementTitle} (현재값: {currentValue}, 목표값: {achievement.targetValue})");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[AchievementManager] 조건별 업적 체크 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 디버그용: 모든 업적 해금
    /// </summary>
    [ContextMenu("모든 업적 해금")]
    public void UnlockAllAchievements()
    {
        if (!isInitialized) return;

        if (achievementDefinitions != null)
        {
            foreach (var achievement in achievementDefinitions)
            {
                if (!achievement.isUnlocked)
                {
                    achievement.isUnlocked = true;
                    SetAchievementWormId(achievement);
                }
            }
        }

        if (GameSaveManager.Instance != null)
        {
            GameSaveManager.Instance.SaveGame();
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

        if (achievementDefinitions != null)
        {
            foreach (var achievement in achievementDefinitions)
            {
                achievement.isUnlocked = false;
                achievement.achieveWormId = -1;
            }
        }

        if (GameSaveManager.Instance != null)
        {
            GameSaveManager.Instance.SaveGame();
        }

        Debug.Log("[AchievementManager] 모든 업적이 리셋되었습니다.");
    }
    }
}
