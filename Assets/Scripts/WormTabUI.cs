    using UnityEngine;
using System.Collections;
using TMPro;

public class WormTabUI : MonoBehaviour
{
    public static WormTabUI Instance { get; private set; }

    [Header("UI 컴포넌트")]
    [SerializeField] private OverViewRenderer overViewRenderer;
    [SerializeField] private TMP_Text ageText;
    [SerializeField] private TMP_Text stageText;
    [SerializeField] private TMP_Text genText;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text rarityText;
    [SerializeField] private TMP_Text statusText;

    [Header("자동 새로고침 설정")]
    [SerializeField] private bool enableAutoRefresh = true;
    [SerializeField] private float refreshInterval = 1f;
    [SerializeField] private bool refreshOnEnable = true;

    [Header("애니메이션 설정")]
    [SerializeField] private bool enableTextAnimation = true;
    [SerializeField] private float textAnimationDuration = 0.3f;
    [SerializeField] private Color highlightColor = Color.yellow;
    [SerializeField] private Color normalColor = Color.white;

    [Header("디버그")]
    [SerializeField] private bool enableDebugLogs = false;

    // 상태 관리
    private bool isInitialized = false;
    private Coroutine autoRefreshCoroutine;
    private Coroutine textAnimationCoroutine;
    private WormData lastWormData;

    // 이벤트 정의
    public delegate void OnWormTabRefreshed(WormData wormData);
    public delegate void OnWormDataChanged(WormData oldData, WormData newData);
    public event OnWormTabRefreshed OnWormTabRefreshedEvent;
    public event OnWormDataChanged OnWormDataChangedEvent;

    // 프로퍼티
    public bool IsAutoRefreshEnabled => enableAutoRefresh;
    public float RefreshInterval => refreshInterval;
    public bool IsInitialized => isInitialized;

    private void Awake()
    {
        InitializeSingleton();
    }

    private void Start()
    {
        InitializeWormTabUI();
    }

    private void InitializeSingleton()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeWormTabUI()
    {
        try
        {
            // 자동 컴포넌트 찾기
            if (overViewRenderer == null)
                overViewRenderer = FindFirstObjectByType<OverViewRenderer>();

            if (ageText == null)
                ageText = transform.Find("AgeText")?.GetComponent<TMP_Text>();

            if (stageText == null)
                stageText = transform.Find("StageText")?.GetComponent<TMP_Text>();

            if (genText == null)
                genText = transform.Find("GenText")?.GetComponent<TMP_Text>();

            if (nameText == null)
                nameText = transform.Find("NameText")?.GetComponent<TMP_Text>();

            if (rarityText == null)
                rarityText = transform.Find("RarityText")?.GetComponent<TMP_Text>();

            if (statusText == null)
                statusText = transform.Find("StatusText")?.GetComponent<TMP_Text>();

            isInitialized = true;
            LogDebug("[WormTabUI] 초기화 완료");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[WormTabUI] 초기화 중 오류: {ex.Message}");
        }
    }

    private void OnEnable()
    {
        if (refreshOnEnable)
        {
            UpdateUI();
        }

        if (enableAutoRefresh)
        {
            StartAutoRefresh();
        }
    }

    private void OnDisable()
    {
        StopAutoRefresh();
    }

    /// <summary>
    /// 자동 새로고침 시작
    /// </summary>
    private void StartAutoRefresh()
    {
        if (autoRefreshCoroutine != null)
        {
            StopCoroutine(autoRefreshCoroutine);
        }

        autoRefreshCoroutine = StartCoroutine(AutoRefreshRoutine());
    }

    /// <summary>
    /// 자동 새로고침 중지
    /// </summary>
    private void StopAutoRefresh()
    {
        if (autoRefreshCoroutine != null)
        {
            StopCoroutine(autoRefreshCoroutine);
            autoRefreshCoroutine = null;
        }
    }

    /// <summary>
    /// 자동 새로고침 코루틴
    /// </summary>
    private IEnumerator AutoRefreshRoutine()
    {
        while (true)
        {
            UpdateUI();
            yield return new WaitForSeconds(refreshInterval);
        }
    }

