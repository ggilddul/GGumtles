using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class AchievementPopup : MonoBehaviour
{
    [Header("UI 요소")]
    [SerializeField] private TMP_Text titleText;           // 업적 제목
    [SerializeField] private TMP_Text descriptionText;     // 업적 설명
    [SerializeField] private TMP_Text progressText;        // 진행도 텍스트
    [SerializeField] private TMP_Text rewardText;          // 보상 텍스트
    [SerializeField] private TMP_Text categoryText;        // 카테고리 텍스트

    [Header("이미지 요소")]
    [SerializeField] private Image achievementIcon;        // 업적 아이콘
    [SerializeField] private Image backgroundImage;        // 배경 이미지
    [SerializeField] private Image progressBarFill;        // 진행도 바

    [Header("애니메이션")]
    [SerializeField] private CanvasGroup canvasGroup;      // 페이드 애니메이션용
    [SerializeField] private RectTransform popupRect;      // 크기 애니메이션용
    [SerializeField] private bool enableAnimations = true;
    [SerializeField] private float showDuration = 0.5f;    // 표시 애니메이션 시간
    [SerializeField] private float hideDuration = 0.3f;    // 숨김 애니메이션 시간
    [SerializeField] private float displayTime = 3f;       // 표시 지속 시간
    [SerializeField] private AnimationCurve showCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private AnimationCurve hideCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

    [Header("색상 설정")]
    [SerializeField] private Color titleColor = Color.white;
    [SerializeField] private Color descriptionColor = Color.gray;
    [SerializeField] private Color progressColor = Color.blue;
    [SerializeField] private Color rewardColor = Color.yellow;

    [Header("사운드")]
    [SerializeField] private bool enableSound = true;
    [SerializeField] private AudioManager.SFXType achievementUnlockSound = AudioManager.SFXType.Achievement;

    [Header("디버그")]
    [SerializeField] private bool enableDebugLogs = false;

    // 데이터 및 상태
    private AchievementData achievementData;               // 업적 데이터
    private AchievementStatus achievementStatus;           // 업적 상태
    private bool isShowing = false;                        // 표시 상태
    private bool isAnimating = false;                      // 애니메이션 상태
    
    // 애니메이션 상태
    private Coroutine showCoroutine;
    private Coroutine hideCoroutine;
    private Coroutine autoHideCoroutine;

    // 프로퍼티
    public AchievementData AchievementData => achievementData;
    public AchievementStatus AchievementStatus => achievementStatus;
    public bool IsShowing => isShowing;
    public bool IsAnimating => isAnimating;

    // 이벤트 정의
    public delegate void OnAchievementPopupShown(AchievementPopup popup);
    public event OnAchievementPopupShown OnAchievementPopupShownEvent;

    public delegate void OnAchievementPopupHidden(AchievementPopup popup);
    public event OnAchievementPopupHidden OnAchievementPopupHiddenEvent;

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

            LogDebug("[AchievementPopup] 컴포넌트 초기화 완료");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[AchievementPopup] 컴포넌트 초기화 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 업적 팝업 설정 (기존 호환성)
    /// </summary>
    public void Setup(string title, bool isAchieved)
    {
        try
        {
            // 간단한 업적 데이터 생성
            var tempData = ScriptableObject.CreateInstance<AchievementData>();
            tempData.ach_title = title;
            tempData.ach_description = "업적 설명";
            tempData.ach_id = "temp_achievement";

            var tempStatus = new AchievementStatus
            {
                isUnlocked = isAchieved,
                unlockTime = isAchieved ? System.DateTime.Now : System.DateTime.MinValue
            };

            Setup(tempData, tempStatus);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[AchievementPopup] Setup 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 업적 팝업 설정 (새로운 방식)
    /// </summary>
    public void Setup(AchievementData data, AchievementStatus status)
    {
        if (data == null)
        {
            Debug.LogWarning("[AchievementPopup] null 업적 데이터가 전달되었습니다.");
            return;
        }

        try
        {
            achievementData = data;
            achievementStatus = status;

            UpdateUI();
            LogDebug($"[AchievementPopup] 업적 팝업 설정 완료: {data.ach_title}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[AchievementPopup] Setup 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 팝업 표시
    /// </summary>
    public void Show()
    {
        if (achievementData == null)
        {
            Debug.LogWarning("[AchievementPopup] 표시할 업적 데이터가 없습니다.");
            return;
        }

        try
        {
            if (isShowing) return;

            gameObject.SetActive(true);
            isShowing = true;

            // 사운드 재생
            if (enableSound && achievementStatus.isUnlocked)
            {
                AudioManager.Instance?.PlaySFX(achievementUnlockSound);
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

            // 자동 숨김 시작
            if (autoHideCoroutine != null)
            {
                StopCoroutine(autoHideCoroutine);
            }
            autoHideCoroutine = StartCoroutine(AutoHideCoroutine());

            LogDebug($"[AchievementPopup] 팝업 표시: {achievementData.ach_title}");
            OnAchievementPopupShownEvent?.Invoke(this);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[AchievementPopup] Show 중 오류: {ex.Message}");
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

            // 자동 숨김 중지
            if (autoHideCoroutine != null)
            {
                StopCoroutine(autoHideCoroutine);
                autoHideCoroutine = null;
            }

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

            LogDebug($"[AchievementPopup] 팝업 숨김: {achievementData?.ach_title}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[AchievementPopup] Hide 중 오류: {ex.Message}");
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

            OnAchievementPopupHiddenEvent?.Invoke(this);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[AchievementPopup] HideImmediate 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// UI 업데이트
    /// </summary>
    private void UpdateUI()
    {
        if (achievementData == null) return;

        try
        {
            UpdateTexts();
            UpdateImages();
            UpdateColors();
            UpdateProgress();
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[AchievementPopup] UI 업데이트 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 텍스트 업데이트
    /// </summary>
    private void UpdateTexts()
    {
        try
        {
            // 제목 텍스트
            if (titleText != null)
            {
                titleText.text = achievementData.ach_title;
            }

            // 설명 텍스트
            if (descriptionText != null)
            {
                descriptionText.text = achievementData.ach_description;
            }

            // 진행도 텍스트
            if (progressText != null && achievementData.hasProgress)
            {
                float progress = achievementData.CalculateProgress(achievementStatus);
                string progressString = achievementData.GetProgressText(progress);
                progressText.text = progressString;
                progressText.gameObject.SetActive(true);
            }
            else if (progressText != null)
            {
                progressText.gameObject.SetActive(false);
            }

            // 보상 텍스트
            if (rewardText != null && achievementData.hasReward)
            {
                string rewardString = achievementData.GetRewardDescription();
                rewardText.text = rewardString;
                rewardText.gameObject.SetActive(true);
            }
            else if (rewardText != null)
            {
                rewardText.gameObject.SetActive(false);
            }

            // 카테고리 텍스트
            if (categoryText != null)
            {
                categoryText.text = achievementData.category.ToString();
                categoryText.gameObject.SetActive(true);
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[AchievementPopup] 텍스트 업데이트 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 이미지 업데이트
    /// </summary>
    private void UpdateImages()
    {
        try
        {
            // 업적 아이콘
            if (achievementIcon != null)
            {
                achievementIcon.sprite = achievementData.achievementIcon;
            }


        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[AchievementPopup] 이미지 업데이트 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 색상 업데이트
    /// </summary>
    private void UpdateColors()
    {
        try
        {
            // 제목 색상
            if (titleText != null)
            {
                titleText.color = titleColor;
            }

            // 설명 색상
            if (descriptionText != null)
            {
                descriptionText.color = descriptionColor;
            }

            // 진행도 색상
            if (progressText != null)
            {
                progressText.color = progressColor;
            }

            // 보상 색상
            if (rewardText != null)
            {
                rewardText.color = rewardColor;
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[AchievementPopup] 색상 업데이트 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 진행도 업데이트
    /// </summary>
    private void UpdateProgress()
    {
        try
        {
            if (progressBarFill != null && achievementData.hasProgress)
            {
                float progress = achievementData.CalculateProgress(achievementStatus);
                progressBarFill.fillAmount = progress;
                progressBarFill.gameObject.SetActive(true);
            }
            else if (progressBarFill != null)
            {
                progressBarFill.gameObject.SetActive(false);
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[AchievementPopup] 진행도 업데이트 중 오류: {ex.Message}");
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
    /// 자동 숨김 코루틴
    /// </summary>
    private IEnumerator AutoHideCoroutine()
    {
        yield return new WaitForSeconds(displayTime);
        Hide();
    }

    /// <summary>
    /// 애니메이션 활성화/비활성화
    /// </summary>
    public void SetAnimationEnabled(bool enabled)
    {
        enableAnimations = enabled;
        LogDebug($"[AchievementPopup] 애니메이션 {(enabled ? "활성화" : "비활성화")}");
    }

    /// <summary>
    /// 사운드 활성화/비활성화
    /// </summary>
    public void SetSoundEnabled(bool enabled)
    {
        enableSound = enabled;
        LogDebug($"[AchievementPopup] 사운드 {(enabled ? "활성화" : "비활성화")}");
    }

    /// <summary>
    /// 표시 시간 설정
    /// </summary>
    public void SetDisplayTime(float time)
    {
        displayTime = time;
        LogDebug($"[AchievementPopup] 표시 시간 설정: {time}초");
    }

    /// <summary>
    /// 팝업 정보 반환
    /// </summary>
    public string GetPopupInfo()
    {
        if (achievementData == null) return "데이터 없음";

        var info = new System.Text.StringBuilder();
        info.AppendLine($"[AchievementPopup 정보]");
                    info.AppendLine($"업적: {achievementData.ach_title}");
                    info.AppendLine($"설명: {achievementData.ach_description}");
        info.AppendLine($"카테고리: {achievementData.category}");
        info.AppendLine($"달성됨: {achievementStatus.isUnlocked}");
        info.AppendLine($"표시됨: {isShowing}");
        info.AppendLine($"애니메이션: {(isAnimating ? "진행 중" : "대기")}");

        if (achievementData.hasProgress)
        {
            float progress = achievementData.CalculateProgress(achievementStatus);
            info.AppendLine($"진행도: {progress:P0}");
        }

        if (achievementData.hasReward)
        {
            info.AppendLine($"보상: {achievementData.GetRewardDescription()}");
        }

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

        if (autoHideCoroutine != null)
        {
            StopCoroutine(autoHideCoroutine);
        }
    }
}
