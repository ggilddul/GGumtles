using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GGumtles.Data;
using System.Collections.Generic;
using GGumtles.Managers;
using GGumtles.Utils;

namespace GGumtles.UI
{
    public class ItemTabUI : MonoBehaviour
{
    public static ItemTabUI Instance { get; private set; }

    [Header("개요 UI")]
    [SerializeField] private GameObject overviewPanel;
    [SerializeField] private Image overviewImage;
    [SerializeField] private Button itemDrawButton;
    [SerializeField] private Button shuffleButton;

    // [Header("렌더러")] - OverviewRenderer 제거
    // [SerializeField] private OverViewRenderer overViewRenderer;

    [Header("모자 UI")]
    [SerializeField] private Image hatPreviewImage;
    [SerializeField] private TMP_Text hatNameText;
    [SerializeField] private Button hatButton;

    [Header("얼굴 UI")]
    [SerializeField] private Image facePreviewImage;
    [SerializeField] private TMP_Text faceNameText;
    [SerializeField] private Button faceButton;

    [Header("의상 UI")]
    [SerializeField] private Image costumePreviewImage;
    [SerializeField] private TMP_Text costumeNameText;
    [SerializeField] private Button costumeButton;

    [Header("디버그")]
    [SerializeField] private bool enableDebugLogs = true;

    // 상태 관리
    private bool isInitialized = false;
    private Dictionary<ItemData.ItemType, ItemData> currentEquippedItems;
    private Coroutine waitWormCoroutine;

    // 이벤트 정의
    public delegate void OnItemTabRefreshed();
    public event OnItemTabRefreshed OnItemTabRefreshedEvent;

    // OnItemPreviewClickedEvent는 더 이상 사용되지 않음 (ReusableButton으로 대체)
    // public delegate void OnItemPreviewClicked(ItemData.ItemType itemType, ItemData itemData);
    // public event OnItemPreviewClicked OnItemPreviewClickedEvent;

    private void Awake()
    {
        InitializeSingleton();
    }

    private void Start()
    {
        // 매니저들이 준비될 때까지 기다린 후 초기화
        StartCoroutine(WaitForManagersAndInitialize());
    }

    private System.Collections.IEnumerator WaitForManagersAndInitialize()
    {
        // ItemManager와 WormManager가 준비될 때까지 대기
        yield return new WaitUntil(() => ItemManager.Instance != null && WormManager.Instance != null);
        
        // 추가로 안전을 위해 잠시 더 대기
        yield return new WaitForSeconds(0.1f);
        
        InitializeItemTab();
    }
    
    /// <summary>
    /// 외부에서 강제 초기화할 수 있는 메서드
    /// </summary>
    public void ForceInitialize()
    {
        if (!isInitialized)
        {
            // GameObject가 비활성화된 경우 직접 초기화
            if (!gameObject.activeInHierarchy)
            {
                InitializeItemTab();
                isInitialized = true;
            }
            else
            {
                StartCoroutine(WaitForManagersAndInitialize());
            }
        }
    }

    private void OnEnable()
    {
        // 탭이 활성화될 때도 항상 최신 상태 보장
        SetupButtonEvents();
        RegisterItemManagerEvents();
        RegisterWormManagerEvents();
        if (WormManager.Instance == null)
        {
            if (waitWormCoroutine != null) StopCoroutine(waitWormCoroutine);
            waitWormCoroutine = StartCoroutine(WaitForWormManagerAndSubscribe());
        }

        if (isInitialized)
        {
            RefreshAllPreviews();
            UpdateButtonsInteractable();
        }
        
        // 버튼 이벤트 재설정 (강제)
        StartCoroutine(DelayedButtonSetup());
    }
    
    private System.Collections.IEnumerator DelayedButtonSetup()
    {
        yield return new WaitForEndOfFrame();
        SetupButtonEvents();
        LogDebug("[ItemTabUI] 지연된 버튼 설정 완료");
    }

