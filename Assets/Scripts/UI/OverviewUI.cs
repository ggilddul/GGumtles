using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OverviewUI : MonoBehaviour
{
    [Header("렌더러")]
    [SerializeField] private OverViewRenderer overViewRenderer;

    [Header("UI 컴포넌트")]
    [SerializeField] private Image overviewImage;
    [SerializeField] private TMP_Text partNameText;
    [SerializeField] private TMP_Text partDescriptionText;
    [SerializeField] private GameObject highlightEffect;
    [SerializeField] private Button refreshButton;

    [Header("설정")]
    [SerializeField] private bool enableDebugLogs = false;
    [SerializeField] private bool enableHighlightEffect = true;
    [SerializeField] private bool autoRefreshOnPartChange = true;

    [Header("하이라이트 효과")]
    [SerializeField] private float highlightDuration = 0.5f;
    [SerializeField] private AnimationCurve highlightCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    // 상태 관리
    private bool isInitialized = false;
    private Coroutine highlightCoroutine;
    private string lastChangedPart = "";

    // 이벤트 정의
    public delegate void OnPartChanged(string partName, Sprite newSprite);
    public delegate void OnOverviewRefreshed();
    public event OnPartChanged OnPartChangedEvent;
    public event OnOverviewRefreshed OnOverviewRefreshedEvent;

    // 프로퍼티
    public OverViewRenderer OverViewRenderer => overViewRenderer;
    public bool IsHighlighting => highlightCoroutine != null;
    public string LastChangedPart => lastChangedPart;

    private void Awake()
    {
        InitializeOverviewUI();
    }

    private void OnEnable()
    {
        SubscribeToEvents();
    }

    private void OnDisable()
    {
        UnsubscribeFromEvents();
    }

    private void InitializeOverviewUI()
    {
        try
        {
            // 자동으로 컴포넌트 찾기
            if (overViewRenderer == null)
                overViewRenderer = GetComponentInChildren<OverViewRenderer>();

            if (overviewImage == null)
                overviewImage = GetComponentInChildren<Image>();

            SetupUI();

            isInitialized = true;
            LogDebug("[OverviewUI] 오버뷰 UI 초기화 완료");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[OverviewUI] 초기화 중 오류: {ex.Message}");
        }
    }

    private void SetupUI()
    {
        try
        {
            // 새로고침 버튼 설정
            if (refreshButton != null)
            {
                refreshButton.onClick.RemoveAllListeners();
                refreshButton.onClick.AddListener(RefreshOverview);
            }

            // 초기 오버뷰 렌더링
            if (overViewRenderer != null)
            {
                RefreshOverview();
            }

            LogDebug("[OverviewUI] UI 설정 완료");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[OverviewUI] UI 설정 중 오류: {ex.Message}");
        }
    }

    private void SubscribeToEvents()
    {
        try
        {
            if (overViewRenderer != null)
            {
                overViewRenderer.PartChanged += HandlePartChanged;
                LogDebug("[OverviewUI] 이벤트 구독 완료");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[OverviewUI] 이벤트 구독 중 오류: {ex.Message}");
        }
    }

    private void UnsubscribeFromEvents()
    {
        try
        {
            if (overViewRenderer != null)
            {
                overViewRenderer.PartChanged -= HandlePartChanged;
                LogDebug("[OverviewUI] 이벤트 구독 해제 완료");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[OverviewUI] 이벤트 구독 해제 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 파트 변경 이벤트 처리
    /// </summary>
    private void HandlePartChanged(string partName, Sprite newSprite)
    {
        try
        {
            lastChangedPart = partName;

            // UI 업데이트
            UpdatePartInfo(partName, newSprite);

            // 하이라이트 효과
            if (enableHighlightEffect)
            {
                StartHighlightEffect();
            }

            // 자동 새로고침
            if (autoRefreshOnPartChange)
            {
                RefreshOverview();
            }

            // 이벤트 발생
            OnPartChangedEvent?.Invoke(partName, newSprite);

            LogDebug($"[OverviewUI] 파트 변경: {partName}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[OverviewUI] 파트 변경 처리 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 파트 정보 업데이트
    /// </summary>
    private void UpdatePartInfo(string partName, Sprite newSprite)
    {
        try
        {
            // 파트 이름 텍스트
            if (partNameText != null)
            {
                partNameText.text = GetPartDisplayName(partName);
            }

            // 파트 설명 텍스트
            if (partDescriptionText != null)
            {
                partDescriptionText.text = GetPartDescription(partName);
            }

            // 오버뷰 이미지 업데이트
            if (overviewImage != null && overViewRenderer != null)
            {
                overviewImage.sprite = overViewRenderer.RenderOverviewSprite();
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[OverviewUI] 파트 정보 업데이트 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 파트 표시 이름 가져오기
    /// </summary>
    private string GetPartDisplayName(string partName)
    {
        return partName switch
        {
            "Body" => "몸체",
            "Face" => "얼굴",
            "Hat" => "모자",
            "Costume" => "의상",
            "Accessory" => "액세서리",
            _ => partName
        };
    }

    /// <summary>
    /// 파트 설명 가져오기
    /// </summary>
    private string GetPartDescription(string partName)
    {
        return partName switch
        {
            "Body" => "벌레의 생명주기에 따른 몸체입니다.",
            "Face" => "벌레의 표정을 나타내는 얼굴입니다.",
            "Hat" => "벌레가 착용한 모자입니다.",
            "Costume" => "벌레가 착용한 의상입니다.",
            "Accessory" => "벌레가 착용한 액세서리입니다.",
            _ => "파트 정보가 없습니다."
        };
    }

    /// <summary>
    /// 오버뷰 새로고침
    /// </summary>
    public void RefreshOverview()
    {
        try
        {
            if (overViewRenderer != null)
            {
                overViewRenderer.RefreshOverview();
                
                if (overviewImage != null)
                {
                    overviewImage.sprite = overViewRenderer.RenderOverviewSprite();
                }

                OnOverviewRefreshedEvent?.Invoke();
                LogDebug("[OverviewUI] 오버뷰 새로고침 완료");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[OverviewUI] 오버뷰 새로고침 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 하이라이트 효과 시작
    /// </summary>
    private void StartHighlightEffect()
    {
        if (highlightCoroutine != null)
        {
            StopCoroutine(highlightCoroutine);
        }

        highlightCoroutine = StartCoroutine(HighlightEffectCoroutine());
    }

    /// <summary>
    /// 하이라이트 효과 코루틴
    /// </summary>
    private System.Collections.IEnumerator HighlightEffectCoroutine()
    {
        if (highlightEffect == null) yield break;

        highlightEffect.SetActive(true);

        float elapsed = 0f;
        Vector3 originalScale = highlightEffect.transform.localScale;

        while (elapsed < highlightDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / highlightDuration;
            float curveValue = highlightCurve.Evaluate(progress);

            // 스케일 애니메이션
            highlightEffect.transform.localScale = Vector3.Lerp(originalScale * 0.8f, originalScale * 1.2f, curveValue);

            // 알파 애니메이션
            var canvasGroup = highlightEffect.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, curveValue);
            }

            yield return null;
        }

        // 원래 상태로 복원
        highlightEffect.transform.localScale = originalScale;
        if (highlightEffect.GetComponent<CanvasGroup>() != null)
        {
            highlightEffect.GetComponent<CanvasGroup>().alpha = 0f;
        }

        highlightEffect.SetActive(false);
        highlightCoroutine = null;
    }

    /// <summary>
    /// 특정 파트 강조 표시
    /// </summary>
    public void HighlightPart(string partName)
    {
        try
        {
            if (overViewRenderer != null)
            {
                // 특정 파트만 강조하는 로직 (예: 색상 변경, 테두리 효과 등)
                LogDebug($"[OverviewUI] 파트 강조: {partName}");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[OverviewUI] 파트 강조 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 오버뷰 이미지 설정
    /// </summary>
    public void SetOverviewImage(Sprite sprite)
    {
        try
        {
            if (overviewImage != null)
            {
                overviewImage.sprite = sprite;
                LogDebug("[OverviewUI] 오버뷰 이미지 설정 완료");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[OverviewUI] 오버뷰 이미지 설정 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 하이라이트 효과 활성화/비활성화
    /// </summary>
    public void SetHighlightEnabled(bool enabled)
    {
        enableHighlightEffect = enabled;
        LogDebug($"[OverviewUI] 하이라이트 효과 {(enabled ? "활성화" : "비활성화")}");
    }

    /// <summary>
    /// 자동 새로고침 활성화/비활성화
    /// </summary>
    public void SetAutoRefreshEnabled(bool enabled)
    {
        autoRefreshOnPartChange = enabled;
        LogDebug($"[OverviewUI] 자동 새로고침 {(enabled ? "활성화" : "비활성화")}");
    }

    /// <summary>
    /// 오버뷰 UI 정보 반환
    /// </summary>
    public string GetOverviewInfo()
    {
        var info = new System.Text.StringBuilder();
        info.AppendLine($"[OverviewUI 정보]");
        info.AppendLine($"초기화됨: {isInitialized}");
        info.AppendLine($"렌더러 연결: {(overViewRenderer != null ? "연결됨" : "연결 안됨")}");
        info.AppendLine($"마지막 변경 파트: {lastChangedPart}");
        info.AppendLine($"하이라이트 효과: {(enableHighlightEffect ? "활성화" : "비활성화")}");
        info.AppendLine($"자동 새로고침: {(autoRefreshOnPartChange ? "활성화" : "비활성화")}");

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
        if (highlightCoroutine != null)
        {
            StopCoroutine(highlightCoroutine);
        }

        // 이벤트 구독 해제
        OnPartChangedEvent = null;
        OnOverviewRefreshedEvent = null;
    }
}
