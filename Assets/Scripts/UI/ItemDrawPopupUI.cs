using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GGumtles.UI
{
    /// <summary>
    /// 아이템 뽑기 팝업 UI
    /// </summary>
    public class ItemDrawPopupUI : MonoBehaviour
    {
        [Header("UI 요소")]
        [SerializeField] private Button hatDrawButton;
        [SerializeField] private Button faceDrawButton;
        [SerializeField] private Button costumeDrawButton;
        [SerializeField] private Button closeButton;
        
        [Header("설정")]
        [SerializeField] private bool enableDebugLogs = false;
        
        private void Start()
        {
            SetupButtons();
        }
        
        /// <summary>
        /// 버튼 설정
        /// </summary>
        private void SetupButtons()
        {
            // 모자 뽑기 버튼
            if (hatDrawButton != null)
            {
                hatDrawButton.onClick.AddListener(() => OnDrawButtonClicked(ItemData.ItemType.Hat));
            }
            
            // 얼굴 뽑기 버튼
            if (faceDrawButton != null)
            {
                faceDrawButton.onClick.AddListener(() => OnDrawButtonClicked(ItemData.ItemType.Face));
            }
            
            // 의상 뽑기 버튼
            if (costumeDrawButton != null)
            {
                costumeDrawButton.onClick.AddListener(() => OnDrawButtonClicked(ItemData.ItemType.Costume));
            }
            
            // 닫기 버튼
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(OnCloseButtonClicked);
            }
        }
        
        /// <summary>
        /// 뽑기 버튼 클릭 처리
        /// </summary>
        private void OnDrawButtonClicked(ItemData.ItemType itemType)
        {
            if (enableDebugLogs)
                Debug.Log($"[ItemDrawPopupUI] {itemType} 뽑기 버튼 클릭됨");
            
            // 뽑기 확인 팝업 열기
            PopupManager.Instance?.OpenDrawConfirmPopup(itemType);
        }
        
        /// <summary>
        /// 닫기 버튼 클릭 처리
        /// </summary>
        private void OnCloseButtonClicked()
        {
            if (enableDebugLogs)
                Debug.Log("[ItemDrawPopupUI] 닫기 버튼 클릭됨");
            
            // 팝업 닫기
            PopupManager.Instance?.CancelItemDraw();
        }
        
        private void OnDestroy()
        {
            // 이벤트 리스너 제거
            if (hatDrawButton != null)
                hatDrawButton.onClick.RemoveAllListeners();
            if (faceDrawButton != null)
                faceDrawButton.onClick.RemoveAllListeners();
            if (costumeDrawButton != null)
                costumeDrawButton.onClick.RemoveAllListeners();
            if (closeButton != null)
                closeButton.onClick.RemoveAllListeners();
        }
    }
}
