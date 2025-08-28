using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

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
    private const float DEFAULT_FADE_DURATION = 1f;
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
        ShowLoading(LoadingType.Logo);
        StartCoroutine(SubscribeToGameSaveManager());
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
        Debug.Log("[LoadingManager] 세이브 데이터 로드 완료 이벤트 수신");
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.Initialize();
        }
        else
        {
            Debug.LogError("[LoadingManager] GameManager.Instance가 null입니다.");
        }
    }

    public void ShowLoading(LoadingType type)
    {
        if (!isInitialized)
        {
            Debug.LogWarning("[LoadingManager] 아직 초기화되지 않았습니다.");
            return;
        }

        var (canvasGroup, infoText) = GetLoadingComponents(type);
        if (canvasGroup != null && infoText != null)
        {
            ActivateLoadingCanvas(canvasGroup, infoText);
        }
    }

    public void HideLoading(LoadingType type, float fadeDuration = DEFAULT_FADE_DURATION)
    {
        if (!isInitialized)
        {
            Debug.LogWarning("[LoadingManager] 아직 초기화되지 않았습니다.");
            return;
        }

        var (canvasGroup, _) = GetLoadingComponents(type);
        if (canvasGroup != null)
        {
            StartFadeOut(canvasGroup, fadeDuration);
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
        if (canvasGroup == null) yield break;

        float startAlpha = canvasGroup.alpha;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float normalizedTime = time / duration;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, FADE_OUT_TARGET_ALPHA, normalizedTime);
            yield return null;
        }

        canvasGroup.alpha = FADE_OUT_TARGET_ALPHA;
        canvasGroup.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        if (GameSaveManager.Instance != null)
        {
            GameSaveManager.Instance.OnGameSaveDataLoadedEvent -= OnGameSaveDataLoaded;
        }
    }
}
