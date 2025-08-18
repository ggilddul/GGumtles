using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class ItemSlotUI : MonoBehaviour
{
    [Header("UI 요소")]
    [SerializeField] private Image itemImage;           // 아이템 이미지
    [SerializeField] private TMP_Text itemNameText;     // 아이템 이름
    [SerializeField] private TMP_Text rarityText;       // 희귀도 텍스트
    [SerializeField] private Image rarityBorder;        // 희귀도 테두리
    [SerializeField] private GameObject equippedIcon;   // 착용 중 아이콘
    [SerializeField] private GameObject selectedIcon;   // 선택됨 아이콘
    [SerializeField] private Button slotButton;         // 슬롯 버튼
    [SerializeField] private Image backgroundImage;     // 배경 이미지

    [Header("애니메이션")]
    [SerializeField] private CanvasGroup canvasGroup;   // 페이드 애니메이션용
    [SerializeField] private RectTransform slotRect;    // 크기 애니메이션용
    [SerializeField] private bool enableAnimations = true;
    [SerializeField] private float clickAnimationDuration = 0.1f;
    [SerializeField] private float selectAnimationDuration = 0.2f;
    [SerializeField] private AnimationCurve clickCurve = AnimationCurve.EaseInOut(0, 1, 1, 0.8f);
    [SerializeField] private AnimationCurve selectCurve = AnimationCurve.EaseInOut(0, 1, 1, 1.1f);

    [Header("색상 설정")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color selectedColor = Color.yellow;
    [SerializeField] private Color equippedColor = Color.green;
    [SerializeField] private Color disabledColor = Color.gray;

    [Header("사운드")]
    [SerializeField] private bool enableSound = true;
    [SerializeField] private AudioManager.SFXType clickSound = AudioManager.SFXType.Button;
    // [SerializeField] private AudioManager.SFXType selectSound = AudioManager.SFXType.Button;  // 미사용

    [Header("디버그")]
    [SerializeField] private bool enableDebugLogs = false;

    // 데이터 및 상태
    private ItemData itemData;                          // 아이템 데이터
    private ItemData.ItemType itemType;                          // 아이템 타입
    private bool isSelected = false;                    // 선택됨 상태
    private bool isEquipped = false;                    // 착용됨 상태
    private bool isInteractable = true;                 // 상호작용 가능 상태
    
    // 애니메이션 상태
    private Coroutine clickAnimationCoroutine;
    private Coroutine selectAnimationCoroutine;

    // 프로퍼티
    public ItemData ItemData => itemData;
    public ItemData.ItemType ItemType => itemType;
    public bool IsSelected => isSelected;
    public bool IsEquipped => isEquipped;
    public bool IsInteractable => isInteractable;

    // 이벤트 정의
    public delegate void OnItemClicked(ItemSlotUI slot);
    public event OnItemClicked OnItemClickedEvent;

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
            if (slotRect == null)
                slotRect = GetComponent<RectTransform>();

            // Button 자동 찾기
            if (slotButton == null)
                slotButton = GetComponent<Button>();
            
            if (slotButton == null)
                slotButton = gameObject.AddComponent<Button>();

            // 버튼 이벤트 설정
            SetupButtonEvents();

            // 초기 상태 설정
            SetSelected(false);
            SetEquipped(false);
            SetInteractable(true);

            LogDebug("[ItemSlotUI] 컴포넌트 초기화 완료");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[ItemSlotUI] 컴포넌트 초기화 중 오류: {ex.Message}");
        }
    }

    private void SetupButtonEvents()
    {
        try
        {
            if (slotButton != null)
            {
                slotButton.onClick.RemoveAllListeners();
                slotButton.onClick.AddListener(OnClick);
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[ItemSlotUI] 버튼 이벤트 설정 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 슬롯 초기화
    /// </summary>
    public void Initialize(ItemData data, ItemData.ItemType type)
    {
        try
        {
            itemData = data;
            itemType = type;

            if (itemData != null)
            {
                UpdateUI();
                LogDebug($"[ItemSlotUI] 슬롯 초기화: {itemData.DisplayName}");
            }
            else
            {
                LogDebug("[ItemSlotUI] 슬롯 초기화: 빈 슬롯");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[ItemSlotUI] 슬롯 초기화 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// UI 업데이트
    /// </summary>
    public void UpdateUI()
    {
        try
        {
            if (itemData == null) return;

            UpdateImages();
            UpdateTexts();
            UpdateColors();
            UpdateStates();

            LogDebug($"[ItemSlotUI] UI 업데이트: {itemData.DisplayName}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[ItemSlotUI] UI 업데이트 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 이미지 업데이트
    /// </summary>
    private void UpdateImages()
    {
        try
        {
            // 아이템 이미지 설정
            if (itemImage != null)
            {
                Sprite itemSprite = GetItemSprite();
                itemImage.sprite = itemSprite;
                itemImage.color = itemSprite != null ? Color.white : Color.clear;
            }

            // 희귀도 테두리 설정
            if (rarityBorder != null)
            {
                rarityBorder.color = GetRarityColor();
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[ItemSlotUI] 이미지 업데이트 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 텍스트 업데이트
    /// </summary>
    private void UpdateTexts()
    {
        try
        {
            // 아이템 이름 설정
            if (itemNameText != null)
            {
                itemNameText.text = itemData.DisplayName;
            }

            // 희귀도 텍스트 설정
            if (rarityText != null)
            {
                rarityText.text = itemData.RarityText;
                rarityText.color = GetRarityColor();
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[ItemSlotUI] 텍스트 업데이트 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 색상 업데이트
    /// </summary>
    private void UpdateColors()
    {
        try
        {
            Color targetColor = normalColor;

            if (isEquipped)
            {
                targetColor = equippedColor;
            }
            else if (isSelected)
            {
                targetColor = selectedColor;
            }

            if (!isInteractable)
            {
                targetColor = disabledColor;
            }

            // 배경 색상 설정
            if (backgroundImage != null)
            {
                backgroundImage.color = targetColor;
            }

            // CanvasGroup 알파 설정
            if (canvasGroup != null)
            {
                canvasGroup.alpha = isInteractable ? 1f : 0.5f;
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[ItemSlotUI] 색상 업데이트 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 상태 업데이트
    /// </summary>
    private void UpdateStates()
    {
        try
        {
            // 착용 아이콘 설정
            if (equippedIcon != null)
            {
                equippedIcon.SetActive(isEquipped);
            }

            // 선택 아이콘 설정
            if (selectedIcon != null)
            {
                selectedIcon.SetActive(isSelected);
            }

            // 버튼 상호작용 설정
            if (slotButton != null)
            {
                slotButton.interactable = isInteractable;
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[ItemSlotUI] 상태 업데이트 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 아이템 스프라이트 가져오기
    /// </summary>
    private Sprite GetItemSprite()
    {
        try
        {
            if (itemData == null) return null;

            // ItemType에 따라 다른 스프라이트 가져오기
            switch (itemType)
            {
                case ItemData.ItemType.Hat:
                    return SpriteManager.Instance?.GetHatSprite(itemData.itemId);
                case ItemData.ItemType.Face:
                    return SpriteManager.Instance?.GetFaceSprite(itemData.itemId);
                case ItemData.ItemType.Costume:
                    return SpriteManager.Instance?.GetCostumeSprite(itemData.itemId);
                case ItemData.ItemType.Accessory:
                    return SpriteManager.Instance?.GetAccessorySprite(itemData.itemId);
                default:
                    return itemData.sprite;
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[ItemSlotUI] 아이템 스프라이트 가져오기 중 오류: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// 희귀도 색상 가져오기
    /// </summary>
    private Color GetRarityColor()
    {
        try
        {
            if (itemData == null) return Color.white;
            return itemData.RarityColor;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[ItemSlotUI] 희귀도 색상 가져오기 중 오류: {ex.Message}");
            return Color.white;
        }
    }

    /// <summary>
    /// 선택 상태 설정
    /// </summary>
    public void SetSelected(bool selected)
    {
        try
        {
            if (isSelected == selected) return;

            isSelected = selected;
            UpdateColors();
            UpdateStates();

            // 선택 애니메이션
            if (enableAnimations && selected)
            {
                StartSelectAnimation();
            }

            LogDebug($"[ItemSlotUI] 선택 상태 변경: {selected}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[ItemSlotUI] 선택 상태 설정 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 착용 상태 설정
    /// </summary>
    public void SetEquipped(bool equipped)
    {
        try
        {
            if (isEquipped == equipped) return;

            isEquipped = equipped;
            UpdateColors();
            UpdateStates();

            LogDebug($"[ItemSlotUI] 착용 상태 변경: {equipped}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[ItemSlotUI] 착용 상태 설정 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 상호작용 가능 상태 설정
    /// </summary>
    public void SetInteractable(bool interactable)
    {
        try
        {
            if (isInteractable == interactable) return;

            isInteractable = interactable;
            UpdateColors();
            UpdateStates();

            LogDebug($"[ItemSlotUI] 상호작용 상태 변경: {interactable}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[ItemSlotUI] 상호작용 상태 설정 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 클릭 이벤트 처리
    /// </summary>
    private void OnClick()
    {
        try
        {
            if (!isInteractable) return;

            // 클릭 애니메이션
            if (enableAnimations)
            {
                StartClickAnimation();
            }

            // 사운드 재생
            if (enableSound)
            {
                AudioManager.Instance?.PlaySFX(clickSound);
            }

            // 이벤트 발생
            OnItemClickedEvent?.Invoke(this);

            LogDebug($"[ItemSlotUI] 슬롯 클릭: {itemData?.DisplayName}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[ItemSlotUI] 클릭 처리 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 클릭 애니메이션 시작
    /// </summary>
    private void StartClickAnimation()
    {
        if (clickAnimationCoroutine != null)
        {
            StopCoroutine(clickAnimationCoroutine);
        }

        clickAnimationCoroutine = StartCoroutine(ClickAnimationCoroutine());
    }

    /// <summary>
    /// 선택 애니메이션 시작
    /// </summary>
    private void StartSelectAnimation()
    {
        if (selectAnimationCoroutine != null)
        {
            StopCoroutine(selectAnimationCoroutine);
        }

        selectAnimationCoroutine = StartCoroutine(SelectAnimationCoroutine());
    }

    /// <summary>
    /// 클릭 애니메이션 코루틴
    /// </summary>
    private IEnumerator ClickAnimationCoroutine()
    {
        if (slotRect == null) yield break;

        Vector3 originalScale = slotRect.localScale;
        float elapsed = 0f;

        while (elapsed < clickAnimationDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / clickAnimationDuration;
            float curveValue = clickCurve.Evaluate(progress);

            slotRect.localScale = Vector3.Lerp(originalScale, originalScale * curveValue, progress);
            yield return null;
        }

        slotRect.localScale = originalScale;
        clickAnimationCoroutine = null;
    }

    /// <summary>
    /// 선택 애니메이션 코루틴
    /// </summary>
    private IEnumerator SelectAnimationCoroutine()
    {
        if (slotRect == null) yield break;

        Vector3 originalScale = slotRect.localScale;
        float elapsed = 0f;

        while (elapsed < selectAnimationDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / selectAnimationDuration;
            float curveValue = selectCurve.Evaluate(progress);

            slotRect.localScale = Vector3.Lerp(originalScale, originalScale * curveValue, progress);
            yield return null;
        }

        slotRect.localScale = originalScale;
        selectAnimationCoroutine = null;
    }

    /// <summary>
    /// 애니메이션 활성화/비활성화
    /// </summary>
    public void SetAnimationEnabled(bool enabled)
    {
        enableAnimations = enabled;
        LogDebug($"[ItemSlotUI] 애니메이션 {(enabled ? "활성화" : "비활성화")}");
    }

    /// <summary>
    /// 사운드 활성화/비활성화
    /// </summary>
    public void SetSoundEnabled(bool enabled)
    {
        enableSound = enabled;
        LogDebug($"[ItemSlotUI] 사운드 {(enabled ? "활성화" : "비활성화")}");
    }

    /// <summary>
    /// 슬롯 정보 반환
    /// </summary>
    public string GetSlotInfo()
    {
        var info = new System.Text.StringBuilder();
        info.AppendLine($"[ItemSlotUI 정보]");
        info.AppendLine($"아이템: {(itemData != null ? itemData.DisplayName : "없음")}");
        info.AppendLine($"타입: {itemType}");
        info.AppendLine($"선택됨: {isSelected}");
        info.AppendLine($"착용됨: {isEquipped}");
        info.AppendLine($"상호작용 가능: {isInteractable}");

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
        if (clickAnimationCoroutine != null)
        {
            StopCoroutine(clickAnimationCoroutine);
        }

        if (selectAnimationCoroutine != null)
        {
            StopCoroutine(selectAnimationCoroutine);
        }

        // 이벤트 초기화
        OnItemClickedEvent = null;
    }
}