    private void OnDisable()
    {
        // 이벤트 중복 방지/정리
        if (ItemManager.Instance != null)
        {
            ItemManager.Instance.OnItemEquippedEvent -= OnItemEquipped;
            ItemManager.Instance.OnItemUnequippedEvent -= OnItemUnequipped;
        }
        if (WormManager.Instance != null)
        {
            WormManager.Instance.OnCurrentWormChangedEvent -= OnCurrentWormChanged;
            WormManager.Instance.OnWormEvolvedEvent -= OnWormEvolved;
        }
        if (waitWormCoroutine != null)
        {
            StopCoroutine(waitWormCoroutine);
            waitWormCoroutine = null;
        }
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
            RegisterItemManagerEvents();
            RegisterWormManagerEvents();
            if (WormManager.Instance == null)
            {
                if (waitWormCoroutine != null) StopCoroutine(waitWormCoroutine);
                waitWormCoroutine = StartCoroutine(WaitForWormManagerAndSubscribe());
            }
            
            // 초기화 완료 플래그를 먼저 세운 뒤 UI 업데이트 (초기 렌더 보장)
            isInitialized = true;
            // 초기 UI 업데이트
            RefreshAllPreviews();

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
            // 모자 버튼 - 직접 OpenItemPopup(0) 할당
            if (hatButton != null)
            {
                // 기존 ReusableButton 제거
                var existingHatButton = hatButton.GetComponent<GGumtles.UI.ReusableButton>();
                if (existingHatButton != null)
                {
                    DestroyImmediate(existingHatButton);
                }
                
                // 기존 onClick 제거
                hatButton.onClick.RemoveAllListeners();
                
                // 직접 OpenItemPopup(0) 할당
                hatButton.onClick.AddListener(() => {
                    LogDebug("[ItemTabUI] HatButton 클릭 - OpenItemPopup(0) 호출");
                    PopupManager.Instance?.OpenItemPopup(0); // 0 = ItemData.ItemType.Hat
                });
                
                LogDebug("[ItemTabUI] HatButton 직접 OpenItemPopup(0) 설정 완료");
            }

            // 얼굴 버튼 - 직접 OpenItemPopup(1) 할당
            if (faceButton != null)
            {
                // 기존 ReusableButton 제거
                var existingFaceButton = faceButton.GetComponent<GGumtles.UI.ReusableButton>();
                if (existingFaceButton != null)
                {
                    DestroyImmediate(existingFaceButton);
                }
                
                // 기존 onClick 제거
                faceButton.onClick.RemoveAllListeners();
                
                // 직접 OpenItemPopup(1) 할당
                faceButton.onClick.AddListener(() => {
                    LogDebug("[ItemTabUI] FaceButton 클릭 - OpenItemPopup(1) 호출");
                    PopupManager.Instance?.OpenItemPopup(1); // 1 = ItemData.ItemType.Face
                });
                
                LogDebug("[ItemTabUI] FaceButton 직접 OpenItemPopup(1) 설정 완료");
            }

            // 의상 버튼 - 직접 OpenItemPopup(2) 할당
            if (costumeButton != null)
            {
                // 기존 ReusableButton 제거
                var existingCostumeButton = costumeButton.GetComponent<GGumtles.UI.ReusableButton>();
                if (existingCostumeButton != null)
                {
                    DestroyImmediate(existingCostumeButton);
                }
                
                // 기존 onClick 제거
                costumeButton.onClick.RemoveAllListeners();
                
                // 직접 OpenItemPopup(2) 할당
                costumeButton.onClick.AddListener(() => {
                    LogDebug("[ItemTabUI] CostumeButton 클릭 - OpenItemPopup(2) 호출");
                    PopupManager.Instance?.OpenItemPopup(2); // 2 = ItemData.ItemType.Costume
                });
                
                LogDebug("[ItemTabUI] CostumeButton 직접 OpenItemPopup(2) 설정 완료");
            }

            // 아이템 뽑기 버튼 (선택 사항: 연결만, 동작은 추후)
            if (itemDrawButton != null)
            {
                itemDrawButton.onClick.RemoveAllListeners();
                itemDrawButton.onClick.AddListener(() =>
                {
                    LogDebug("[ItemTabUI] ItemDrawButton 클릭");
                    if (IsCurrentWormEgg())
                    {
                        PopupManager.Instance?.ShowToast("알 단계에서는 착용/뽑기 불가", 1.5f);
                        return;
                    }
                    PopupManager.Instance?.OpenPopup(PopupManager.PopupType.ItemDraw);
                });
            }

            // 셔플 버튼 (선택 사항: 연결만, 동작은 추후)
            if (shuffleButton != null)
            {
                shuffleButton.onClick.RemoveAllListeners();
                shuffleButton.onClick.AddListener(() =>
                {
                    LogDebug("[ItemTabUI] ShuffleButton 클릭");
                    if (IsCurrentWormEgg())
                    {
                        PopupManager.Instance?.ShowToast("알 단계에서는 변경 불가", 1.5f);
                        return;
                    }
                });
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[ItemTabUI] 버튼 이벤트 설정 중 오류: {ex.Message}");
        }
    }

