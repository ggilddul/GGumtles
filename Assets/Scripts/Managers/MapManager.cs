using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq; // Added for .Any()

public class MapManager : MonoBehaviour
{
    public static MapManager Instance { get; private set; }

    [Header("맵 설정")]
    [SerializeField] private BackgroundType backgroundType = BackgroundType.SpriteRenderer;
    [SerializeField] private List<MapData> availableMaps = new List<MapData>();
    [SerializeField] private bool useTransitionAnimation = true;
    // [SerializeField] private float transitionDuration = 1f;  // 미사용
    
    [Header("Background Components")]
    [SerializeField] private SpriteRenderer backgroundRenderer;
    [SerializeField] private Image backgroundImage;
    
    [Header("애니메이션 설정")]
    [SerializeField] private AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private bool useFadeTransition = true;
    [SerializeField] private float fadeDuration = 0.5f;

    // 맵 타입 열거형
    public enum BackgroundType
    {
        SpriteRenderer,
        UIImage
    }

    // 맵 데이터 클래스
    [System.Serializable]
    public class MapData
    {
        public string mapId;
        public string mapName;
        public SpriteManager.MapType mapType;
        public SpriteManager.MapPhase defaultPhase = SpriteManager.MapPhase.Day;
        public bool isUnlocked = true;
        public int unlockRequirement = 0;
        public string description;
        public Color mapTint = Color.white;
        public float mapBrightness = 1f;
        public AudioClip backgroundMusic;
        
        public MapData(string id, string name, SpriteManager.MapType type)
        {
            mapId = id;
            mapName = name;
            mapType = type;
        }
    }

    // 맵 상태 클래스
    [System.Serializable]
    public class MapState
    {
        public string mapId;
        public SpriteManager.MapType mapType;
        public SpriteManager.MapPhase currentPhase;
        public float lastPhaseChangeTime;
        public bool isActive;
        
        public MapState(string id, SpriteManager.MapType type)
        {
            mapId = id;
            mapType = type;
            currentPhase = SpriteManager.MapPhase.Day;
            lastPhaseChangeTime = 0f;
            isActive = false;
        }
    }

    // 이벤트 정의
    public delegate void OnMapChanged(string fromMapId, string toMapId, SpriteManager.MapType fromType, SpriteManager.MapType toType);
    public event OnMapChanged OnMapChangedEvent;

    public delegate void OnMapPhaseChanged(string mapId, SpriteManager.MapPhase fromPhase, SpriteManager.MapPhase toPhase);
    public event OnMapPhaseChanged OnMapPhaseChangedEvent;

    // 상태 관리
    private MapState currentMapState;
    private MapData currentMapData;
    private bool isInitialized = false;
    private bool isTransitioning = false;
    private Coroutine transitionCoroutine;

    // 프로퍼티
    public string CurrentMapId => currentMapState?.mapId ?? "";
    public SpriteManager.MapType CurrentMapType => currentMapState?.mapType ?? SpriteManager.MapType.TypeA;
    public SpriteManager.MapPhase CurrentMapPhase => currentMapState?.currentPhase ?? SpriteManager.MapPhase.Day;
    public bool IsTransitioning => isTransitioning;
    public int AvailableMapCount => availableMaps?.Count ?? 0;

    private void Awake()
    {
        InitializeSingleton();
    }

