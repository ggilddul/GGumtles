using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using GGumtles.Data;
using GGumtles.UI;
using GGumtles.Managers;

namespace GGumtles.Managers
{
    public class LoadingManager : MonoBehaviour
{
    public static LoadingManager Instance { get; private set; }

    [Header("접속 로딩")]
    public CanvasGroup logoLoadingCanvasGroup;
    public TMP_Text logoLoadingInfoText;

    [Header("맵 변경 로딩")]
    public CanvasGroup mapLoadingCanvasGroup;
    public TMP_Text mapLoadingInfoText;

    // 상수 분리
    private const float DEFAULT_FADE_DURATION = 2.0f; // fade out 시간을 2초로 조정
    private const float MINIMUM_LOADING_TIME = 2.0f; // 최소 로딩 시간 2초
    private const float INITIAL_ALPHA = 0f;
    private const float TARGET_ALPHA = 1f;
    private const float FADE_OUT_TARGET_ALPHA = 0f;

    // 로딩 메시지 확장성 개선
    private readonly List<string> infoMessages = new List<string>
    {
        "꿈틀즈에 오신 걸 환영해요!",
        "알고계셨나요?\n예쁜꼬마선충을 이용한 4번의 연구는 노벨상을 수상했어요!"
    };

    private Coroutine fadeCoroutine;
    private bool isInitialized = false;
    private float loadingStartTime; // 로딩 시작 시간

    public enum LoadingType
    {
        Logo,
        MapChange
    }

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
            InitializeLoadingManager();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeLoadingManager()
    {
        ValidateComponents();
        loadingStartTime = Time.realtimeSinceStartup; // 더 정확한 시간 측정
        Debug.Log($"[LoadingManager] 로딩 시작 - 시간: {loadingStartTime:F3}");
        
        ShowLoading(LoadingType.Logo);
        StartCoroutine(SubscribeToGameSaveManager());
        isInitialized = true;
    }

    private void ValidateComponents()
    {
        if (logoLoadingCanvasGroup == null || logoLoadingInfoText == null)
        {
            Debug.LogError("[LoadingManager] 로고 로딩 컴포넌트가 설정되지 않았습니다.");
        }

        if (mapLoadingCanvasGroup == null || mapLoadingInfoText == null)
        {
            Debug.LogError("[LoadingManager] 맵 로딩 컴포넌트가 설정되지 않았습니다.");
        }
    }

    private IEnumerator SubscribeToGameSaveManager()
    {
        yield return new WaitUntil(() => GameSaveManager.Instance != null);

        Debug.Log("[LoadingManager] GameSaveManager 이벤트 구독");

        GameSaveManager.Instance.OnGameSaveDataLoadedEvent -= OnGameSaveDataLoaded;
        GameSaveManager.Instance.OnGameSaveDataLoadedEvent += OnGameSaveDataLoaded;

        GameSaveManager.Instance.Initialize();
    }

    private void OnGameSaveDataLoaded()
    {
        float currentTime = Time.realtimeSinceStartup;
        float elapsedTime = currentTime - loadingStartTime;
        
        Debug.Log($"[LoadingManager] 세이브 데이터 로드 완료 이벤트 수신 - 경과 시간: {elapsedTime:F3}초");
        
        // 다른 매니저들 초기화
        InitializeOtherManagers();
        
        if (GameManager.Instance != null)
        {
            // GameManager 초기화 완료 이벤트 구독
            GameManager.Instance.OnGameInitializedEvent -= OnGameInitialized;
            GameManager.Instance.OnGameInitializedEvent += OnGameInitialized;
            
            GameManager.Instance.Initialize();
            Debug.Log("[LoadingManager] GameManager 초기화 시작");
        }
        else
        {
            Debug.LogError("[LoadingManager] GameManager.Instance가 null입니다.");
            // GameManager가 없어도 로딩은 종료
            HideLoading(LoadingType.Logo);
        }
    }

    private void OnGameInitialized()
    {
        float currentTime = Time.realtimeSinceStartup;
        float elapsedTime = currentTime - loadingStartTime;
        
        Debug.Log($"[LoadingManager] GameManager 초기화 완료 이벤트 수신 - 경과 시간: {elapsedTime:F3}초");
        
        // GameManager 이벤트 구독 해제
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameInitializedEvent -= OnGameInitialized;
        }
        
        // UI 초기화 (매니저들이 모두 준비된 후)
        InitializeUI();
        
