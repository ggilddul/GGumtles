using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AchievementUI : MonoBehaviour
{
    [Header("UI 컴포넌트")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text progressText;
    [SerializeField] private Image progressBar;
    [SerializeField] private GameObject unlockedIcon;
    [SerializeField] private GameObject lockedIcon;

    [Header("설정")]
    [SerializeField] private bool enableAnimations = true;
    [SerializeField] private bool enableDebugLogs = false;

    [Header("애니메이션")]
    [SerializeField] private float updateAnimationDuration = 0.3f;
    [SerializeField] private AnimationCurve updateCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    // 데이터
    private AchievementData achievementData;
    private AchievementStatus achievementStatus;

    // 상태
    private bool isAnimating = false;
    private Coroutine updateAnimationCoroutine;

    // 프로퍼티
    public AchievementData AchievementData => achievementData;
    public AchievementStatus AchievementStatus => achievementStatus;
    public bool IsUnlocked => achievementStatus?.isUnlocked ?? false;

    // 이벤트 정의
    public delegate void OnAchievementUIUpdated(AchievementUI achievementUI);
    public event OnAchievementUIUpdated OnAchievementUIUpdatedEvent;

    private void Awake()
    {
        InitializeComponents();
    }

    private void InitializeComponents()
    {
        try
        {
            // 자동으로 컴포넌트 찾기
            if (iconImage == null)
                iconImage = GetComponentInChildren<Image>();
            
            if (titleText == null)
                titleText = GetComponentInChildren<TMP_Text>();

            LogDebug("[AchievementUI] 컴포넌트 초기화 완료");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[AchievementUI] 컴포넌트 초기화 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 업적 UI 설정
    /// </summary>
    public void Set(AchievementData definition, AchievementStatus status)
    {
        try
        {
            achievementData = definition;
            achievementStatus = status;

            UpdateUI(definition, status);

            LogDebug($"[AchievementUI] 업적 UI 설정 완료: {definition?.ach_title}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[AchievementUI] 업적 UI 설정 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 업적 상태 업데이트
    /// </summary>
    public void UpdateStatus(AchievementStatus newStatus)
    {
        try
        {
            bool wasUnlocked = achievementStatus?.isUnlocked ?? false;
            achievementStatus = newStatus;

            if (enableAnimations && wasUnlocked != newStatus.isUnlocked)
            {
                // 해금 상태가 변경된 경우 애니메이션 실행
                StartUpdateAnimation();
            }
            else
            {
                UpdateUI(achievementData, newStatus);
            }

            OnAchievementUIUpdatedEvent?.Invoke(this);
            LogDebug($"[AchievementUI] 업적 상태 업데이트: {achievementData?.ach_title} - {newStatus.isUnlocked}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[AchievementUI] 상태 업데이트 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// UI 업데이트
    /// </summary>
    private void UpdateUI(AchievementData definition, AchievementStatus status)
    {
        if (definition == null || status == null) return;

        try
        {
            // 제목 텍스트
            if (titleText != null)
            {
                titleText.text = definition.ach_title ?? "알 수 없는 업적";
            }

            // 설명 텍스트
            if (descriptionText != null)
            {
                descriptionText.text = definition.ach_description ?? "";
            }

            // 아이콘 이미지
            if (iconImage != null)
            {
                // 아이콘 가져오기 (SpriteManager에서 또는 정의에서)
                Sprite icon = GetAchievementIcon(definition);
                iconImage.sprite = icon;
                
                // 해금 상태에 따른 색상 설정
                iconImage.color = status.isUnlocked ? Color.white : new Color(0.8f, 0.8f, 0.8f, 1f);
            }

            // 진행도 텍스트
            if (progressText != null)
            {
                if (definition.hasProgress)
                {
                    float progress = status.progress;
                    float targetValue = definition.targetValue;
                    progressText.text = $"{progress:F0}/{targetValue:F0}";
                }
                else
                {
                    progressText.text = status.isUnlocked ? "완료" : "미완료";
                }
            }

            // 진행도 바
            if (progressBar != null && definition.hasProgress)
            {
                float progress = status.progress / definition.targetValue;
                progressBar.fillAmount = Mathf.Clamp01(progress);
            }

            // 해금/잠금 아이콘
            if (unlockedIcon != null)
            {
                unlockedIcon.SetActive(status.isUnlocked);
            }

            if (lockedIcon != null)
            {
                lockedIcon.SetActive(!status.isUnlocked);
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[AchievementUI] UI 업데이트 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 업적 아이콘 가져오기
    /// </summary>
    private Sprite GetAchievementIcon(AchievementData definition)
    {
        try
        {
            // SpriteManager에서 가져오기 시도
            if (SpriteManager.Instance != null)
            {
                Sprite icon = SpriteManager.Instance.GetAchievementSprite(definition.ach_id);
                if (icon != null) return icon;
            }

            // 정의에서 직접 가져오기
            if (definition.achievementIcon != null)
            {
                return definition.achievementIcon;
            }

            // 기본 아이콘 반환
            return null;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[AchievementUI] 아이콘 가져오기 중 오류: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// 업데이트 애니메이션 시작
    /// </summary>
    private void StartUpdateAnimation()
    {
        if (isAnimating) return;

        if (updateAnimationCoroutine != null)
        {
            StopCoroutine(updateAnimationCoroutine);
        }

        updateAnimationCoroutine = StartCoroutine(UpdateAnimationCoroutine());
    }

    /// <summary>
    /// 업데이트 애니메이션 코루틴
    /// </summary>
    private System.Collections.IEnumerator UpdateAnimationCoroutine()
    {
        isAnimating = true;

        // 페이드 아웃
        float elapsed = 0f;
        float startAlpha = 1f;

        while (elapsed < updateAnimationDuration * 0.5f)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / (updateAnimationDuration * 0.5f);
            float curveValue = updateCurve.Evaluate(progress);

            if (iconImage != null)
            {
                Color color = iconImage.color;
                color.a = Mathf.Lerp(startAlpha, 0.5f, curveValue);
                iconImage.color = color;
            }

            yield return null;
        }

        // UI 업데이트
        UpdateUI(achievementData, achievementStatus);

        // 페이드 인
        elapsed = 0f;
        startAlpha = 0.5f;

        while (elapsed < updateAnimationDuration * 0.5f)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / (updateAnimationDuration * 0.5f);
            float curveValue = updateCurve.Evaluate(progress);

            if (iconImage != null)
            {
                Color color = iconImage.color;
                color.a = Mathf.Lerp(startAlpha, 1f, curveValue);
                iconImage.color = color;
            }

            yield return null;
        }

        // 최종 상태 설정
        if (iconImage != null)
        {
            Color finalColor = iconImage.color;
            finalColor.a = 1f;
            iconImage.color = finalColor;
        }

        isAnimating = false;
        updateAnimationCoroutine = null;
    }

    /// <summary>
    /// 애니메이션 활성화/비활성화
    /// </summary>
    public void SetAnimationEnabled(bool enabled)
    {
        enableAnimations = enabled;
        LogDebug($"[AchievementUI] 애니메이션 {(enabled ? "활성화" : "비활성화")}");
    }

    /// <summary>
    /// UI 활성화/비활성화
    /// </summary>
    public void SetUIEnabled(bool enabled)
    {
        gameObject.SetActive(enabled);
    }

    /// <summary>
    /// 업적 정보 반환
    /// </summary>
    public string GetAchievementInfo()
    {
        var info = new System.Text.StringBuilder();
        info.AppendLine($"[AchievementUI 정보]");
        info.AppendLine($"업적: {achievementData?.ach_title ?? "없음"}");
        info.AppendLine($"해금됨: {achievementStatus?.isUnlocked ?? false}");
        info.AppendLine($"진행도: {(achievementStatus?.progress ?? 0):P0}");
        info.AppendLine($"애니메이션: {(enableAnimations ? "활성화" : "비활성화")}");

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
        if (updateAnimationCoroutine != null)
        {
            StopCoroutine(updateAnimationCoroutine);
        }

        OnAchievementUIUpdatedEvent = null;
    }
}
