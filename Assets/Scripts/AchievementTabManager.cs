using UnityEngine;
using System.Collections.Generic;

public class AchievementTabManager : MonoBehaviour
{
    [Header("UI 컴포넌트")]
    [SerializeField] private Transform contentRoot; // ScrollView Content
    [SerializeField] private GameObject achievementPrefab;
    [SerializeField] private Sprite[] achievementIcons; // 업적 아이콘 배열

    [Header("설정")]
    [SerializeField] private bool enableAnimations = true;
    [SerializeField] private bool enableDebugLogs = false;
    [SerializeField] private bool useObjectPooling = true;
    [SerializeField] private int poolSize = 20;

    [Header("애니메이션")]
    [SerializeField] private float spawnDelay = 0.1f;
    [SerializeField] private AnimationCurve spawnCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    // 상태 관리
    private bool isInitialized = false;
    private List<AchievementButtonUI> activeButtons = new List<AchievementButtonUI>();
    private Queue<AchievementButtonUI> buttonPool = new Queue<AchievementButtonUI>();

    // 이벤트 정의
    public delegate void OnAchievementTabShown();
    public delegate void OnAchievementButtonCreated(AchievementButtonUI button, int index);
    public event OnAchievementTabShown OnAchievementTabShownEvent;
    public event OnAchievementButtonCreated OnAchievementButtonCreatedEvent;

    private void Awake()
    {
        InitializeAchievementTab();
    }

    private void OnEnable()
    {
        ShowAchievements();
    }

    private void InitializeAchievementTab()
    {
        try
        {
            // 자동으로 컴포넌트 찾기
            if (contentRoot == null)
            {
                var scrollView = GetComponentInChildren<UnityEngine.UI.ScrollRect>();
                if (scrollView != null && scrollView.content != null)
                {
                    contentRoot = scrollView.content;
                }
            }

            // 오브젝트 풀 초기화
            if (useObjectPooling)
            {
                InitializeButtonPool();
            }

            isInitialized = true;
            LogDebug("[AchievementTabManager] 업적 탭 초기화 완료");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[AchievementTabManager] 초기화 중 오류: {ex.Message}");
        }
    }

    private void InitializeButtonPool()
    {
        if (achievementPrefab == null) return;

        try
        {
            for (int i = 0; i < poolSize; i++)
            {
                GameObject pooledObject = Instantiate(achievementPrefab, contentRoot);
                var buttonUI = pooledObject.GetComponent<AchievementButtonUI>();
                
                if (buttonUI != null)
                {
                    buttonPool.Enqueue(buttonUI);
                    pooledObject.SetActive(false);
                }
            }

            LogDebug($"[AchievementTabManager] 버튼 풀 초기화 완료 (크기: {poolSize})");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[AchievementTabManager] 버튼 풀 초기화 중 오류: {ex.Message}");
        }
    }

    private void ShowAchievements()
    {
        if (!isInitialized) return;

        try
        {
            // 기존 버튼들 정리
            ClearExistingButtons();

            // 업적 데이터 가져오기
            var definitions = AchievementManager.Instance?.GetAllDefinitions();
            var statuses = AchievementManager.Instance?.GetAllStatuses();

            if (definitions == null || statuses == null)
            {
                Debug.LogWarning("[AchievementTabManager] 업적 데이터를 가져올 수 없습니다.");
                return;
            }

            int count = Mathf.Min(definitions.Count, statuses.Count);
            LogDebug($"[AchievementTabManager] 업적 표시 시작: {count}개");

            if (enableAnimations)
            {
                StartCoroutine(ShowAchievementsWithAnimation(definitions, statuses, count));
            }
            else
            {
                ShowAchievementsImmediate(definitions, statuses, count);
            }

            OnAchievementTabShownEvent?.Invoke();
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[AchievementTabManager] 업적 표시 중 오류: {ex.Message}");
        }
    }

    private void ShowAchievementsImmediate(List<AchievementData> definitions, List<AchievementStatus> statuses, int count)
    {
        for (int i = 0; i < count; i++)
        {
            CreateAchievementButton(definitions[i], statuses[i], i);
        }
    }

    private System.Collections.IEnumerator ShowAchievementsWithAnimation(List<AchievementData> definitions, List<AchievementStatus> statuses, int count)
    {
        for (int i = 0; i < count; i++)
        {
            CreateAchievementButton(definitions[i], statuses[i], i);
            
            // 애니메이션 효과
            if (activeButtons.Count > 0)
            {
                var lastButton = activeButtons[activeButtons.Count - 1];
                StartCoroutine(AnimateButtonSpawn(lastButton));
            }

            yield return new WaitForSeconds(spawnDelay);
        }
    }

    private System.Collections.IEnumerator AnimateButtonSpawn(AchievementButtonUI button)
    {
        if (button == null) yield break;

        var rectTransform = button.GetComponent<RectTransform>();
        if (rectTransform == null) yield break;

        Vector3 originalScale = rectTransform.localScale;
        rectTransform.localScale = Vector3.zero;

        float elapsed = 0f;
        float duration = 0.3f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            float curveValue = spawnCurve.Evaluate(progress);

            rectTransform.localScale = Vector3.Lerp(Vector3.zero, originalScale, curveValue);
            yield return null;
        }