        // 최소 로딩 시간 보장
        StartCoroutine(EnsureMinimumLoadingTime());
    }

    private IEnumerator EnsureMinimumLoadingTime()
    {
        float currentTime = Time.realtimeSinceStartup;
        float elapsedTime = currentTime - loadingStartTime;
        
        if (elapsedTime < MINIMUM_LOADING_TIME)
        {
            float remainingTime = MINIMUM_LOADING_TIME - elapsedTime;
            Debug.Log($"[LoadingManager] 최소 로딩 시간 보장 - 추가 대기: {remainingTime:F3}초");
            yield return new WaitForSeconds(remainingTime);
        }
        
        // 로딩 화면 숨기기
        HideLoading(LoadingType.Logo);
    }

    private void InitializeOtherManagers()
    {
        // AudioManager 초기화
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.Initialize();
        }

        // PopupManager 초기화 (WormManager보다 먼저)
        if (PopupManager.Instance != null)
        {
            PopupManager.Instance.Initialize();
        }

        // WormManager 초기화 (저장된 벌레 데이터와 함께)
        if (WormManager.Instance != null)
        {
            // 먼저 기본 초기화
            WormManager.Instance.Initialize();
            
            // 그 다음 저장된 벌레 데이터로 초기화
            var savedWorms = GameSaveManager.Instance?.GetWormData() ?? new List<WormData>();
            WormManager.Instance.Initialize(savedWorms);
            Debug.Log($"[LoadingManager] WormManager 초기화 - 저장된 벌레: {savedWorms.Count}마리");
        }

        // ItemManager 초기화
        if (ItemManager.Instance != null)
        {
            ItemManager.Instance.Initialize();
            
            // 저장된 아이템 데이터로 초기화
            var savedItemIds = GameSaveManager.Instance?.GetOwnedItemIds() ?? new List<string>();
            
            // 저장된 아이템이 없으면 초기 아이템 추가
            if (savedItemIds.Count == 0)
            {
                ItemManager.Instance.AddItem("100", 1, false);
                ItemManager.Instance.AddItem("200", 1, false);
                ItemManager.Instance.AddItem("300", 1, false);
                
                // 초기 아이템 착용
                ItemManager.Instance.EquipItem("100"); // Hat
                ItemManager.Instance.EquipItem("200"); // Face  
                ItemManager.Instance.EquipItem("300"); // Costume
                
                Debug.Log("[LoadingManager] 초기 아이템 추가 및 착용 완료");
            }
            else
            {
                ItemManager.Instance.Initialize(savedItemIds);
                
                // 저장된 착용 아이템 정보 로드
                var saveData = GameSaveManager.Instance?.currentSaveData;
                if (saveData != null)
                {
                    if (!string.IsNullOrEmpty(saveData.equippedHatId))
                        ItemManager.Instance.EquipItem(saveData.equippedHatId);
                    if (!string.IsNullOrEmpty(saveData.equippedFaceId))
                        ItemManager.Instance.EquipItem(saveData.equippedFaceId);
                    if (!string.IsNullOrEmpty(saveData.equippedCostumeId))
                        ItemManager.Instance.EquipItem(saveData.equippedCostumeId);
                }
            }
            
            Debug.Log($"[LoadingManager] ItemManager 초기화 - 아이템: {savedItemIds.Count}개");
        }

        // AchievementManager 초기화
        if (AchievementManager.Instance != null)
        {
            AchievementManager.Instance.Initialize();
        }

        // MapManager 초기화
        if (MapManager.Instance != null)
        {
            MapManager.Instance.Initialize();
        }

        // WormFamilyManager 제거됨 - 가계도 기능은 WormManager에서 처리

        // TabManager 초기화
        if (TabManager.Instance != null)
        {
            TabManager.Instance.Initialize();
        }

        // BottomBarManager 초기화
        if (BottomBarManager.Instance != null)
        {
            BottomBarManager.Instance.Initialize();
        }

        // TopBarManager 초기화
        if (TopBarManager.Instance != null)
        {
            TopBarManager.Instance.Initialize();
        }

