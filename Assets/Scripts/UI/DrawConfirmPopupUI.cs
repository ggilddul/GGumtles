using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GGumtles.UI
{
    /// <summary>
    /// 뽑기 확인 팝업 UI
    /// </summary>
    public class DrawConfirmPopupUI : MonoBehaviour
    {
        [Header("UI 요소")]
        [SerializeField] private TextMeshProUGUI confirmText;
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button declineButton;
        
        [Header("디버그")]
        [SerializeField] private bool enableDebugLogs = false;
        
        private ItemData.ItemType selectedItemType;
        
        /// <summary>
        /// 팝업 초기화
        /// </summary>
        public void Initialize(ItemData.ItemType itemType)
        {
            selectedItemType = itemType;
            
            // 텍스트 설정
            if (confirmText != null)
            {
                string itemTypeName = GetItemTypeName(itemType);
                confirmText.text = $"{itemTypeName}을(를) 뽑으시겠습니까?";
            }
            
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
            
            // 랜덤 아이템 획득
            ItemData drawnItem = DrawRandomItem(selectedItemType);
            
            // 뽑기 결과 팝업 열기
            PopupManager.Instance?.OpenDrawResultPopup(drawnItem);
            
            // 팝업 닫기
            ClosePopup();
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
        private ItemData DrawRandomItem(ItemData.ItemType itemType)
        {
            // ItemManager에서 해당 타입의 아이템들 가져오기
            var itemManager = ItemManager.Instance;
            if (itemManager == null) return null;
            
            var items = itemManager.GetItemsByType(itemType);
            if (items == null || items.Count == 0) return null;
            
            // 랜덤 선택
            int randomIndex = Random.Range(0, items.Count);
            ItemData selectedItem = items[randomIndex];
            
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
        
        private void LogDebug(string message)
        {
            if (enableDebugLogs)
            {
                Debug.Log(message);
            }
        }
    }
}