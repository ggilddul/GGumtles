using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ButtonManager : MonoBehaviour, IPointerDownHandler
{
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
        UseAcorn
    }

    public ButtonActionType actionType;
    public int index;        // 탭/팝업/아이템 인덱스
    public int soundIndex;   // 버튼 사운드 인덱스
    public TreeController treeController;
    public MapManager mapManager;
    public PickAcorn pickAcorn;
    public PickDiamond pickDiamond;
    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        AutoAssignSoundIndex(); // 여기 추가
        button.onClick.AddListener(HandleClick);
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
            _ => 0
        };
    }

    // 여기서 버튼 누르는 순간 사운드 재생
    public void OnPointerDown(PointerEventData eventData)
    {
        AudioManager.Instance.PlayButtonSound(soundIndex);
    }

    private void HandleClick()
    {
        switch (actionType)
        {
            case ButtonActionType.OpenTab:
                TabManager.Instance?.OpenTab(index);
                break;

            case ButtonActionType.OpenPopup:
                PopupManager.Instance?.OpenPopup(index);
                break;

            case ButtonActionType.ClosePopup:
                PopupManager.Instance?.ClosePopup(index);
                break;

            case ButtonActionType.OpenClosePopup:
                PopupManager.Instance?.OpenClosePopup(index);
                break;

            case ButtonActionType.PlayToast:
                PopupManager.Instance?.PlayToast(index);
                break;

            case ButtonActionType.ShakeTree:
                treeController?.ShakeTree();
                break;

            case ButtonActionType.PickAcorn:
                pickAcorn.Pick();
                break;

            case ButtonActionType.PickDiamond:
                pickDiamond?.Pick();
                break;

            case ButtonActionType.ShowAchievementPopup:
                PopupManager.Instance?.ShowAchievementPopup(index);
                break;

            case ButtonActionType.ChangeMap:
                mapManager.ChangeMapByIndex(index);
                break;

            case ButtonActionType.OpenHatPopup:
                PopupManager.Instance?.OpenHatPopup();
                break;

            case ButtonActionType.OpenFacePopup:
                PopupManager.Instance?.OpenFacePopup();
                break;

            case ButtonActionType.OpenCostumePopup:
                PopupManager.Instance?.OpenCostumePopup();
                break;

            case ButtonActionType.BuyHatPopup:
                PopupManager.Instance?.OpenHatPopup();
                break;

            case ButtonActionType.BuyFacePopup:
                PopupManager.Instance?.OpenFacePopup();
                break;

            case ButtonActionType.BuyCostumePopup:
                PopupManager.Instance?.OpenCostumePopup();
                break;

            case ButtonActionType.UseAcorn:
                GameManager.Instance?.UseAcorn();
                break;
        }
    }
}
