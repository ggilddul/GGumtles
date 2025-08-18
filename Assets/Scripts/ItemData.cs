using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(fileName = "ItemData", menuName = "GGumtles/Item Data")]
public class ItemData : ScriptableObject
{
    [Header("기본 정보")]
    public string itemId = "";
    public string itemName = "";
    [TextArea(3, 5)]
    public string description = "";
    public ItemType type = ItemType.Hat;
    public ItemRarity rarity = ItemRarity.Common;
    public ItemCategory category = ItemCategory.Accessory;

    [Header("시각적 정보")]
    public Sprite sprite;
    public Sprite iconSprite;
    public Vector2 positionOffset = Vector2.zero;
    public Vector2 scale = Vector2.one;
    public Color tintColor = Color.white;
    public bool isVisible = true;
    public bool useCustomAnimation = false;
    public RuntimeAnimatorController customAnimator;

    [Header("게임플레이 설정")]
    public bool isStackable = false;
    public int maxStackSize = 99;
    public bool isConsumable = false;
    public bool isEquippable = true;
    public bool isTradeable = true;
    public bool isDroppable = true;
    public float durability = -1f; // -1 = 무제한
    public int levelRequirement = 0;

    [Header("경제 설정")]
    public int basePrice = 0;
    public CurrencyType currencyType = CurrencyType.Acorn;
    public bool isSellable = true;
    public float sellPriceMultiplier = 0.5f;

    [Header("효과 설정")]
    public List<ItemEffect> itemEffects = new List<ItemEffect>();
    public List<ItemRequirement> requirements = new List<ItemRequirement>();
    public List<string> tags = new List<string>();

    [Header("메타데이터")]
    public string creator = "";
    public string version = "1.0";
    public System.DateTime creationDate;
    public System.DateTime lastModifiedDate;
    public string notes = "";

    // 열거형 정의
    public enum ItemType
    {
        Hat,
        Face,
        Costume,
        Accessory,
        Consumable,
        Material,
        Tool,
        Special
    }

    public enum ItemRarity
    {
        Common = 0,
        Uncommon = 1,
        Rare = 2,
        Epic = 3,
        Legendary = 4,
        Mythic = 5
    }

    public enum ItemCategory
    {
        Accessory,
        Clothing,
        Consumable,
        Material,
        Tool,
        Collectible,
        Event,
        Special
    }

    public enum CurrencyType
    {
        Acorn,
        Diamond,
        RealMoney,
        EventCurrency
    }

    // 아이템 효과 클래스
    [System.Serializable]
    public class ItemEffect
    {
        public enum EffectType
        {
            StatBoost,
            SpecialAbility,
            VisualEffect,
            AudioEffect,
            GameplayModifier
        }

        public enum StatType
        {
            Health,
            Speed,
            JumpPower,
            AttackPower,
            Defense,
            Luck,
            Experience,
            Custom
        }

        public EffectType effectType = EffectType.StatBoost;
        public StatType statType = StatType.Custom;
        public string effectName = "";
        public string customStatName = "";
        public float effectValue = 0f;
        public float duration = -1f; // -1 = 영구
        public bool isPercentage = false;
        public bool isStackable = true;
        public int maxStacks = 1;
        public AnimationCurve effectCurve = AnimationCurve.Linear(0, 1, 1, 1);
        
        [Header("조건부 효과")]
        public bool hasCondition = false;
        public string conditionType = "";
        public float conditionValue = 0f;
        
        [Header("시각적 효과")]
        public bool hasVisualEffect = false;
        public GameObject visualEffectPrefab;
        public Color effectColor = Color.white;
        public float effectIntensity = 1f;

        public ItemEffect()
        {
            effectCurve = AnimationCurve.Linear(0, 1, 1, 1);
        }

        public ItemEffect(EffectType type, StatType stat, string name, float value)
        {
            effectType = type;
            statType = stat;
            effectName = name;
            effectValue = value;
            effectCurve = AnimationCurve.Linear(0, 1, 1, 1);
        }