        // 모든 매니저 초기화 완료 후 게임 데이터 저장
        if (GameSaveManager.Instance != null)
        {
            GameSaveManager.Instance.SaveGame();
            Debug.Log("[LoadingManager] 모든 매니저 초기화 완료 - 게임 데이터 저장");
        }
    }
    
    /// <summary>
    /// UI 초기화 (모든 매니저가 준비된 후 호출)
    /// </summary>
    private void InitializeUI()
    {
        Debug.Log("[LoadingManager] UI 초기화 시작");
        
        // UI 초기화를 코루틴으로 처리 (UI 오브젝트들이 준비될 때까지 대기)
        StartCoroutine(InitializeUIWhenReady());
    }
    
    /// <summary>
    /// UI 오브젝트들이 준비될 때까지 대기한 후 초기화
    /// </summary>
    private IEnumerator InitializeUIWhenReady()
    {
        Debug.Log("[LoadingManager] UI 오브젝트 대기 시작");
        
        // UI 오브젝트들이 활성화될 때까지 대기 (최대 10초로 연장)
        float timeout = 10f;
        float elapsed = 0f;
        
        while (elapsed < timeout)
        {
            // 비활성화된 오브젝트도 포함하여 검색
            var achieveTabUI = FindFirstObjectByType<AchieveTabUI>(FindObjectsInactive.Include);
            var itemTabUI = FindFirstObjectByType<ItemTabUI>(FindObjectsInactive.Include);
            var wormTabUI = FindFirstObjectByType<WormTabUI>(FindObjectsInactive.Include);
            
            // 디버깅 정보 추가
            if (elapsed % 2f < 0.1f) // 2초마다 한 번씩 로그 출력
            {
                Debug.Log($"[LoadingManager] UI 오브젝트 검색 중... (경과시간: {elapsed:F1}초)");
                Debug.Log($"[LoadingManager] AchieveTabUI: {(achieveTabUI != null ? $"발견 (활성: {achieveTabUI.gameObject.activeInHierarchy})" : "없음")}");
                Debug.Log($"[LoadingManager] ItemTabUI: {(itemTabUI != null ? $"발견 (활성: {itemTabUI.gameObject.activeInHierarchy})" : "없음")}");
                Debug.Log($"[LoadingManager] WormTabUI: {(wormTabUI != null ? $"발견 (활성: {wormTabUI.gameObject.activeInHierarchy})" : "없음")}");
            }
            
            if (achieveTabUI != null && itemTabUI != null && wormTabUI != null)
            {
                Debug.Log("[LoadingManager] 모든 UI 오브젝트 발견됨");
                break;
            }
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // AchieveTabUI 초기화
        var achieveTabUI_final = FindFirstObjectByType<AchieveTabUI>(FindObjectsInactive.Include);
        if (achieveTabUI_final != null)
        {
            achieveTabUI_final.ForceInitialize();
            Debug.Log("[LoadingManager] AchieveTabUI 초기화 완료");
        }
        else
        {
            Debug.LogWarning("[LoadingManager] AchieveTabUI를 찾을 수 없습니다. (타임아웃)");
        }
        
        // ItemTabUI 초기화
        var itemTabUI_final = FindFirstObjectByType<ItemTabUI>(FindObjectsInactive.Include);
        if (itemTabUI_final != null)
        {
            itemTabUI_final.ForceInitialize();
            Debug.Log("[LoadingManager] ItemTabUI 초기화 완료");
        }
        else
        {
            Debug.LogWarning("[LoadingManager] ItemTabUI를 찾을 수 없습니다. (타임아웃)");
        }
        
        // WormTabUI 초기화
        var wormTabUI_final = FindFirstObjectByType<WormTabUI>(FindObjectsInactive.Include);
        if (wormTabUI_final != null)
        {
            wormTabUI_final.ForceInitialize();
            Debug.Log("[LoadingManager] WormTabUI 초기화 완료");
        }
        else
        {
            Debug.LogWarning("[LoadingManager] WormTabUI를 찾을 수 없습니다. (타임아웃)");
        }
        
        Debug.Log("[LoadingManager] UI 초기화 완료");
    }

    public void ShowLoading(LoadingType type)
    {
        if (!isInitialized)
        {
            Debug.Log("[LoadingManager] 아직 초기화되지 않았습니다. 초기화를 기다립니다.");
            StartCoroutine(WaitForInitializationAndShowLoading(type));
            return;
        }

        var (canvasGroup, infoText) = GetLoadingComponents(type);
        if (canvasGroup != null && infoText != null)
        {
            ActivateLoadingCanvas(canvasGroup, infoText);
        }
    }

    private IEnumerator WaitForInitializationAndShowLoading(LoadingType type)
    {
        yield return new WaitUntil(() => isInitialized);
        ShowLoading(type);
    }

    public void HideLoading(LoadingType type, float fadeDuration = DEFAULT_FADE_DURATION)
    {
        float currentTime = Time.realtimeSinceStartup;
        float elapsedTime = currentTime - loadingStartTime;
        
        Debug.Log($"[LoadingManager] HideLoading 호출됨 - 타입: {type}, 초기화됨: {isInitialized}");
        Debug.Log($"[LoadingManager] 로딩 완료 시간 측정 - 경과 시간: {elapsedTime:F3}초");
        
        if (!isInitialized)
        {
            Debug.LogWarning("[LoadingManager] 아직 초기화되지 않았습니다.");
            return;
        }

        var (canvasGroup, _) = GetLoadingComponents(type);
        Debug.Log($"[LoadingManager] CanvasGroup 찾음: {canvasGroup != null}");
        
        if (canvasGroup != null)
        {
            Debug.Log($"[LoadingManager] FadeOut 시작 - 현재 알파: {canvasGroup.alpha}");
            StartFadeOut(canvasGroup, fadeDuration);
        }
        else
        {
            Debug.LogError($"[LoadingManager] CanvasGroup을 찾을 수 없습니다 - 타입: {type}");
        }
    }

    private (CanvasGroup canvasGroup, TMP_Text infoText) GetLoadingComponents(LoadingType type)
    {
        return type switch
        {
            LoadingType.Logo => (logoLoadingCanvasGroup, logoLoadingInfoText),
            LoadingType.MapChange => (mapLoadingCanvasGroup, mapLoadingInfoText),
            _ => (null, null)
        };
    }

    private void ActivateLoadingCanvas(CanvasGroup targetCanvasGroup, TMP_Text targetInfoText)
    {
        DeactivateAllLoadingCanvases();
        ActivateTargetCanvas(targetCanvasGroup);
        ShowRandomLoadingMessage(targetInfoText);
    }

    private void DeactivateAllLoadingCanvases()
    {
        DeactivateCanvas(logoLoadingCanvasGroup);
        DeactivateCanvas(mapLoadingCanvasGroup);
    }

    private void DeactivateCanvas(CanvasGroup canvasGroup)
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = INITIAL_ALPHA;
            canvasGroup.gameObject.SetActive(false);
        }
    }

    private void ActivateTargetCanvas(CanvasGroup canvasGroup)
    {
        if (canvasGroup != null)
        {
            canvasGroup.gameObject.SetActive(true);
            canvasGroup.alpha = TARGET_ALPHA;
        }
    }

    private void ShowRandomLoadingMessage(TMP_Text targetText)
    {
        if (targetText != null && infoMessages.Count > 0)
        {
            int index = Random.Range(0, infoMessages.Count);
            targetText.text = infoMessages[index];
        }
    }

    public void AddLoadingMessage(string message)
    {
        if (!string.IsNullOrEmpty(message) && !infoMessages.Contains(message))
        {
            infoMessages.Add(message);
        }
    }

    private void StartFadeOut(CanvasGroup canvasGroup, float duration)
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
        fadeCoroutine = StartCoroutine(FadeOutLoading(canvasGroup, duration));
    }

    private IEnumerator FadeOutLoading(CanvasGroup canvasGroup, float duration)
    {
        if (canvasGroup == null) 
        {
            Debug.LogError("[LoadingManager] FadeOutLoading: CanvasGroup이 null입니다.");
            yield break;
        }

        Debug.Log($"[LoadingManager] FadeOutLoading 시작 - 시작 알파: {canvasGroup.alpha}, 목표 알파: {FADE_OUT_TARGET_ALPHA}");
        
        float startAlpha = canvasGroup.alpha;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float normalizedTime = time / duration;
            
            // Easing 효과 추가 (부드러운 fade out)
            float easedTime = 1f - Mathf.Pow(1f - normalizedTime, 3f); // Ease Out Cubic
            canvasGroup.alpha = Mathf.Lerp(startAlpha, FADE_OUT_TARGET_ALPHA, easedTime);
            
            yield return null;
        }

        canvasGroup.alpha = FADE_OUT_TARGET_ALPHA;
        canvasGroup.gameObject.SetActive(false);
        
        Debug.Log($"[LoadingManager] FadeOutLoading 완료 - 최종 알파: {canvasGroup.alpha}");
    }

    private void OnDestroy()
    {
        if (GameSaveManager.Instance != null)
        {
            GameSaveManager.Instance.OnGameSaveDataLoadedEvent -= OnGameSaveDataLoaded;
        }
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameInitializedEvent -= OnGameInitialized;
        }
    }
    }
}
