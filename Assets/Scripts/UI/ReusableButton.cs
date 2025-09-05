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
        
        // 업적 팝업 관련 (통합)
        OpenAchievementPopup,  // parameter로 Achievement Index 구분 (해금 상태에 따라 Achievement1/2 자동 선택)
        
        StartGame,
        
        // GFC 팝업 관련
        OpenGfcPopup,
        
        // 알 발견 팝업 관련
        ConfirmEggFound,  // 알 발견 확인 - 벌레 생성 및 나이 증가
        
        // 벌레 사망 팝업 관련
        WormDieConfirm,  // 벌레 사망 확인 - EggFound 팝업 열기

        QuitGame  // 게임 종료 - MainUI로 돌아가기
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
            // 사운드 재생을 가장 먼저 처리 (딜레이 최소화)
            PlayButtonClickSound();
            
            OnButtonClickedEvent?.Invoke(buttonAction, actionParameter);
            OnThisButtonClickedEvent?.Invoke(buttonAction, actionParameter); // 개별 이벤트 발생
            HandleButtonAction();
        }
        
        /// <summary>
        /// 버튼 클릭 사운드 재생
        /// </summary>
        private void PlayButtonClickSound()
        {
            // 특별한 사운드가 필요한 액션들만 예외 처리
            switch (buttonAction)
            {
                case ButtonAction.PlaySFX:
                    // PlaySFX 액션은 HandleButtonAction에서 처리
                    return;
                case ButtonAction.ShakeTree:
                    AudioManager.Instance?.PlaySFX(AudioManager.SFXType.ShakeTree);
                    return;
                case ButtonAction.CollectAcorn:
                case ButtonAction.CollectDiamond:
                    AudioManager.Instance?.PlaySFX(AudioManager.SFXType.EarnItem);
                    return;
                case ButtonAction.FeedWorm:
                    AudioManager.Instance?.PlaySFX(AudioManager.SFXType.FeedWorm);
                    return;
                default:
                    // 기본 버튼 클릭 사운드
                    AudioManager.Instance?.PlayButtonSound(0);
                    break;
            }
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
                    
                case ButtonAction.PlaySFX:
                    AudioManager.Instance?.PlaySFX((AudioManager.SFXType)actionParameter);
                    break;
                    
                case ButtonAction.EquipItem:
                    // ItemManager.EquipItemByType이 없으므로 임시 처리
                    Debug.Log($"[ReusableButton] 아이템 장착: {actionParameter}");
                    break;
                    
                case ButtonAction.ShakeTree:
                    TreeController.Instance?.ShakeTree();
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
                    
                // 업적 팝업 관련 (통합)
                case ButtonAction.OpenAchievementPopup:
                    int achievementIndex = actionParameter;
                    // AchievementManager에서 해금 상태 확인하여 적절한 팝업 열기
                    if (AchievementManager.Instance != null)
                    {
                        var allDefinitions = AchievementManager.Instance.GetAllDefinitions();
                        // -1이면 첫 번째 업적(0) 열기, 그 외에는 지정된 인덱스 사용
                        if (achievementIndex == -1)
                        {
                            achievementIndex = 0;
                        }
                        if (achievementIndex >= 0 && achievementIndex < allDefinitions.Count)
                        {
                            var achievementData = allDefinitions[achievementIndex];
                            bool isUnlocked = AchievementManager.Instance.IsUnlocked(achievementData.achievementId);
                            if (isUnlocked)
                            {
                                PopupManager.Instance?.OpenPopup(PopupManager.PopupType.Achievement1, PopupManager.PopupPriority.Normal, achievementIndex);
                            }
                            else
                            {
                                PopupManager.Instance?.OpenPopup(PopupManager.PopupType.Achievement2, PopupManager.PopupPriority.Normal, achievementIndex);
                            }
                        }
                    }
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
                    
                case ButtonAction.QuitGame:
                    // GamePanel에서 게임 종료 처리
                    var quitGamePanel = FindFirstObjectByType<GamePanel>();
                    if (quitGamePanel != null)
                    {
                        quitGamePanel.ReturnToMainUI();
                        Debug.Log("[ReusableButton] 게임 종료 - MainUI로 돌아가기");
                    }
                    else
                    {
                        Debug.LogWarning("[ReusableButton] GamePanel을 찾을 수 없습니다.");
                    }
                    break;
                    
                // GFC 팝업 관련
                case ButtonAction.OpenGfcPopup:
                    // WormFamilyManager에서 GFC 팝업 열기
                    var wormFamilyManager = FindFirstObjectByType<WormFamilyManager>();
                    if (wormFamilyManager != null)
                    {
                        wormFamilyManager.OpenGfcPopup();
                    }
                    else
                    {
                        Debug.LogWarning("[ReusableButton] WormFamilyManager를 찾을 수 없습니다.");
                    }
                    break;
                    
                // 알 발견 팝업 관련
                case ButtonAction.ConfirmEggFound:
                    // WormManager에서 새 벌레 생성
                    if (WormManager.Instance != null)
                    {
                        // 새 벌레 생성
                        WormManager.Instance.CreateNewWorm();
                        
                        // 기존 벌레가 있을 때만 나이 증가 (최초 접속 시에는 벌레가 없으므로 생략)
                        if (WormManager.Instance.TotalWorms > 1)
                        {
                            WormManager.Instance.AgeAllWorms();
                        }
                        
                        // EggFound 팝업 닫기
                        PopupManager.Instance?.CloseCustomPopup(PopupManager.PopupType.EggFound);
                        
                        Debug.Log("[ReusableButton] 알 발견 확인 - 새 벌레 생성 완료");
                    }
                    else
                    {
                        Debug.LogWarning("[ReusableButton] WormManager를 찾을 수 없습니다.");
                    }
                    break;
                    
                // 벌레 사망 팝업 관련
                case ButtonAction.WormDieConfirm:
                    // WormDie 팝업 닫기
                    PopupManager.Instance?.CloseCustomPopup(PopupManager.PopupType.Die);
                    
                    // EggFound 팝업 열기
                    PopupManager.Instance?.OpenPopup(PopupManager.PopupType.EggFound, PopupManager.PopupPriority.Normal);
                    
                    Debug.Log("[ReusableButton] 벌레 사망 확인 - EggFound 팝업 열기");
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
