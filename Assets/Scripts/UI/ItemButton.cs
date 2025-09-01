using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemButton : MonoBehaviour
{
    [Header("UI 요소")]
    [SerializeField] private Image itemImage;           // 아이템 이미지
    [SerializeField] private TMP_Text itemNameText;     // 아이템 이름 텍스트
    [SerializeField] private Image itemEquippedMarkImage; // 착용 표시 이미지
    [SerializeField] private Button itemButton;          // 버튼 컴포넌트
    
    [Header("디버그")]
    [SerializeField] private bool enableDebugLogs = false;

    // 데이터 및 상태
    private ItemData itemData;                          // 아이템 데이터
    private ItemData.ItemType itemType;                // 아이템 타입
    private bool isSelected = false;                   // 선택됨 상태
    private bool isEquipped = false;                   // 착용됨 상태

    // 프로퍼티
    public ItemData ItemData => itemData;
    public ItemData.ItemType ItemType => itemType;
    public bool IsSelected => isSelected;
    public bool IsEquipped => isEquipped;

    // 이벤트 정의
    public delegate void OnItemClicked(ItemButton itemButton);
    public event OnItemClicked OnItemClickedEvent;

    private void Awake()
    {
        InitializeComponents();
    }

    private void InitializeComponents()
    {
        try
        {
            // Button 자동 찾기
            if (itemButton == null)
                itemButton = GetComponent<Button>();
            
            if (itemButton == null)
                itemButton = gameObject.AddComponent<Button>();

            // 버튼 이벤트 설정
            SetupButtonEvents();

            // 초기 상태 설정
            SetSelected(false);
            SetEquipped(false);

            LogDebug("[ItemButton] 컴포넌트 초기화 완료");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[ItemButton] 컴포넌트 초기화 중 오류: {ex.Message}");
        }
    }

    private void SetupButtonEvents()
    {
        try
        {
            if (itemButton != null)
            {
                itemButton.onClick.RemoveAllListeners();
                itemButton.onClick.AddListener(OnClick);
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[ItemButton] 버튼 이벤트 설정 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 아이템 버튼 초기화
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
                LogDebug($"[ItemButton] 아이템 버튼 초기화: {itemData.itemName}");
            }
            else
            {
                LogDebug("[ItemButton] 아이템 버튼 초기화: 빈 데이터");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[ItemButton] 아이템 버튼 초기화 중 오류: {ex.Message}");
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
            UpdateStates();

            LogDebug($"[ItemButton] UI 업데이트: {itemData.itemName}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[ItemButton] UI 업데이트 중 오류: {ex.Message}");
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
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[ItemButton] 이미지 업데이트 중 오류: {ex.Message}");
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
                itemNameText.text = itemData.itemName;
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[ItemButton] 텍스트 업데이트 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 상태 업데이트
    /// </summary>
    private void UpdateStates()
    {
        try
        {
            // 착용 표시 이미지 설정
            if (itemEquippedMarkImage != null)
            {
                itemEquippedMarkImage.gameObject.SetActive(isEquipped);
            }

            // 버튼 상호작용 설정 (착용된 아이템은 비활성화)
            if (itemButton != null)
            {
                itemButton.interactable = !isEquipped;
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[ItemButton] 상태 업데이트 중 오류: {ex.Message}");
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
                default:
                    return null;
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[ItemButton] 아이템 스프라이트 가져오기 중 오류: {ex.Message}");
            return null;
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
            LogDebug($"[ItemButton] 선택 상태 변경: {selected}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[ItemButton] 선택 상태 설정 중 오류: {ex.Message}");
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
            UpdateStates();

            LogDebug($"[ItemButton] 착용 상태 변경: {equipped}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[ItemButton] 착용 상태 설정 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 클릭 이벤트 처리
    /// </summary>
    private void OnClick()
    {
        try
        {
            if (isEquipped) return; // 착용된 아이템은 클릭 불가

            // 이벤트 발생
            OnItemClickedEvent?.Invoke(this);

            LogDebug($"[ItemButton] 아이템 버튼 클릭: {itemData?.itemName}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[ItemButton] 클릭 처리 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 버튼 정보 반환
    /// </summary>
    public string GetButtonInfo()
    {
        var info = new System.Text.StringBuilder();
        info.AppendLine($"[ItemButton 정보]");
        info.AppendLine($"아이템: {(itemData != null ? itemData.itemName : "없음")}");
        info.AppendLine($"타입: {itemType}");
        info.AppendLine($"선택됨: {isSelected}");
        info.AppendLine($"착용됨: {isEquipped}");

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
        OnItemClickedEvent = null;
    }
}
