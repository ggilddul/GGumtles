using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class WormEvolvePopupUI : MonoBehaviour
{
    [Header("렌더러")]
    [SerializeField] private OverViewRenderer overviewRenderer;

    [Header("UI 요소")]
    [SerializeField] private TMP_Text nameText;            // 이름 텍스트
    [SerializeField] private TMP_Text beforeText;          // 진화 전 텍스트
    [SerializeField] private TMP_Text afterText;           // 진화 후 텍스트
    [SerializeField] private TMP_Text ageText;             // 나이 텍스트
    [SerializeField] private TMP_Text rarityText;          // 희귀도 텍스트
    [SerializeField] private TMP_Text generationText;      // 세대 텍스트

    [Header("이미지")]
    [SerializeField] private Image beforeImage;            // 진화 전 이미지
    [SerializeField] private Image afterImage;             // 진화 후 이미지
    [SerializeField] private Image arrowImage;             // 화살표 이미지

    [Header("애니메이션")]
    [SerializeField] private CanvasGroup canvasGroup;      // 페이드 애니메이션용
    [SerializeField] private RectTransform popupRect;      // 크기 애니메이션용
    [SerializeField] private bool enableAnimations = true;
    [SerializeField] private float showDuration = 0.5f;    // 표시 애니메이션 시간
    [SerializeField] private float hideDuration = 0.3f;    // 숨김 애니메이션 시간
    [SerializeField] private float evolutionDuration = 2f; // 진화 애니메이션 시간
    [SerializeField] private AnimationCurve showCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private AnimationCurve hideCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
    [SerializeField] private AnimationCurve evolutionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("색상 설정")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color evolutionColor = Color.yellow;
    [SerializeField] private Color textColor = Color.white;

    [Header("사운드")]
    [SerializeField] private bool enableSound = true;
    [SerializeField] private AudioManager.SFXType evolutionSound = AudioManager.SFXType.Evolve;

    [Header("디버그")]
    [SerializeField] private bool enableDebugLogs = false;

    // 데이터 및 상태
    private WormData currentWorm;                          // 현재 벌레 데이터
    private int previousLifeStage;                         // 이전 생명주기
    private int currentLifeStage;                          // 현재 생명주기
    private bool isShowing = false;                        // 표시 상태
    private bool isAnimating = false;                      // 애니메이션 상태
    
    // 애니메이션 상태
    private Coroutine showCoroutine;
    private Coroutine hideCoroutine;
    private Coroutine evolutionCoroutine;

    // 프로퍼티
    public WormData CurrentWorm => currentWorm;
    public int PreviousLifeStage => previousLifeStage;
    public int CurrentLifeStage => currentLifeStage;
    public bool IsShowing => isShowing;
    public bool IsAnimating => isAnimating;

    // 이벤트 정의
    public delegate void OnWormEvolvePopupShown(WormEvolvePopupUI popup);
    public event OnWormEvolvePopupShown OnWormEvolvePopupShownEvent;

    public delegate void OnWormEvolvePopupHidden(WormEvolvePopupUI popup);
    public event OnWormEvolvePopupHidden OnWormEvolvePopupHiddenEvent;

    public delegate void OnEvolutionComplete(WormEvolvePopupUI popup, int fromStage, int toStage);
    public event OnEvolutionComplete OnEvolutionCompleteEvent;

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

            LogDebug("[WormEvolvePopupUI] 컴포넌트 초기화 완료");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[WormEvolvePopupUI] 컴포넌트 초기화 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 진화 팝업 열기
    /// </summary>
    public void OpenPopup(WormData worm, int fromStage, int toStage)
    {
        if (worm == null)
        {
            Debug.LogWarning("[WormEvolvePopupUI] null 벌레 데이터가 전달되었습니다.");
            return;
        }

        try
        {
            currentWorm = worm;
            previousLifeStage = fromStage;
            currentLifeStage = toStage;

            UpdateUI();
            Show();

            LogDebug($"[WormEvolvePopupUI] 진화 팝업 열기: {worm.DisplayName} ({fromStage} → {toStage})");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[WormEvolvePopupUI] 팝업 열기 중 오류: {ex.Message}");
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
                AudioManager.Instance?.PlaySFX(evolutionSound);
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

            LogDebug("[WormEvolvePopupUI] 진화 팝업 표시");
            OnWormEvolvePopupShownEvent?.Invoke(this);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[WormEvolvePopupUI] Show 중 오류: {ex.Message}");
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

            LogDebug("[WormEvolvePopupUI] 진화 팝업 숨김");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[WormEvolvePopupUI] Hide 중 오류: {ex.Message}");
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

            OnWormEvolvePopupHiddenEvent?.Invoke(this);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[WormEvolvePopupUI] HideImmediate 중 오류: {ex.Message}");
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
            UpdateImages();
            UpdateColors();
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[WormEvolvePopupUI] UI 업데이트 중 오류: {ex.Message}");
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

            // 진화 전 텍스트
            if (beforeText != null)
            {
                beforeText.text = GetLifeStageName(previousLifeStage);
            }

            // 진화 후 텍스트
            if (afterText != null)
            {
                afterText.text = GetLifeStageName(currentLifeStage);
            }

            // 나이 텍스트
            if (ageText != null)
            {
                ageText.text = $"나이: {FormatAge(currentWorm.age)}";
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
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[WormEvolvePopupUI] 텍스트 업데이트 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 이미지 업데이트
    /// </summary>
    private void UpdateImages()
    {
        try
        {
            // 진화 전 이미지
            if (beforeImage != null)
            {
                beforeImage.sprite = GetLifeStageSprite(previousLifeStage);
            }

            // 진화 후 이미지
            if (afterImage != null)
            {
                afterImage.sprite = GetLifeStageSprite(currentLifeStage);
            }

            // 오버뷰 렌더러 업데이트
            if (overviewRenderer != null)
            {
                overviewRenderer.RefreshOverview();
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[WormEvolvePopupUI] 이미지 업데이트 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 색상 업데이트
    /// </summary>
    private void UpdateColors()
    {
        try
        {
            // 진화 색상 적용
            if (beforeText != null)
            {
                beforeText.color = normalColor;
            }

            if (afterText != null)
            {
                afterText.color = evolutionColor;
            }

            if (arrowImage != null)
            {
                arrowImage.color = evolutionColor;
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[WormEvolvePopupUI] 색상 업데이트 중 오류: {ex.Message}");
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
            Debug.LogError($"[WormEvolvePopupUI] 생명주기 스프라이트 로드 중 오류: {ex.Message}");
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
            Debug.LogError($"[WormEvolvePopupUI] 나이 포맷팅 중 오류: {ex.Message}");
            return "알 수 없음";
        }
    }

    /// <summary>
    /// 진화 애니메이션 시작
    /// </summary>
    public void StartEvolutionAnimation()
    {
        if (evolutionCoroutine != null)
        {
            StopCoroutine(evolutionCoroutine);
        }

        evolutionCoroutine = StartCoroutine(EvolutionAnimationCoroutine());
    }

    /// <summary>
    /// 진화 애니메이션 코루틴
    /// </summary>
    private IEnumerator EvolutionAnimationCoroutine()
    {
        isAnimating = true;

        float elapsed = 0f;

        while (elapsed < evolutionDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / evolutionDuration;
            float curveValue = evolutionCurve.Evaluate(progress);

            // 진화 전 이미지 페이드 아웃
            if (beforeImage != null)
            {
                Color beforeColor = beforeImage.color;
                beforeColor.a = 1f - curveValue;
                beforeImage.color = beforeColor;
            }

            // 진화 후 이미지 페이드 인
            if (afterImage != null)
            {
                Color afterColor = afterImage.color;
                afterColor.a = curveValue;
                afterImage.color = afterColor;
            }

            // 화살표 애니메이션
            if (arrowImage != null)
            {
                arrowImage.transform.localScale = Vector3.one * (1f + Mathf.Sin(progress * Mathf.PI * 4) * 0.1f);
            }

            yield return null;
        }

        // 최종 상태 설정
        if (beforeImage != null)
        {
            Color beforeColor = beforeImage.color;
            beforeColor.a = 0f;
            beforeImage.color = beforeColor;
        }

        if (afterImage != null)
        {
            Color afterColor = afterImage.color;
            afterColor.a = 1f;
            afterImage.color = afterColor;
        }

        // 진화 완료 이벤트 발생
        OnEvolutionCompleteEvent?.Invoke(this, previousLifeStage, currentLifeStage);

        LogDebug($"[WormEvolvePopupUI] 진화 애니메이션 완료: {previousLifeStage} → {currentLifeStage}");

        isAnimating = false;
        evolutionCoroutine = null;
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
        LogDebug($"[WormEvolvePopupUI] 애니메이션 {(enabled ? "활성화" : "비활성화")}");
    }

    /// <summary>
    /// 사운드 활성화/비활성화
    /// </summary>
    public void SetSoundEnabled(bool enabled)
    {
        enableSound = enabled;
        LogDebug($"[WormEvolvePopupUI] 사운드 {(enabled ? "활성화" : "비활성화")}");
    }

    /// <summary>
    /// 팝업 정보 반환
    /// </summary>
    public string GetPopupInfo()
    {
        if (currentWorm == null) return "데이터 없음";

        var info = new System.Text.StringBuilder();
        info.AppendLine($"[WormEvolvePopupUI 정보]");
        info.AppendLine($"벌레: {currentWorm.DisplayName}");
        info.AppendLine($"진화: {GetLifeStageName(previousLifeStage)} → {GetLifeStageName(currentLifeStage)}");
        info.AppendLine($"나이: {FormatAge(currentWorm.age)}");
        info.AppendLine($"희귀도: {currentWorm.GetRarityText()}");
        info.AppendLine($"세대: {currentWorm.generation}");
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

        if (evolutionCoroutine != null)
        {
            StopCoroutine(evolutionCoroutine);
        }

        // 이벤트 초기화
        OnWormEvolvePopupShownEvent = null;
        OnWormEvolvePopupHiddenEvent = null;
        OnEvolutionCompleteEvent = null;
    }
}
