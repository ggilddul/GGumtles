using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GGumtles.UI
{
    /// <summary>
    /// 뽑기 결과 팝업 UI
    /// </summary>
    public class DrawResultPopupUI : MonoBehaviour
    {
        [Header("UI 요소")]
        [SerializeField] private TextMeshProUGUI resultText;
        [SerializeField] private Image itemIcon;
        [SerializeField] private Button confirmButton;
        
        [Header("설정")]
        [SerializeField] private bool enableDebugLogs = false;
        
        private ItemData resultItem;
        
        private void Start()
        {
            SetupButtons();
        }
        
        /// <summary>
        /// 팝업 초기화
        /// </summary>
        public void Initialize(ItemData itemData)
        {
            resultItem = itemData;
            
            // 텍스트 설정
            if (resultText != null)
            {
                resultText.text = $"{itemData.itemName}을(를) 획득했습니다!";
            }
            
            // 아이콘 설정 (단순화)
            if (itemIcon != null)
            {
                itemIcon.sprite = null; // 단순화: 기본 스프라이트 제거
                itemIcon.gameObject.SetActive(true);
            }
            
            if (enableDebugLogs)
                Debug.Log($"[DrawResultPopupUI] {itemData.itemName} 획득 결과 팝업 초기화");
        }
        
        /// <summary>
        /// 버튼 설정
        /// </summary>
        private void SetupButtons()
        {
            // 확인 버튼
            if (confirmButton != null)
            {
                confirmButton.onClick.AddListener(OnConfirmButtonClicked);
            }
        }
        
        /// <summary>
        /// 확인 버튼 클릭 처리
        /// </summary>
        private void OnConfirmButtonClicked()
        {
            if (enableDebugLogs)
                Debug.Log("[DrawResultPopupUI] 확인 버튼 클릭됨");
            
            // 팝업 닫기
            PopupManager.Instance?.ConfirmDrawResult();
        }
        
        private void OnDestroy()
        {
            // 이벤트 리스너 제거
            if (confirmButton != null)
                confirmButton.onClick.RemoveAllListeners();
        }
    }
}
