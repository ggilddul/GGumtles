using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using GGumtles.Data;
using GGumtles.Managers;

namespace GGumtles.Utils
{
    /// <summary>
    /// 스프라이트 매니저 - 맵 스프라이트 관리 및 벌레 스프라이트 합성
    /// 1. 맵 스프라이트 관리 (타입별, 시간대별)
    /// 2. WormData의 아이템 정보를 통해 완성된 벌레 스프라이트 생성
    /// 3. 생명주기에 따른 크기 조절된 스프라이트를 홈 탭에 표시
    /// </summary>
    public class SpriteManager : MonoBehaviour
{
    public static SpriteManager Instance { get; private set; }

    [Header("맵 스프라이트")]
    [SerializeField] private List<MapSpriteSet> mapSpriteSets;
    
    [Header("벌레 기본 스프라이트")]
    [SerializeField] private Sprite[] lifeStageSprites;  // 생명주기별 기본 벌레 스프라이트
    
    [Header("아이템 스프라이트")]
    [SerializeField] private List<ItemSpriteSet> itemSpriteSets;
    
    [Header("업적 스프라이트")]
    [SerializeField] private Sprite[] achievementSprites;  // 업적 아이콘 스프라이트들
    
    [Header("스프라이트 합성 설정")]
    [SerializeField] private Vector2 baseWormSize = new Vector2(300f, 300f);  // 기본 벌레 크기
    [SerializeField] private float[] lifeStageScales = { 0.3f, 0.4f, 0.55f, 0.7f, 0.85f, 1.0f, 1.0f };  // 생명주기별 크기 배율 (성체가 1.0, 사망은 0.8)
    
    [Header("캐시 설정")]
    [SerializeField] private bool enableCaching = true;
    [SerializeField] private int maxCacheSize = 50;
    [SerializeField] private float cacheCleanupInterval = 300f; // 5분
    
    [Header("디버그 설정")]
    [SerializeField] private bool enableDebugLogs = false;
    
    [Header("기본 스프라이트")]
    [SerializeField] private Sprite defaultSprite;  // 맵 스프라이트가 없을 때 사용할 기본 스프라이트

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

    // 생명주기 열거형
    public enum LifeStage
    {
        Egg = 0,        // 알
        Larva1 = 1,     // 제1유충기
        Larva2 = 2,     // 제2유충기
        Larva3 = 3,     // 제3유충기
        Larva4 = 4,     // 제4유충기
        Adult = 5,      // 성체
        Dead = 6        // 사망
    }

    // 맵 스프라이트 세트
    [System.Serializable]
    public class MapSpriteSet
    {
        public MapType mapType;
        public Sprite daySprite;
        public Sprite sunsetSprite;
        public Sprite nightSprite;
        public string description;
    }

    // 아이템 스프라이트 세트
    [System.Serializable]
    public class ItemSpriteSet
    {
        public ItemData.ItemType itemType;
        public string itemId;
        public Sprite sprite;
        public Vector2 offset = Vector2.zero;  // 벌레 기준 오프셋
        public Vector2 scale = Vector2.one;    // 크기 조절
        public int sortingOrder = 0;           // 렌더링 순서
        public string description;
    }

    // 완성된 벌레 스프라이트 정보
    [System.Serializable]
    public class CompletedWormSprite
    {
        public Sprite sprite;                  // 최종 합성된 스프라이트
        public Vector2 size;                   // 최종 크기
        public float scale;                    // 적용된 크기 배율
        public LifeStage lifeStage;            // 생명주기
        public List<string> equippedItems;     // 장착된 아이템들
    }

    // 캐시 시스템
    private Dictionary<string, Sprite> spriteCache = new Dictionary<string, Sprite>();
    private Dictionary<string, float> cacheTimestamps = new Dictionary<string, float>();
    private Queue<string> cacheAccessOrder = new Queue<string>();
    
    // 맵 스프라이트 테이블
    private Dictionary<MapType, Dictionary<MapPhase, Sprite>> mapSpriteTable;
    
    // 완성된 벌레 스프라이트 캐시
    private Dictionary<string, CompletedWormSprite> completedWormCache = new Dictionary<string, CompletedWormSprite>();
    
    // 상태 관리
    private bool isInitialized = false;
    private Coroutine cacheCleanupCoroutine;

    // 이벤트 정의
    public delegate void OnWormSpriteCompleted(string wormId, CompletedWormSprite completedSprite);
    public event OnWormSpriteCompleted OnWormSpriteCompletedEvent;

    // 프로퍼티
    public bool IsInitialized => isInitialized;
    public int CacheSize => spriteCache.Count;
    public int CompletedWormCacheSize => completedWormCache.Count;

    #region Unity 생명주기

    private void Awake()
    {
        InitializeSingleton();
    }

