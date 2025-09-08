using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GGumtles.Data;
using GGumtles.Managers;

namespace GGumtles.UI
{
    /// <summary>
    /// 뽑기 결과 팝업 UI
    /// </summary>
    public class DrawResultPopupUI : MonoBehaviour
    {
        [Header("UI 요소")]
        [SerializeField] private Image itemPanelImage;        // 뽑기 결과에 해당하는 아이템 이미지
        [SerializeField] private TextMeshProUGUI popupDescText; // 뽑기 결과 아이템 설명
        [SerializeField] private Button confirmDrawResult;    // 확인 버튼
        
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
            
            // 이미지 설정 (리소스 로딩 또는 ItemData 내 스프라이트 사용이 있다면 교체)
            if (itemPanelImage != null)
            {
                itemPanelImage.sprite = null;
                itemPanelImage.gameObject.SetActive(true);
            }

            // 설명 패널 텍스트 설정
            if (popupDescText != null)
            {
                popupDescText.text = GetItemDescription(itemData);
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
            if (confirmDrawResult != null)
            {
                confirmDrawResult.onClick.AddListener(OnConfirmButtonClicked);
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
            if (confirmDrawResult != null)
                confirmDrawResult.onClick.RemoveAllListeners();
        }

        private string GetItemDescription(ItemData item)
        {
            // ItemData에 설명 필드가 있으면 사용, 없으면 이름 기반 임시 설명
            if (item == null) return "";
            return string.IsNullOrEmpty(item.itemDescription) ? $"{item.itemName}을(를) 획득했습니다." : item.itemDescription;
        }
    }
}
