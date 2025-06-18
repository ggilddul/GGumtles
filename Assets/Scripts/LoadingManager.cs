using UnityEngine;
using TMPro;
using System.Collections;

public class LoadingManager : MonoBehaviour
{
    public static LoadingManager Instance { get; private set; }

    [Header("접속 로딩")]
    public CanvasGroup logoLoadingCanvasGroup;
    public TMP_Text logoLoadingInfoText;

    [Header("맵 변경 로딩")]
    public CanvasGroup mapLoadingCanvasGroup;
    public TMP_Text mapLoadingInfoText;

    private string[] infoMessages = {
        "꿈틀즈에 오신 걸 환영해요!",
        "알고계셨나요?\n예쁜꼬마선충을 이용한 4번의 연구는 노벨상을 수상했어요!"
    };

    private Coroutine fadeCoroutine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // 최초 접속 로딩 화면 보여주기
            ShowLoading(LoadingType.Logo);

            StartCoroutine(SubscribeToGameSaveManager());
        }
        else
        {
            Destroy(gameObject);
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

        GameManager.Instance.Initialize();
    }

    public enum LoadingType
    {
        Logo,
        MapChange
    }

    // public으로 노출하는 ShowLoading, HideLoading에 LoadingType 인자 추가
    public void ShowLoading(LoadingType type)
    {
        switch (type)
        {
            case LoadingType.Logo:
                ActivateLoadingCanvas(logoLoadingCanvasGroup, logoLoadingInfoText);
                break;
            case LoadingType.MapChange:
                ActivateLoadingCanvas(mapLoadingCanvasGroup, mapLoadingInfoText);
                break;
        }
    }

    public void HideLoading(LoadingType type, float fadeDuration = 1f)
    {
        switch (type)
        {
            case LoadingType.Logo:
                StartFadeOut(logoLoadingCanvasGroup, fadeDuration);
                break;
            case LoadingType.MapChange:
                StartFadeOut(mapLoadingCanvasGroup, fadeDuration);
                break;
        }
    }

    private void ActivateLoadingCanvas(CanvasGroup canvasGroup, TMP_Text infoText)
    {
        logoLoadingCanvasGroup.alpha = 0f;
        logoLoadingCanvasGroup.gameObject.SetActive(false);

        mapLoadingCanvasGroup.alpha = 0f;
        mapLoadingCanvasGroup.gameObject.SetActive(false);

        canvasGroup.gameObject.SetActive(true);
        canvasGroup.alpha = 1f;

        ShowRandomLoadingMessage(infoText);
    }

    private void ShowRandomLoadingMessage(TMP_Text targetText)
    {
        int index = Random.Range(0, infoMessages.Length);
        targetText.text = infoMessages[index];
    }

    private void StartFadeOut(CanvasGroup canvasGroup, float duration)
    {
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeOutLoading(canvasGroup, duration));
    }

    private IEnumerator FadeOutLoading(CanvasGroup canvasGroup, float duration)
    {
        float startAlpha = canvasGroup.alpha;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, time / duration);
            yield return null;
        }

        canvasGroup.alpha = 0f;
        canvasGroup.gameObject.SetActive(false);
    }
}