    private void Start()
    {
        InitializeSpriteSystem();
    }

    #endregion

    #region 초기화

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
            
            Debug.Log($"[SpriteManager] 스프라이트 시스템 초기화 완료");
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
            Debug.LogWarning("[SpriteManager] 생명주기 스프라이트가 설정되지 않았습니다.");
        }
        
        if (mapSpriteSets == null || mapSpriteSets.Count == 0)
        {
            Debug.LogWarning("[SpriteManager] 맵 스프라이트 세트가 설정되지 않았습니다.");
        }

        if (itemSpriteSets == null || itemSpriteSets.Count == 0)
        {
            Debug.LogWarning("[SpriteManager] 아이템 스프라이트 세트가 설정되지 않았습니다. Inspector에서 설정해주세요.");
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

    #endregion

    #region 맵 스프라이트 관리

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

        // 맵 스프라이트가 설정되지 않은 경우 기본 스프라이트 반환
        Debug.LogWarning($"[SpriteManager] 맵 스프라이트를 찾을 수 없습니다: {mapType}, {phase}. 기본 스프라이트를 사용합니다.");
        
        // 기본 스프라이트가 있다면 반환
        if (defaultSprite != null)
        {
            return defaultSprite;
        }
        
        return null;
    }

    #endregion

    #region 벌레 스프라이트 합성

