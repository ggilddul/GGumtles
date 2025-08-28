using UnityEngine;

[CreateAssetMenu(fileName = "AchievementData", menuName = "GGumtles/Achievement Data")]
public class AchievementData : ScriptableObject
{
    [Header("기본 정보")]
    public string ach_id;                   // 고유 ID ("Ach_01_Acorn_Collector")
    public string ach_title;                // 제목 ("도토리 부자")
    public string ach_description;          // 조건 설명 ("도토리 5개를 모으세요!")
    
    [Header("카테고리")]
    public AchievementCategory category;    // 업적 카테고리
    
    [Header("조건 설정")]
    public AchievementConditionType conditionType;  // 조건 타입
    public float targetValue;               // 목표 값
    public string customCondition;          // 커스텀 조건 (확장용)
    
    [Header("진행도")]
    public bool hasProgress;                // 진행도 표시 여부
    public float currentProgress;           // 현재 진행도
    public string progressFormat = "{0}/{1}"; // 진행도 표시 형식
    
    [Header("보상")]
    public bool hasReward;                  // 보상 여부
    public RewardType rewardType;           // 보상 타입
    public int rewardAmount;                // 보상 수량
    public string rewardItemId;             // 보상 아이템 ID
    
    [Header("시각적 요소")]
    public Sprite achievementIcon;          // 업적 아이콘
    public Color achievementColor = Color.white; // 업적 색상
    public bool isSecret;                   // 비밀 업적 여부
    
    [Header("메타데이터")]
    public int sortOrder;                   // 정렬 순서
    public string[] tags;                   // 태그 (검색용)
    
    // 업적 카테고리 열거형
    public enum AchievementCategory
    {
        Collection,     // 수집
        Progress,       // 진행
        Challenge,      // 도전
        Social,         // 소셜
        Hidden,         // 숨겨진
        Special         // 특별
    }
    
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
    
    // 보상 타입 열거형
    public enum RewardType
    {
        None,           // 보상 없음
        Acorn,          // 도토리
        Diamond,        // 다이아몬드
        Item,           // 아이템
        Worm,           // 벌레
        Custom          // 커스텀
    }
    
    /// <summary>
    /// 업적 조건 검증
    /// </summary>
    public bool CheckCondition(object currentValue)
    {
        if (conditionType == AchievementConditionType.Custom)
        {
            return CheckCustomCondition(currentValue);
        }
        
        if (currentValue is float floatValue)
        {
            return floatValue >= targetValue;
        }
        else if (currentValue is int intValue)
        {
            return intValue >= targetValue;
        }
        
        return false;
    }
    
    /// <summary>
    /// 커스텀 조건 검증
    /// </summary>
    private bool CheckCustomCondition(object currentValue)
    {
        // 여기에 커스텀 조건 로직 구현
        // 예: 특정 아이템 조합, 특정 시퀀스 등
        return false;
    }
    
    /// <summary>
    /// 진행도 계산
    /// </summary>
    public float CalculateProgress(object currentValue)
    {
        if (!hasProgress) return 0f;
        
        if (currentValue is float floatValue)
        {
            return Mathf.Clamp01(floatValue / targetValue);
        }
        else if (currentValue is int intValue)
        {
            return Mathf.Clamp01((float)intValue / targetValue);
        }
        
        return 0f;
    }
    
    /// <summary>
    /// 진행도 텍스트 생성
    /// </summary>
    public string GetProgressText(object currentValue)
    {
        if (!hasProgress) return "";
        
        float progress = CalculateProgress(currentValue);
        string current = currentValue?.ToString() ?? "0";
        string target = targetValue.ToString();
        
        return string.Format(progressFormat, current, target);
    }
    
    /// <summary>
    /// 보상 정보 가져오기
    /// </summary>
    public string GetRewardDescription()
    {
        if (!hasReward) return "보상 없음";
        
        switch (rewardType)
        {
            case RewardType.Acorn:
                return $"도토리 {rewardAmount}개";
            case RewardType.Diamond:
                return $"다이아몬드 {rewardAmount}개";
            case RewardType.Item:
                return $"아이템: {rewardItemId}";
            case RewardType.Worm:
                return $"특별한 벌레";
            case RewardType.Custom:
                return "특별한 보상";
            default:
                return "보상 없음";
        }
    }
    
