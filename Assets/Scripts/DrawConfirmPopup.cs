using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DrawConfirmPopup : MonoBehaviour
{
    [Header("UI 컴포넌트")]
    [SerializeField] private TMP_Text diamondCountText;
    [SerializeField] private TMP_Text drawCostText;
    [SerializeField] private TMP_Text drawDescriptionText;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;
    [SerializeField] private Button closeButton;

    [Header("설정")]
    [SerializeField] private bool enableSound = true;
    [SerializeField] private bool enableDebugLogs = false;
    [SerializeField] private int drawCost = 10; // 뽑기 비용

    [Header("애니메이션")]
    [SerializeField] private bool enableAnimations = true;
    [SerializeField] private float showDuration = 0.3f;
    [SerializeField] private float hideDuration = 0.2f;
    [SerializeField] private AnimationCurve showCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private AnimationCurve hideCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    // 상태 관리
    private bool isShowing = false;
    private bool isAnimating = false;
    private Coroutine showCoroutine;
    private Coroutine hideCoroutine;

    // 이벤트 정의
    public delegate void OnDrawConfirmed();
    public delegate void OnDrawCancelled();
    public delegate void OnDrawPopupShown();
    public delegate void OnDrawPopupHidden();
    public event OnDrawConfirmed OnDrawConfirmedEvent;
    public event OnDrawCancelled OnDrawCancelledEvent;
    public event OnDrawPopupShown OnDrawPopupShownEvent;
    public event OnDrawPopupHidden OnDrawPopupHiddenEvent;

    // 프로퍼티
    public bool IsShowing => isShowing;
    public bool IsAnimating => isAnimating;
    public int DrawCost => drawCost;

    private void Awake()
    {
        InitializeComponents();
    }

    private void InitializeComponents()
    {
        try
        {
            // 자동으로 컴포넌트 찾기
            if (diamondCountText == null)
                diamondCountText = GetComponentInChildren<TMP_Text>();

            if (confirmButton == null)
                confirmButton = GetComponentInChildren<Button>();

            SetupButtonEvents();

            LogDebug("[DrawConfirmPopup] 컴포넌트 초기화 완료");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[DrawConfirmPopup] 컴포넌트 초기화 중 오류: {ex.Message}");
        }
    }

    private void SetupButtonEvents()
    {
        try
        {
            if (confirmButton != null)
            {
                confirmButton.onClick.RemoveAllListeners();
                confirmButton.onClick.AddListener(OnConfirmClicked);
            }

            if (cancelButton != null)
            {
                cancelButton.onClick.RemoveAllListeners();
                cancelButton.onClick.AddListener(OnCancelClicked);
            }

            if (closeButton != null)
            {
                closeButton.onClick.RemoveAllListeners();
                closeButton.onClick.AddListener(OnCancelClicked);
            }

            LogDebug("[DrawConfirmPopup] 버튼 이벤트 설정 완료");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[DrawConfirmPopup] 버튼 이벤트 설정 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 뽑기 확인 팝업 표시
    /// </summary>
    public void Show()
    {
        try
        {
            if (isShowing) return;

            UpdateUI();
            
            if (enableAnimations)
            {
                StartShowAnimation();
            }
            else
            {
                ShowImmediate();
            }

            LogDebug("[DrawConfirmPopup] 뽑기 확인 팝업 표시");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[DrawConfirmPopup] 팝업 표시 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 뽑기 확인 팝업 숨김
    /// </summary>
    public void Hide()
    {
        try
        {
            if (!isShowing) return;

            if (enableAnimations)
            {
                StartHideAnimation();
            }
            else
            {
                HideImmediate();
            }

            LogDebug("[DrawConfirmPopup] 뽑기 확인 팝업 숨김");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[DrawConfirmPopup] 팝업 숨김 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 즉시 표시
    /// </summary>
    private void ShowImmediate()
    {
        gameObject.SetActive(true);
        isShowing = true;
        OnDrawPopupShownEvent?.Invoke();
    }

    /// <summary>
    /// 즉시 숨김
    /// </summary>
    private void HideImmediate()
    {
        gameObject.SetActive(false);
        isShowing = false;
        OnDrawPopupHiddenEvent?.Invoke();
    }

    /// <summary>
    /// UI 업데이트
    /// </summary>
    private void UpdateUI()
    {
        try
        {
            // 현재 다이아몬드 수 표시
            if (diamondCountText != null)
            {
                int currentDiamonds = GameManager.Instance?.diamondCount ?? 0;
                diamondCountText.text = $"보유 다이아몬드 수: {currentDiamonds:N0}";
            }

            // 뽑기 비용 표시
            if (drawCostText != null)
            {
                drawCostText.text = $"뽑기 비용: {drawCost:N0} 다이아몬드";
            }

            // 뽑기 설명 표시
            if (drawDescriptionText != null)
            {
                drawDescriptionText.text = "아이템을 뽑으시겠습니까?\n확률에 따라 희귀한 아이템을 얻을 수 있습니다.";
            }

            // 확인 버튼 활성화/비활성화 (다이아몬드 부족 시)
            if (confirmButton != null)
            {
                int currentDiamonds = GameManager.Instance?.diamondCount ?? 0;
                bool canDraw = currentDiamonds >= drawCost;
                confirmButton.interactable = canDraw;
                
                // 버튼 색상 변경
                var buttonColors = confirmButton.colors;
                buttonColors.normalColor = canDraw ? Color.white : Color.gray;
                confirmButton.colors = buttonColors;
            }

            LogDebug("[DrawConfirmPopup] UI 업데이트 완료");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[DrawConfirmPopup] UI 업데이트 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 확인 버튼 클릭
    /// </summary>
    private void OnConfirmClicked()
    {
        try
        {
            // 사운드 재생
            if (enableSound)
            {
                AudioManager.Instance?.PlaySFX(AudioManager.SFXType.Button);
            }

            // 다이아몬드 차감
            if (GameManager.Instance != null)
            {
                int currentDiamonds = GameManager.Instance.diamondCount;
                if (currentDiamonds >= drawCost)
                {
                    GameManager.Instance.diamondCount -= drawCost;
                    
                    // 뽑기 실행
                    ExecuteDraw();
                    
                    // 팝업 숨김
                    Hide();
                    
                    LogDebug($"[DrawConfirmPopup] 뽑기 실행 완료 - 소모 다이아몬드: {drawCost}");
                }
                else
                {
                    Debug.LogWarning("[DrawConfirmPopup] 다이아몬드가 부족합니다.");
                    PopupManager.Instance?.ShowToast("다이아몬드가 부족합니다!");
                }
            }

            OnDrawConfirmedEvent?.Invoke();
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[DrawConfirmPopup] 확인 버튼 클릭 처리 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 취소 버튼 클릭
    /// </summary>
    private void OnCancelClicked()
    {
        try
        {
            // 사운드 재생
            if (enableSound)
            {
                AudioManager.Instance?.PlaySFX(AudioManager.SFXType.Button);
            }

            Hide();
            OnDrawCancelledEvent?.Invoke();

            LogDebug("[DrawConfirmPopup] 뽑기 취소");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[DrawConfirmPopup] 취소 버튼 클릭 처리 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 뽑기 실행
    /// </summary>
    private void ExecuteDraw()
    {
        try
        {
            // ItemManager에서 뽑기 실행
            if (ItemManager.Instance != null)
            {
                // 랜덤 아이템 뽑기 (예시)
                string[] possibleItems = { "Item_100", "Item_101", "Item_102", "Item_103" };
                string randomItemId = possibleItems[Random.Range(0, possibleItems.Length)];
                
                ItemManager.Instance.AddItem(randomItemId, 1, true);
                
                // 뽑기 결과 팝업 표시
                PopupManager.Instance?.ShowToast($"아이템을 획득했습니다!");
                
                LogDebug($"[DrawConfirmPopup] 뽑기 결과: {randomItemId}");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[DrawConfirmPopup] 뽑기 실행 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 표시 애니메이션 시작
    /// </summary>
    private void StartShowAnimation()
    {
        if (showCoroutine != null)
        {
            StopCoroutine(showCoroutine);
        }

        showCoroutine = StartCoroutine(ShowAnimationCoroutine());
    }

    /// <summary>
    /// 숨김 애니메이션 시작
    /// </summary>
    private void StartHideAnimation()
    {
        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
        }

        hideCoroutine = StartCoroutine(HideAnimationCoroutine());
    }

    /// <summary>
    /// 표시 애니메이션 코루틴
    /// </summary>
    private System.Collections.IEnumerator ShowAnimationCoroutine()
    {
        isAnimating = true;
        gameObject.SetActive(true);

        var canvasGroup = GetComponent<CanvasGroup>();
        var rectTransform = GetComponent<RectTransform>();

        if (canvasGroup != null) canvasGroup.alpha = 0f;
        if (rectTransform != null) rectTransform.localScale = Vector3.zero;

        float elapsed = 0f;

        while (elapsed < showDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / showDuration;
            float curveValue = showCurve.Evaluate(progress);

            if (canvasGroup != null)
            {
                canvasGroup.alpha = curveValue;
            }

            if (rectTransform != null)
            {
                rectTransform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, curveValue);
            }

            yield return null;
        }

        if (canvasGroup != null) canvasGroup.alpha = 1f;
        if (rectTransform != null) rectTransform.localScale = Vector3.one;

        isShowing = true;
        isAnimating = false;
        showCoroutine = null;

        OnDrawPopupShownEvent?.Invoke();
    }

    /// <summary>
    /// 숨김 애니메이션 코루틴
    /// </summary>
    private System.Collections.IEnumerator HideAnimationCoroutine()
    {
        isAnimating = true;

        var canvasGroup = GetComponent<CanvasGroup>();
        var rectTransform = GetComponent<RectTransform>();

        float startAlpha = canvasGroup != null ? canvasGroup.alpha : 1f;
        Vector3 startScale = rectTransform != null ? rectTransform.localScale : Vector3.one;

        float elapsed = 0f;

        while (elapsed < hideDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / hideDuration;
            float curveValue = hideCurve.Evaluate(progress);

            if (canvasGroup != null)
            {
                canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, curveValue);
            }

            if (rectTransform != null)
            {
                rectTransform.localScale = Vector3.Lerp(startScale, Vector3.zero, curveValue);
            }

            yield return null;
        }

        HideImmediate();
        isAnimating = false;
        hideCoroutine = null;
    }

    /// <summary>
    /// 뽑기 비용 설정
    /// </summary>
    public void SetDrawCost(int cost)
    {
        drawCost = Mathf.Max(0, cost);
        UpdateUI();
        LogDebug($"[DrawConfirmPopup] 뽑기 비용 설정: {drawCost}");
    }

    /// <summary>
    /// 애니메이션 활성화/비활성화
    /// </summary>
    public void SetAnimationEnabled(bool enabled)
    {
        enableAnimations = enabled;
        LogDebug($"[DrawConfirmPopup] 애니메이션 {(enabled ? "활성화" : "비활성화")}");
    }

    /// <summary>
    /// 사운드 활성화/비활성화
    /// </summary>
    public void SetSoundEnabled(bool enabled)
    {
        enableSound = enabled;
        LogDebug($"[DrawConfirmPopup] 사운드 {(enabled ? "활성화" : "비활성화")}");
    }

    /// <summary>
    /// 팝업 정보 반환
    /// </summary>
    public string GetPopupInfo()
    {
        var info = new System.Text.StringBuilder();
        info.AppendLine($"[DrawConfirmPopup 정보]");
        info.AppendLine($"표시 중: {isShowing}");
        info.AppendLine($"애니메이션 중: {isAnimating}");
        info.AppendLine($"뽑기 비용: {drawCost}");
        info.AppendLine($"애니메이션: {(enableAnimations ? "활성화" : "비활성화")}");
        info.AppendLine($"사운드: {(enableSound ? "활성화" : "비활성화")}");

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
        if (showCoroutine != null)
        {
            StopCoroutine(showCoroutine);
        }

        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
        }

        // 이벤트 구독 해제
        OnDrawConfirmedEvent = null;
        OnDrawCancelledEvent = null;
        OnDrawPopupShownEvent = null;
        OnDrawPopupHiddenEvent = null;
    }
}