    /// <summary>
    /// WormData를 기반으로 완성된 벌레 스프라이트 생성
    /// </summary>
    public CompletedWormSprite CreateCompletedWormSprite(WormData wormData)
    {
        if (!isInitialized || wormData == null)
        {
            Debug.LogWarning("[SpriteManager] 초기화되지 않았거나 유효하지 않은 WormData입니다.");
            return null;
        }

        // 캐시 키 생성
        string cacheKey = GenerateWormCacheKey(wormData);
        
        // 캐시에서 확인
        if (completedWormCache.TryGetValue(cacheKey, out var cachedSprite))
        {
            return cachedSprite;
        }

        try
        {
            // 기본 벌레 스프라이트 가져오기
            Sprite baseSprite = GetLifeStageSprite(wormData.lifeStage);
            if (baseSprite == null)
            {
                Debug.LogError($"[SpriteManager] 생명주기 스프라이트를 찾을 수 없습니다: {wormData.lifeStage}");
                return null;
            }

            // 아이템 스프라이트들 수집 (알 단계에서는 아이템 장착 불가)
            List<Sprite> itemSprites = new List<Sprite>();
            List<Vector2> itemOffsets = new List<Vector2>();
            List<Vector2> itemScales = new List<Vector2>();
            List<int> itemSortingOrders = new List<int>();

            // 장착된 아이템들 처리 (알 단계나 사망 단계가 아닐 때만)
            if (wormData.lifeStage != (int)LifeStage.Egg && wormData.lifeStage != (int)LifeStage.Dead)
            {
                // 개별 아이템 필드들 처리
                string[] equippedItems = { wormData.hatItemId, wormData.faceItemId, wormData.costumeItemId };
                
                foreach (var itemId in equippedItems)
                {
                    if (!string.IsNullOrEmpty(itemId))
                    {
                        var itemSprite = GetItemSprite(itemId);
                        if (itemSprite != null)
                        {
                            itemSprites.Add(itemSprite.sprite);
                            itemOffsets.Add(itemSprite.offset);
                            itemScales.Add(itemSprite.scale);
                            itemSortingOrders.Add(itemSprite.sortingOrder);
                        }
                    }
                }
            }

            // 스프라이트 합성
            Sprite completedSprite = CombineSprites(baseSprite, itemSprites, itemOffsets, itemScales, itemSortingOrders);
            
            // 사망한 벌레는 회색조 처리
            if (wormData.lifeStage == (int)LifeStage.Dead)
            {
                completedSprite = ProcessDeadWormSprite(completedSprite);
            }
            
            // 생명주기에 따른 크기 계산
            float lifeStageScale = GetLifeStageScale(wormData.lifeStage);
            Vector2 finalSize = baseWormSize * lifeStageScale;

            // 완성된 스프라이트 정보 생성
            var completedWormSprite = new CompletedWormSprite
            {
                sprite = completedSprite,
                size = finalSize,
                scale = lifeStageScale,
                lifeStage = (LifeStage)wormData.lifeStage,
                equippedItems = (wormData.lifeStage == (int)LifeStage.Egg || wormData.lifeStage == (int)LifeStage.Dead) ? 
                    new List<string>() : // 알 단계나 사망 단계에서는 빈 리스트
                    GetEquippedItemsList(wormData)
            };

            // 캐시에 저장
            completedWormCache[cacheKey] = completedWormSprite;

            // 이벤트 발생
            OnWormSpriteCompletedEvent?.Invoke(wormData.wormId.ToString(), completedWormSprite);

            string lifeStageName = GetLifeStageName(wormData.lifeStage);
            Debug.Log($"[SpriteManager] 벌레 스프라이트 완성: {wormData.name} (생명주기: {lifeStageName}, 크기: {finalSize})");

            return completedWormSprite;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[SpriteManager] 벌레 스프라이트 생성 중 오류: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// 생명주기 스프라이트 가져오기
    /// </summary>
    public Sprite GetLifeStageSprite(int lifeStage)
    {
        if (lifeStage < 0 || lifeStage >= lifeStageSprites.Length)
        {
            Debug.LogError($"[SpriteManager] 잘못된 생명주기 단계: {lifeStage}");
            return null;
        }

        return lifeStageSprites[lifeStage];
    }

    /// <summary>
    /// 아이템 스프라이트 정보 가져오기
    /// </summary>
    private ItemSpriteSet GetItemSprite(string itemId)
    {
        if (itemSpriteSets == null) return null;

        return itemSpriteSets.FirstOrDefault(s => s.itemId == itemId);
    }

    /// <summary>
    /// 생명주기에 따른 크기 배율 가져오기
    /// </summary>
    private float GetLifeStageScale(int lifeStage)
    {
        if (lifeStage < 0 || lifeStage >= lifeStageScales.Length)
        {
            return 1.0f; // 기본 크기
        }

        return lifeStageScales[lifeStage];
    }

    /// <summary>
    /// 생명주기 이름 가져오기
    /// </summary>
    private string GetLifeStageName(int lifeStage)
    {
        switch (lifeStage)
        {
            case 0: return "알";
            case 1: return "제1유충기";
            case 2: return "제2유충기";
            case 3: return "제3유충기";
            case 4: return "제4유충기";
            case 5: return "성체";
            case 6: return "사망";
            default: return $"단계{lifeStage}";
        }
    }

    /// <summary>
    /// 벌레 캐시 키 생성
    /// </summary>
    private string GenerateWormCacheKey(WormData wormData)
    {
        var equippedItems = GetEquippedItemsList(wormData);
        string itemsString = string.Join(",", equippedItems.OrderBy(x => x));
        return $"worm_{wormData.lifeStage}_{itemsString}";
    }

    /// <summary>
    /// WormData에서 장착된 아이템 리스트 생성
    /// </summary>
    private List<string> GetEquippedItemsList(WormData wormData)
    {
        var items = new List<string>();
        if (!string.IsNullOrEmpty(wormData.hatItemId)) items.Add(wormData.hatItemId);
        if (!string.IsNullOrEmpty(wormData.faceItemId)) items.Add(wormData.faceItemId);
        if (!string.IsNullOrEmpty(wormData.costumeItemId)) items.Add(wormData.costumeItemId);
        return items;
    }

    /// <summary>
    /// 스프라이트 합성 (기본 벌레 + 아이템들)
    /// </summary>
    private Sprite CombineSprites(Sprite baseSprite, List<Sprite> itemSprites, List<Vector2> offsets, List<Vector2> scales, List<int> sortingOrders)
    {
        // 간단한 구현: 현재는 기본 스프라이트만 반환
        // 실제로는 RenderTexture를 사용하여 스프라이트들을 합성해야 함
        // 여기서는 기본 스프라이트를 반환하고, 향후 고급 합성 기능을 추가할 수 있음
        
        return baseSprite;
    }

    /// <summary>
    /// 사망한 벌레 스프라이트 처리 (회색조 적용)
    /// </summary>
    private Sprite ProcessDeadWormSprite(Sprite originalSprite)
    {
        // 간단한 구현: 현재는 원본 스프라이트 반환
        // 실제로는 Material을 사용하여 회색조 효과를 적용해야 함
        // 여기서는 원본 스프라이트를 반환하고, 향후 고급 효과를 추가할 수 있음
        
        return originalSprite;
    }

    #endregion

    #region 홈 탭용 스프라이트

    /// <summary>
    /// 홈 탭용 완성된 벌레 스프라이트 가져오기 (크기 조절됨)
    /// </summary>
    public CompletedWormSprite GetHomeTabWormSprite(WormData wormData)
    {
        var completedSprite = CreateCompletedWormSprite(wormData);
        if (completedSprite != null)
        {
            // 홈 탭용 추가 크기 조절 (필요시)
            completedSprite.size *= 1.2f; // 홈 탭에서는 20% 더 크게
        }
        return completedSprite;
    }

    /// <summary>
    /// 완성된 벌레 스프라이트 캐시 정리
    /// </summary>
    public void ClearCompletedWormCache()
    {
        completedWormCache.Clear();
        Debug.Log("[SpriteManager] 완성된 벌레 스프라이트 캐시 정리 완료");
    }

    #endregion

    #region 기존 호환성 메서드

    /// <summary>
    /// 모자 스프라이트 가져오기
    /// </summary>
    public Sprite GetHatSprite(string itemId)
    {
        return GetItemSprite(itemId)?.sprite;
    }

    /// <summary>
    /// 얼굴 스프라이트 가져오기
    /// </summary>
    public Sprite GetFaceSprite(string itemId)
    {
        return GetItemSprite(itemId)?.sprite;
    }

    /// <summary>
    /// 의상 스프라이트 가져오기
    /// </summary>
    public Sprite GetCostumeSprite(string itemId)
    {
        return GetItemSprite(itemId)?.sprite;
    }

    /// <summary>
    /// 업적 스프라이트 가져오기
    /// </summary>
    public Sprite GetAchievementSprite(string achievementId, bool isUnlocked = true)
    {
        if (enableDebugLogs)
        {
            Debug.Log($"[SpriteManager] GetAchievementSprite 호출됨: {achievementId}, {isUnlocked}");
        }
        
        // 1. achievementSprites에서 해당 업적 스프라이트 찾기
        if (enableDebugLogs)
        {
            Debug.Log($"[SpriteManager] achievementSprites 확인: {achievementSprites != null}, 길이: {achievementSprites?.Length ?? 0}");
        }
        
        if (achievementSprites != null && achievementSprites.Length > 0)
        {
            // achievementId를 기반으로 인덱스 계산 (간단한 해시 방식)
            int index = Mathf.Abs(achievementId.GetHashCode()) % achievementSprites.Length;
            if (enableDebugLogs)
            {
                Debug.Log($"[SpriteManager] achievementSprites 인덱스 계산: {index}");
            }
            
            if (achievementSprites[index] != null)
            {
                if (enableDebugLogs)
                {
                    Debug.Log($"[SpriteManager] achievementSprites에서 스프라이트 반환: {achievementSprites[index].name}");
                }
                return achievementSprites[index];
            }
            else
            {
                if (enableDebugLogs)
                {
                    Debug.Log($"[SpriteManager] achievementSprites[{index}]가 null입니다");
                }
            }
        }
        
        // 2. achievementSprites가 없으면 기본 아이콘 반환 (아이템 스프라이트 사용)
        if (enableDebugLogs)
        {
            Debug.Log($"[SpriteManager] itemSpriteSets 확인: {itemSpriteSets != null}, 개수: {itemSpriteSets?.Count ?? 0}");
        }
        
        if (itemSpriteSets != null && itemSpriteSets.Count > 0 && itemSpriteSets[0].sprite != null)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"[SpriteManager] itemSpriteSets에서 스프라이트 반환: {itemSpriteSets[0].sprite.name}");
            }
            return itemSpriteSets[0].sprite;
        }
        
