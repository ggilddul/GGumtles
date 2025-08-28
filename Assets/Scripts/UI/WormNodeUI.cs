using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class WormNodeUI : MonoBehaviour
{
    [Header("외형 이미지")]
    [SerializeField] private Image hatImage;
    [SerializeField] private Image faceImage;
    [SerializeField] private Image costumeImage;
    [SerializeField] private Image bodyImage;           // 생명주기별 몸체 이미지

    [Header("텍스트 요소")]
    [SerializeField] private TMP_Text generationText;   // 세대 텍스트
    [SerializeField] private TMP_Text nameText;         // 이름 텍스트
    [SerializeField] private TMP_Text ageText;          // 나이 텍스트
    [SerializeField] private TMP_Text lifeStageText;    // 생명주기 텍스트
    [SerializeField] private TMP_Text rarityText;       // 희귀도 텍스트
    [SerializeField] private TMP_Text statusText;       // 상태 텍스트 (생존/사망)

    [Header("상태 표시")]
    [SerializeField] private Image statusIcon;          // 상태 아이콘
    [SerializeField] private Image rarityIcon;          // 희귀도 아이콘
    [SerializeField] private Image lifeStageIcon;       // 생명주기 아이콘
    [SerializeField] private Image backgroundImage;     // 배경 이미지

    [Header("애니메이션")]
    [SerializeField] private CanvasGroup canvasGroup;   // 페이드 애니메이션용
    [SerializeField] private bool enableAnimations = true;
    [SerializeField] private float animationDuration = 0.3f;
    [SerializeField] private AnimationCurve animationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("색상 설정")]
    [SerializeField] private Color aliveColor = Color.white;
    [SerializeField] private Color deadColor = Color.gray;
    [SerializeField] private Color selectedColor = Color.yellow;
    [SerializeField] private Color normalColor = Color.white;

    [Header("디버그")]
    [SerializeField] private bool enableDebugLogs = false;

    // 데이터 및 상태
    private WormData nodeData;                          // 노드 데이터
    private bool isSelected = false;                    // 선택 상태
    private bool isHovered = false;                     // 호버 상태
    
    // 표시 옵션
    private bool showDeathStatus = true;
    private bool showLifeStage = true;
    private bool showRarity = true;
    
    // 애니메이션 상태
    private bool isAnimating = false;
    private Coroutine animationCoroutine;

    // 프로퍼티
    public WormData NodeData => nodeData;
    public int Generation => nodeData?.generation ?? 0;
    public bool IsSelected => isSelected;
    public bool IsHovered => isHovered;
    public bool IsAnimating => isAnimating;

    // 이벤트 정의
    public delegate void OnNodeClicked(WormNodeUI node);
    public event OnNodeClicked OnNodeClickedEvent;

    public delegate void OnNodeHovered(WormNodeUI node, bool isHovered);
    public event OnNodeHovered OnNodeHoveredEvent;

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

            // Button 컴포넌트 추가 (클릭 이벤트용)
            var button = GetComponent<Button>();
            if (button == null)
                button = gameObject.AddComponent<Button>();

            // 클릭 이벤트 설정
            button.onClick.AddListener(OnClick);

            LogDebug("[WormNodeUI] 컴포넌트 초기화 완료");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[WormNodeUI] 컴포넌트 초기화 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 노드 데이터 설정
    /// </summary>
    public void SetData(WormData newData)
    {
        if (newData == null)
        {
            Debug.LogWarning("[WormNodeUI] null 데이터가 전달되었습니다.");
            return;
        }

        try
        {
            // 이전 데이터와 비교하여 변경사항 확인
            bool isDataChanged = nodeData != newData;
            bool isLifeStageChanged = nodeData?.lifeStage != newData.lifeStage;
            bool isStatusChanged = nodeData?.IsAlive != newData.IsAlive;

            nodeData = newData;

            // UI 업데이트
            UpdateUI();

            // 애니메이션 처리
            if (enableAnimations && isDataChanged)
            {
                if (isLifeStageChanged || isStatusChanged)
                {
                    StartUpdateAnimation();
                }
            }

            LogDebug($"[WormNodeUI] 데이터 설정 완료: {newData.DisplayName} (세대: {newData.generation})");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[WormNodeUI] 데이터 설정 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 표시 옵션 설정
    /// </summary>
    public void SetDisplayOptions(bool showDeath, bool showLifeStage, bool showRarity)
    {
        showDeathStatus = showDeath;
        this.showLifeStage = showLifeStage;
        this.showRarity = showRarity;

        // UI 업데이트
        UpdateUI();
    }

    /// <summary>
    /// 선택 상태 설정
    /// </summary>
    public void SetSelected(bool selected)
    {
        if (isSelected != selected)
        {
            isSelected = selected;
            UpdateSelectionVisual();
        }
    }

    /// <summary>
    /// 호버 상태 설정
    /// </summary>
    public void SetHovered(bool hovered)
    {
        if (isHovered != hovered)
        {
            isHovered = hovered;
            UpdateHoverVisual();
            OnNodeHoveredEvent?.Invoke(this, hovered);
        }
    }

    /// <summary>
    /// UI 전체 업데이트
    /// </summary>
    private void UpdateUI()
    {
        if (nodeData == null) return;

        try
        {
            UpdateImages();
            UpdateTexts();
            UpdateStatusDisplay();
            UpdateColors();
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[WormNodeUI] UI 업데이트 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 이미지 업데이트
    /// </summary>
    private void UpdateImages()
    {
        try
        {
            // 외형 이미지 설정
            bool isDead = !nodeData.IsAlive;

            // 죽었으면 꾸미기 제거
            if (hatImage != null)
                hatImage.sprite = isDead ? null : GetItemSprite(nodeData.hatItemId);
            
            if (faceImage != null)
                faceImage.sprite = isDead ? null : GetItemSprite(nodeData.faceItemId);
            
            if (costumeImage != null)
                costumeImage.sprite = isDead ? null : GetItemSprite(nodeData.costumeItemId);
            
            if (bodyImage != null)
                bodyImage.sprite = GetLifeStageSprite(nodeData.lifeStage);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[WormNodeUI] 이미지 업데이트 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 텍스트 업데이트
    /// </summary>
    private void UpdateTexts()
    {
        try
        {
            // 세대 텍스트
            if (generationText != null)
            {
                generationText.text = $"{nodeData.generation}세대";
            }

            // 이름 텍스트
            if (nameText != null)
            {
                nameText.text = nodeData.DisplayName;
            }

            // 나이 텍스트
            if (ageText != null)
            {
                ageText.text = FormatAge(nodeData.age);
            }

            // 생명주기 텍스트
            if (lifeStageText != null && showLifeStage)
            {
                lifeStageText.text = GetLifeStageText(nodeData.lifeStage);
                lifeStageText.gameObject.SetActive(true);
            }
            else if (lifeStageText != null)
            {
                lifeStageText.gameObject.SetActive(false);
            }

            // 희귀도 텍스트
            if (rarityText != null && showRarity)
            {
                rarityText.text = nodeData.GetRarityText();
                rarityText.color = nodeData.GetRarityColor();
                rarityText.gameObject.SetActive(true);
            }
            else if (rarityText != null)
            {
                rarityText.gameObject.SetActive(false);
            }

            // 상태 텍스트
            if (statusText != null && showDeathStatus)
            {
                statusText.text = nodeData.IsAlive ? "생존" : "사망";
                statusText.color = nodeData.IsAlive ? aliveColor : deadColor;
                statusText.gameObject.SetActive(true);
            }
            else if (statusText != null)
            {
                statusText.gameObject.SetActive(false);
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[WormNodeUI] 텍스트 업데이트 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 상태 표시 업데이트
    /// </summary>
    private void UpdateStatusDisplay()
    {
        try
        {
            // 상태 아이콘
            if (statusIcon != null && showDeathStatus)
            {
                statusIcon.gameObject.SetActive(true);
                // 여기에 상태별 아이콘 설정 로직 추가 가능
            }
            else if (statusIcon != null)
            {
                statusIcon.gameObject.SetActive(false);
            }

            // 희귀도 아이콘
            if (rarityIcon != null && showRarity)
            {
                rarityIcon.gameObject.SetActive(true);
                rarityIcon.color = nodeData.GetRarityColor();
            }
            else if (rarityIcon != null)
            {
                rarityIcon.gameObject.SetActive(false);
            }

            // 생명주기 아이콘
            if (lifeStageIcon != null && showLifeStage)
            {
                lifeStageIcon.gameObject.SetActive(true);
                // 여기에 생명주기별 아이콘 설정 로직 추가 가능
            }
            else if (lifeStageIcon != null)
            {
                lifeStageIcon.gameObject.SetActive(false);
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[WormNodeUI] 상태 표시 업데이트 중 오류: {ex.Message}");
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
                canvasGroup.alpha = nodeData.IsAlive ? 1f : 0.7f;
            }

            // 이름 텍스트 색상
            if (nameText != null)
            {
                nameText.color = nodeData.IsAlive ? aliveColor : deadColor;
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[WormNodeUI] 색상 업데이트 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 선택 상태 시각적 업데이트
    /// </summary>
    private void UpdateSelectionVisual()
    {
        try
        {
            if (backgroundImage != null)
            {
                backgroundImage.color = isSelected ? selectedColor : normalColor;
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[WormNodeUI] 선택 상태 업데이트 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 호버 상태 시각적 업데이트
    /// </summary>
    private void UpdateHoverVisual()
    {
        try
        {
            // 호버 효과 (예: 스케일 변경)
            if (isHovered)
            {
                transform.localScale = Vector3.one * 1.1f;
            }
            else
            {
                transform.localScale = Vector3.one;
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[WormNodeUI] 호버 상태 업데이트 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 클릭 이벤트 처리
    /// </summary>
    private void OnClick()
    {
        try
        {
            LogDebug($"[WormNodeUI] 노드 클릭: {nodeData?.DisplayName}");
            OnNodeClickedEvent?.Invoke(this);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[WormNodeUI] 클릭 이벤트 처리 중 오류: {ex.Message}");
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
            Debug.LogError($"[WormNodeUI] 아이템 스프라이트 로드 중 오류: {ex.Message}");
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
            Debug.LogError($"[WormNodeUI] 생명주기 스프라이트 로드 중 오류: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// 나이 포맷팅
    /// </summary>
    private string FormatAge(int age)
    {
        try
        {
            int days = age / 1440;
            int hours = (age % 1440) / 60;
            int minutes = age % 60;

            if (days > 0)
                return $"{days}일 {hours}시간";
            else if (hours > 0)
                return $"{hours}시간 {minutes}분";
            else
                return $"{minutes}분";
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[WormNodeUI] 나이 포맷팅 중 오류: {ex.Message}");
            return "알 수 없음";
        }
    }

    /// <summary>
    /// 생명주기 텍스트 가져오기
    /// </summary>
    private string GetLifeStageText(int stage)
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
    /// 업데이트 애니메이션 시작
    /// </summary>
    private void StartUpdateAnimation()
    {
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }

        animationCoroutine = StartCoroutine(UpdateAnimationCoroutine());
    }

    /// <summary>
    /// 업데이트 애니메이션 코루틴
    /// </summary>
    private IEnumerator UpdateAnimationCoroutine()
    {
        if (canvasGroup == null) yield break;

        isAnimating = true;

        // 페이드 아웃
        float elapsed = 0f;
        float startAlpha = canvasGroup.alpha;

        while (elapsed < animationDuration * 0.5f)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / (animationDuration * 0.5f);
            float curveValue = animationCurve.Evaluate(progress);
            
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0.5f, curveValue);
            yield return null;
        }

        // UI 업데이트 (애니메이션 중간에)
        UpdateUI();

        // 페이드 인
        elapsed = 0f;
        startAlpha = canvasGroup.alpha;

        while (elapsed < animationDuration * 0.5f)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / (animationDuration * 0.5f);
            float curveValue = animationCurve.Evaluate(progress);
            
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 1f, curveValue);
            yield return null;
        }

        canvasGroup.alpha = 1f;

        isAnimating = false;
        animationCoroutine = null;
    }

    /// <summary>
    /// 애니메이션 활성화/비활성화
    /// </summary>
    public void SetAnimationEnabled(bool enabled)
    {
        enableAnimations = enabled;
        LogDebug($"[WormNodeUI] 애니메이션 {(enabled ? "활성화" : "비활성화")}");
    }

    /// <summary>
    /// 현재 데이터 가져오기 (하위 호환성)
    /// </summary>
    public WormData GetCurrentData()
    {
        return nodeData;
    }

    /// <summary>
    /// 노드 정보 반환
    /// </summary>
    public string GetNodeInfo()
    {
        if (nodeData == null) return "데이터 없음";

        var info = new System.Text.StringBuilder();
        info.AppendLine($"[WormNodeUI 정보]");
        info.AppendLine($"세대: {nodeData.generation}");
        info.AppendLine($"이름: {nodeData.DisplayName}");
        info.AppendLine($"나이: {FormatAge(nodeData.age)}");
        info.AppendLine($"생명주기: {GetLifeStageText(nodeData.lifeStage)}");
        info.AppendLine($"희귀도: {nodeData.GetRarityText()}");
        info.AppendLine($"상태: {(nodeData.IsAlive ? "생존" : "사망")}");
        info.AppendLine($"선택됨: {isSelected}");
        info.AppendLine($"호버됨: {isHovered}");
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
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }
    }
}
