using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ItemTabUI : MonoBehaviour
{
    public static ItemTabUI Instance { get; private set; }

    [Header("렌더러")]
    [SerializeField] private OverViewRenderer overViewRenderer;

    [Header("모자 UI")]
    [SerializeField] private Image hatPreviewImage;
    [SerializeField] private TMP_Text hatNameText;
    [SerializeField] private Button hatButton;
    [SerializeField] private GameObject hatEquippedIcon;

    [Header("얼굴 UI")]
    [SerializeField] private Image facePreviewImage;
    [SerializeField] private TMP_Text faceNameText;
    [SerializeField] private Button faceButton;
    [SerializeField] private GameObject faceEquippedIcon;

    [Header("의상 UI")]
    [SerializeField] private Image costumePreviewImage;
    [SerializeField] private TMP_Text costumeNameText;
    [SerializeField] private Button costumeButton;
    [SerializeField] private GameObject costumeEquippedIcon;

    [Header("애니메이션")]
    [SerializeField] private bool enableAnimations = true;
    // [SerializeField] private float animationDuration = 0.3f;  // 미사용
    [SerializeField] private AnimationCurve animationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("디버그")]
    [SerializeField] private bool enableDebugLogs = false;

    // 상태 관리
    private bool isInitialized = false;
    private Dictionary<ItemData.ItemType, ItemData> currentEquippedItems;

    // 이벤트 정의
    public delegate void OnItemTabRefreshed();
    public event OnItemTabRefreshed OnItemTabRefreshedEvent;

    public delegate void OnItemPreviewClicked(ItemData.ItemType itemType, ItemData itemData);
    public event OnItemPreviewClicked OnItemPreviewClickedEvent;

    private void Awake()
    {
        InitializeSingleton();
    }

    private void Start()
    {
        InitializeItemTab();
    }

    private void InitializeSingleton()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeItemTab()
    {
        try
        {
            currentEquippedItems = new Dictionary<ItemData.ItemType, ItemData>();
            
            // 버튼 이벤트 설정
            SetupButtonEvents();
            
            // 초기 UI 업데이트
            RefreshAllPreviews();
            
            isInitialized = true;

            LogDebug("[ItemTabUI] 아이템 탭 초기화 완료");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[ItemTabUI] 초기화 중 오류: {ex.Message}");
        }
    }

    private void SetupButtonEvents()
    {
        try
        {
            // 모자 버튼
            if (hatButton != null)
            {
                hatButton.onClick.RemoveAllListeners();
                hatButton.onClick.AddListener(() => OnItemButtonClicked(ItemData.ItemType.Hat));
            }

            // 얼굴 버튼
            if (faceButton != null)
            {
                faceButton.onClick.RemoveAllListeners();
                faceButton.onClick.AddListener(() => OnItemButtonClicked(ItemData.ItemType.Face));
            }

            // 의상 버튼
            if (costumeButton != null)
            {
                costumeButton.onClick.RemoveAllListeners();
                costumeButton.onClick.AddListener(() => OnItemButtonClicked(ItemData.ItemType.Costume));
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[ItemTabUI] 버튼 이벤트 설정 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 모든 프리뷰 새로고침
    /// </summary>
    public void RefreshAllPreviews()
    {
        if (!isInitialized)
        {
            Debug.LogWarning("[ItemTabUI] 아직 초기화되지 않았습니다.");
            return;
        }

        try
        {
            RefreshPreview(ItemData.ItemType.Hat, hatPreviewImage, hatNameText, hatEquippedIcon);
            RefreshPreview(ItemData.ItemType.Face, facePreviewImage, faceNameText, faceEquippedIcon);
            RefreshPreview(ItemData.ItemType.Costume, costumePreviewImage, costumeNameText, costumeEquippedIcon);
            
            // 오버뷰 렌더러 새로고침
            if (overViewRenderer != null)
            {
                overViewRenderer.RefreshOverview();
            }

            LogDebug("[ItemTabUI] 모든 프리뷰 새로고침 완료");
            OnItemTabRefreshedEvent?.Invoke();
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[ItemTabUI] 프리뷰 새로고침 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 특정 아이템 타입 프리뷰 새로고침
    /// </summary>
    public void RefreshPreview(ItemData.ItemType itemType)
    {
        if (!isInitialized) return;

        try
        {
            switch (itemType)
            {
                case ItemData.ItemType.Hat:
                    RefreshPreview(ItemData.ItemType.Hat, hatPreviewImage, hatNameText, hatEquippedIcon);
                    break;
                case ItemData.ItemType.Face:
                    RefreshPreview(ItemData.ItemType.Face, facePreviewImage, faceNameText, faceEquippedIcon);
                    break;
                case ItemData.ItemType.Costume:
                    RefreshPreview(ItemData.ItemType.Costume, costumePreviewImage, costumeNameText, costumeEquippedIcon);
                    break;
            }

            // 오버뷰 렌더러 새로고침
            if (overViewRenderer != null)
            {
                overViewRenderer.RefreshOverview();
            }

            LogDebug($"[ItemTabUI] {itemType} 프리뷰 새로고침 완료");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[ItemTabUI] {itemType} 프리뷰 새로고침 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 프리뷰 새로고침 (내부 메서드)
    /// </summary>
    private void RefreshPreview(ItemData.ItemType type, Image previewImage, TMP_Text nameText, GameObject equippedIcon)
    {
        try
        {
            // 현재 착용된 아이템 ID 가져오기
            string itemId = GetEquippedItemId(type);
            var item = ItemManager.Instance?.GetItemById(itemId);

            // UI 업데이트
            if (item != null && item.IsValid)
            {
                // 프리뷰 이미지
                if (previewImage != null)
                {
                    previewImage.sprite = item.sprite;
                    previewImage.color = Color.white;
                }

                // 이름 텍스트
                if (nameText != null)
                {
                    nameText.text = item.DisplayName;
                }

                // 착용 아이콘
                if (equippedIcon != null)
                {
                    equippedIcon.SetActive(true);
                }

                // 현재 착용 아이템 저장
                currentEquippedItems[type] = item;
            }
            else
            {
                // 아이템이 없는 경우
                if (previewImage != null)
                {
                    previewImage.sprite = null;
                    previewImage.color = new Color(1f, 1f, 1f, 0.5f); // 반투명
                }

                if (nameText != null)
                {
                    nameText.text = GetDefaultItemName(type);
                }

                if (equippedIcon != null)
                {
                    equippedIcon.SetActive(false);
                }

                // 현재 착용 아이템에서 제거
                if (currentEquippedItems.ContainsKey(type))
                {
                    currentEquippedItems.Remove(type);
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[ItemTabUI] {type} 프리뷰 업데이트 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 착용된 아이템 ID 가져오기
    /// </summary>
    private string GetEquippedItemId(ItemData.ItemType type)
    {
        try
        {
            if (ItemManager.Instance == null) return "";

            switch (type)
            {
                case ItemData.ItemType.Hat:
                    return ItemManager.Instance.GetEquippedItemId(ItemData.ItemType.Hat);
                case ItemData.ItemType.Face:
                    return ItemManager.Instance.GetEquippedItemId(ItemData.ItemType.Face);
                case ItemData.ItemType.Costume:
                    return ItemManager.Instance.GetEquippedItemId(ItemData.ItemType.Costume);
                default:
                    return "";
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[ItemTabUI] 착용 아이템 ID 가져오기 중 오류: {ex.Message}");
            return "";
        }
    }

    /// <summary>
    /// 기본 아이템 이름 가져오기
    /// </summary>
    private string GetDefaultItemName(ItemData.ItemType type)
    {
        switch (type)
        {
            case ItemData.ItemType.Hat: return "모자 없음";
            case ItemData.ItemType.Face: return "얼굴 없음";
            case ItemData.ItemType.Costume: return "의상 없음";
            default: return "아이템 없음";
        }
    }

    /// <summary>
    /// 아이템 버튼 클릭 이벤트
    /// </summary>
    private void OnItemButtonClicked(ItemData.ItemType itemType)
    {
        try
        {
            // 현재 착용된 아이템 가져오기
            ItemData currentItem = null;
            if (currentEquippedItems.TryGetValue(itemType, out currentItem))
            {
                LogDebug($"[ItemTabUI] {itemType} 아이템 클릭: {currentItem.DisplayName}");
            }
            else
            {
                LogDebug($"[ItemTabUI] {itemType} 아이템 클릭: 착용된 아이템 없음");
            }

            // 이벤트 발생
            OnItemPreviewClickedEvent?.Invoke(itemType, currentItem);

            // 아이템 선택 팝업 열기 (추후 구현)
            LogDebug($"[ItemTabUI] {itemType} 아이템 선택 팝업 열기 요청");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[ItemTabUI] 아이템 버튼 클릭 처리 중 오류: {ex.Message}");
        }
    }



    /// <summary>
    /// 특정 아이템 타입의 현재 착용 아이템 가져오기
    /// </summary>
    public ItemData GetCurrentEquippedItem(ItemData.ItemType itemType)
    {
        return currentEquippedItems.TryGetValue(itemType, out var item) ? item : null;
    }

    /// <summary>
    /// 모든 착용 아이템 가져오기
    /// </summary>
    public Dictionary<ItemData.ItemType, ItemData> GetAllEquippedItems()
    {
        return new Dictionary<ItemData.ItemType, ItemData>(currentEquippedItems);
    }

    /// <summary>
    /// 착용된 아이템 개수 가져오기
    /// </summary>
    public int GetEquippedItemCount()
    {
        return currentEquippedItems.Count;
    }

    /// <summary>
    /// 애니메이션 활성화/비활성화
    /// </summary>
    public void SetAnimationEnabled(bool enabled)
    {
        enableAnimations = enabled;
        LogDebug($"[ItemTabUI] 애니메이션 {(enabled ? "활성화" : "비활성화")}");
    }

    /// <summary>
    /// 탭 정보 반환
    /// </summary>
    public string GetTabInfo()
    {
        var info = new System.Text.StringBuilder();
        info.AppendLine($"[ItemTabUI 정보]");
        info.AppendLine($"초기화됨: {isInitialized}");
        info.AppendLine($"착용된 아이템 수: {GetEquippedItemCount()}");
        
        foreach (var kvp in currentEquippedItems)
        {
            info.AppendLine($"{kvp.Key}: {kvp.Value.DisplayName}");
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
        // 이벤트 초기화
        OnItemTabRefreshedEvent = null;
        OnItemPreviewClickedEvent = null;
    }
}
