using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class WormData
{
    [Header("기본 정보")]
    public int wormId = -1;
    public string name = "";
    public string description = "";
    public int generation = 0;           // 세대
    public WormGender gender = WormGender.Unknown;
    public WormPersonality personality = WormPersonality.Normal;
    public WormRarity rarity = WormRarity.Common;

    [Header("생명 정보")]
    public int age = 0;                  // 현재 나이 (분 단위)
    public int lifespan = 0;             // 전체 수명 (분 단위, RandomManager에서 설정)
    public int lifeStage = 0;            // 생애 주기 (0=알, 1=L1, ..., 5=성체, 6=사망 등)
    public WormHealthStatus healthStatus = WormHealthStatus.Healthy;
    public float health = 100f;          // 현재 체력 (0-100)
    public float maxHealth = 100f;       // 최대 체력
    public float hunger = 0f;            // 배고픔 (0-100)
    public float energy = 100f;          // 에너지 (0-100)

    [Header("장착 아이템")]
    public string hatItemId = "";
    public string faceItemId = "";
    public string costumeItemId = "";

    [Header("관계 정보")]
    public int parentId1 = -1;           // 부모 1 ID
    public int parentId2 = -1;           // 부모 2 ID
    public List<int> childrenIds = new List<int>(); // 자식들 ID

    [Header("상태 정보")]
    public WormState currentState = WormState.Idle;

    public bool isAlive = true;
    public bool isActive = true;
    public bool isSleeping = false;
    public bool isEating = false;
    public bool isPlaying = false;
    public float lastActionTime = 0f;    // 마지막 행동 시간
    public float lastFedTime = 0f;       // 마지막 먹이 준 시간

    [Header("통계 정보")]
    public WormStatistics statistics = new WormStatistics();
    public List<WormAchievement> achievements = new List<WormAchievement>();
    public List<string> tags = new List<string>();

    [Header("메타데이터")]
    public System.DateTime birthDate;
    public System.DateTime lastUpdateTime;
    public string creator = "";
    public string version = "1.0";

    // 열거형 정의
    public enum WormGender
    {
        Unknown,
        Male,
        Female,
        Hermaphrodite
    }

    public enum WormPersonality
    {
        Normal,
        Shy,
        Active,
        Lazy,
        Curious,
        Aggressive,
        Friendly,
        Independent
    }

    public enum WormRarity
    {
        Common = 0,    // 일반
        Uncommon = 1,  // 고급
        Rare = 2,      // 희귀
        Legendary = 3  // 전설
    }

    public enum WormHealthStatus
    {
        Healthy,
        Sick,
        Injured,
        Starving,
        Exhausted,
        Critical
    }

    public enum WormState
    {
        Idle,
        Sleeping,
        Eating,
        Playing,
        Exploring,
        Socializing,
        Training,
        Resting,
        Sick,
        Dead
    }



    // 통계 클래스
    [System.Serializable]
    public class WormStatistics
    {
        public int totalPlayTime = 0;        // 총 놀이 시간 (분)
        public int totalSleepTime = 0;       // 총 수면 시간 (분)
        public int totalEatCount = 0;        // 총 먹이 횟수
        public int totalPlayCount = 0;       // 총 놀이 횟수
        public int totalSocialCount = 0;     // 총 사회화 횟수
        public int totalChildrenCount = 0;   // 총 자식 수
        public float totalDistanceMoved = 0f; // 총 이동 거리
        public int totalAchievements = 0;    // 총 업적 수
        public System.DateTime firstPlayDate;
        public System.DateTime lastPlayDate;

        public WormStatistics()
        {
            firstPlayDate = System.DateTime.Now;
            lastPlayDate = System.DateTime.Now;
        }

        public void UpdateLastPlayDate()
        {
            lastPlayDate = System.DateTime.Now;
        }

        public int GetTotalActiveTime()
        {
            return totalPlayTime + totalSocialCount;
        }

        public float GetAverageActivityPerDay()
        {
            if (firstPlayDate == System.DateTime.MinValue) return 0f;
            
            var daysSinceFirst = (System.DateTime.Now - firstPlayDate).TotalDays;
            if (daysSinceFirst <= 0) return 0f;
            
            return GetTotalActiveTime() / (float)daysSinceFirst;
        }
    }

    // 업적 클래스
    [System.Serializable]
    public class WormAchievement
    {
        public string achievementId = "";
        public string achievementName = "";
        public string description = "";
        public System.DateTime unlockDate;
        public bool isUnlocked = false;
        public int progress = 0;
        public int targetProgress = 1;

        public WormAchievement(string id, string name, string desc)
        {
            achievementId = id;
            achievementName = name;
            description = desc;
            unlockDate = System.DateTime.MinValue;
        }

        public void Unlock()
        {
            if (!isUnlocked)
            {
                isUnlocked = true;
                unlockDate = System.DateTime.Now;
                progress = targetProgress;
            }
        }

        public void UpdateProgress(int newProgress)
        {
            progress = Mathf.Min(newProgress, targetProgress);
            if (progress >= targetProgress && !isUnlocked)
            {
                Unlock();
            }
        }

        public float GetProgressPercentage()
        {
            return (float)progress / targetProgress;
        }
    }

    // 프로퍼티
    public string DisplayName => string.IsNullOrEmpty(name) ? $"벌레 #{wormId}" : name;
    public bool IsValid => ValidateData();
    public bool IsAlive => isAlive && health > 0;
    public bool IsDead => !isAlive || health <= 0;
    public bool IsAdult => lifeStage >= 5;
    public bool IsBaby => lifeStage <= 2;
    public bool IsTeenager => lifeStage >= 3 && lifeStage <= 4;
    public float AgeInHours => age / 60f;
    public float AgeInDays => age / 1440f;
    public float LifeProgress => (float)age / lifespan;
    public float HealthPercentage => health / maxHealth;
    public float HungerPercentage => hunger / 100f;
    public float EnergyPercentage => energy / 100f;
    public Color RarityColor => GetRarityColor();
    public string RarityText => GetRarityText();
    public int TotalChildren => childrenIds?.Count ?? 0;
    public bool HasParents => parentId1 >= 0 || parentId2 >= 0;
    public bool HasChildren => TotalChildren > 0;

    // 생성자
    public WormData()
    {
        birthDate = System.DateTime.Now;
        lastUpdateTime = System.DateTime.Now;
        lifespan = RandomManager.GenerateWormLifespan(); // 랜덤 수명 생성
        statistics = new WormStatistics();
        achievements = new List<WormAchievement>();
        tags = new List<string>();
    }

    public WormData(int id, string wormName)
    {
        wormId = id;
        name = wormName;
        birthDate = System.DateTime.Now;
        lastUpdateTime = System.DateTime.Now;
        lifespan = RandomManager.GenerateWormLifespan(); // 랜덤 수명 생성
        statistics = new WormStatistics();
        achievements = new List<WormAchievement>();
        tags = new List<string>();
    }

    /// <summary>
    /// 데이터 유효성 검사
    /// </summary>
    public bool ValidateData()
    {
        var errors = new List<string>();

        // 기본 정보 검사
        if (wormId < 0)
            errors.Add("벌레 ID가 유효하지 않습니다.");

        if (string.IsNullOrEmpty(name))
            errors.Add("벌레 이름이 비어있습니다.");

        if (generation < 0)
            errors.Add("세대가 음수입니다.");

        // 생명 정보 검사
        if (age < 0)
            errors.Add("나이가 음수입니다.");

        if (lifespan <= 0)
            errors.Add("수명이 0 이하입니다.");

        if (lifeStage < 0 || lifeStage > 6)
            errors.Add("생애 주기가 유효하지 않습니다.");

        if (health < 0 || health > maxHealth)
            errors.Add("체력이 유효하지 않습니다.");

        if (hunger < 0 || hunger > 100)
            errors.Add("배고픔이 유효하지 않습니다.");

        if (energy < 0 || energy > 100)
            errors.Add("에너지가 유효하지 않습니다.");

        // 에러가 있으면 로그 출력
        if (errors.Count > 0)
        {
            Debug.LogError($"[WormData] {wormId} 유효성 검사 실패:\n{string.Join("\n", errors)}");
            return false;
        }

        return true;
    }

    /// <summary>
    /// 희귀도에 따른 색상 반환
    /// </summary>
    public Color GetRarityColor()
    {
        switch (rarity)
        {
            case WormRarity.Common: return Color.white;
            case WormRarity.Uncommon: return Color.green;
            case WormRarity.Rare: return Color.blue;
            case WormRarity.Legendary: return Color.yellow;
            default: return Color.white;
        }
    }

    /// <summary>
    /// 희귀도 텍스트 반환
    /// </summary>
    public string GetRarityText()
    {
        switch (rarity)
        {
            case WormRarity.Common: return "일반";
            case WormRarity.Uncommon: return "고급";
            case WormRarity.Rare: return "희귀";
            case WormRarity.Legendary: return "전설";
            default: return "알 수 없음";
        }
    }

    /// <summary>
    /// 나이 증가
    /// </summary>
    public void AgeUp(int minutes = 1)
    {
        if (!IsAlive) return;

        age += minutes;
        lastUpdateTime = System.DateTime.Now;

        // 수명 초과 시 사망
        if (age >= lifespan)
        {
            Die("노화");
        }

        // 생애 주기 업데이트
        UpdateLifeStage();
    }

    /// <summary>
    /// 생애 주기 업데이트
    /// </summary>
    private void UpdateLifeStage()
    {
        int newLifeStage = CalculateLifeStage();
        if (newLifeStage != lifeStage)
        {
            lifeStage = newLifeStage;
            OnLifeStageChanged();
        }
    }

    /// <summary>
    /// 생애 주기 계산 (시간 기반 매직넘버)
    /// </summary>
    private int CalculateLifeStage()
    {
        int ageInMinutes = age;
        
        // 시간 기반 매직넘버로 생명주기 결정
        if (ageInMinutes < 20) return 0;           // 알 (0~20분)
        if (ageInMinutes < 120) return 1;          // L1 (20분~2시간)
        if (ageInMinutes < 360) return 2;          // L2 (2시간~6시간)
        if (ageInMinutes < 720) return 3;          // L3 (6시간~12시간)
        if (ageInMinutes < 1440) return 4;          // L4 (12시간~1일)
        if (ageInMinutes < lifespan * 0.8f) return 5; // 성체 (1일~수명의 80%)
        return 6;                                  // 노화 (수명의 80% 이후)
    }

    /// <summary>
    /// 생애 주기 변경 시 호출
    /// </summary>
    private void OnLifeStageChanged()
    {
        // 성장에 따른 체력 증가
        if (lifeStage > 0)
        {
            maxHealth += 10f;
            health = maxHealth; // 성장 시 체력 회복
        }
        
        Debug.Log($"[WormData] {DisplayName} 생애 주기 변경: {lifeStage}");
    }

    /// <summary>
    /// 체력 변경
    /// </summary>
    public void ChangeHealth(float amount)
    {
        if (!IsAlive) return;

        health = Mathf.Clamp(health + amount, 0, maxHealth);
        
        if (health <= 0)
        {
            Die("체력 부족");
        }
    }

    /// <summary>
    /// 배고픔 변경
    /// </summary>
    public void ChangeHunger(float amount)
    {
        hunger = Mathf.Clamp(hunger + amount, 0, 100);
        
        if (hunger >= 100)
        {
            ChangeHealth(-5f); // 배고픔으로 인한 체력 감소
        }
    }

    /// <summary>
    /// 에너지 변경
    /// </summary>
    public void ChangeEnergy(float amount)
    {
        energy = Mathf.Clamp(energy + amount, 0, 100);
    }

    /// <summary>
    /// 먹이 주기
    /// </summary>
    public void Feed()
    {
        if (!IsAlive) return;

        ChangeHunger(-30f); // 배고픔 감소
        lastFedTime = Time.time;
        statistics.totalEatCount++;
        
        Debug.Log($"[WormData] {DisplayName}에게 먹이를 주었습니다.");
    }

    /// <summary>
    /// 놀이하기
    /// </summary>
    public void Play()
    {
        if (!IsAlive) return;

        ChangeEnergy(-20f); // 에너지 소모
        statistics.totalPlayCount++;
        statistics.totalPlayTime += 10; // 10분 놀이
        
        Debug.Log($"[WormData] {DisplayName}와 놀이했습니다.");
    }

    /// <summary>
    /// 사망
    /// </summary>
    public void Die(string reason = "알 수 없음")
    {
        if (!IsAlive) return;

        isAlive = false;
        currentState = WormState.Dead;
        health = 0;
        
        Debug.Log($"[WormData] {DisplayName}이(가) 사망했습니다. (사유: {reason})");
    }

    /// <summary>
    /// 부모 설정
    /// </summary>
    public void SetParents(int parent1, int parent2)
    {
        parentId1 = parent1;
        parentId2 = parent2;
    }

    /// <summary>
    /// 자식 추가
    /// </summary>
    public void AddChild(int childId)
    {
        if (!childrenIds.Contains(childId))
        {
            childrenIds.Add(childId);
            statistics.totalChildrenCount++;
        }
    }

    /// <summary>
    /// 업적 추가
    /// </summary>
    public void AddAchievement(string achievementId, string name, string description)
    {
        var achievement = new WormAchievement(achievementId, name, description);
        achievements.Add(achievement);
        statistics.totalAchievements++;
    }

    /// <summary>
    /// 업적 해금
    /// </summary>
    public void UnlockAchievement(string achievementId)
    {
        var achievement = achievements.Find(a => a.achievementId == achievementId);
        if (achievement != null)
        {
            achievement.Unlock();
        }
    }

    /// <summary>
    /// 태그 추가
    /// </summary>
    public void AddTag(string tag)
    {
        if (!tags.Contains(tag))
        {
            tags.Add(tag);
        }
    }

    /// <summary>
    /// 태그 제거
    /// </summary>
    public void RemoveTag(string tag)
    {
        tags.Remove(tag);
    }

    /// <summary>
    /// 태그 확인
    /// </summary>
    public bool HasTag(string tag)
    {
        return tags.Contains(tag);
    }

    /// <summary>
    /// 벌레 정보 요약 반환
    /// </summary>
    public string GetSummary()
    {
        var summary = new System.Text.StringBuilder();
        summary.AppendLine($"[{DisplayName} 정보]");
        summary.AppendLine($"ID: {wormId}, 세대: {generation}");
        summary.AppendLine($"나이: {AgeInDays:F1}일 ({age}분)");
        summary.AppendLine($"생애 주기: {lifeStage}/6");
        summary.AppendLine($"체력: {health:F0}/{maxHealth:F0} ({HealthPercentage:P0})");
        summary.AppendLine($"배고픔: {hunger:F0}%");
        summary.AppendLine($"에너지: {energy:F0}%");
        summary.AppendLine($"상태: {currentState}");
        summary.AppendLine($"희귀도: {GetRarityText()}");
        summary.AppendLine($"자식: {TotalChildren}명");

        return summary.ToString();
    }

    /// <summary>
    /// 디버그 정보 반환
    /// </summary>
    public string GetDebugInfo()
    {
        var info = new System.Text.StringBuilder();
        info.AppendLine($"[벌레 디버그 정보]");
        info.AppendLine($"ID: {wormId}");
        info.AppendLine($"이름: {name}");
        info.AppendLine($"생존: {IsAlive}");
        info.AppendLine($"활성: {isActive}");
        info.AppendLine($"유효성: {IsValid}");
        info.AppendLine($"업적 수: {statistics.totalAchievements}");
        info.AppendLine($"태그 수: {tags.Count}");

        return info.ToString();
    }

    /// <summary>
    /// 벌레 복사본 생성
    /// </summary>
    public WormData Clone()
    {
        var clone = new WormData(wormId, name);
        
        // 기본 정보 복사
        clone.description = this.description;
        clone.generation = this.generation;
        clone.gender = this.gender;
        clone.personality = this.personality;
        clone.rarity = this.rarity;

        // 생명 정보 복사
        clone.age = this.age;
        clone.lifespan = this.lifespan; // 기존 수명 유지
        clone.lifeStage = this.lifeStage;
        clone.healthStatus = this.healthStatus;
        clone.health = this.health;
        clone.maxHealth = this.maxHealth;
        clone.hunger = this.hunger;
        clone.energy = this.energy;

        // 장착 아이템 복사
        clone.hatItemId = this.hatItemId;
        clone.faceItemId = this.faceItemId;
        clone.costumeItemId = this.costumeItemId;


        // 관계 정보 복사
        clone.parentId1 = this.parentId1;
        clone.parentId2 = this.parentId2;
        clone.childrenIds = new List<int>(this.childrenIds);


        // 상태 정보 복사
        clone.currentState = this.currentState;

        clone.isAlive = this.isAlive;
        clone.isActive = this.isActive;
        clone.isSleeping = this.isSleeping;
        clone.isEating = this.isEating;
        clone.isPlaying = this.isPlaying;
        clone.lastActionTime = this.lastActionTime;
        clone.lastFedTime = this.lastFedTime;


        // 통계 정보 복사
        clone.statistics = this.statistics;
        clone.achievements = new List<WormAchievement>(this.achievements);
        clone.tags = new List<string>(this.tags);

        // 메타데이터 복사
        clone.birthDate = this.birthDate;
        clone.lastUpdateTime = System.DateTime.Now;
        clone.creator = this.creator;
        clone.version = this.version;

        return clone;
    }
}