        public string GetEffectDescription()
        {
            string desc = effectName;
            
            if (effectType == EffectType.StatBoost)
            {
                string statName = statType == StatType.Custom ? customStatName : statType.ToString();
                string valueText = isPercentage ? $"{effectValue}%" : effectValue.ToString();
                desc = $"{statName} +{valueText}";
            }
            
            if (duration > 0)
            {
                desc += $" ({duration}초)";
            }
            
            return desc;
        }

        public bool IsValid()
        {
            if (string.IsNullOrEmpty(effectName))
                return false;
                
            if (effectType == EffectType.StatBoost && statType == StatType.Custom && string.IsNullOrEmpty(customStatName))
                return false;
                
            return true;
        }
    }

    // 아이템 요구사항 클래스
    [System.Serializable]
    public class ItemRequirement
    {
        public enum RequirementType
        {
            Level,
            Item,
            Achievement,
            Currency,
            Custom
        }

        public RequirementType requirementType = RequirementType.Level;
        public string requirementId = "";
        public float requirementValue = 0f;
        public string customRequirementName = "";
        public bool isOptional = false;

        public ItemRequirement()
        {
        }

        public ItemRequirement(RequirementType type, string id, float value)
        {
            requirementType = type;
            requirementId = id;
            requirementValue = value;
        }

        public string GetRequirementDescription()
        {
            switch (requirementType)
            {
                case RequirementType.Level:
                    return $"레벨 {requirementValue} 필요";
                case RequirementType.Item:
                    return $"{requirementId} 아이템 필요";
                case RequirementType.Achievement:
                    return $"{requirementId} 업적 필요";
                case RequirementType.Currency:
                    return $"{requirementValue} {requirementId} 필요";
                case RequirementType.Custom:
                    return customRequirementName;
                default:
                    return "알 수 없는 요구사항";
            }
        }

        public bool IsValid()
        {
            if (string.IsNullOrEmpty(requirementId) && requirementType != RequirementType.Level)
                return false;
                
            if (requirementType == RequirementType.Custom && string.IsNullOrEmpty(customRequirementName))
                return false;
                
            return true;
        }
    }

    // 프로퍼티
    public string DisplayName => string.IsNullOrEmpty(itemName) ? itemId : itemName;
    public bool IsValid => ValidateData();
    public bool HasEffects => itemEffects != null && itemEffects.Count > 0;
    public bool HasRequirements => requirements != null && requirements.Count > 0;
    public int SellPrice => Mathf.RoundToInt(basePrice * sellPriceMultiplier);
    public Color RarityColor => GetRarityColor();
    public string RarityText => GetRarityText();

    private void OnValidate()
    {
        // 데이터 유효성 검사
        ValidateData();
        
        // 마지막 수정 날짜 업데이트
        lastModifiedDate = System.DateTime.Now;
    }

    private void OnEnable()
    {
        // 생성 날짜 설정 (최초 한 번만)
        if (creationDate == System.DateTime.MinValue)
        {
            creationDate = System.DateTime.Now;
            lastModifiedDate = creationDate;
        }
    }

    /// <summary>
    /// 데이터 유효성 검사
    /// </summary>
    public bool ValidateData()
    {
        var errors = new List<string>();

        // 기본 정보 검사
        if (string.IsNullOrEmpty(itemId))
            errors.Add("아이템 ID가 비어있습니다.");

        if (string.IsNullOrEmpty(itemName))
            errors.Add("아이템 이름이 비어있습니다.");

        if (string.IsNullOrEmpty(description))
            errors.Add("아이템 설명이 비어있습니다.");

        // 시각적 정보 검사
        if (sprite == null)
            errors.Add("아이템 스프라이트가 설정되지 않았습니다.");

        // 게임플레이 설정 검사
        if (maxStackSize <= 0)
            errors.Add("최대 스택 크기는 0보다 커야 합니다.");

        if (levelRequirement < 0)
            errors.Add("레벨 요구사항은 0 이상이어야 합니다.");

        if (basePrice < 0)
            errors.Add("기본 가격은 0 이상이어야 합니다.");

        if (sellPriceMultiplier < 0 || sellPriceMultiplier > 1)
            errors.Add("판매 가격 배율은 0~1 사이여야 합니다.");

        // 효과 검사
        if (itemEffects != null)
        {
            for (int i = 0; i < itemEffects.Count; i++)
            {
                if (!itemEffects[i].IsValid())
                {
                    errors.Add($"효과 {i + 1}이 유효하지 않습니다.");
                }
            }
        }

        // 요구사항 검사
        if (requirements != null)
        {
            for (int i = 0; i < requirements.Count; i++)
            {
                if (!requirements[i].IsValid())
                {
                    errors.Add($"요구사항 {i + 1}이 유효하지 않습니다.");
                }
            }
        }

        // 에러가 있으면 로그 출력
        if (errors.Count > 0)
        {
            Debug.LogError($"[ItemData] {itemId} 유효성 검사 실패:\n{string.Join("\n", errors)}");
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
            case ItemRarity.Common: return Color.white;
            case ItemRarity.Uncommon: return Color.green;
            case ItemRarity.Rare: return Color.blue;
            case ItemRarity.Epic: return Color.magenta;
            case ItemRarity.Legendary: return Color.yellow;
            case ItemRarity.Mythic: return Color.red;
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
            case ItemRarity.Common: return "일반";
            case ItemRarity.Uncommon: return "고급";
            case ItemRarity.Rare: return "희귀";
            case ItemRarity.Epic: return "영웅";
            case ItemRarity.Legendary: return "전설";
            case ItemRarity.Mythic: return "신화";
            default: return "알 수 없음";
        }
    }