    private void Start()
    {
        InitializeMapSystem();
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

    private void InitializeMapSystem()
    {
        try
        {
            ValidateComponents();
            InitializeDefaultMaps();
            LoadMapState();
            isInitialized = true;
            
            Debug.Log($"[MapManager] 맵 시스템 초기화 완료 - 사용 가능한 맵: {AvailableMapCount}개");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[MapManager] 초기화 중 오류: {ex.Message}");
        }
    }

    private void ValidateComponents()
    {
        if (backgroundRenderer == null && backgroundImage == null)
        {
            Debug.LogError("[MapManager] 배경 컴포넌트가 설정되지 않았습니다.");
        }
        
        if (availableMaps == null || availableMaps.Count == 0)
        {
            Debug.LogWarning("[MapManager] 사용 가능한 맵이 설정되지 않았습니다.");
        }
    }

    private void InitializeDefaultMaps()
    {
        if (availableMaps.Count == 0)
        {
            // 기본 맵 추가
            availableMaps.Add(new MapData("map_001", "기본 맵", SpriteManager.MapType.TypeA));
            availableMaps.Add(new MapData("map_002", "숲 맵", SpriteManager.MapType.TypeB));
        }
    }

    private void LoadMapState()
    {
        // 저장된 맵 상태 로드 (필요시 구현)
        if (availableMaps.Count > 0)
        {
            var firstMap = availableMaps[0];
            currentMapData = firstMap;
            currentMapState = new MapState(firstMap.mapId, firstMap.mapType);
            currentMapState.isActive = true;
        }
    }

    /// <summary>
    /// 맵 변경
    /// </summary>
    public void ChangeMap(SpriteManager.MapType newMapType)
    {
        if (!isInitialized)
        {
            Debug.LogWarning("[MapManager] 아직 초기화되지 않았습니다.");
            return;
        }

        if (isTransitioning)
        {
            Debug.LogWarning("[MapManager] 맵 전환 중입니다.");
            return;
        }

        var targetMap = availableMaps.Find(m => m.mapType == newMapType);
        if (targetMap == null)
        {
            Debug.LogError($"[MapManager] 찾을 수 없는 맵 타입: {newMapType}");
            return;
        }

        if (!targetMap.isUnlocked)
        {
            Debug.LogWarning($"[MapManager] 잠긴 맵: {targetMap.mapName}");
            ShowMapLockedMessage(targetMap);
            return;
        }

        if (currentMapState?.mapType == newMapType)
        {
            Debug.Log($"[MapManager] 이미 선택된 맵: {newMapType}");
            return;
        }

        StartCoroutine(ChangeMapCoroutine(targetMap));
    }

    /// <summary>
    /// 맵 ID로 변경
    /// </summary>
    public void ChangeMapById(string mapId)
    {
        if (!isInitialized) return;

        var targetMap = availableMaps.Find(m => m.mapId == mapId);
        if (targetMap != null)
        {
            ChangeMap(targetMap.mapType);
        }
        else
        {
            Debug.LogError($"[MapManager] 찾을 수 없는 맵 ID: {mapId}");
        }
    }

    /// <summary>
    /// 인덱스로 맵 변경
    /// </summary>
    public void ChangeMapByIndex(int index)
    {
        if (!isInitialized) return;

        if (index < 0 || index >= availableMaps.Count)
        {
            Debug.LogError($"[MapManager] 잘못된 맵 인덱스: {index}");
            return;
        }

        ChangeMap(availableMaps[index].mapType);
    }

    private IEnumerator ChangeMapCoroutine(MapData targetMap)
    {
        isTransitioning = true;
        string fromMapId = currentMapState?.mapId ?? "";
        SpriteManager.MapType fromType = currentMapState?.mapType ?? SpriteManager.MapType.TypeA;

        // 로딩 화면 표시
        if (LoadingManager.Instance != null)
        {
            LoadingManager.Instance.ShowLoading(LoadingManager.LoadingType.MapChange);
        }

        // 페이드 아웃
        if (useFadeTransition)
        {
            yield return StartCoroutine(FadeOutBackground());
        }

        // 맵 상태 업데이트
        if (currentMapState != null)
        {
            currentMapState.isActive = false;
        }

        currentMapData = targetMap;
        currentMapState = new MapState(targetMap.mapId, targetMap.mapType);
        currentMapState.isActive = true;

        // 새 맵 배경 적용
        UpdateMapBackground();

        // 페이드 인
        if (useFadeTransition)
        {
            yield return StartCoroutine(FadeInBackground());
        }

        // 로딩 화면 숨김
        if (LoadingManager.Instance != null)
        {
            LoadingManager.Instance.HideLoading(LoadingManager.LoadingType.MapChange);
        }

        // 이벤트 발생
        OnMapChangedEvent?.Invoke(fromMapId, targetMap.mapId, fromType, targetMap.mapType);

        isTransitioning = false;
        
        Debug.Log($"[MapManager] 맵 변경 완료: {targetMap.mapName}");
    }

    private IEnumerator FadeOutBackground()
    {
        CanvasGroup canvasGroup = GetBackgroundCanvasGroup();
        if (canvasGroup == null) yield break;

        float elapsed = 0f;
        float startAlpha = canvasGroup.alpha;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float progress = transitionCurve.Evaluate(elapsed / fadeDuration);
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, progress);
            yield return null;
        }

