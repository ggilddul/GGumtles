using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameSaveData
{
    // 옵션 enum 정의
    public enum AudioOption
    {
        Off = 0,
        Low = 1,
        High = 2
    }

    // 기본값 상수
    private const int DEFAULT_MAP_INDEX = 0;
    private const AudioOption DEFAULT_SFX_OPTION = AudioOption.High;
    private const AudioOption DEFAULT_BGM_OPTION = AudioOption.High;
    private const int DEFAULT_ACORN_COUNT = 0;
    private const int DEFAULT_DIAMOND_COUNT = 0;
    private const float DEFAULT_PLAY_TIME = 0f;

    // 게임 데이터
    [Header("플레이어 정보")]
    public int userId;
    public float totalPlayTime;

    [Header("게임 재화")]
    public int acornCount;
    public int diamondCount;

    [Header("게임 진행")]
    public List<WormData> wormList;
    public List<string> ownedItemIds;
    public List<string> unlockedAchIds;
    public int selectedMapIndex;

    [Header("설정")]
    public AudioOption sfxOption;
    public AudioOption bgmOption;

    /// <summary>
    /// 기본값으로 초기화된 새 인스턴스 생성
    /// </summary>
    public GameSaveData()
    {
        InitializeWithDefaults();
    }

    /// <summary>
    /// 기본값으로 초기화
    /// </summary>
    public void InitializeWithDefaults()
    {
        userId = 0;
        totalPlayTime = DEFAULT_PLAY_TIME;
        acornCount = DEFAULT_ACORN_COUNT;
        diamondCount = DEFAULT_DIAMOND_COUNT;
        
        wormList = new List<WormData>();
        ownedItemIds = new List<string>();
        unlockedAchIds = new List<string>();
        
        selectedMapIndex = DEFAULT_MAP_INDEX;
        sfxOption = DEFAULT_SFX_OPTION;
        bgmOption = DEFAULT_BGM_OPTION;
    }

    /// <summary>
    /// 데이터 유효성 검사
    /// </summary>
    public bool IsValid()
    {
        // 기본 값 검증
        if (acornCount < 0 || diamondCount < 0) return false;
        if (totalPlayTime < 0) return false;
        if (selectedMapIndex < 0) return false;

        // enum 값 검증
        if (!System.Enum.IsDefined(typeof(AudioOption), sfxOption)) return false;
        if (!System.Enum.IsDefined(typeof(AudioOption), bgmOption)) return false;

        // 리스트 null 체크
        if (wormList == null || ownedItemIds == null || unlockedAchIds == null) return false;

        return true;
    }

    /// <summary>
    /// 데이터 복사
    /// </summary>
    public GameSaveData Clone()
    {
        var clone = new GameSaveData
        {
            userId = this.userId,
            totalPlayTime = this.totalPlayTime,
            acornCount = this.acornCount,
            diamondCount = this.diamondCount,
            selectedMapIndex = this.selectedMapIndex,
            sfxOption = this.sfxOption,
            bgmOption = this.bgmOption
        };

        // 리스트 깊은 복사
        clone.wormList = new List<WormData>(this.wormList);
        clone.ownedItemIds = new List<string>(this.ownedItemIds);
        clone.unlockedAchIds = new List<string>(this.unlockedAchIds);

        return clone;
    }

    /// <summary>
    /// 다른 데이터로부터 복사
    /// </summary>
    public void CopyFrom(GameSaveData other)
    {
        if (other == null) return;

        userId = other.userId;
        totalPlayTime = other.totalPlayTime;
        acornCount = other.acornCount;
        diamondCount = other.diamondCount;
        selectedMapIndex = other.selectedMapIndex;
        sfxOption = other.sfxOption;
        bgmOption = other.bgmOption;

        wormList.Clear();
        wormList.AddRange(other.wormList);

        ownedItemIds.Clear();
        ownedItemIds.AddRange(other.ownedItemIds);

        unlockedAchIds.Clear();
        unlockedAchIds.AddRange(other.unlockedAchIds);
    }

    /// <summary>
    /// 통계 정보 반환
    /// </summary>
    public string GetStatistics()
    {
        return $"플레이 시간: {totalPlayTime:F1}초, " +
               $"도토리: {acornCount}개, " +
               $"다이아몬드: {diamondCount}개, " +
               $"벌레: {wormList.Count}마리, " +
               $"아이템: {ownedItemIds.Count}개, " +
               $"업적: {unlockedAchIds.Count}개";
    }

    /// <summary>
    /// 아이템 보유 여부 확인
    /// </summary>
    public bool HasItem(string itemId)
    {
        return ownedItemIds.Contains(itemId);
    }

    /// <summary>
    /// 아이템 추가
    /// </summary>
    public void AddItem(string itemId)
    {
        if (!string.IsNullOrEmpty(itemId) && !ownedItemIds.Contains(itemId))
        {
            ownedItemIds.Add(itemId);
        }
    }

    /// <summary>
    /// 업적 해금 여부 확인
    /// </summary>
    public bool IsAchievementUnlocked(string achievementId)
    {
        return unlockedAchIds.Contains(achievementId);
    }

    /// <summary>
    /// 업적 해금
    /// </summary>
    public void UnlockAchievement(string achievementId)
    {
        if (!string.IsNullOrEmpty(achievementId) && !unlockedAchIds.Contains(achievementId))
        {
            unlockedAchIds.Add(achievementId);
        }
    }
}
