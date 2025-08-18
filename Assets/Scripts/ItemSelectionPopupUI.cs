using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class ItemSelectionPopupUI : MonoBehaviour
{
    [Header("UI 요소")]
    [SerializeField] private TMP_Text titleText;           // 제목 텍스트
    [SerializeField] private TMP_Text descriptionText;     // 설명 텍스트
    [SerializeField] private Transform slotParent;         // 슬롯 붙을 부모 (그리드 레이아웃)
    [SerializeField] private GameObject itemSlotPrefab;    // ItemSlotUI 프리팹
    [SerializeField] private Button closeButton;           // 닫기 버튼
    [SerializeField] private Button equipButton;           // 착용 버튼
    [SerializeField] private Button unequipButton;         // 해제 버튼

    [Header("애니메이션")]
    [SerializeField] private CanvasGroup canvasGroup;      // 페이드 애니메이션용
    [SerializeField] private RectTransform popupRect;      // 크기 애니메이션용
    [SerializeField] private bool enableAnimations = true;
    [SerializeField] private float showDuration = 0.3f;    // 표시 애니메이션 시간
    [SerializeField] private float hideDuration = 0.2f;    // 숨김 애니메이션 시간
    [SerializeField] private AnimationCurve showCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private AnimationCurve hideCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

    [Header("색상 설정")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color selectedColor = Color.yellow;
    [SerializeField] private Color equippedColor = Color.green;

    [Header("사운드")]
    [SerializeField] private bool enableSound = true;
    [SerializeField] private AudioManager.SFXType openSound = AudioManager.SFXType.Button;
    [SerializeField] private AudioManager.SFXType closeSound = AudioManager.SFXType.Button;
    [SerializeField] private AudioManager.SFXType selectSound = AudioManager.SFXType.Button;

    [Header("디버그")]
    [SerializeField] private bool enableDebugLogs = false;

    // 데이터 및 상태
    private ItemData.ItemType currentItemType;                       // 현재 아이템 타입
    private List<ItemSlotUI> slotList;                     // 슬롯 리스트
    private ItemSlotUI selectedSlot;                       // 선택된 슬롯
    private bool isShowing = false;                        // 표시 상태
    private bool isAnimating = false;                      // 애니메이션 상태
    
    // 애니메이션 상태
    private Coroutine showCoroutine;
    private Coroutine hideCoroutine;

    // 프로퍼티
    public ItemData.ItemType CurrentItemType => currentItemType;
    public ItemSlotUI SelectedSlot => selectedSlot;
    public bool IsShowing => isShowing;
    public bool IsAnimating => isAnimating;

    // 이벤트 정의
    public delegate void OnItemSelectionPopupShown(ItemSelectionPopupUI popup);
    public event OnItemSelectionPopupShown OnItemSelectionPopupShownEvent;

    public delegate void OnItemSelectionPopupHidden(ItemSelectionPopupUI popup);
    public event OnItemSelectionPopupHidden OnItemSelectionPopupHiddenEvent;

    public delegate void OnItemSelected(ItemSelectionPopupUI popup, ItemData itemData);
    public event OnItemSelected OnItemSelectedEvent;

    public delegate void OnItemEquipped(ItemSelectionPopupUI popup, ItemData itemData);
    public event OnItemEquipped OnItemEquippedEvent;

    public delegate void OnItemUnequipped(ItemSelectionPopupUI popup, ItemData.ItemType itemType);
    public event OnItemUnequipped OnItemUnequippedEvent;

    private void Awake()
    {
        InitializeComponents();
    }

    private void InitializeComponents()
    {
        try
        {
            slotList = new List<ItemSlotUI>();

            // CanvasGroup 자동 찾기
            if (canvasGroup == null)
                canvasGroup = GetComponent<CanvasGroup>();
            
            if (canvasGroup == null)
                canvasGroup = gameObject.AddComponent<CanvasGroup>();

            // RectTransform 자동 찾기
            if (popupRect == null)
                popupRect = GetComponent<RectTransform>();

            // 버튼 이벤트 설정
            SetupButtonEvents();

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

            LogDebug("[ItemSelectionPopupUI] 컴포넌트 초기화 완료");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[ItemSelectionPopupUI] 컴포넌트 초기화 중 오류: {ex.Message}");
        }
    }

    private void SetupButtonEvents()
    {
        try
        {
            // 닫기 버튼
            if (closeButton != null)
            {
                closeButton.onClick.RemoveAllListeners();
                closeButton.onClick.AddListener(ClosePopup);
            }

            // 착용 버튼
            if (equipButton != null)
            {
                equipButton.onClick.RemoveAllListeners();
                equipButton.onClick.AddListener(EquipSelectedItem);
            }

            // 해제 버튼
            if (unequipButton != null)
            {
                unequipButton.onClick.RemoveAllListeners();
                unequipButton.onClick.AddListener(UnequipCurrentItem);
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[ItemSelectionPopupUI] 버튼 이벤트 설정 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 아이템 선택 팝업 열기
    /// </summary>
    public void OpenPopup(ItemData.ItemType itemType)
    {
        try
        {
            currentItemType = itemType;
            UpdateTitle();
            RefreshSlots();
            Show();

            LogDebug($"[ItemSelectionPopupUI] 아이템 선택 팝업 열기: {itemType}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[ItemSelectionPopupUI] 팝업 열기 중 오류: {ex.Message}");
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
                AudioManager.Instance?.PlaySFX(openSound);
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

            LogDebug("[ItemSelectionPopupUI] 아이템 선택 팝업 표시");
            OnItemSelectionPopupShownEvent?.Invoke(this);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[ItemSelectionPopupUI] Show 중 오류: {ex.Message}");
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

            LogDebug("[ItemSelectionPopupUI] 아이템 선택 팝업 숨김");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[ItemSelectionPopupUI] Hide 중 오류: {ex.Message}");
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

            OnItemSelectionPopupHiddenEvent?.Invoke(this);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[ItemSelectionPopupUI] HideImmediate 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 팝업 닫기 (버튼 이벤트용)
    /// </summary>
    public void ClosePopup()
    {
        try
        {
            // 사운드 재생
            if (enableSound)
            {
                AudioManager.Instance?.PlaySFX(closeSound);
            }

            Hide();
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[ItemSelectionPopupUI] ClosePopup 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 제목 업데이트
    /// </summary>
    private void UpdateTitle()
    {
        try
        {
            if (titleText != null)
            {
                titleText.text = GetItemTypeTitle(currentItemType);
            }

            if (descriptionText != null)
            {
                descriptionText.text = GetItemTypeDescription(currentItemType);
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[ItemSelectionPopupUI] 제목 업데이트 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 아이템 타입별 제목 가져오기
    /// </summary>
    private string GetItemTypeTitle(ItemData.ItemType itemType)
    {
        switch (itemType)
        {
            case ItemData.ItemType.Hat: return "모자 선택";
            case ItemData.ItemType.Face: return "얼굴 선택";
            case ItemData.ItemType.Costume: return "의상 선택";
            case ItemData.ItemType.Accessory: return "액세서리 선택";
            default: return "아이템 선택";
        }
    }

    /// <summary>
    /// 아이템 타입별 설명 가져오기
    /// </summary>
    private string GetItemTypeDescription(ItemData.ItemType itemType)
    {
        switch (itemType)
        {
            case ItemData.ItemType.Hat: return "착용할 모자를 선택하세요";
            case ItemData.ItemType.Face: return "착용할 얼굴을 선택하세요";
            case ItemData.ItemType.Costume: return "착용할 의상을 선택하세요";
            case ItemData.ItemType.Accessory: return "착용할 액세서리를 선택하세요";
            default: return "착용할 아이템을 선택하세요";
        }
    }

    /// <summary>
    /// 슬롯 새로고침
    /// </summary>
    public void RefreshSlots()
    {
        try
        {
            ClearSlots();

            var items = GetItemsByType(currentItemType);
            foreach (var item in items)
            {
                CreateItemSlot(item);
            }

            UpdateButtonStates();

            LogDebug($"[ItemSelectionPopupUI] 슬롯 새로고침 완료: {items.Count}개 아이템");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[ItemSelectionPopupUI] 슬롯 새로고침 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 아이템 타입별 아이템 가져오기
    /// </summary>
    private List<ItemData> GetItemsByType(ItemData.ItemType itemType)
    {
        try
        {
            if (ItemManager.Instance == null) return new List<ItemData>();

            return ItemManager.Instance.GetOwnedItemInfosByType(itemType)
                .ConvertAll(info => ItemManager.Instance.GetItemById(info.itemId))
                .FindAll(item => item != null);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[ItemSelectionPopupUI] 아이템 가져오기 중 오류: {ex.Message}");
            return new List<ItemData>();
        }
    }

    /// <summary>
    /// 아이템 슬롯 생성
    /// </summary>
    private void CreateItemSlot(ItemData item)
    {
        try
        {
            if (itemSlotPrefab == null || slotParent == null) return;

            GameObject go = Instantiate(itemSlotPrefab, slotParent);
            ItemSlotUI slot = go.GetComponent<ItemSlotUI>();
            
            if (slot != null)
            {
                slot.Initialize(item, currentItemType);
                slot.OnItemClickedEvent += OnItemSlotClicked;
                slotList.Add(slot);

                // 현재 착용된 아이템인지 확인
                bool isEquipped = IsItemEquipped(item);
                slot.SetEquipped(isEquipped);
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[ItemSelectionPopupUI] 아이템 슬롯 생성 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 아이템이 착용되었는지 확인
    /// </summary>
    private bool IsItemEquipped(ItemData item)
    {
        try
        {
            if (ItemManager.Instance == null) return false;

            string equippedItemId = ItemManager.Instance.GetEquippedItemId(currentItemType);
            return item.itemId == equippedItemId;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[ItemSelectionPopupUI] 착용 상태 확인 중 오류: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// 아이템 슬롯 클릭 이벤트
    /// </summary>
    private void OnItemSlotClicked(ItemSlotUI slot)
    {
        try
        {
            // 이전 선택 해제
            if (selectedSlot != null)
            {
                selectedSlot.SetSelected(false);
            }

            // 새 선택 설정
            selectedSlot = slot;
            if (selectedSlot != null)
            {
                selectedSlot.SetSelected(true);
            }

            // 사운드 재생
            if (enableSound)
            {
                AudioManager.Instance?.PlaySFX(selectSound);
            }

            // 버튼 상태 업데이트
            UpdateButtonStates();

            // 이벤트 발생
            OnItemSelectedEvent?.Invoke(this, slot?.ItemData);

            LogDebug($"[ItemSelectionPopupUI] 아이템 선택: {slot?.ItemData?.DisplayName}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[ItemSelectionPopupUI] 아이템 슬롯 클릭 처리 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 버튼 상태 업데이트
    /// </summary>
    private void UpdateButtonStates()
    {
        try
        {
            if (equipButton != null)
            {
                equipButton.interactable = selectedSlot != null && !selectedSlot.IsEquipped;
            }

            if (unequipButton != null)
            {
                unequipButton.interactable = selectedSlot != null && selectedSlot.IsEquipped;
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[ItemSelectionPopupUI] 버튼 상태 업데이트 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 선택된 아이템 착용
    /// </summary>
    public void EquipSelectedItem()
    {
        try
        {
            if (selectedSlot == null || selectedSlot.ItemData == null) return;

            if (ItemManager.Instance != null)
            {
                ItemManager.Instance.EquipItem(selectedSlot.ItemData.itemId);
                
                // 슬롯 상태 업데이트
                RefreshSlots();
                
                // 이벤트 발생
                OnItemEquippedEvent?.Invoke(this, selectedSlot.ItemData);

                LogDebug($"[ItemSelectionPopupUI] 아이템 착용: {selectedSlot.ItemData.DisplayName}");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[ItemSelectionPopupUI] 아이템 착용 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 현재 아이템 해제
    /// </summary>
    public void UnequipCurrentItem()
    {
        try
        {
            if (ItemManager.Instance != null)
            {
                ItemManager.Instance.UnequipItem(currentItemType);
                
                // 슬롯 상태 업데이트
                RefreshSlots();
                
                // 이벤트 발생
                OnItemUnequippedEvent?.Invoke(this, currentItemType);

                LogDebug($"[ItemSelectionPopupUI] 아이템 해제: {currentItemType}");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[ItemSelectionPopupUI] 아이템 해제 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 슬롯 정리
    /// </summary>
    private void ClearSlots()
    {
        try
        {
            foreach (var slot in slotList)
            {
                if (slot != null)
                {
                    slot.OnItemClickedEvent -= OnItemSlotClicked;
                    Destroy(slot.gameObject);
                }
            }
            slotList.Clear();
            selectedSlot = null;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[ItemSelectionPopupUI] 슬롯 정리 중 오류: {ex.Message}");
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
        LogDebug($"[ItemSelectionPopupUI] 애니메이션 {(enabled ? "활성화" : "비활성화")}");
    }

    /// <summary>
    /// 사운드 활성화/비활성화
    /// </summary>
    public void SetSoundEnabled(bool enabled)
    {
        enableSound = enabled;
        LogDebug($"[ItemSelectionPopupUI] 사운드 {(enabled ? "활성화" : "비활성화")}");
    }

    /// <summary>
    /// 팝업 정보 반환
    /// </summary>
    public string GetPopupInfo()
    {
        var info = new System.Text.StringBuilder();
        info.AppendLine($"[ItemSelectionPopupUI 정보]");
        info.AppendLine($"아이템 타입: {currentItemType}");
        info.AppendLine($"슬롯 수: {slotList.Count}");
        info.AppendLine($"선택된 슬롯: {(selectedSlot != null ? selectedSlot.ItemData?.DisplayName : "없음")}");
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
        OnItemSelectionPopupShownEvent = null;
        OnItemSelectionPopupHiddenEvent = null;
        OnItemSelectedEvent = null;
        OnItemEquippedEvent = null;
        OnItemUnequippedEvent = null;
    }
}