        canvasGroup.alpha = 0f;
    }

    private IEnumerator FadeInBackground()
    {
        CanvasGroup canvasGroup = GetBackgroundCanvasGroup();
        if (canvasGroup == null) yield break;

        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float progress = transitionCurve.Evaluate(elapsed / fadeDuration);
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, progress);
            yield return null;
        }

        canvasGroup.alpha = 1f;
    }

    private CanvasGroup GetBackgroundCanvasGroup()
    {
        switch (backgroundType)
        {
            case BackgroundType.SpriteRenderer:
                return backgroundRenderer?.GetComponent<CanvasGroup>();
            case BackgroundType.UIImage:
                return backgroundImage?.GetComponent<CanvasGroup>();
            default:
                return null;
        }
    }

    /// <summary>
    /// 맵 페이즈 변경
    /// </summary>
    public void ChangeMapPhase(SpriteManager.MapPhase newPhase)
    {
        if (!isInitialized || currentMapState == null) return;

        if (currentMapState.currentPhase == newPhase) return;

        SpriteManager.MapPhase fromPhase = currentMapState.currentPhase;
        currentMapState.currentPhase = newPhase;
        currentMapState.lastPhaseChangeTime = Time.time;

        UpdateMapBackground();
        
        OnMapPhaseChangedEvent?.Invoke(currentMapState.mapId, fromPhase, newPhase);
        
        Debug.Log($"[MapManager] 맵 페이즈 변경: {fromPhase} -> {newPhase}");
    }

    /// <summary>
    /// 맵 배경 업데이트
    /// </summary>
    public void UpdateMapBackground()
    {
        if (!isInitialized || currentMapState == null) return;

        Sprite newSprite = SpriteManager.Instance?.GetMapSprite(currentMapState.mapType, currentMapState.currentPhase);
        if (newSprite == null)
        {
            Debug.LogWarning($"[MapManager] 맵 스프라이트를 찾을 수 없습니다: {currentMapState.mapType}, {currentMapState.currentPhase}");
            return;
        }

        UpdateBackgroundSprite(newSprite);
        ApplyMapSettings();
    }

    private void UpdateBackgroundSprite(Sprite newSprite)
    {
        switch (backgroundType)
        {
            case BackgroundType.SpriteRenderer:
                if (backgroundRenderer != null)
                {
                    backgroundRenderer.sprite = newSprite;
                }
                break;

            case BackgroundType.UIImage:
                if (backgroundImage != null)
                {
                    backgroundImage.sprite = newSprite;
                }
                break;
        }
    }

    private void ApplyMapSettings()
    {
        if (currentMapData == null) return;

        // 색상 조정
        if (backgroundRenderer != null)
        {
            backgroundRenderer.color = currentMapData.mapTint;
        }
        
        if (backgroundImage != null)
        {
            backgroundImage.color = currentMapData.mapTint;
        }

        // 밝기 조정 (Material 사용 시)
        // 추가 구현 필요
    }

    /// <summary>
    /// 맵 잠금 해제
    /// </summary>
    public void UnlockMap(string mapId)
    {
        var mapData = availableMaps.Find(m => m.mapId == mapId);
        if (mapData != null)
        {
            mapData.isUnlocked = true;
            Debug.Log($"[MapManager] 맵 잠금 해제: {mapData.mapName}");
        }
    }

    /// <summary>
    /// 맵 잠금
    /// </summary>
    public void LockMap(string mapId)
    {
        var mapData = availableMaps.Find(m => m.mapId == mapId);
        if (mapData != null)
        {
            mapData.isUnlocked = false;
            Debug.Log($"[MapManager] 맵 잠금: {mapData.mapName}");
        }
    }

    private void ShowMapLockedMessage(MapData mapData)
    {
        string message = $"이 맵을 열려면 {mapData.unlockRequirement}개가 필요합니다.";
        
        if (PopupManager.Instance != null)
        {
            PopupManager.Instance.ShowToast(message, 2f);
        }
    }

    /// <summary>
    /// 현재 맵 정보 반환
    /// </summary>
    public MapData GetCurrentMapData()
    {
        return currentMapData;
    }

    /// <summary>
    /// 사용 가능한 맵 목록 반환
    /// </summary>
    public List<MapData> GetAvailableMaps()
    {
        return availableMaps?.FindAll(m => m.isUnlocked) ?? new List<MapData>();
    }

    /// <summary>
    /// 맵 존재 여부 확인
    /// </summary>
    public bool HasMap(string mapId)
    {
        return availableMaps?.Any(m => m.mapId == mapId) ?? false;
    }

    /// <summary>
    /// 맵 잠금 여부 확인
    /// </summary>
    public bool IsMapLocked(string mapId)
    {
        var mapData = availableMaps?.Find(m => m.mapId == mapId);
        return mapData?.isUnlocked == false;
    }

    /// <summary>
    /// 애니메이션 설정 변경
    /// </summary>
    public void SetTransitionAnimation(bool enabled)
    {
        useTransitionAnimation = enabled;
        Debug.Log($"[MapManager] 전환 애니메이션 {(enabled ? "활성화" : "비활성화")}");
    }

    /// <summary>
    /// 페이드 전환 설정 변경
    /// </summary>
    public void SetFadeTransition(bool enabled)
    {
        useFadeTransition = enabled;
        Debug.Log($"[MapManager] 페이드 전환 {(enabled ? "활성화" : "비활성화")}");
    }

    /// <summary>
    /// 맵 상태 정보 반환
    /// </summary>
    public string GetMapStatus()
    {
        if (currentMapData == null) return "맵 없음";
        
        return $"현재 맵: {currentMapData.mapName}, 페이즈: {currentMapState?.currentPhase}, " +
               $"전환 중: {isTransitioning}";
    }

    private void OnDestroy()
    {
        // 이벤트 초기화
        OnMapChangedEvent = null;
        OnMapPhaseChangedEvent = null;
    }
}
