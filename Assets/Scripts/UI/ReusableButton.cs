using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GGumtles.UI
{
    /// <summary>
    /// 버튼 액션 타입
    /// </summary>
    public enum ButtonAction
    {
        None,
        OpenTab,
        OpenPopup,
        ClosePopup,

        CreateLayoutElement,
        PlaySFX,
        EquipItem,
        ShakeTree,
        CollectAcorn,
        CollectDiamond,
        FeedWorm,
        
        // 아이템 뽑기 관련
        OpenItemDrawPopup,
        CancelItemDraw,
        CancelDrawConfirm,
        ConfirmDrawResult,
        
        // 아이템 팝업 관련 (통합)
        OpenItemPopup,  // parameter로 ItemType 구분
        
        // 업적 팝업 관련
        OpenAchievement1Popup,  // parameter로 Achievement Index 구분
        OpenAchievement2Popup,  // parameter로 Achievement Index 구분
        
        // 게임 관련
        StartGame
    }

    public class ReusableButton : MonoBehaviour
    {
        [Header("액션 설정")]
        [SerializeField] private ButtonAction buttonAction;
        [SerializeField] private int actionParameter = -1; // 탭 인덱스, 팝업 타입 등
        
        // [Header("디버그 설정")] // 필드와 함께 주석 처리
        // enableDebugLogs 제거 - 사용되지 않음
        
        // 이벤트
        public delegate void ButtonClickedHandler(ButtonAction action, int parameter);
        public static event ButtonClickedHandler OnButtonClickedEvent; // 중앙 이벤트
        
        // 개별 버튼 이벤트
        public event ButtonClickedHandler OnThisButtonClickedEvent; // 개별 이벤트
        
        private void Awake()
        {
            // 자동으로 컴포넌트 찾기
            Button button = GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(OnButtonClicked);
            }
        }
        
        private void Start()
        {
            // 추가 초기화가 필요한 경우 여기에 구현
        }
        
        private void OnDestroy()
        {
            // 이벤트 구독 해제
            Button button = GetComponent<Button>();
            if (button != null)
                button.onClick.RemoveListener(OnButtonClicked);
        }
        
        /// <summary>
        /// 버튼 클릭 처리
        /// </summary>
        private void OnButtonClicked()
        {
            OnButtonClickedEvent?.Invoke(buttonAction, actionParameter);
            OnThisButtonClickedEvent?.Invoke(buttonAction, actionParameter); // 개별 이벤트 발생
            HandleButtonAction();
        }
        
        /// <summary>
        /// 버튼 액션 처리
        /// </summary>
        private void HandleButtonAction()
        {
            switch (buttonAction)
            {
                case ButtonAction.OpenTab:
                    TabManager.Instance?.SwitchToTab(actionParameter);
                    break;
                    
                case ButtonAction.OpenPopup:
                    PopupManager.Instance?.OpenPopup((PopupManager.PopupType)actionParameter);
                    break;
                    
                case ButtonAction.ClosePopup:
                    PopupManager.Instance?.CloseAllPopups();
                    break;
                    
                
                    
                case ButtonAction.CreateLayoutElement:
                    // UIPrefabManager 삭제로 인해 직접 처리 불가
                    Debug.LogWarning("[ReusableButton] CreateLayoutElement는 더 이상 지원되지 않습니다.");
                    break;
                    
                case ButtonAction.PlaySFX:
                    AudioManager.Instance?.PlaySFX((AudioManager.SFXType)actionParameter);
                    break;
                    
                case ButtonAction.EquipItem:
                    // ItemManager.EquipItemByType이 없으므로 임시 처리
                    Debug.Log($"[ReusableButton] 아이템 장착: {actionParameter}");
                    break;
                    
                case ButtonAction.ShakeTree:
                    // TreeController에서 나무 흔들기 실행 (사운드 포함)
                    var treeController = FindFirstObjectByType<TreeController>();
                    if (treeController != null)
                    {
                        treeController.ShakeTree();
                    }
                    else
                    {
                        Debug.LogWarning("[ReusableButton] TreeController를 찾을 수 없습니다.");
                    }
                    break;
                    
                case ButtonAction.CollectAcorn:
                    GameManager.Instance?.PickAcorn();
                    break;
                    
                case ButtonAction.CollectDiamond:
                    GameManager.Instance?.PickDiamond();
                    break;
                    
                case ButtonAction.FeedWorm:
                    // WormManager에서 웜 먹이주기 실행
                    var wormManager = FindFirstObjectByType<WormManager>();
                    if (wormManager != null)
                    {
                        wormManager.FeedWorm();
                    }
                    else
                    {
                        Debug.LogWarning("[ReusableButton] WormManager를 찾을 수 없습니다.");
                    }
                    break;
                    
                // 아이템 뽑기 관련 (통합)
                case ButtonAction.OpenItemDrawPopup:
                    // parameter로 ItemType 전달
                    ItemData.ItemType drawType = (ItemData.ItemType)actionParameter;
                    PopupManager.Instance?.OpenItemDrawPopup(drawType);
                    break;
                    
                case ButtonAction.CancelItemDraw:
                    PopupManager.Instance?.CancelItemDraw();
                    break;
                    
                case ButtonAction.CancelDrawConfirm:
                    PopupManager.Instance?.CancelDrawConfirm();
                    break;
                    
                case ButtonAction.ConfirmDrawResult:
                    PopupManager.Instance?.ConfirmDrawResult();
                    break;
                    
                // 아이템 팝업 관련 (통합)
                case ButtonAction.OpenItemPopup:
                    ItemData.ItemType itemTypeParam = (ItemData.ItemType)actionParameter;
                    PopupManager.Instance?.OpenItemPopup(itemTypeParam);
                    break;
                    
                // 업적 팝업 관련
                case ButtonAction.OpenAchievement1Popup:
                    int achievement1Index = actionParameter;
                    PopupManager.Instance?.OpenPopup(PopupManager.PopupType.Achievement1, PopupManager.PopupPriority.Normal, achievement1Index);
                    break;
                    
                case ButtonAction.OpenAchievement2Popup:
                    int achievement2Index = actionParameter;
                    PopupManager.Instance?.OpenPopup(PopupManager.PopupType.Achievement2, PopupManager.PopupPriority.Normal, achievement2Index);
                    break;
                    
                case ButtonAction.StartGame:
                    // GamePanel에서 게임 시작 처리
                    var gamePanel = FindFirstObjectByType<GamePanel>();
                    if (gamePanel != null)
                    {
                        gamePanel.StartGame(actionParameter);
                    }
                    else
                    {
                        Debug.LogWarning("[ReusableButton] GamePanel을 찾을 수 없습니다.");
                    }
                    break;
                    
                default:
                    Debug.LogWarning($"[ReusableButton] 처리되지 않은 액션: {buttonAction}");
                    break;
            }
        }
        
        /// <summary>
        /// 버튼 활성화/비활성화
        /// </summary>
        public void SetInteractable(bool interactable)
        {
            Button button = GetComponent<Button>();
            if (button != null)
                button.interactable = interactable;
        }
        
        /// <summary>
        /// 버튼 액션 설정
        /// </summary>
        public void SetAction(ButtonAction action, int parameter = -1)
        {
            buttonAction = action;
            actionParameter = parameter;
        }
    }
}