    private void RegisterItemManagerEvents()
    {
        if (ItemManager.Instance == null) return;

        // 중복 구독 방지
        ItemManager.Instance.OnItemEquippedEvent -= OnItemEquipped;
        ItemManager.Instance.OnItemUnequippedEvent -= OnItemUnequipped;

        ItemManager.Instance.OnItemEquippedEvent += OnItemEquipped;
        ItemManager.Instance.OnItemUnequippedEvent += OnItemUnequipped;
    }

    private void OnItemEquipped(string itemId, ItemData.ItemType itemType)
    {
        RefreshPreview(itemType);
    }

    private void OnItemUnequipped(string itemId, ItemData.ItemType itemType)
    {
        RefreshPreview(itemType);
    }

    private void RegisterWormManagerEvents()
    {
        if (WormManager.Instance == null) return;

        WormManager.Instance.OnCurrentWormChangedEvent -= OnCurrentWormChanged;
        WormManager.Instance.OnWormEvolvedEvent -= OnWormEvolved;

        WormManager.Instance.OnCurrentWormChangedEvent += OnCurrentWormChanged;
        WormManager.Instance.OnWormEvolvedEvent += OnWormEvolved;
    }

    private void OnCurrentWormChanged(WormData prev, WormData curr)
    {
        RefreshAllPreviews();
    }

    private void OnWormEvolved(WormData worm, int fromStage, int toStage)
    {
        RefreshAllPreviews();
    }

    private System.Collections.IEnumerator WaitForWormManagerAndSubscribe()
    {
        // WormManager 준비될 때까지 대기
        while (WormManager.Instance == null)
        {
            yield return null;
        }
        // 초기화 완료와 현재 웜 생성 대기(선택)
        while (WormManager.Instance != null && WormManager.Instance.GetCurrentWorm() == null)
        {
            yield return null;
        }

        RegisterWormManagerEvents();
        RefreshAllPreviews();
        UpdateButtonsInteractable();
        waitWormCoroutine = null;
    }

    private bool IsCurrentWormEgg()
    {
        var worm = WormManager.Instance?.GetCurrentWorm();
        bool isEgg = worm != null && worm.lifeStage == 0; // 0 = Egg
        LogDebug($"[ItemTabUI] IsCurrentWormEgg: worm={worm?.name ?? "null"}, lifeStage={worm?.lifeStage ?? -1}, isEgg={isEgg}");
        return isEgg;
    }

