using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AchievementUI : MonoBehaviour
{
    [Header("UI 컴포넌트")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descriptionText;
    
    [Header("Achievement 2 전용")]
    [SerializeField] private Image achieveWormImage;
    [SerializeField] private TMP_Text achieveWormNameText;

    [Header("설정")]
    [SerializeField] private bool enableAnimations = true;
    [SerializeField] private bool enableDebugLogs = false;

    [Header("애니메이션")]
    [SerializeField] private float updateAnimationDuration = 0.3f;
    [SerializeField] private AnimationCurve updateCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    // 데이터
    private AchievementData achievementData;
    private bool isUnlocked = false;

    // 상태
    private bool isAnimating = false;
    private Coroutine updateAnimationCoroutine;

    // 프로퍼티
    public AchievementData AchievementData => achievementData;
    public bool IsUnlocked => isUnlocked;

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
    public void Set(AchievementData definition, bool unlocked)
    {
        try
        {
            achievementData = definition;
            isUnlocked = unlocked;

            UpdateUI(definition, unlocked);

            LogDebug($"[AchievementUI] 업적 UI 설정 완료: {definition?.achievementTitle}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[AchievementUI] 업적 UI 설정 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 업적 UI 설정 (Achievement2 전용)
    /// </summary>
    public void Set(AchievementData definition, bool unlocked, bool isAchievement2)
    {
        try
        {
            achievementData = definition;
            isUnlocked = unlocked;

            UpdateUI(definition, unlocked, isAchievement2);

            LogDebug($"[AchievementUI] 업적 UI 설정 완료: {definition?.achievementTitle} (Achievement2: {isAchievement2})");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[AchievementUI] 업적 UI 설정 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 업적 인덱스로 초기화 (PopupManager용)
    /// </summary>
    public void Initialize(int achievementIndex)
    {
        try
        {
            if (AchievementManager.Instance == null)
            {
                Debug.LogWarning("[AchievementUI] AchievementManager 인스턴스가 없습니다.");
                return;
            }

            var definitions = AchievementManager.Instance.GetAllDefinitions();
            if (achievementIndex >= 0 && achievementIndex < definitions.Count)
            {
                var definition = definitions[achievementIndex];
                bool isUnlocked = AchievementManager.Instance.IsUnlocked(definition.achievementId);
                Set(definition, isUnlocked);
            }
            else
            {
                Debug.LogWarning($"[AchievementUI] 유효하지 않은 업적 인덱스: {achievementIndex}");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[AchievementUI] 초기화 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 업적 인덱스로 초기화 (Achievement2 전용)
    /// </summary>
    public void Initialize(int achievementIndex, bool isAchievement2)
    {
        try
        {
            if (AchievementManager.Instance == null)
            {
                Debug.LogWarning("[AchievementUI] AchievementManager 인스턴스가 없습니다.");
                return;
            }

            var definitions = AchievementManager.Instance.GetAllDefinitions();
            if (achievementIndex >= 0 && achievementIndex < definitions.Count)
            {
                var definition = definitions[achievementIndex];
                bool isUnlocked = AchievementManager.Instance.IsUnlocked(definition.achievementId);
                Set(definition, isUnlocked, isAchievement2);
            }
            else
            {
                Debug.LogWarning($"[AchievementUI] 유효하지 않은 업적 인덱스: {achievementIndex}");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[AchievementUI] 초기화 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 업적 상태 업데이트
    /// </summary>
    public void UpdateStatus(bool unlocked)
    {
        try
        {
            bool wasUnlocked = isUnlocked;
            isUnlocked = unlocked;

            if (enableAnimations && wasUnlocked != unlocked)
            {
                // 해금 상태가 변경된 경우 애니메이션 실행
                StartUpdateAnimation();
            }
            else
            {
                UpdateUI(achievementData, unlocked);
            }

            OnAchievementUIUpdatedEvent?.Invoke(this);
            LogDebug($"[AchievementUI] 업적 상태 업데이트: {achievementData?.achievementTitle} - {unlocked}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[AchievementUI] 상태 업데이트 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// UI 업데이트
    /// </summary>
    private void UpdateUI(AchievementData definition, bool unlocked)
    {
        if (definition == null) return;

        try
        {
            // 제목 텍스트
            if (titleText != null)
            {
                titleText.text = definition.achievementTitle ?? "알 수 없는 업적";
            }

            // 설명 텍스트
            if (descriptionText != null)
            {
                descriptionText.text = definition.achievementDescription ?? "";
            }

            // 아이콘 이미지
            if (iconImage != null)
            {
                // 아이콘 가져오기 (SpriteManager에서 또는 정의에서)
                Sprite icon = GetAchievementIcon(definition);
                iconImage.sprite = icon;
                
                // 해금 상태에 따른 색상 설정
                iconImage.color = unlocked ? Color.white : new Color(0.8f, 0.8f, 0.8f, 1f);
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[AchievementUI] UI 업데이트 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// UI 업데이트 (Achievement2 전용)
    /// </summary>
    private void UpdateUI(AchievementData definition, bool unlocked, bool isAchievement2)
    {
        if (definition == null) return;

        try
        {
            // 기본 UI 업데이트
            UpdateUI(definition, unlocked);

            // Achievement2 전용 UI 업데이트
            if (isAchievement2)
            {
                UpdateAchievement2UI(definition);
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[AchievementUI] UI 업데이트 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// Achievement2 전용 UI 업데이트
    /// </summary>
    private void UpdateAchievement2UI(AchievementData definition)
    {
        try
        {
            // 달성 웜 이미지
            if (achieveWormImage != null)
            {
                // AchievementData에서 달성 웜 정보 가져오기
                // TODO: AchievementData에 달성 웜 정보 필드 추가 필요
                Sprite wormSprite = GetAchievementWormSprite(definition);
                achieveWormImage.sprite = wormSprite;
            }

            // 달성 웜 이름 텍스트
            if (achieveWormNameText != null)
            {
                // AchievementData에서 달성 웜 이름 가져오기
                string wormName = GetAchievementWormName(definition);
                achieveWormNameText.text = wormName ?? "알 수 없는 웜";
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[AchievementUI] Achievement2 UI 업데이트 중 오류: {ex.Message}");
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
                Sprite icon = SpriteManager.Instance.GetAchievementSprite(definition.achievementId);
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
    /// 달성 웜 스프라이트 가져오기
    /// </summary>
    private Sprite GetAchievementWormSprite(AchievementData definition)
    {
        try
        {
            // AchievementData에서 달성 웜 ID 가져오기
            string wormId = GetAchievementWormId(definition);
            if (!string.IsNullOrEmpty(wormId))
            {
                // WormManager에서 웜 데이터 가져오기
                if (WormManager.Instance != null)
                {
                    var wormData = WormManager.Instance.GetWormById(int.Parse(wormId));
                    if (wormData != null)
                    {
                        // WormData에서 스프라이트 가져오기
                        return GetWormSprite(wormData);
                    }
                }
            }

            return null;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[AchievementUI] 달성 웜 스프라이트 가져오기 중 오류: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// 달성 웜 이름 가져오기
    /// </summary>
    private string GetAchievementWormName(AchievementData definition)
    {
        try
        {
            // AchievementData에서 달성 웜 ID 가져오기
            string wormId = GetAchievementWormId(definition);
            if (!string.IsNullOrEmpty(wormId))
            {
                // WormManager에서 웜 데이터 가져오기
                if (WormManager.Instance != null)
                {
                    var wormData = WormManager.Instance.GetWormById(int.Parse(wormId));
                    if (wormData != null)
                    {
                        return wormData.name;
                    }
                }
            }

            return null;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[AchievementUI] 달성 웜 이름 가져오기 중 오류: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// AchievementData에서 달성 웜 ID 가져오기
    /// </summary>
    private string GetAchievementWormId(AchievementData definition)
    {
        // AchievementData에서 달성 웜 ID 가져오기
        if (definition != null && definition.achieveWormId >= 0)
        {
            return definition.achieveWormId.ToString();
        }
        return null;
    }

    /// <summary>
    /// 웜 데이터에서 스프라이트 가져오기
    /// </summary>
    private Sprite GetWormSprite(WormData wormData)
    {
        try
        {
            // WormData의 생명주기 단계를 기반으로 스프라이트 가져오기
            if (SpriteManager.Instance != null)
            {
                return SpriteManager.Instance.GetLifeStageSprite(wormData.lifeStage);
            }
            
            return null;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[AchievementUI] 웜 스프라이트 가져오기 중 오류: {ex.Message}");
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
        UpdateUI(achievementData, isUnlocked);

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
        info.AppendLine($"업적: {achievementData?.achievementTitle ?? "없음"}");
        info.AppendLine($"해금됨: {isUnlocked}");
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
