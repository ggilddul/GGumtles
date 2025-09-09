using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GGumtles.Data;
using GGumtles.Managers;

namespace GGumtles.UI
{
    /// <summary>
    /// 뽑기 확인 팝업 UI
    /// </summary>
    public class DrawConfirmPopupUI : MonoBehaviour
    {
        [Header("UI 요소")]
        [SerializeField] private TextMeshProUGUI itemDrawTypeText1; // 랜덤한 xx 1개를 뽑으시겠습니까?
        [SerializeField] private TextMeshProUGUI itemDrawTypeText2; // 보유중인 다이아몬드 : x개
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button declineButton;
        [SerializeField] private GameObject diamondToast; // 다이아몬드 부족 시 표시할 Toast
        
        [Header("디버그")]
        [SerializeField] private bool enableDebugLogs = false;
        
        private ItemData.ItemType selectedItemType;
        
        /// <summary>
        /// 팝업 초기화
        /// </summary>
        public void Initialize(ItemData.ItemType itemType)
        {
            selectedItemType = itemType;
            
            // 텍스트 설정 - 타입 안내
            if (itemDrawTypeText1 != null)
            {
                string itemTypeName = GetItemTypeName(itemType);
                itemDrawTypeText1.text = $"랜덤한 {itemTypeName} 1개를 뽑으시겠습니까?";
            }

            // 텍스트 설정 - 다이아 개수
            UpdateDiamondCountText();
            
            // 버튼 설정
            SetupButtons();
            
            LogDebug($"[DrawConfirmPopupUI] {itemType} 뽑기 확인 팝업 초기화");
        }
        
        /// <summary>
        /// 팝업 닫기
        /// </summary>
        public void ClosePopup()
        {
            Destroy(gameObject);
            LogDebug("[DrawConfirmPopupUI] 뽑기 확인 팝업 닫기");
        }
        
        /// <summary>
        /// 다이아몬드 개수 업데이트
        /// </summary>
        private void UpdateDiamondCountText()
        {
            try
            {
                if (itemDrawTypeText2 != null)
                {
                    // GameSaveManager에서 현재 다이아몬드 개수 가져오기
                    int currentDiamonds = 0;
                    if (GameSaveManager.Instance != null)
                    {
                        currentDiamonds = GameSaveManager.Instance.GetDiamondCount();
                    }
                    
                    itemDrawTypeText2.text = $"보유중인 다이아몬드 : {currentDiamonds}개";
                    LogDebug($"[DrawConfirmPopupUI] 다이아몬드 개수 표시: {currentDiamonds}개");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[DrawConfirmPopupUI] 다이아몬드 개수 업데이트 중 오류: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 버튼 설정
        /// </summary>
        private void SetupButtons()
        {
            // 확인 버튼
            if (confirmButton != null)
            {
                confirmButton.onClick.RemoveAllListeners();
                confirmButton.onClick.AddListener(OnConfirmButtonClicked);
            }
            
            // 취소 버튼
            if (declineButton != null)
            {
                declineButton.onClick.RemoveAllListeners();
                declineButton.onClick.AddListener(OnDeclineButtonClicked);
            }
        }
        
        /// <summary>
        /// 확인 버튼 클릭 처리
        /// </summary>
        private void OnConfirmButtonClicked()
        {
            LogDebug($"[DrawConfirmPopupUI] 확인 버튼 클릭됨 - {selectedItemType}");
            
            // 다이아몬드 차감 시도
            if (GameManager.Instance != null)
            {
                if (GameManager.Instance.diamondCount > 0)
                {
                    // 다이아몬드 차감
                    GameManager.Instance.UseDiamond();
                    
                    // 미보유 우선 랜덤 획득
                    ItemData drawnItem = DrawRandomUnownedItem(selectedItemType);

                    // 현재 Confirm 팝업 제거
                    Destroy(gameObject);

                    // 결과 팝업 열기
                    if (drawnItem != null)
                    {
                        PopupManager.Instance?.OpenDrawResultPopup(drawnItem);
                    }
                    
                    LogDebug($"[DrawConfirmPopupUI] 다이아몬드 차감 후 뽑기 실행 - 남은 다이아몬드: {GameManager.Instance.diamondCount}");
                }
                else
                {
                    // 다이아몬드 부족 - Toast 표시
                    ShowDiamondToast();
                    LogDebug("[DrawConfirmPopupUI] 다이아몬드 부족 - Toast 표시");
                }
            }
            else
            {
                Debug.LogWarning("[DrawConfirmPopupUI] GameManager가 없습니다!");
            }
        }
        
        /// <summary>
        /// 취소 버튼 클릭 처리
        /// </summary>
        private void OnDeclineButtonClicked()
        {
            LogDebug("[DrawConfirmPopupUI] 취소 버튼 클릭됨");
            
            // 팝업 닫기
            ClosePopup();
        }
        
        /// <summary>
        /// 랜덤 아이템 뽑기
        /// </summary>
        private ItemData DrawRandomUnownedItem(ItemData.ItemType itemType)
        {
            // ItemManager에서 해당 타입의 아이템들 가져오기
            var itemManager = ItemManager.Instance;
            if (itemManager == null) return null;
            
            var allTypeItems = itemManager.GetItemsByType(itemType);
            if (allTypeItems == null || allTypeItems.Count == 0) return null;

            // 미보유 아이템만 필터링
            var owned = itemManager.GetOwnedItems();
            var ownedSet = new System.Collections.Generic.HashSet<string>();
            foreach (var oi in owned) ownedSet.Add(oi.itemId);

            var candidates = new System.Collections.Generic.List<ItemData>();
            foreach (var it in allTypeItems)
            {
                if (!ownedSet.Contains(it.itemId)) candidates.Add(it);
            }

            if (candidates.Count == 0) candidates = allTypeItems;

            // 랜덤 선택
            int randomIndex = Random.Range(0, candidates.Count);
            ItemData selectedItem = candidates[randomIndex];
            
            // 아이템 획득
            itemManager.AddItem(selectedItem.itemId, 1);
            
            LogDebug($"[DrawConfirmPopupUI] {selectedItem.itemName} 획득!");
            
            return selectedItem;
        }
        
        /// <summary>
        /// 아이템 타입 이름 가져오기
        /// </summary>
        private string GetItemTypeName(ItemData.ItemType itemType)
        {
            return itemType switch
            {
                ItemData.ItemType.Hat => "모자",
                ItemData.ItemType.Face => "얼굴",
                ItemData.ItemType.Costume => "의상",
                _ => "아이템"
            };
        }
        
        /// <summary>
        /// 다이아몬드 부족 Toast 표시
        /// </summary>
        private void ShowDiamondToast()
        {
            if (diamondToast != null)
            {
                diamondToast.SetActive(true);
                LogDebug("[DrawConfirmPopupUI] 다이아몬드 부족 Toast 활성화");
            }
            else
            {
                Debug.LogWarning("[DrawConfirmPopupUI] diamondToast가 연결되지 않았습니다!");
            }
        }

        private void LogDebug(string message)
        {
            if (enableDebugLogs)
            {
                Debug.Log(message);
            }
        }
    }
}