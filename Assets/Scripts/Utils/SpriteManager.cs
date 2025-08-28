using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

public class SpriteManager : MonoBehaviour
{
    public static SpriteManager Instance { get; private set; }

    [Header("LifeStage Sprites")]
    [SerializeField] private Sprite[] lifeStageSprites;
    
    [Header("Map Sprites")]
    [SerializeField] private List<MapSpriteSet> mapSpriteSets;
    
    [Header("UI Sprites")]
    [SerializeField] private List<UISpriteSet> uiSpriteSets;
    
    [Header("Item Sprites")]
    [SerializeField] private List<ItemSpriteSet> itemSpriteSets;
    
    [Header("Achievement Sprites")]
    [SerializeField] private List<AchievementSpriteSet> achievementSpriteSets;
    
    [Header("설정")]
    [SerializeField] private bool enableCaching = true;
    // [SerializeField] private bool enableAsyncLoading = true;  // 미사용
    [SerializeField] private int maxCacheSize = 100;
    [SerializeField] private float cacheCleanupInterval = 300f; // 5분

    // 스프라이트 타입 열거형
    public enum SpriteType
    {
        LifeStage,
        Map,
        UI,
        Item,
        Achievement,
        Custom
    }

    // 맵 타입 열거형
    public enum MapType 
    { 
        TypeA, 
        TypeB,
        TypeC,
        TypeD
    }

    // 맵 페이즈 열거형
    public enum MapPhase 
    { 
        Day, 
        Sunset, 
        Night 
    }

    // UI 타입 열거형
    public enum UIType
    {
        Button,
        Icon,
        Background,
        Frame,
        Custom
    }

    // 아이템 타입은 ItemData.ItemType 사용

    // 스프라이트 세트 클래스들
    [System.Serializable]
    public class MapSpriteSet
    {
        public MapType mapType;
        public Sprite daySprite;
        public Sprite sunsetSprite;
        public Sprite nightSprite;
        public string description;
    }

    [System.Serializable]
    public class UISpriteSet
    {
        public UIType uiType;
        public string spriteName;
        public Sprite sprite;
        public string description;
    }

    [System.Serializable]
    public class ItemSpriteSet
    {
        public ItemData.ItemType itemType;
        public string itemId;
        public Sprite sprite;
        public Sprite icon;
        public string description;
    }

    [System.Serializable]
    public class AchievementSpriteSet
    {
        public string achievementId;
        public Sprite unlockedSprite;
        public Sprite lockedSprite;
        public Sprite icon;
        public string description;
    }

    // 캐시 시스템
    private Dictionary<string, Sprite> spriteCache = new Dictionary<string, Sprite>();
    private Dictionary<string, float> cacheTimestamps = new Dictionary<string, float>();
    private Queue<string> cacheAccessOrder = new Queue<string>();
    
    // 맵 스프라이트 테이블
    private Dictionary<MapType, Dictionary<MapPhase, Sprite>> mapSpriteTable;
    
    // 로딩 상태
    private bool isInitialized = false;
    private bool isLoading = false;
    private Coroutine cacheCleanupCoroutine;

    // 이벤트 정의
    public delegate void OnSpriteLoaded(string spriteKey, Sprite sprite);
    public event OnSpriteLoaded OnSpriteLoadedEvent;

    public delegate void OnSpriteLoadFailed(string spriteKey, string error);
    public event OnSpriteLoadFailed OnSpriteLoadFailedEvent;

    // 프로퍼티
    public bool IsInitialized => isInitialized;
    public bool IsLoading => isLoading;
    public int CacheSize => spriteCache.Count;
    public int MaxCacheSize => maxCacheSize;

    private void Awake()
    {
        InitializeSingleton();
    }