    private void UpdateButtonsInteractable()
    {
        bool isEgg = IsCurrentWormEgg();
        if (hatButton != null) hatButton.interactable = !isEgg;
        if (faceButton != null) faceButton.interactable = !isEgg;
        if (costumeButton != null) costumeButton.interactable = !isEgg;
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
            if (IsCurrentWormEgg())
            {
                // 알 단계: 장착 불가 표기
                LogDebug("[ItemTabUI] 알 상태 - 장착 불가 상태 설정");
                SetUnavailableState(hatPreviewImage, hatNameText);
                SetUnavailableState(facePreviewImage, faceNameText);
                SetUnavailableState(costumePreviewImage, costumeNameText);
            }
            else
            {
                // 알이 아님: 아이템 프리뷰/텍스트 표시
                RefreshPreview(ItemData.ItemType.Hat, hatPreviewImage, hatNameText);
                RefreshPreview(ItemData.ItemType.Face, facePreviewImage, faceNameText);
                RefreshPreview(ItemData.ItemType.Costume, costumePreviewImage, costumeNameText);
            }
            UpdateButtonsInteractable();
            
            // 오버뷰 렌더러 새로고침 - 제거됨
            // if (overViewRenderer != null)
            // {
            //     overViewRenderer.RefreshOverview();
            // }

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
            if (IsCurrentWormEgg())
            {
                switch (itemType)
                {
                    case ItemData.ItemType.Hat:
                        SetUnavailableState(hatPreviewImage, hatNameText);
                        break;
                    case ItemData.ItemType.Face:
                        SetUnavailableState(facePreviewImage, faceNameText);
                        break;
                    case ItemData.ItemType.Costume:
                        SetUnavailableState(costumePreviewImage, costumeNameText);
                        break;
                }
                UpdateButtonsInteractable();
                return;
            }
            switch (itemType)
            {
                case ItemData.ItemType.Hat:
                    RefreshPreview(ItemData.ItemType.Hat, hatPreviewImage, hatNameText);
                    break;
                case ItemData.ItemType.Face:
                    RefreshPreview(ItemData.ItemType.Face, facePreviewImage, faceNameText);
                    break;
                case ItemData.ItemType.Costume:
                    RefreshPreview(ItemData.ItemType.Costume, costumePreviewImage, costumeNameText);
                    break;
            }

            // 오버뷰 렌더러 새로고침 - 제거됨
            // if (overViewRenderer != null)
            // {
            //     overViewRenderer.RefreshOverview();
            // }

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
    private void RefreshPreview(ItemData.ItemType type, Image previewImage, TMP_Text nameText)
    {
        try
        {
            LogDebug($"[ItemTabUI] RefreshPreview 호출됨 - Type: {type}, IsEgg: {IsCurrentWormEgg()}");
            
            if (IsCurrentWormEgg())
            {
                LogDebug($"[ItemTabUI] 알 상태 - SetNoneState 호출");
                SetNoneState(previewImage, nameText, type);
                return;
            }
            // 현재 착용된 아이템 ID 가져오기
            string itemId = GetEquippedItemId(type);
            LogDebug($"[ItemTabUI] 착용된 아이템 ID: {itemId}");
            var item = ItemManager.Instance?.GetItemById(itemId);
            LogDebug($"[ItemTabUI] 아이템 데이터: {item?.itemName ?? "null"}");

            // UI 업데이트
            if (item != null && !string.IsNullOrEmpty(item.itemId))
            {
                LogDebug($"[ItemTabUI] 아이템 있음 - UI 업데이트 시작");
                // 프리뷰 이미지 (SpriteManager에서 스프라이트 조회)
                if (previewImage != null)
                {
                    Sprite sprite = null;
                    switch (type)
                    {
                        case ItemData.ItemType.Hat:
                            sprite = SpriteManager.Instance?.GetHatSprite(item.itemId);
                            break;
                        case ItemData.ItemType.Face:
                            sprite = SpriteManager.Instance?.GetFaceSprite(item.itemId);
                            break;
                        case ItemData.ItemType.Costume:
                            sprite = SpriteManager.Instance?.GetCostumeSprite(item.itemId);
                            break;
                    }

                    previewImage.sprite = sprite;
                    previewImage.color = Color.white;
                }

                // 이름 텍스트
                if (nameText != null)
                {
                    nameText.text = item.itemName;
                }

                // 현재 착용 아이템 저장
                currentEquippedItems[type] = item;
            }
            else
            {
                // 아이템이 없는 경우
                SetNoneState(previewImage, nameText, type);

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

    private void SetNoneState(Image previewImage, TMP_Text nameText, ItemData.ItemType type)
    {
        if (previewImage != null)
        {
            previewImage.sprite = null;
            previewImage.color = new Color(1f, 1f, 1f, 0.5f); // 반투명
        }

        if (nameText != null)
        {
            nameText.text = GetDefaultItemName(type);
        }
    }

    private void SetUnavailableState(Image previewImage, TMP_Text nameText)
    {
        LogDebug($"[ItemTabUI] SetUnavailableState 호출됨 - previewImage: {previewImage != null}, nameText: {nameText != null}");
        
        if (previewImage != null)
        {
            previewImage.sprite = null;
            previewImage.color = new Color(1f, 1f, 1f, 0.5f);
            LogDebug("[ItemTabUI] previewImage 설정 완료");
        }
        else
        {
            Debug.LogWarning("[ItemTabUI] previewImage가 null입니다!");
        }

        if (nameText != null)
        {
            nameText.text = "장착 불가";
            LogDebug($"[ItemTabUI] nameText 설정 완료: {nameText.text}");
        }
        else
        {
            Debug.LogWarning("[ItemTabUI] nameText가 null입니다!");
        }
    }

    /// <summary>
    /// 착용된 아이템 ID 가져오기
    /// </summary>
    private string GetEquippedItemId(ItemData.ItemType type)
    {
        try
        {
            string itemId = "";

            // 1) ItemManager에서 우선 조회
            if (ItemManager.Instance != null)
            {
                try
                {
                    itemId = ItemManager.Instance.GetEquippedItemId(type);
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"[ItemTabUI] ItemManager.GetEquippedItemId 호출 중 오류: {ex.Message}");
                }
            }
            else
            {
                Debug.LogWarning("[ItemTabUI] ItemManager.Instance가 null입니다.");
            }

            // 2) 비어있다면 WormData에서 폴백 (현재 웜의 기본 장착값)
            if (string.IsNullOrEmpty(itemId))
            {
                if (WormManager.Instance != null)
                {
                    try
                    {
                        var worm = WormManager.Instance.GetCurrentWorm();
                        if (worm != null)
                        {
                            switch (type)
                            {
                                case ItemData.ItemType.Hat:
                                    itemId = worm.hatItemId;
                                    break;
                                case ItemData.ItemType.Face:
                                    itemId = worm.faceItemId;
                                    break;
                                case ItemData.ItemType.Costume:
                                    itemId = worm.costumeItemId;
                                    break;
                            }
                        }
                        else
                        {
                            Debug.LogWarning("[ItemTabUI] 현재 웜 데이터가 null입니다.");
                        }
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogWarning($"[ItemTabUI] WormManager.GetCurrentWorm 호출 중 오류: {ex.Message}");
                    }
                }
                else
                {
                    Debug.LogWarning("[ItemTabUI] WormManager.Instance가 null입니다.");
                }
            }

            return itemId ?? "";
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
    // OnItemButtonClicked 메서드는 ReusableButton으로 대체되어 더 이상 사용되지 않음
    // ReusableButton의 ButtonAction.OpenItemPopup이 직접 PopupManager.OpenItemPopup을 호출함
    /*
    private void OnItemButtonClicked(ItemData.ItemType itemType)
    {
        try
        {
            LogDebug($"[ItemTabUI] {itemType} 아이템 버튼 클릭됨");
            
            // 현재 착용된 아이템 가져오기
            ItemData currentItem = null;
            if (currentEquippedItems.TryGetValue(itemType, out currentItem))
            {
                LogDebug($"[ItemTabUI] {itemType} 아이템 클릭: {currentItem.itemName}");
            }
            else
            {
                LogDebug($"[ItemTabUI] {itemType} 아이템 클릭: 착용된 아이템 없음");
            }

            // 이벤트 발생 (더 이상 사용되지 않음)
            // OnItemPreviewClickedEvent?.Invoke(itemType, currentItem);

            // 아이템 팝업 열기
            LogDebug($"[ItemTabUI] OpenItemPopup 호출: {itemType}");
            PopupManager.Instance?.OpenItemPopup(itemType);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[ItemTabUI] 아이템 버튼 클릭 처리 중 오류: {ex.Message}");
        }
    }
    */



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
            info.AppendLine($"{kvp.Key}: {kvp.Value.itemName}");
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
        // OnItemPreviewClickedEvent = null; // 더 이상 사용되지 않음
    }
    }
}