        // 3. 마지막으로 lifeStageSprites 사용 (알 스프라이트 대신)
        if (enableDebugLogs)
        {
            Debug.Log($"[SpriteManager] lifeStageSprites 확인: {lifeStageSprites != null}, 길이: {lifeStageSprites?.Length ?? 0}");
        }
        
        if (lifeStageSprites != null && lifeStageSprites.Length > 1)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"[SpriteManager] lifeStageSprites에서 스프라이트 반환: {lifeStageSprites[1]?.name}");
            }
            return lifeStageSprites[1]; // 알(0) 대신 다음 단계(1) 사용
        }
        
        return null;
    }

    #endregion

    #region 캐시 시스템

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

    #endregion

    #region 유틸리티 메서드

    /// <summary>
    /// 스프라이트 매니저 정보 반환
    /// </summary>
    public string GetSpriteManagerInfo()
    {
        var info = new System.Text.StringBuilder();
        info.AppendLine($"[스프라이트 매니저 정보]");
        info.AppendLine($"초기화됨: {isInitialized}");
        info.AppendLine($"캐시 크기: {CacheSize}/{maxCacheSize}");
        info.AppendLine($"완성된 벌레 캐시: {CompletedWormCacheSize}");
        info.AppendLine($"맵 스프라이트 세트: {mapSpriteSets?.Count ?? 0}개");
        info.AppendLine($"생명주기 스프라이트: {lifeStageSprites?.Length ?? 0}개");
        info.AppendLine($"아이템 스프라이트: {itemSpriteSets?.Count ?? 0}개");

        return info.ToString();
    }

    #endregion

    #region 이벤트 정리

    private void OnDestroy()
    {
        if (cacheCleanupCoroutine != null)
        {
            StopCoroutine(cacheCleanupCoroutine);
        }

        OnWormSpriteCompletedEvent = null;
    }

    #endregion
    }
}
