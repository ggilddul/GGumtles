    using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.UI;
using GGumtles.Data;
using GGumtles.Managers;
using GGumtles.Utils;

namespace GGumtles.UI
{
    public class WormTabUI : MonoBehaviour
{
    public static WormTabUI Instance { get; private set; }

    [Header("UI 컴포넌트")]
    // [SerializeField] private OverViewRenderer overViewRenderer; // 제거됨
    [SerializeField] private TMP_Text ageText;
    [SerializeField] private TMP_Text stageText;
    [SerializeField] private TMP_Text genText;
    [SerializeField] private TMP_Text nameText;
    
    [Header("Worm Overview Panel")]
    [SerializeField] private GameObject wormOverviewPanel;
    [SerializeField] private Image wormSpriteImage;

    [Header("자동 새로고침 설정")]
    [SerializeField] private bool enableAutoRefresh = true;
    [SerializeField] private float refreshInterval = 1f;
    [SerializeField] private bool refreshOnEnable = true;

    [Header("디버그")]
    [SerializeField] private bool enableDebugLogs = false;

    // 상태 관리
    private bool isInitialized = false;
    private Coroutine autoRefreshCoroutine;
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
        StartCoroutine(WaitForManagersAndInitialize());
    }
    
    /// <summary>
    /// 매니저들이 초기화될 때까지 대기한 후 WormTabUI 초기화
    /// </summary>
    private IEnumerator WaitForManagersAndInitialize()
    {
        LogDebug("[WormTabUI] 매니저 초기화 대기 시작");
        
        // WormManager 인스턴스 대기
        while (WormManager.Instance == null)
        {
            LogDebug("[WormTabUI] WormManager.Instance 대기 중...");
            yield return null;
        }
        LogDebug("[WormTabUI] WormManager.Instance 확인됨");
        
        // WormManager 초기화 대기
        while (!WormManager.Instance.IsInitialized)
        {
            LogDebug("[WormTabUI] WormManager 초기화 대기 중...");
            yield return null;
        }
        LogDebug("[WormTabUI] WormManager 초기화 완료");
        
        // SpriteManager 인스턴스 대기
        while (SpriteManager.Instance == null)
        {
            LogDebug("[WormTabUI] SpriteManager.Instance 대기 중...");
            yield return null;
        }
        LogDebug("[WormTabUI] SpriteManager.Instance 확인됨");
        
        // 이제 안전하게 초기화
        if (!isInitialized)
        {
            InitializeWormTabUI();
            SubscribeToEvents();
            LogDebug("[WormTabUI] WormTabUI 초기화 완료");
        }
    }
    
    /// <summary>
    /// 외부에서 강제 초기화할 수 있는 메서드
    /// </summary>
    public void ForceInitialize()
    {
        if (!isInitialized)
        {
            // GameObject가 비활성화된 경우 직접 초기화
            if (!gameObject.activeInHierarchy)
            {
                InitializeWormTabUI();
                SubscribeToEvents();
                isInitialized = true;
            }
            else
            {
                StartCoroutine(WaitForManagersAndInitialize());
            }
        }
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
            // if (overViewRenderer == null)
            //     overViewRenderer = FindFirstObjectByType<OverViewRenderer>(); // 제거됨

            if (ageText == null)
                ageText = transform.Find("AgeText")?.GetComponent<TMP_Text>();

            if (stageText == null)
                stageText = transform.Find("StageText")?.GetComponent<TMP_Text>();

            if (genText == null)
                genText = transform.Find("GenText")?.GetComponent<TMP_Text>();

            if (nameText == null)
                nameText = transform.Find("NameText")?.GetComponent<TMP_Text>();
                
            // Worm Overview Panel 관련 컴포넌트 찾기
            if (wormOverviewPanel == null)
                wormOverviewPanel = transform.Find("WormOverviewPanel")?.gameObject;
                
            if (wormSpriteImage == null)
                wormSpriteImage = transform.Find("WormOverviewPanel/WormSpriteImage")?.GetComponent<Image>();

            LogDebug("[WormTabUI] WormTabUI 초기화 완료");
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
               false || // rarity 비교 제거
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

        // Worm Sprite 업데이트
        UpdateWormSprite(worm);
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
    /// Worm Sprite 업데이트
    /// </summary>
    private void UpdateWormSprite(WormData worm)
    {
        if (wormSpriteImage == null)
        {
            Debug.LogWarning("[WormTabUI] WormSpriteImage가 설정되지 않았습니다.");
            return;
        }
        
        try
        {
            // SpriteManager를 통해 현재 웜의 스프라이트 가져오기
            if (SpriteManager.Instance != null)
            {
                var completedWormSprite = SpriteManager.Instance.CreateCompletedWormSprite(worm);
                if (completedWormSprite != null && completedWormSprite.sprite != null)
                {
                    wormSpriteImage.sprite = completedWormSprite.sprite;
                    // Life Stage Scale 적용 (절대값으로 설정, 누적 방지)
                    wormSpriteImage.transform.localScale = Vector3.one * completedWormSprite.scale;
                    wormSpriteImage.enabled = true;
                    LogDebug($"[WormTabUI] Worm Sprite 업데이트: {worm.name}, Scale: {completedWormSprite.scale}");
                }
                else
                {
                    wormSpriteImage.enabled = false;
                    LogDebug("[WormTabUI] Worm Sprite를 찾을 수 없습니다.");
                }
            }
            else
            {
                Debug.LogWarning("[WormTabUI] SpriteManager.Instance가 null입니다.");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[WormTabUI] Worm Sprite 업데이트 중 오류: {ex.Message}");
        }
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
        info.AppendLine($"현재 웜: {(lastWormData != null ? lastWormData.name : "없음")}");

        return info.ToString();
    }
    
    /// <summary>
    /// 이벤트 구독
    /// </summary>
    private void SubscribeToEvents()
    {
        // TabManager 이벤트 구독
        if (TabManager.Instance != null)
        {
            TabManager.Instance.OnTabChangedEvent += OnTabChanged;
            LogDebug("[WormTabUI] TabManager 이벤트 구독 완료");
        }
        
        // WormManager 이벤트 구독
        if (WormManager.Instance != null)
        {
            // WormManager에 이벤트가 있다면 구독
            LogDebug("[WormTabUI] WormManager 이벤트 구독 완료");
        }
    }
    
    /// <summary>
    /// 이벤트 구독 해제
    /// </summary>
    private void UnsubscribeFromEvents()
    {
        // TabManager 이벤트 구독 해제
        if (TabManager.Instance != null)
        {
            TabManager.Instance.OnTabChangedEvent -= OnTabChanged;
            LogDebug("[WormTabUI] TabManager 이벤트 구독 해제 완료");
        }
    }
    
    /// <summary>
    /// 탭 변경 시 호출
    /// </summary>
    private void OnTabChanged(int fromIndex, int toIndex, TabManager.TabType fromType, TabManager.TabType toType)
    {
        // Worm 탭으로 변경되었을 때만 UI 업데이트
        if (toType == TabManager.TabType.Worm)
        {
            LogDebug("[WormTabUI] Worm 탭으로 변경됨 - UI 업데이트");
            UpdateUI();
        }
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

        // 이벤트 구독 해제
        UnsubscribeFromEvents();
        OnWormTabRefreshedEvent = null;
        OnWormDataChangedEvent = null;
    }
    }
}
