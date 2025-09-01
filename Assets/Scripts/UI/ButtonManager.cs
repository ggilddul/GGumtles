using UnityEngine;
using GGumtles.UI;

namespace GGumtles.UI
{
    /// <summary>
    /// 버튼 이벤트 리스너
    /// ReusableButton의 이벤트를 처리하는 중앙 관리자
    /// </summary>
    public class ButtonManager : MonoBehaviour
    {
        [Header("디버그 설정")]
        [SerializeField] private bool enableDebugLogs = true;
        
        private void Start()
        {
            // ReusableButton 이벤트 구독
            ReusableButton.OnButtonClickedEvent += HandleButtonClicked;
        }
        
        private void OnDestroy()
        {
            // 이벤트 구독 해제
            ReusableButton.OnButtonClickedEvent -= HandleButtonClicked;
        }
        
        /// <summary>
        /// 버튼 클릭 이벤트 처리
        /// </summary>
        private void HandleButtonClicked(ButtonAction action, int parameter)
        {
            try
            {
                if (enableDebugLogs)
                    Debug.Log($"[ButtonManager] 버튼 액션 처리: {action}, 파라미터: {parameter}");
                
                // 추가적인 버튼 로직이 필요한 경우 여기에 구현
                switch (action)
                {
                    case ButtonAction.EquipItem:
                        HandleItemEquip(parameter);
                        break;
                        
                    case ButtonAction.ShakeTree:
                        HandleTreeShake();
                        break;
                        
                    case ButtonAction.CollectAcorn:
                        HandleAcornCollect();
                        break;
                        
                    case ButtonAction.CollectDiamond:
                        HandleDiamondCollect();
                        break;
                        
                    case ButtonAction.FeedWorm:
                        HandleWormFeed();
                        break;
                        
                    default:
                        Debug.LogWarning($"[ButtonManager] 처리되지 않은 액션: {action}");
                        break;
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[ButtonManager] 버튼 액션 처리 중 오류: {ex.Message}");
            }
        }
        
        #region 액션 핸들러들
        
        private void HandleItemEquip(int itemType)
        {
            // 아이템 장착 시 추가 로직
            Debug.Log($"[ButtonManager] 아이템 장착: {itemType}");
        }
        
        private void HandleTreeShake()
        {
            // 나무 흔들기 시 추가 로직
            Debug.Log("[ButtonManager] 나무 흔들기");
        }
        
        private void HandleAcornCollect()
        {
            // 도토리 수집 시 추가 로직
            Debug.Log("[ButtonManager] 도토리 수집");
        }
        
        private void HandleDiamondCollect()
        {
            // 다이아몬드 수집 시 추가 로직
            Debug.Log("[ButtonManager] 다이아몬드 수집");
        }
        
        private void HandleWormFeed()
        {
            // 웜 먹이주기 시 추가 로직
            Debug.Log("[ButtonManager] 웜 먹이주기");
        }
        
        #endregion
    }
}
