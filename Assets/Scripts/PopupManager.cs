using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class PopupManager : MonoBehaviour
{
    public static PopupManager Instance { get; private set; }

    [SerializeField] private GameObject achievementPopupObject;
    
    [Header("타입 1 팝업 필드")]
    [SerializeField] private GameObject popupType1;
    [SerializeField] private TMP_Text popupTitleTextType1;
    [SerializeField] private TMP_Text popupDescTextType1;

    [Header("타입 2 팝업 필드")]
    [SerializeField] private GameObject popupType2;
    [SerializeField] private TMP_Text popupTitleTextType2;
    [SerializeField] private TMP_Text popupDescTextType2;
    [SerializeField] private TMP_Text achieverNameText;
    [SerializeField] private Image achieverImage;

    [SerializeField] private List<GameObject> popups;
    [SerializeField] private List<GameObject> toasts;


    [SerializeField] private GameObject wormEvolvePopupObject;
    private WormEvolvePopupUI wormEvolvePopupUI;
    [SerializeField] private GameObject wormDiePopupObject;
    private WormDiePopupUI wormDiePopupUI;

    public GameObject hatPopup;
    public GameObject facePopup;
    public GameObject costumePopup;
    public GameObject eggPopup;

    public TMP_Text hatDiamondCount;
    public TMP_Text faceDiamondCount;
    public TMP_Text costumeDiamondCount;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            wormEvolvePopupUI = wormEvolvePopupObject.GetComponent<WormEvolvePopupUI>();
        }

        else
        {
            Destroy(gameObject);
        }
    }

    public void UpdateHatDiamondPopup()
    {
        HatDiamondCount.text = $"보유 다이아몬드 수 : {GameManager.Instance.diamondCount}";
    }
    public void UpdateFaceDiamondPopup()
    {
        FaceDiamondCount.text = $"보유 다이아몬드 수 : {GameManager.Instance.diamondCount}";
    }
    public void UpdateCostumeDiamondPopup()
    {
        CostumeDiamondCount.text = $"보유 다이아몬드 수 : {GameManager.Instance.diamondCount}";
    }

    public void OpenPopup(int index)
    {
        if (index >= 0 && index < popups.Count)
            popups[index].SetActive(true);
    }

    public void ClosePopup(int index)
    {
        if (index >= 0 && index < popups.Count)
            popups[index].SetActive(false);
    }

    public void OpenClosePopup(int index)
    {
        if (index > 0 && index < popups.Count)
        {
            popups[index].SetActive(true);
            popups[index - 1].SetActive(false);
        }
    }

    public void PlayToast(int index)
    {
        if (index >= 0 && index < toasts.Count)
            StartCoroutine(ShowToastCoroutine(toasts[index]));
    }


    public void ShowAchievementPopup(int index)
    {
        // 업적 정의
        var definition = AchievementManager.Instance.GetAllDefinitions()[index];

        // 업적 달성 상태 가져오기 (id 기준)
        var status = AchievementManager.Instance.GetStatusById(definition.id);  // 이 메서드는 직접 만들어야 할 수도 있습니다.

        // 업적 달성 여부 확인
        bool isUnlocked = status != null && status.isUnlocked;

        // 팝업 셋업
        var popup = achievementPopupObject.GetComponent<AchievementPopup>();
        popup.Setup(definition.title, isUnlocked);

        if (isUnlocked)
        {
            popupType2.SetActive(true);
            popupTitleTextType2.text = definition.title;
            popupDescTextType2.text = definition.description;
            /*
            achieverNameText.text = definition.achieverName;
            achieverImage.sprite = definition.achieverSprite;
            */
            achieverImage.color = Color.white;

            popupType1.SetActive(false);
        }
        else
        {
            popupType1.SetActive(true);
            popupTitleTextType1.text = definition.title;
            popupDescTextType1.text = definition.description;

            popupType2.SetActive(false);
        }

        achievementPopupObject.SetActive(true);

    }

    public void OpenHatPopup()
    {
        hatPopup.SetActive(true);
    }

    public void OpenFacePopup()
    {
        facePopup.SetActive(true);
    }

    public void OpenCostumePopup()
    {
        costumePopup.SetActive(true);
    }

    public void OpenEggPopup()
    {
        eggPopup.SetActive(true);
    }

    public void OpenDiePopup(WormData worm)
    {
        wormDiePopupUI.OpenPopup(worm);
    }

    public void OpenEvolvePopup(WormData worm)
    {
        wormEvolvePopupUI.OpenPopup(worm);
    }

    private IEnumerator ShowToastCoroutine(GameObject toast)
    {
        toast.SetActive(true);

        CanvasGroup canvasGroup = toast.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = toast.AddComponent<CanvasGroup>();
        }

        canvasGroup.alpha = 0.7f;
        yield return new WaitForSeconds(1.2f);

        float fadeDuration = 0.6f;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = 0.7f - 0.7f * (elapsed / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = 0f;
        toast.SetActive(false);
    }
}