    /// <summary>
    /// 아이템 설명 생성
    /// </summary>
    public string GetFullDescription()
    {
        var desc = new System.Text.StringBuilder();
        
        // 기본 설명
        if (!string.IsNullOrEmpty(description))
        {
            desc.AppendLine(description);
            desc.AppendLine();
        }

        // 효과 설명
        if (HasEffects)
        {
            desc.AppendLine("[효과]");
            foreach (var effect in itemEffects)
            {
                desc.AppendLine($"• {effect.GetEffectDescription()}");
            }
            desc.AppendLine();
        }

        // 요구사항 설명
        if (HasRequirements)
        {
            desc.AppendLine("[요구사항]");
            foreach (var req in requirements)
            {
                desc.AppendLine($"• {req.GetRequirementDescription()}");
            }
            desc.AppendLine();
        }

        // 기타 정보
        if (levelRequirement > 0)
        {
            desc.AppendLine($"레벨 {levelRequirement} 필요");
        }

        if (durability > 0)
        {
            desc.AppendLine($"내구도: {durability}");
        }

        if (basePrice > 0)
        {
            desc.AppendLine($"가격: {basePrice} {currencyType}");
        }

        return desc.ToString().TrimEnd();
    }

    /// <summary>
    /// 특정 효과가 있는지 확인
    /// </summary>
    public bool HasEffect(ItemEffect.EffectType effectType)
    {
        return itemEffects?.Any(e => e.effectType == effectType) ?? false;
    }

    /// <summary>
    /// 특정 스탯 효과가 있는지 확인
    /// </summary>
    public bool HasStatEffect(ItemEffect.StatType statType)
    {
        return itemEffects?.Any(e => e.effectType == ItemEffect.EffectType.StatBoost && e.statType == statType) ?? false;
    }

    /// <summary>
    /// 특정 효과 가져오기
    /// </summary>
    public List<ItemEffect> GetEffects(ItemEffect.EffectType effectType)
    {
        return itemEffects?.Where(e => e.effectType == effectType).ToList() ?? new List<ItemEffect>();
    }

    /// <summary>
    /// 특정 스탯 효과 가져오기
    /// </summary>
    public List<ItemEffect> GetStatEffects(ItemEffect.StatType statType)
    {
        return itemEffects?.Where(e => e.effectType == ItemEffect.EffectType.StatBoost && e.statType == statType).ToList() ?? new List<ItemEffect>();
    }

    /// <summary>
    /// 특정 태그가 있는지 확인
    /// </summary>
    public bool HasTag(string tag)
    {
        return tags?.Contains(tag) ?? false;
    }

    /// <summary>
    /// 태그 추가
    /// </summary>
    public void AddTag(string tag)
    {
        if (tags == null)
            tags = new List<string>();
            
        if (!tags.Contains(tag))
            tags.Add(tag);
    }