    /// <summary>
    /// 보상 지급
    /// </summary>
    public void GrantReward()
    {
        if (!hasReward) return;
        
        switch (rewardType)
        {
            case RewardType.Acorn:
                if (GameManager.Instance != null)
                {
                    for (int i = 0; i < rewardAmount; i++)
                    {
                        GameManager.Instance.PickAcorn();
                    }
                }
                break;
                
            case RewardType.Diamond:
                if (GameManager.Instance != null)
                {
                    for (int i = 0; i < rewardAmount; i++)
                    {
                        GameManager.Instance.PickDiamond();
                    }
                }
                break;
                
            case RewardType.Item:
                if (ItemManager.Instance != null && !string.IsNullOrEmpty(rewardItemId))
                {
                    ItemManager.Instance.AddItem(rewardItemId, 1, true);
                }
                break;
                
            case RewardType.Worm:
                if (WormManager.Instance != null)
                {
                    // 특별한 벌레 생성 로직
                    WormManager.Instance.CreateNewWorm(1);
                }
                break;
        }
        
        Debug.Log($"[AchievementData] 보상 지급 완료: {GetRewardDescription()}");
    }
    
    /// <summary>
    /// 업적 정보 검증
    /// </summary>
    public bool IsValid()
    {
        if (string.IsNullOrEmpty(ach_id))
        {
            Debug.LogError($"[AchievementData] 업적 ID가 비어있습니다: {name}");
            return false;
        }
        
        if (string.IsNullOrEmpty(ach_title))
        {
            Debug.LogWarning($"[AchievementData] 업적 제목이 비어있습니다: {ach_id}");
        }
        
        if (targetValue <= 0 && conditionType != AchievementConditionType.Custom)
        {
            Debug.LogError($"[AchievementData] 목표 값이 0 이하입니다: {ach_id}");
            return false;
        }
        
        return true;
    }
    
    /// <summary>
    /// 디버그 정보 출력
    /// </summary>
    public string GetDebugInfo()
    {
        return $"ID: {ach_id}, 제목: {ach_title}, 카테고리: {category}, " +
               $"조건: {conditionType} >= {targetValue}, 진행도: {hasProgress}, " +
               $"보상: {rewardType} x{rewardAmount}";
    }
    
    /// <summary>
    /// 태그 검색
    /// </summary>
    public bool HasTag(string tag)
    {
        if (tags == null || tags.Length == 0) return false;
        return System.Array.Exists(tags, t => t.Equals(tag, System.StringComparison.OrdinalIgnoreCase));
    }
}

[System.Serializable]
public class AchievementStatus
{
    public string ach_id;
    public bool isUnlocked;
    public float progress;                  // 진행도 (0.0 ~ 1.0)
    public System.DateTime unlockTime;      // 해금 시간
    public bool rewardClaimed;              // 보상 수령 여부
    
    public AchievementStatus()
    {
        ach_id = "";
        isUnlocked = false;
        progress = 0f;
        unlockTime = System.DateTime.MinValue;
        rewardClaimed = false;
    }
    
    public AchievementStatus(string id)
    {
        ach_id = id;
        isUnlocked = false;
        progress = 0f;
        unlockTime = System.DateTime.MinValue;
        rewardClaimed = false;
    }
    
    /// <summary>
    /// 진행도 업데이트
    /// </summary>
    public void UpdateProgress(float newProgress)
    {
        progress = Mathf.Clamp01(newProgress);
    }
    
    /// <summary>
    /// 업적 해금
    /// </summary>
    public void Unlock()
    {
        isUnlocked = true;
        progress = 1f;
        unlockTime = System.DateTime.Now;
    }
    
    /// <summary>
    /// 보상 수령
    /// </summary>
    public void ClaimReward()
    {
        rewardClaimed = true;
    }
    
    /// <summary>
    /// 해금 후 경과 시간
    /// </summary>
    public System.TimeSpan GetTimeSinceUnlock()
    {
        if (!isUnlocked) return System.TimeSpan.Zero;
        return System.DateTime.Now - unlockTime;
    }
}