        rectTransform.localScale = originalScale;
    }

    private void CreateAchievementButton(AchievementData definition, AchievementStatus status, int index)
    {
        try
        {
            AchievementButtonUI buttonUI = null;

            if (useObjectPooling && buttonPool.Count > 0)
            {
                // 풀에서 가져오기
                buttonUI = buttonPool.Dequeue();
                buttonUI.gameObject.SetActive(true);
            }
            else
            {
                // 새로 생성
                GameObject item = Instantiate(achievementPrefab, contentRoot);
                buttonUI = item.GetComponent<AchievementButtonUI>();
            }

            if (buttonUI != null)
            {
                Sprite icon = GetIconFor(index);
                buttonUI.Set(definition, icon, index, status);
                
                activeButtons.Add(buttonUI);
                OnAchievementButtonCreatedEvent?.Invoke(buttonUI, index);

                LogDebug($"[AchievementTabManager] 업적 버튼 생성: {definition?.ach_title}");
            }
            else
            {
                Debug.LogWarning("[AchievementTabManager] AchievementPrefab에 AchievementButtonUI 컴포넌트가 없습니다.");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[AchievementTabManager] 업적 버튼 생성 중 오류: {ex.Message}");
        }
    }

    private void ClearExistingButtons()
    {
        try
        {
            if (useObjectPooling)
            {
                // 풀로 반환
                foreach (var button in activeButtons)
                {
                    if (button != null)
                    {
                        button.gameObject.SetActive(false);
                        buttonPool.Enqueue(button);
                    }
                }
            }
            else
            {
                // 기존 자식들 모두 삭제
                foreach (Transform child in contentRoot)
                {
                    Destroy(child.gameObject);
                }
            }

            activeButtons.Clear();
            LogDebug("[AchievementTabManager] 기존 버튼들 정리 완료");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[AchievementTabManager] 버튼 정리 중 오류: {ex.Message}");
        }
    }

    private Sprite GetIconFor(int index)
    {
        try
        {
            if (achievementIcons != null && index >= 0 && index < achievementIcons.Length)
            {
                return achievementIcons[index];
            }
            
            if (achievementIcons != null && achievementIcons.Length > 0)
            {
                return achievementIcons[0]; // 기본 아이콘
            }

            // SpriteManager에서 가져오기 시도
            if (SpriteManager.Instance != null)
            {
                return SpriteManager.Instance.GetAchievementSprite($"Ach_{index:D2}");
            }

            return null;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[AchievementTabManager] 아이콘 가져오기 중 오류: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// 업적 탭 새로고침
    /// </summary>
    public void RefreshAchievements()
    {
        ShowAchievements();
        LogDebug("[AchievementTabManager] 업적 탭 새로고침");
    }

    /// <summary>
    /// 특정 업적 버튼 업데이트
    /// </summary>
    public void UpdateAchievementButton(int index)
    {
        try
        {
            if (index >= 0 && index < activeButtons.Count)
            {
                var button = activeButtons[index];
                var definition = AchievementManager.Instance?.GetAchievementData($"Ach_{index:D2}");
                var status = AchievementManager.Instance?.GetStatusById($"Ach_{index:D2}");

                if (definition != null && status != null)
                {
                    Sprite icon = GetIconFor(index);
                    button.UpdateStatus(status);
                    LogDebug($"[AchievementTabManager] 업적 버튼 업데이트: {definition.ach_title}");
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[AchievementTabManager] 업적 버튼 업데이트 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 애니메이션 활성화/비활성화
    /// </summary>
    public void SetAnimationEnabled(bool enabled)
    {
        enableAnimations = enabled;
        LogDebug($"[AchievementTabManager] 애니메이션 {(enabled ? "활성화" : "비활성화")}");
    }

    /// <summary>
    /// 오브젝트 풀링 활성화/비활성화
    /// </summary>
    public void SetObjectPoolingEnabled(bool enabled)
    {
        useObjectPooling = enabled;
        LogDebug($"[AchievementTabManager] 오브젝트 풀링 {(enabled ? "활성화" : "비활성화")}");
    }

    /// <summary>
    /// 탭 정보 반환
    /// </summary>
    public string GetTabInfo()
    {
        var info = new System.Text.StringBuilder();
        info.AppendLine($"[AchievementTabManager 정보]");
        info.AppendLine($"활성 버튼 수: {activeButtons.Count}");
        info.AppendLine($"풀 크기: {buttonPool.Count}");
        info.AppendLine($"애니메이션: {(enableAnimations ? "활성화" : "비활성화")}");
        info.AppendLine($"오브젝트 풀링: {(useObjectPooling ? "활성화" : "비활성화")}");

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
        // 이벤트 구독 해제
        OnAchievementTabShownEvent = null;
        OnAchievementButtonCreatedEvent = null;
    }
}