    /// <summary>
    /// 태그 제거
    /// </summary>
    public void RemoveTag(string tag)
    {
        tags?.Remove(tag);
    }

    /// <summary>
    /// 아이템 복사본 생성
    /// </summary>
    public ItemData Clone()
    {
        var clone = CreateInstance<ItemData>();
        
        // 기본 정보 복사
        clone.itemId = this.itemId;
        clone.itemName = this.itemName;
        clone.description = this.description;
        clone.type = this.type;
        clone.rarity = this.rarity;
        clone.category = this.category;

        // 시각적 정보 복사
        clone.sprite = this.sprite;
        clone.iconSprite = this.iconSprite;
        clone.positionOffset = this.positionOffset;
        clone.scale = this.scale;
        clone.tintColor = this.tintColor;
        clone.isVisible = this.isVisible;
        clone.useCustomAnimation = this.useCustomAnimation;
        clone.customAnimator = this.customAnimator;

        // 게임플레이 설정 복사
        clone.isStackable = this.isStackable;
        clone.maxStackSize = this.maxStackSize;
        clone.isConsumable = this.isConsumable;
        clone.isEquippable = this.isEquippable;
        clone.isTradeable = this.isTradeable;
        clone.isDroppable = this.isDroppable;
        clone.durability = this.durability;
        clone.levelRequirement = this.levelRequirement;

        // 경제 설정 복사
        clone.basePrice = this.basePrice;
        clone.currencyType = this.currencyType;
        clone.isSellable = this.isSellable;
        clone.sellPriceMultiplier = this.sellPriceMultiplier;

        // 효과 복사
        clone.itemEffects = new List<ItemEffect>();
        foreach (var effect in this.itemEffects)
        {
            clone.itemEffects.Add(new ItemEffect
            {
                effectType = effect.effectType,
                statType = effect.statType,
                effectName = effect.effectName,
                customStatName = effect.customStatName,
                effectValue = effect.effectValue,
                duration = effect.duration,
                isPercentage = effect.isPercentage,
                isStackable = effect.isStackable,
                maxStacks = effect.maxStacks,
                effectCurve = effect.effectCurve,
                hasCondition = effect.hasCondition,
                conditionType = effect.conditionType,
                conditionValue = effect.conditionValue,
                hasVisualEffect = effect.hasVisualEffect,
                visualEffectPrefab = effect.visualEffectPrefab,
                effectColor = effect.effectColor,
                effectIntensity = effect.effectIntensity
            });
        }

        // 요구사항 복사
        clone.requirements = new List<ItemRequirement>();
        foreach (var req in this.requirements)
        {
            clone.requirements.Add(new ItemRequirement
            {
                requirementType = req.requirementType,
                requirementId = req.requirementId,
                requirementValue = req.requirementValue,
                customRequirementName = req.customRequirementName,
                isOptional = req.isOptional
            });
        }

        // 태그 복사
        clone.tags = new List<string>(this.tags);

        // 메타데이터 복사
        clone.creator = this.creator;
        clone.version = this.version;
        clone.creationDate = this.creationDate;
        clone.lastModifiedDate = System.DateTime.Now;
        clone.notes = this.notes;

        return clone;
    }

    /// <summary>
    /// 디버그 정보 반환
    /// </summary>
    public string GetDebugInfo()
    {
        var info = new System.Text.StringBuilder();
        info.AppendLine($"[아이템 정보]");
        info.AppendLine($"ID: {itemId}");
        info.AppendLine($"이름: {itemName}");
        info.AppendLine($"타입: {type}");
        info.AppendLine($"희귀도: {rarity} ({GetRarityText()})");
        info.AppendLine($"카테고리: {category}");
        info.AppendLine($"스택 가능: {isStackable}");
        info.AppendLine($"소모성: {isConsumable}");
        info.AppendLine($"장착 가능: {isEquippable}");
        info.AppendLine($"가격: {basePrice} {currencyType}");
        info.AppendLine($"효과 개수: {itemEffects?.Count ?? 0}");
        info.AppendLine($"요구사항 개수: {requirements?.Count ?? 0}");
        info.AppendLine($"태그 개수: {tags?.Count ?? 0}");
        info.AppendLine($"유효성: {IsValid}");

        return info.ToString();
    }
}
