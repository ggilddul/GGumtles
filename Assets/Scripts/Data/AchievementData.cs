using UnityEngine;

namespace GGumtles.Data
{
    [CreateAssetMenu(fileName = "AchievementData", menuName = "GGumtles/Achievement Data")]
    public class AchievementData : ScriptableObject
{
    [Header("기본 정보")]
    public string achievementId = "";
    public string achievementTitle = "";
    [TextArea(3, 5)]
    public string achievementDescription = "";
    
    [Header("조건 설정")]
    public AchievementConditionType conditionType = AchievementConditionType.AcornCount;
    public float targetValue = 1f;
    
    [Header("보상")]
    public RewardType rewardType = RewardType.Diamond;
    public int rewardAmount = 1; // 다이아몬드 1개로 통일
    
    [Header("시각적 요소")]
    public Sprite achievementIcon;
    public Color achievementColor = Color.white;
    
    [Header("상태")]
    public bool isUnlocked = false;        // 달성 여부
    
    [Header("달성 웜 정보 (Achievement2 전용)")]
    public int achieveWormId = -1;         // 달성 시 웜 ID (-1은 미설정)
    
    // 조건 타입 열거형
    public enum AchievementConditionType
    {
        AcornCount,         // 도토리 개수
        DiamondCount,       // 다이아몬드 개수
        WormAge,           // 벌레 나이
        PlayTime,          // 플레이 시간
        ItemCount,         // 아이템 개수
        WormCount,         // 벌레 개수
        Custom             // 커스텀 조건
    }
    
    // 보상 타입 열거형 (다이아몬드 1개로 통일)
    public enum RewardType
    {
        Diamond        // 다이아몬드 1개
    }
    }
}