    private void Start()
    {
        InitializeSpriteSystem();
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

    private void InitializeSpriteSystem()
    {
        try
        {
            ValidateComponents();
            InitializeMapSpriteTable();
            InitializeCaching();
            StartCacheCleanup();
            isInitialized = true;
            
            Debug.Log($"[SpriteManager] 스프라이트 시스템 초기화 완료 - 캐시: {enableCaching}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[SpriteManager] 초기화 중 오류: {ex.Message}");
        }
    }

    private void ValidateComponents()
    {
        if (lifeStageSprites == null || lifeStageSprites.Length == 0)
        {
            Debug.LogWarning("[SpriteManager] LifeStage 스프라이트가 설정되지 않았습니다.");
        }
        
        if (mapSpriteSets == null || mapSpriteSets.Count == 0)
        {
            Debug.LogWarning("[SpriteManager] 맵 스프라이트 세트가 설정되지 않았습니다.");
        }
    }

    private void InitializeMapSpriteTable()
    {
        mapSpriteTable = new Dictionary<MapType, Dictionary<MapPhase, Sprite>>();

        if (mapSpriteSets != null)
        {
            foreach (var set in mapSpriteSets)
            {
                if (set == null) continue;

                var phaseDict = new Dictionary<MapPhase, Sprite>
                {
                    { MapPhase.Day, set.daySprite },
                    { MapPhase.Sunset, set.sunsetSprite },
                    { MapPhase.Night, set.nightSprite }
                };

                mapSpriteTable[set.mapType] = phaseDict;
            }
        }
    }

    private void InitializeCaching()
    {
        if (!enableCaching) return;

        spriteCache.Clear();
        cacheTimestamps.Clear();
        cacheAccessOrder.Clear();
    }

    private void StartCacheCleanup()
    {
        if (!enableCaching) return;

        if (cacheCleanupCoroutine != null)
        {
            StopCoroutine(cacheCleanupCoroutine);
        }
        
        cacheCleanupCoroutine = StartCoroutine(CacheCleanupRoutine());
    }

    private IEnumerator CacheCleanupRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(cacheCleanupInterval);
            CleanupCache();
        }
    }

    private void CleanupCache()
    {
        if (!enableCaching || spriteCache.Count <= maxCacheSize) return;

        var itemsToRemove = cacheTimestamps
            .OrderBy(x => x.Value)
            .Take(spriteCache.Count - maxCacheSize)
            .Select(x => x.Key)
            .ToList();

        foreach (var key in itemsToRemove)
        {
            RemoveFromCache(key);
        }

        Debug.Log($"[SpriteManager] 캐시 정리 완료 - 제거된 항목: {itemsToRemove.Count}");
    }

    private void AddToCache(string key, Sprite sprite)
    {
        if (!enableCaching) return;

        if (spriteCache.Count >= maxCacheSize)
        {
            CleanupCache();
        }

        spriteCache[key] = sprite;
        cacheTimestamps[key] = Time.time;
        cacheAccessOrder.Enqueue(key);
    }

    private Sprite GetFromCache(string key)
    {
        if (!enableCaching || !spriteCache.ContainsKey(key)) return null;

        cacheTimestamps[key] = Time.time;
        return spriteCache[key];
    }

    private void RemoveFromCache(string key)
    {
        if (!enableCaching) return;

        spriteCache.Remove(key);
        cacheTimestamps.Remove(key);
    }

    /// <summary>
    /// 생명주기 스프라이트 가져오기
    /// </summary>
    public Sprite GetLifeStageSprite(int stage)
    {
        if (!isInitialized)
        {
            Debug.LogWarning("[SpriteManager] 아직 초기화되지 않았습니다.");
            return null;
        }

        if (stage < 0 || stage >= lifeStageSprites.Length)
        {
            Debug.LogError($"[SpriteManager] 잘못된 생명주기 단계: {stage}");
            return null;
        }

        string cacheKey = $"lifeStage_{stage}";
        Sprite cachedSprite = GetFromCache(cacheKey);
        
        if (cachedSprite != null)
        {
            return cachedSprite;
        }

        Sprite sprite = lifeStageSprites[stage];
        if (sprite != null)
        {
            AddToCache(cacheKey, sprite);
        }

        return sprite;
    }

    /// <summary>
    /// 맵 스프라이트 가져오기
    /// </summary>
    public Sprite GetMapSprite(MapType mapType, MapPhase phase)
    {
        if (!isInitialized)
        {
            Debug.LogWarning("[SpriteManager] 아직 초기화되지 않았습니다.");
            return null;
        }

        string cacheKey = $"map_{mapType}_{phase}";
        Sprite cachedSprite = GetFromCache(cacheKey);
        
        if (cachedSprite != null)
        {
            return cachedSprite;
        }

        if (mapSpriteTable.TryGetValue(mapType, out var phaseDict) &&
            phaseDict.TryGetValue(phase, out var sprite))
        {
            if (sprite != null)
            {
                AddToCache(cacheKey, sprite);
            }
            return sprite;
        }

        Debug.LogWarning($"[SpriteManager] 맵 스프라이트를 찾을 수 없습니다: {mapType}, {phase}");
        return null;
    }

    /// <summary>
    /// UI 스프라이트 가져오기
    /// </summary>
    public Sprite GetUISprite(UIType uiType, string spriteName)
    {
        if (!isInitialized) return null;

        string cacheKey = $"ui_{uiType}_{spriteName}";
        Sprite cachedSprite = GetFromCache(cacheKey);
        
        if (cachedSprite != null)
        {
            return cachedSprite;
        }

        if (uiSpriteSets != null)
        {
            var spriteSet = uiSpriteSets.FirstOrDefault(s => s.uiType == uiType && s.spriteName == spriteName);
            if (spriteSet?.sprite != null)
            {
                AddToCache(cacheKey, spriteSet.sprite);
                return spriteSet.sprite;
            }
        }

        Debug.LogWarning($"[SpriteManager] UI 스프라이트를 찾을 수 없습니다: {uiType}, {spriteName}");
        return null;
    }

    /// <summary>
    /// 아이템 스프라이트 가져오기
    /// </summary>
    public Sprite GetItemSprite(ItemData.ItemType itemType, string itemId)
    {
        if (!isInitialized) return null;

        string cacheKey = $"item_{itemType}_{itemId}";
        Sprite cachedSprite = GetFromCache(cacheKey);
        
        if (cachedSprite != null)
        {
            return cachedSprite;
        }

        if (itemSpriteSets != null)
        {
            var spriteSet = itemSpriteSets.FirstOrDefault(s => s.itemType == itemType && s.itemId == itemId);
            if (spriteSet?.sprite != null)
            {
                AddToCache(cacheKey, spriteSet.sprite);
                return spriteSet.sprite;
            }
        }

        Debug.LogWarning($"[SpriteManager] 아이템 스프라이트를 찾을 수 없습니다: {itemType}, {itemId}");
        return null;
    }

    /// <summary>
    /// 아이템 아이콘 가져오기
    /// </summary>
    public Sprite GetItemIcon(ItemData.ItemType itemType, string itemId)
    {
        if (!isInitialized) return null;

        string cacheKey = $"itemIcon_{itemType}_{itemId}";
        Sprite cachedSprite = GetFromCache(cacheKey);
        
        if (cachedSprite != null)
        {
            return cachedSprite;
        }

        if (itemSpriteSets != null)
        {
            var spriteSet = itemSpriteSets.FirstOrDefault(s => s.itemType == itemType && s.itemId == itemId);
            if (spriteSet?.icon != null)
            {
                AddToCache(cacheKey, spriteSet.icon);
                return spriteSet.icon;
            }
        }

        Debug.LogWarning($"[SpriteManager] 아이템 아이콘을 찾을 수 없습니다: {itemType}, {itemId}");
        return null;
    }

    /// <summary>
    /// 모자 스프라이트 가져오기
    /// </summary>
    public Sprite GetHatSprite(string itemId)
    {
        return GetItemSprite(ItemData.ItemType.Hat, itemId);
    }

    /// <summary>
    /// 얼굴 스프라이트 가져오기
    /// </summary>
    public Sprite GetFaceSprite(string itemId)
    {
        return GetItemSprite(ItemData.ItemType.Face, itemId);
    }

    /// <summary>
    /// 의상 스프라이트 가져오기
    /// </summary>
    public Sprite GetCostumeSprite(string itemId)
    {
        return GetItemSprite(ItemData.ItemType.Costume, itemId);
    }

    /// <summary>
    /// 액세서리 스프라이트 가져오기
    /// </summary>
    public Sprite GetAccessorySprite(string itemId)
    {
        return GetItemSprite(ItemData.ItemType.Accessory, itemId);
    }

    /// <summary>
    /// 업적 스프라이트 가져오기
    /// </summary>
    public Sprite GetAchievementSprite(string achievementId, bool isUnlocked = true)
    {
        if (!isInitialized) return null;

        string cacheKey = $"achievement_{achievementId}_{isUnlocked}";
        Sprite cachedSprite = GetFromCache(cacheKey);
        
        if (cachedSprite != null)
        {
            return cachedSprite;
        }

        if (achievementSpriteSets != null)
        {
            var spriteSet = achievementSpriteSets.FirstOrDefault(s => s.achievementId == achievementId);
            if (spriteSet != null)
            {
                Sprite sprite = isUnlocked ? spriteSet.unlockedSprite : spriteSet.lockedSprite;
                if (sprite != null)
                {
                    AddToCache(cacheKey, sprite);
                    return sprite;
                }
            }
        }

        Debug.LogWarning($"[SpriteManager] 업적 스프라이트를 찾을 수 없습니다: {achievementId}, {isUnlocked}");
        return null;
    }

    /// <summary>
    /// 비동기 스프라이트 로딩
    /// </summary>
    public IEnumerator LoadSpriteAsync(string spriteKey, System.Action<Sprite> onComplete)
    {
        if (!isInitialized)
        {
            onComplete?.Invoke(null);
            yield break;
        }

        isLoading = true;
        
        // 실제 구현에서는 Resources.LoadAsync 등을 사용
        yield return new WaitForSeconds(0.1f); // 시뮬레이션
        
        Sprite sprite = GetSpriteByKey(spriteKey);
        
        if (sprite != null)
        {
            OnSpriteLoadedEvent?.Invoke(spriteKey, sprite);
            onComplete?.Invoke(sprite);
        }
        else
        {
            OnSpriteLoadFailedEvent?.Invoke(spriteKey, "스프라이트를 찾을 수 없습니다.");
            onComplete?.Invoke(null);
        }
        
        isLoading = false;
    }

    private Sprite GetSpriteByKey(string spriteKey)
    {
        // 키 파싱 및 적절한 스프라이트 반환
        if (spriteKey.StartsWith("lifeStage_"))
        {
            if (int.TryParse(spriteKey.Split('_')[1], out int stage))
            {
                return GetLifeStageSprite(stage);
            }
        }
        else if (spriteKey.StartsWith("map_"))
        {
            var parts = spriteKey.Split('_');
            if (parts.Length >= 3)
            {
                if (System.Enum.TryParse<MapType>(parts[1], out MapType mapType) &&
                    System.Enum.TryParse<MapPhase>(parts[2], out MapPhase phase))
                {
                    return GetMapSprite(mapType, phase);
                }
            }
        }
        
        return null;
    }

    /// <summary>
    /// 캐시 초기화
    /// </summary>
    public void ClearCache()
    {
        if (!enableCaching) return;

        spriteCache.Clear();
        cacheTimestamps.Clear();
        cacheAccessOrder.Clear();
        
        Debug.Log("[SpriteManager] 캐시 초기화 완료");
    }

    /// <summary>
    /// 캐시 정보 반환
    /// </summary>
    public string GetCacheInfo()
    {
        return $"캐시 크기: {spriteCache.Count}/{maxCacheSize}, 활성화: {enableCaching}";
    }

    /// <summary>
    /// 스프라이트 존재 여부 확인
    /// </summary>
    public bool HasSprite(SpriteType type, string identifier)
    {
        switch (type)
        {
            case SpriteType.LifeStage:
                return int.TryParse(identifier, out int stage) && stage >= 0 && stage < lifeStageSprites.Length;
            case SpriteType.Map:
                return mapSpriteTable.ContainsKey(System.Enum.Parse<MapType>(identifier));
            case SpriteType.UI:
                return uiSpriteSets?.Any(s => s.spriteName == identifier) ?? false;
            case SpriteType.Item:
                return itemSpriteSets?.Any(s => s.itemId == identifier) ?? false;
            case SpriteType.Achievement:
                return achievementSpriteSets?.Any(s => s.achievementId == identifier) ?? false;
            default:
                return false;
        }
    }

    /// <summary>
    /// 사용 가능한 맵 타입 목록 반환
    /// </summary>
    public List<MapType> GetAvailableMapTypes()
    {
        return mapSpriteTable.Keys.ToList();
    }

    /// <summary>
    /// 사용 가능한 UI 타입 목록 반환
    /// </summary>
    public List<UIType> GetAvailableUITypes()
    {
        return uiSpriteSets?.Select(s => s.uiType).Distinct().ToList() ?? new List<UIType>();
    }

    private void OnDestroy()
    {
        if (cacheCleanupCoroutine != null)
        {
            StopCoroutine(cacheCleanupCoroutine);
        }
        
        ClearCache();
        
        // 이벤트 초기화
        OnSpriteLoadedEvent = null;
        OnSpriteLoadFailedEvent = null;
    }
}
