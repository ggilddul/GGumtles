using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ButtonManager : MonoBehaviour, IPointerDownHandler
{
    [Header("버튼 설정")]
    [SerializeField] private ButtonActionType actionType;
    [SerializeField] private int index = 0;        // 탭/팝업/아이템 인덱스
    [SerializeField] private int soundIndex = 0;   // 버튼 사운드 인덱스

    [Header("참조 컴포넌트")]
    [SerializeField] private TreeController treeController;
    [SerializeField] private MapManager mapManager;
    [SerializeField] private PickAcorn pickAcorn;
    [SerializeField] private PickDiamond pickDiamond;

    [Header("설정")]
    [SerializeField] private bool enableSound = true;
    [SerializeField] private bool enableDebugLogs = false;

    // 컴포넌트
    private Button button;

    // 이벤트 정의
    public delegate void OnButtonClicked(ButtonManager buttonManager, ButtonActionType actionType, int index);
    public event OnButtonClicked OnButtonClickedEvent;

    public enum ButtonActionType
    {
        OpenTab,
        OpenPopup,
        ClosePopup,
        OpenClosePopup,
        PlayToast,
        ShakeTree,
        PickAcorn,
        PickDiamond,
        ShowAchievementPopup,
        ChangeMap,
        OpenHatPopup,
        OpenFacePopup,
        OpenCostumePopup,
        RandomizeOutfit,
        BuyHatPopup,
        BuyFacePopup,
        BuyCostumePopup,
        UseAcorn,
        EquipItem,
        UnequipItem,
        FeedWorm,
        PlayWithWorm,
        EvolveWorm,
        CreateNewWorm
    }

    // 프로퍼티
    public ButtonActionType ActionType => actionType;
    public int Index => index;
    public int SoundIndex => soundIndex;

    private void Awake()
    {
        InitializeComponents();
    }

    private void InitializeComponents()
    {
        try
        {
            button = GetComponent<Button>();
            if (button == null)
            {
                button = gameObject.AddComponent<Button>();
            }

            AutoAssignSoundIndex();
            SetupButtonEvents();

            LogDebug($"[ButtonManager] 컴포넌트 초기화 완료 - ActionType: {actionType}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[ButtonManager] 컴포넌트 초기화 중 오류: {ex.Message}");
        }
    }

    private void SetupButtonEvents()
    {
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(HandleClick);
        }
    }

    private void AutoAssignSoundIndex()
    {
        soundIndex = actionType switch
        {
            ButtonActionType.OpenTab => 0,
            ButtonActionType.OpenPopup => 0,
            ButtonActionType.ClosePopup => 1,
            ButtonActionType.OpenClosePopup => 0,
            ButtonActionType.PlayToast => 2,
            ButtonActionType.ShakeTree => 3,
            ButtonActionType.PickAcorn => 4,
            ButtonActionType.PickDiamond => 4,
            ButtonActionType.ShowAchievementPopup => 0,
            ButtonActionType.ChangeMap => 0,
            ButtonActionType.OpenHatPopup => 0,
            ButtonActionType.OpenFacePopup => 0,
            ButtonActionType.OpenCostumePopup => 0,
            ButtonActionType.BuyHatPopup => 0,
            ButtonActionType.BuyFacePopup => 0,
            ButtonActionType.BuyCostumePopup => 0,
            ButtonActionType.UseAcorn => 0,
            ButtonActionType.EquipItem => 0,
            ButtonActionType.UnequipItem => 0,
            ButtonActionType.FeedWorm => 0,
            ButtonActionType.PlayWithWorm => 0,
            ButtonActionType.EvolveWorm => 0,
            ButtonActionType.CreateNewWorm => 0,
            _ => 0
        };
    }

    // 버튼 누르는 순간 사운드 재생
    public void OnPointerDown(PointerEventData eventData)
    {
        if (enableSound)
        {
            AudioManager.Instance?.PlaySFX(AudioManager.SFXType.Button);
        }
    }

    private void HandleClick()
    {
        try
        {
            switch (actionType)
            {
                case ButtonActionType.OpenTab:
                    TabManager.Instance?.OpenTab(index);
                    break;

                case ButtonActionType.OpenPopup:
                    PopupManager.Instance?.OpenPopup((PopupManager.PopupType)index);
                    break;

                case ButtonActionType.ClosePopup:
                    PopupManager.Instance?.ClosePopup(0);
                    break;

                case ButtonActionType.OpenClosePopup:
                    PopupManager.Instance?.ClosePopup(0);
                    break;

                case ButtonActionType.PlayToast:
                    PopupManager.Instance?.ShowToast($"토스트 메시지 {index}");
                    break;

                case ButtonActionType.ShakeTree:
                    treeController?.ShakeTree();
                    break;

                case ButtonActionType.PickAcorn:
                    pickAcorn?.Pick();
                    break;

                case ButtonActionType.PickDiamond:
                    pickDiamond?.Pick();
                    break;

                case ButtonActionType.ShowAchievementPopup:
                    PopupManager.Instance?.OpenPopup(PopupManager.PopupType.Achievement, PopupManager.PopupPriority.Normal, index);
                    break;

                case ButtonActionType.ChangeMap:
                    mapManager?.ChangeMapByIndex(index);
                    break;

                case ButtonActionType.OpenHatPopup:
                    PopupManager.Instance?.OpenPopup(PopupManager.PopupType.ItemSelection, PopupManager.PopupPriority.Normal, (int)ItemData.ItemType.Hat);
                    break;

                case ButtonActionType.OpenFacePopup:
                    PopupManager.Instance?.OpenPopup(PopupManager.PopupType.ItemSelection, PopupManager.PopupPriority.Normal, (int)ItemData.ItemType.Face);
                    break;

                case ButtonActionType.OpenCostumePopup:
                    PopupManager.Instance?.OpenPopup(PopupManager.PopupType.ItemSelection, PopupManager.PopupPriority.Normal, (int)ItemData.ItemType.Costume);
                    break;

                case ButtonActionType.BuyHatPopup:
                    PopupManager.Instance?.OpenPopup(PopupManager.PopupType.ItemSelection, PopupManager.PopupPriority.Normal, (int)ItemData.ItemType.Hat);
                    break;

                case ButtonActionType.BuyFacePopup:
                    PopupManager.Instance?.OpenPopup(PopupManager.PopupType.ItemSelection, PopupManager.PopupPriority.Normal, (int)ItemData.ItemType.Face);
                    break;

                case ButtonActionType.BuyCostumePopup:
                    PopupManager.Instance?.OpenPopup(PopupManager.PopupType.ItemSelection, PopupManager.PopupPriority.Normal, (int)ItemData.ItemType.Costume);
                    break;

                case ButtonActionType.UseAcorn:
                    GameManager.Instance?.UseAcorn();
                    break;

                case ButtonActionType.EquipItem:
                    if (ItemManager.Instance != null)
                    {
                        // 아이템 장착 로직 (인덱스를 아이템 ID로 사용)
                        string itemId = $"Item_{index:D3}";
                        ItemManager.Instance.EquipItem(itemId);
                    }
                    break;

                case ButtonActionType.UnequipItem:
                    if (ItemManager.Instance != null)
                    {
                        // 아이템 해제 로직
                        ItemData.ItemType itemType = (ItemData.ItemType)index;
                        ItemManager.Instance.UnequipItem(itemType);
                    }
                    break;

                case ButtonActionType.FeedWorm:
                    if (WormManager.Instance != null)
                    {
                        WormData currentWorm = WormManager.Instance.GetCurrentWorm();
                        if (currentWorm != null)
                        {
                            currentWorm.Feed();
                        }
                    }
                    break;

                case ButtonActionType.PlayWithWorm:
                    if (WormManager.Instance != null)
                    {
                        WormData currentWorm = WormManager.Instance.GetCurrentWorm();
                        if (currentWorm != null)
                        {
                            currentWorm.Play();
                        }
                    }
                    break;

                case ButtonActionType.EvolveWorm:
                    if (WormManager.Instance != null)
                    {
                        WormManager.Instance.CheckEvolution();
                    }
                    break;

                case ButtonActionType.CreateNewWorm:
                    if (WormManager.Instance != null)
                    {
                        WormManager.Instance.CreateNewWorm();
                    }
                    break;

                default:
                    LogDebug($"[ButtonManager] 알 수 없는 액션 타입: {actionType}");
                    break;
            }

            // 이벤트 발생
            OnButtonClickedEvent?.Invoke(this, actionType, index);

            LogDebug($"[ButtonManager] 버튼 클릭 처리 완료: {actionType} (인덱스: {index})");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[ButtonManager] 버튼 클릭 처리 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 액션 타입 설정
    /// </summary>
    public void SetActionType(ButtonActionType newActionType)
    {
        actionType = newActionType;
        AutoAssignSoundIndex();
        LogDebug($"[ButtonManager] 액션 타입 변경: {newActionType}");
    }

    /// <summary>
    /// 인덱스 설정
    /// </summary>
    public void SetIndex(int newIndex)
    {
        index = newIndex;
        LogDebug($"[ButtonManager] 인덱스 변경: {newIndex}");
    }

    /// <summary>
    /// 사운드 인덱스 설정
    /// </summary>
    public void SetSoundIndex(int newSoundIndex)
    {
        soundIndex = newSoundIndex;
        LogDebug($"[ButtonManager] 사운드 인덱스 변경: {newSoundIndex}");
    }

    /// <summary>
    /// 사운드 활성화/비활성화
    /// </summary>
    public void SetSoundEnabled(bool enabled)
    {
        enableSound = enabled;
        LogDebug($"[ButtonManager] 사운드 {(enabled ? "활성화" : "비활성화")}");
    }

    /// <summary>
    /// 버튼 활성화/비활성화
    /// </summary>
    public void SetInteractable(bool interactable)
    {
        if (button != null)
        {
            button.interactable = interactable;
        }
    }

    /// <summary>
    /// 버튼 정보 반환
    /// </summary>
    public string GetButtonInfo()
    {
        var info = new System.Text.StringBuilder();
        info.AppendLine($"[ButtonManager 정보]");
        info.AppendLine($"액션 타입: {actionType}");
        info.AppendLine($"인덱스: {index}");
        info.AppendLine($"사운드 인덱스: {soundIndex}");
        info.AppendLine($"사운드: {(enableSound ? "활성화" : "비활성화")}");

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
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
        }
    }
}