    /// <summary>
    /// UI 업데이트
    /// </summary>
    public void UpdateUI()
    {
        try
        {
            var currentWorm = WormManager.Instance?.GetCurrentWorm();
            
            // 웜 데이터 변경 감지
            if (HasWormDataChanged(lastWormData, currentWorm))
            {
                OnWormDataChangedEvent?.Invoke(lastWormData, currentWorm);
                lastWormData = currentWorm;
            }

            if (currentWorm == null)
            {
                SetEmptyUI();
                return;
            }

            UpdateWormInfo(currentWorm);
            OnWormTabRefreshedEvent?.Invoke(currentWorm);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[WormTabUI] UI 업데이트 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 웜 데이터 변경 감지
    /// </summary>
    private bool HasWormDataChanged(WormData oldData, WormData newData)
    {
        if (oldData == null && newData == null) return false;
        if (oldData == null || newData == null) return true;

        return oldData.wormId != newData.wormId ||
               oldData.age != newData.age ||
               oldData.lifeStage != newData.lifeStage ||
               oldData.generation != newData.generation ||
               oldData.name != newData.name ||
               oldData.rarity != newData.rarity ||
               oldData.isAlive != newData.isAlive;
    }

    /// <summary>
    /// 빈 UI 설정
    /// </summary>
    private void SetEmptyUI()
    {
        SetTextSafely(ageText, "-");
        SetTextSafely(stageText, "-");
        SetTextSafely(genText, "-");
        SetTextSafely(nameText, "-");
        SetTextSafely(rarityText, "-");
        SetTextSafely(statusText, "-");
    }

    /// <summary>
    /// 웜 정보 업데이트
    /// </summary>
    private void UpdateWormInfo(WormData worm)
    {
        SetTextSafely(ageText, FormatAge(worm.age));
        SetTextSafely(stageText, GetLifeStageName(worm.lifeStage));
        SetTextSafely(genText, $"{worm.generation}세대");
        SetTextSafely(nameText, worm.name);
        SetTextSafely(rarityText, GetRarityText(worm.rarity));
        SetTextSafely(statusText, GetStatusText(worm));

        // 텍스트 애니메이션
        if (enableTextAnimation)
        {
            StartTextAnimation();
        }
    }

    /// <summary>
    /// 안전한 텍스트 설정
    /// </summary>
    private void SetTextSafely(TMP_Text textComponent, string text)
    {
        if (textComponent != null)
        {
            textComponent.text = text;
        }
    }

    /// <summary>
    /// 나이 포맷팅
    /// </summary>
    private string FormatAge(int ageInMinutes)
    {
        int days = ageInMinutes / 1440;
        int hours = (ageInMinutes % 1440) / 60;
        int minutes = ageInMinutes % 60;

        string result = "";
        if (days > 0) result += $"{days}일 ";
        if (hours > 0) result += $"{hours}시간 ";
        if (minutes > 0 || result == "") result += $"{minutes}분";

        return result.Trim();
    }

    /// <summary>
    /// 생명주기 단계 이름 반환
    /// </summary>
    private string GetLifeStageName(int stage)
    {
        return stage switch
        {
            0 => "알",
            1 => "제 1 유충기",
            2 => "제 2 유충기",
            3 => "제 3 유충기",
            4 => "제 4 유충기",
            5 => "성체",
            6 => "영혼",
            _ => "?"
        };
    }

    /// <summary>
    /// 희귀도 텍스트 반환
    /// </summary>
    private string GetRarityText(WormData.WormRarity rarity)
    {
        return rarity switch
        {
            WormData.WormRarity.Common => "일반",
            WormData.WormRarity.Uncommon => "희귀",
            WormData.WormRarity.Rare => "매우 희귀",
            WormData.WormRarity.Legendary => "전설",
            _ => "?"
        };
    }

    /// <summary>
    /// 상태 텍스트 반환
    /// </summary>
    private string GetStatusText(WormData worm)
    {
        if (!worm.IsAlive)
            return "사망";

        if (worm.lifeStage == 0)
            return "부화 대기";

        if (worm.lifeStage == 6)
            return "영혼 상태";

        return "생존";
    }

    /// <summary>
    /// 텍스트 애니메이션 시작
    /// </summary>
    private void StartTextAnimation()
    {
        if (textAnimationCoroutine != null)
        {
            StopCoroutine(textAnimationCoroutine);
        }

        textAnimationCoroutine = StartCoroutine(TextAnimationCoroutine());
    }

    /// <summary>
    /// 텍스트 애니메이션 코루틴
    /// </summary>
    private IEnumerator TextAnimationCoroutine()
    {
        // 모든 텍스트 컴포넌트를 하이라이트 색상으로 변경
        TMP_Text[] textComponents = { ageText, stageText, genText, nameText, rarityText, statusText };
        Color[] originalColors = new Color[textComponents.Length];

        for (int i = 0; i < textComponents.Length; i++)
        {
            if (textComponents[i] != null)
            {
                originalColors[i] = textComponents[i].color;
                textComponents[i].color = highlightColor;
            }
        }

        // 애니메이션 지속
        yield return new WaitForSeconds(textAnimationDuration);

        // 원래 색상으로 복원
        for (int i = 0; i < textComponents.Length; i++)
        {
            if (textComponents[i] != null)
            {
                textComponents[i].color = originalColors[i];
            }
        }

        textAnimationCoroutine = null;
    }

    /// <summary>
    /// 자동 새로고침 활성화/비활성화
    /// </summary>
    public void SetAutoRefreshEnabled(bool enabled)
    {
        enableAutoRefresh = enabled;
        
        if (enabled && gameObject.activeInHierarchy)
        {
            StartAutoRefresh();
        }
        else
        {
            StopAutoRefresh();
        }

        LogDebug($"[WormTabUI] 자동 새로고침 {(enabled ? "활성화" : "비활성화")}");
    }

    /// <summary>
    /// 새로고침 간격 설정
    /// </summary>
    public void SetRefreshInterval(float interval)
    {
        refreshInterval = Mathf.Max(0.1f, interval);
        LogDebug($"[WormTabUI] 새로고침 간격 설정: {refreshInterval}초");
    }

    /// <summary>
    /// 텍스트 애니메이션 활성화/비활성화
    /// </summary>
    public void SetTextAnimationEnabled(bool enabled)
    {
        enableTextAnimation = enabled;
        LogDebug($"[WormTabUI] 텍스트 애니메이션 {(enabled ? "활성화" : "비활성화")}");
    }

    /// <summary>
    /// 하이라이트 색상 설정
    /// </summary>
    public void SetHighlightColor(Color color)
    {
        highlightColor = color;
        LogDebug($"[WormTabUI] 하이라이트 색상 설정: {color}");
    }

    /// <summary>
    /// 수동 새로고침
    /// </summary>
    public void RefreshUI()
    {
        UpdateUI();
        LogDebug("[WormTabUI] 수동 새로고침 실행");
    }

    /// <summary>
    /// 웜 탭 UI 정보 반환
    /// </summary>
    public string GetWormTabInfo()
    {
        var info = new System.Text.StringBuilder();
        info.AppendLine($"[WormTabUI 정보]");
        info.AppendLine($"초기화됨: {isInitialized}");
        info.AppendLine($"자동 새로고침: {(enableAutoRefresh ? "활성화" : "비활성화")}");
        info.AppendLine($"새로고침 간격: {refreshInterval}초");
        info.AppendLine($"텍스트 애니메이션: {(enableTextAnimation ? "활성화" : "비활성화")}");
        info.AppendLine($"현재 웜: {(lastWormData != null ? lastWormData.name : "없음")}");

        return info.ToString();
    }

    private void LogDebug(string message)
    {
        if (enableDebugLogs)
        {
            Debug.Log(message);
        }
    }

    private void OnDestroy()
    {
        StopAutoRefresh();
        
        if (textAnimationCoroutine != null)
        {
            StopCoroutine(textAnimationCoroutine);
        }

        // 이벤트 구독 해제
        OnWormTabRefreshedEvent = null;
        OnWormDataChangedEvent = null;
    }
}
