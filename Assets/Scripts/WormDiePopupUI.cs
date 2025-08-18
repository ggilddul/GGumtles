using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class WormDiePopupUI : MonoBehaviour
{
    [Header("UI 요소")]
    [SerializeField] private Image wormImage;              // 벌레 이미지
    [SerializeField] private TMP_Text nameText;            // 이름 텍스트
    [SerializeField] private TMP_Text ageText;             // 나이 텍스트
    [SerializeField] private TMP_Text lifeStageText;       // 생명주기 텍스트
    [SerializeField] private TMP_Text rarityText;          // 희귀도 텍스트
    [SerializeField] private TMP_Text generationText;      // 세대 텍스트
    [SerializeField] private TMP_Text lifespanText;        // 수명 텍스트

    [Header("애니메이션")]
    [SerializeField] private CanvasGroup canvasGroup;      // 페이드 애니메이션용
    [SerializeField] private RectTransform popupRect;      // 크기 애니메이션용
    [SerializeField] private bool enableAnimations = true;
    [SerializeField] private float showDuration = 0.5f;    // 표시 애니메이션 시간
    [SerializeField] private float hideDuration = 0.3f;    // 숨김 애니메이션 시간
    [SerializeField] private AnimationCurve showCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private AnimationCurve hideCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

    [Header("색상 설정")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color deathColor = Color.gray;
    [SerializeField] private Color textColor = Color.white;

    [Header("사운드")]
    [SerializeField] private bool enableSound = true;
    [SerializeField] private AudioManager.SFXType deathSound = AudioManager.SFXType.Error;

    [Header("디버그")]
    [SerializeField] private bool enableDebugLogs = false;

    // 데이터 및 상태
    private WormData currentWorm;                          // 현재 벌레 데이터
    private bool isShowing = false;                        // 표시 상태
    private bool isAnimating = false;                      // 애니메이션 상태
    
    // 애니메이션 상태
    private Coroutine showCoroutine;
    private Coroutine hideCoroutine;

    // 프로퍼티
    public WormData CurrentWorm => currentWorm;
    public bool IsShowing => isShowing;
    public bool IsAnimating => isAnimating;

    // 이벤트 정의
    public delegate void OnWormDiePopupShown(WormDiePopupUI popup);
    public event OnWormDiePopupShown OnWormDiePopupShownEvent;

    public delegate void OnWormDiePopupHidden(WormDiePopupUI popup);
    public event OnWormDiePopupHidden OnWormDiePopupHiddenEvent;

    private void Awake()
    {
        InitializeComponents();
    }

    private void InitializeComponents()
    {
        try
        {
            // CanvasGroup 자동 찾기
            if (canvasGroup == null)
                canvasGroup = GetComponent<CanvasGroup>();
            
            if (canvasGroup == null)
                canvasGroup = gameObject.AddComponent<CanvasGroup>();

            // RectTransform 자동 찾기
            if (popupRect == null)
                popupRect = GetComponent<RectTransform>();

            // 초기 상태 설정
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
            }

            if (popupRect != null)
            {
                popupRect.localScale = Vector3.zero;
            }

            gameObject.SetActive(false);

            LogDebug("[WormDiePopupUI] 컴포넌트 초기화 완료");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[WormDiePopupUI] 컴포넌트 초기화 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 사망 팝업 열기
    /// </summary>
    public void OpenPopup(WormData worm)
    {
        if (worm == null)
        {
            Debug.LogWarning("[WormDiePopupUI] null 벌레 데이터가 전달되었습니다.");
            return;
        }

        try
        {
            currentWorm = worm;
            UpdateUI();
            Show();

            LogDebug($"[WormDiePopupUI] 사망 팝업 열기: {worm.DisplayName}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[WormDiePopupUI] 팝업 열기 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 팝업 표시
    /// </summary>
    public void Show()
    {
        try
        {
            if (isShowing) return;

            gameObject.SetActive(true);
            isShowing = true;

            // 사운드 재생
            if (enableSound)
            {
                AudioManager.Instance?.PlaySFX(deathSound);
            }

            // 애니메이션 시작
            if (enableAnimations)
            {
                StartShowAnimation();
            }
            else
            {
                // 애니메이션 없이 즉시 표시
                if (canvasGroup != null) canvasGroup.alpha = 1f;
                if (popupRect != null) popupRect.localScale = Vector3.one;
            }

            LogDebug("[WormDiePopupUI] 사망 팝업 표시");
            OnWormDiePopupShownEvent?.Invoke(this);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[WormDiePopupUI] Show 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 팝업 숨김
    /// </summary>
    public void Hide()
    {
        try
        {
            if (!isShowing) return;

            // 애니메이션 시작
            if (enableAnimations)
            {
                StartHideAnimation();
            }
            else
            {
                // 애니메이션 없이 즉시 숨김
                HideImmediate();
            }

            LogDebug("[WormDiePopupUI] 사망 팝업 숨김");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[WormDiePopupUI] Hide 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 즉시 숨김
    /// </summary>
    public void HideImmediate()
    {
        try
        {
            isShowing = false;
            isAnimating = false;

            if (canvasGroup != null) canvasGroup.alpha = 0f;
            if (popupRect != null) popupRect.localScale = Vector3.zero;

            gameObject.SetActive(false);

            OnWormDiePopupHiddenEvent?.Invoke(this);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[WormDiePopupUI] HideImmediate 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// UI 업데이트
    /// </summary>
    private void UpdateUI()
    {
        if (currentWorm == null) return;

        try
        {
            UpdateTexts();
            UpdateImage();
            UpdateColors();
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[WormDiePopupUI] UI 업데이트 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 텍스트 업데이트
    /// </summary>
    private void UpdateTexts()
    {
        try
        {
            // 이름 텍스트
            if (nameText != null)
            {
                nameText.text = currentWorm.DisplayName;
            }

            // 나이 텍스트
            if (ageText != null)
            {
                ageText.text = $"나이: {FormatAge(currentWorm.age)}";
            }

            // 생명주기 텍스트
            if (lifeStageText != null)
            {
                lifeStageText.text = $"생명주기: {GetLifeStageName(currentWorm.lifeStage)}";
            }

            // 희귀도 텍스트
            if (rarityText != null)
            {
                rarityText.text = $"희귀도: {currentWorm.GetRarityText()}";
                rarityText.color = currentWorm.GetRarityColor();
            }

            // 세대 텍스트
            if (generationText != null)
            {
                generationText.text = $"세대: {currentWorm.generation}";
            }

            // 수명 텍스트
            if (lifespanText != null)
            {
                lifespanText.text = $"수명: {FormatAge(currentWorm.lifespan)}";
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[WormDiePopupUI] 텍스트 업데이트 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 이미지 업데이트
    /// </summary>
    private void UpdateImage()
    {
        try
        {
            if (wormImage != null)
            {
                // 생명주기 스프라이트 사용 (OverViewRenderer 대신)
                wormImage.sprite = GetLifeStageSprite(currentWorm.lifeStage);

                // 사망 상태로 색상 조정
                wormImage.color = deathColor;
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[WormDiePopupUI] 이미지 업데이트 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 색상 업데이트
    /// </summary>
    private void UpdateColors()
    {
        try
        {
            // 전체 색상 설정 (사망 시 회색조)
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0.8f; // 약간 투명하게
            }

            // 텍스트 색상
            if (nameText != null)
            {
                nameText.color = deathColor;
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[WormDiePopupUI] 색상 업데이트 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 아이템 스프라이트 가져오기
    /// </summary>
    private Sprite GetItemSprite(string itemId)
    {
        if (string.IsNullOrEmpty(itemId)) return null;

        try
        {
            var item = ItemManager.Instance?.GetItemById(itemId);
            return item?.sprite;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[WormDiePopupUI] 아이템 스프라이트 로드 중 오류: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// 생명주기 스프라이트 가져오기
    /// </summary>
    private Sprite GetLifeStageSprite(int stage)
    {
        try
        {
            return SpriteManager.Instance?.GetLifeStageSprite(stage);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[WormDiePopupUI] 생명주기 스프라이트 로드 중 오류: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// 생명주기 이름 가져오기
    /// </summary>
    private string GetLifeStageName(int stage)
    {
        switch (stage)
        {
            case 0: return "알";
            case 1: return "L1";
            case 2: return "L2";
            case 3: return "L3";
            case 4: return "L4";
            case 5: return "성체";
            case 6: return "노년";
            default: return "알 수 없음";
        }
    }

    /// <summary>
    /// 나이 포맷팅
    /// </summary>
    public static string FormatAge(int ageInMinutes)
    {
        try
        {
            int days = ageInMinutes / 1440;           // 1일 = 1440분
            int hours = (ageInMinutes % 1440) / 60;    // 나머지에서 시간 추출
            int minutes = ageInMinutes % 60;           // 나머지에서 분 추출

            var parts = new System.Collections.Generic.List<string>();
            if (days > 0) parts.Add($"{days}일");
            if (hours > 0) parts.Add($"{hours}시간");
            if (minutes > 0 || parts.Count == 0) parts.Add($"{minutes}분");

            return string.Join(" ", parts);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[WormDiePopupUI] 나이 포맷팅 중 오류: {ex.Message}");
            return "알 수 없음";
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
    private IEnumerator ShowAnimationCoroutine()
    {
        isAnimating = true;

        float elapsed = 0f;

        while (elapsed < showDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / showDuration;
            float curveValue = showCurve.Evaluate(progress);

            // 알파 애니메이션
            if (canvasGroup != null)
            {
                canvasGroup.alpha = curveValue;
            }

            // 스케일 애니메이션
            if (popupRect != null)
            {
                popupRect.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, curveValue);
            }

            yield return null;
        }

        // 최종 상태 설정
        if (canvasGroup != null) canvasGroup.alpha = 1f;
        if (popupRect != null) popupRect.localScale = Vector3.one;

        isAnimating = false;
        showCoroutine = null;
    }

    /// <summary>
    /// 숨김 애니메이션 코루틴
    /// </summary>
    private IEnumerator HideAnimationCoroutine()
    {
        isAnimating = true;

        float elapsed = 0f;
        float startAlpha = canvasGroup != null ? canvasGroup.alpha : 1f;
        Vector3 startScale = popupRect != null ? popupRect.localScale : Vector3.one;

        while (elapsed < hideDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / hideDuration;
            float curveValue = hideCurve.Evaluate(progress);

            // 알파 애니메이션
            if (canvasGroup != null)
            {
                canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, curveValue);
            }

            // 스케일 애니메이션
            if (popupRect != null)
            {
                popupRect.localScale = Vector3.Lerp(startScale, Vector3.zero, curveValue);
            }

            yield return null;
        }

        // 최종 상태 설정
        HideImmediate();

        isAnimating = false;
        hideCoroutine = null;
    }

    /// <summary>
    /// 애니메이션 활성화/비활성화
    /// </summary>
    public void SetAnimationEnabled(bool enabled)
    {
        enableAnimations = enabled;
        LogDebug($"[WormDiePopupUI] 애니메이션 {(enabled ? "활성화" : "비활성화")}");
    }

    /// <summary>
    /// 사운드 활성화/비활성화
    /// </summary>
    public void SetSoundEnabled(bool enabled)
    {
        enableSound = enabled;
        LogDebug($"[WormDiePopupUI] 사운드 {(enabled ? "활성화" : "비활성화")}");
    }

    /// <summary>
    /// 팝업 정보 반환
    /// </summary>
    public string GetPopupInfo()
    {
        if (currentWorm == null) return "데이터 없음";

        var info = new System.Text.StringBuilder();
        info.AppendLine($"[WormDiePopupUI 정보]");
        info.AppendLine($"벌레: {currentWorm.DisplayName}");
        info.AppendLine($"나이: {FormatAge(currentWorm.age)}");
        info.AppendLine($"생명주기: {GetLifeStageName(currentWorm.lifeStage)}");
        info.AppendLine($"희귀도: {currentWorm.GetRarityText()}");
        info.AppendLine($"세대: {currentWorm.generation}");
        info.AppendLine($"수명: {FormatAge(currentWorm.lifespan)}");
        info.AppendLine($"표시됨: {isShowing}");
        info.AppendLine($"애니메이션: {(isAnimating ? "진행 중" : "대기")}");

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

        // 이벤트 초기화
        OnWormDiePopupShownEvent = null;
        OnWormDiePopupHiddenEvent = null;
    }
}
