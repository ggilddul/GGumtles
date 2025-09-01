using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using GGumtles.UI;
using TMPro;

public class PopupManager : MonoBehaviour
{
    public static PopupManager Instance { get; private set; }

    [Header("GameUI Parent")]
    [SerializeField] private Transform gameUIParent;
    [SerializeField] private GameObject gameType1PanelPrefab;
    [SerializeField] private GameObject gameType2PanelPrefab;
    [SerializeField] private GameObject gameType3PanelPrefab;
    [SerializeField] private GameObject gameType4PanelPrefab;

    [Header("Popup Parent")]
    [SerializeField] private Transform popupParent;
    [SerializeField] private GameObject diePopupPrefab;
    [SerializeField] private GameObject drawConfirmPopupPrefab;
    [SerializeField] private GameObject drawResultPopupPrefab;
    [SerializeField] private GameObject evolvePopupPrefab;
    [SerializeField] private GameObject itemPopupPrefab;
    [SerializeField] private GameObject acornPopupPrefab;
    [SerializeField] private GameObject agePopupPrefab;
    [SerializeField] private GameObject diamondPopupPrefab;
    [SerializeField] private GameObject medalPopupPrefab;
    [SerializeField] private GameObject namePopupPrefab;
    [SerializeField] private GameObject stagesPopupPrefab;
    [SerializeField] private GameObject generationPopupPrefab;
    [SerializeField] private GameObject eggFoundPopupPrefab;
    [SerializeField] private GameObject gfcPopupPrefab;
    [SerializeField] private GameObject itemDrawPopupPrefab;
    [SerializeField] private GameObject optionPopupPrefab;
    [SerializeField] private GameObject statsPopupPrefab;
    [SerializeField] private GameObject gameTimePopupPrefab;
    [SerializeField] private GameObject achievement1PopupPrefab;
    [SerializeField] private GameObject achievement2PopupPrefab;

    [Header("Toast")]
    [SerializeField] private Transform toastParent;
    [SerializeField] private GameObject toastPrefab;
    
    [Header("설정")]
    [SerializeField] private int maxConcurrentPopups = 3;
    [SerializeField] private float toastDuration = 2f;
    [SerializeField] private float toastFadeDuration = 0.5f;

    // 팝업 타입 열거형
    public enum PopupType
    {
        // GameUI 관련
        GameType1Panel,
        GameType2Panel,
        GameType3Panel,
        GameType4Panel,
        
        // Popup 관련
        Die,
        DrawConfirm,
        DrawResult,
        EvolvePopup,
        Item,
        Acorn,
        Age,
        Diamond,
        Medal,
        Name,
        Stages,
        Generation,
        EggFound,
        GFC,
        ItemDraw,
        Option,
        Stats,
        GameTimePopup,
        Achievement1,
        Achievement2
    }

    // 팝업 우선순위 열거형
    public enum PopupPriority
    {
        Low = 0,
        Normal = 1,
        High = 2,
        Critical = 3
    }

    // 팝업 정보 클래스
    [System.Serializable]
    public class PopupInfo
    {
        public PopupType type;
        public PopupPriority priority;
        public GameObject popupObject;
        public bool isModal;
        public System.Action onClose;
        public float autoCloseTime;
        
        public PopupInfo(PopupType type, PopupPriority priority, GameObject popupObject, bool isModal = false, System.Action onClose = null, float autoCloseTime = 0f)
        {
            this.type = type;
            this.priority = priority;
            this.popupObject = popupObject;
            this.isModal = isModal;
            this.onClose = onClose;
            this.autoCloseTime = autoCloseTime;
        }
    }

    // 팝업 스택 관리
    private Stack<PopupInfo> popupStack = new Stack<PopupInfo>();
    private List<PopupInfo> activePopups = new List<PopupInfo>();
    private Queue<GameObject> toastQueue = new Queue<GameObject>();
    
    // 컴포넌트 캐시
    private Dictionary<PopupType, GameObject> popupPrefabs = new Dictionary<PopupType, GameObject>();
    private Dictionary<GameObject, PopupInfo> popupInfoMap = new Dictionary<GameObject, PopupInfo>();
    
    // 상태 관리
    private bool isInitialized = false;
    private Coroutine autoCloseCoroutine;

    private void Awake()
    {
        InitializeSingleton();
    }

    public void Initialize()
    {
        InitializePopupSystem();
    }

    private void InitializeSingleton()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializePopupSystem()
    {
        try
        {
            ValidateComponents();
            InitializePopupPrefabs();
            SetupParents();
            isInitialized = true;
            
            Debug.Log("[PopupManager] 팝업 시스템 초기화 완료");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[PopupManager] 초기화 중 오류: {ex.Message}");
        }
    }

    private void ValidateComponents()
    {
        if (popupParent == null)
        {
            popupParent = transform;
            Debug.LogWarning("[PopupManager] 팝업 부모가 설정되지 않아 기본값을 사용합니다.");
        }
        
        if (toastParent == null)
        {
            toastParent = transform;
            Debug.LogWarning("[PopupManager] 토스트 부모가 설정되지 않아 기본값을 사용합니다.");
        }
    }

    private void InitializePopupPrefabs()
    {
        // 수동으로 설정된 프리팹들 등록
        RegisterPrefabIfNotNull(gameType1PanelPrefab, PopupType.GameType1Panel);
        RegisterPrefabIfNotNull(gameType2PanelPrefab, PopupType.GameType2Panel);
        RegisterPrefabIfNotNull(gameType3PanelPrefab, PopupType.GameType3Panel);
        RegisterPrefabIfNotNull(gameType4PanelPrefab, PopupType.GameType4Panel);
        RegisterPrefabIfNotNull(drawConfirmPopupPrefab, PopupType.DrawConfirm);
        RegisterPrefabIfNotNull(drawResultPopupPrefab, PopupType.DrawResult);
        RegisterPrefabIfNotNull(evolvePopupPrefab, PopupType.EvolvePopup);
        RegisterPrefabIfNotNull(itemPopupPrefab, PopupType.Item);
        RegisterPrefabIfNotNull(acornPopupPrefab, PopupType.Acorn);
        RegisterPrefabIfNotNull(agePopupPrefab, PopupType.Age);
        RegisterPrefabIfNotNull(diamondPopupPrefab, PopupType.Diamond);
        RegisterPrefabIfNotNull(medalPopupPrefab, PopupType.Medal);
        RegisterPrefabIfNotNull(namePopupPrefab, PopupType.Name);
        RegisterPrefabIfNotNull(stagesPopupPrefab, PopupType.Stages);
        RegisterPrefabIfNotNull(generationPopupPrefab, PopupType.Generation);
        RegisterPrefabIfNotNull(diePopupPrefab, PopupType.Die);
        RegisterPrefabIfNotNull(eggFoundPopupPrefab, PopupType.EggFound);
        RegisterPrefabIfNotNull(gfcPopupPrefab, PopupType.GFC);
        RegisterPrefabIfNotNull(itemDrawPopupPrefab, PopupType.ItemDraw);
        RegisterPrefabIfNotNull(optionPopupPrefab, PopupType.Option);
        RegisterPrefabIfNotNull(statsPopupPrefab, PopupType.Stats);
        RegisterPrefabIfNotNull(gameTimePopupPrefab, PopupType.GameTimePopup);
        RegisterPrefabIfNotNull(achievement1PopupPrefab, PopupType.Achievement1);
        RegisterPrefabIfNotNull(achievement2PopupPrefab, PopupType.Achievement2);


    }

    private void RegisterPrefabIfNotNull(GameObject prefab, PopupType type)
    {
        if (prefab != null)
        {
            popupPrefabs[type] = prefab;
            Debug.Log($"[PopupManager] 프리팹 등록: {type} - {prefab.name}");
        }
    }



    private void SetupParents()
    {
        // 부모 설정이 없는 경우 자동 생성
        if (gameUIParent == null)
        {
            GameObject gameUIParentObj = new GameObject("GameUIParent");
            gameUIParentObj.transform.SetParent(transform);
            gameUIParent = gameUIParentObj.transform;
        }
        if (popupParent == null)
        {
            GameObject popupParentObj = new GameObject("PopupParent");
            popupParentObj.transform.SetParent(transform);
            popupParent = popupParentObj.transform;
        }
        
        if (toastParent == null)
        {
            GameObject toastParentObj = new GameObject("ToastParent");
            toastParentObj.transform.SetParent(transform);
            toastParent = toastParentObj.transform;
        }
    }



    /// <summary>
    /// 팝업 열기 (통합된 메서드)
    /// </summary>
    /// <param name="type">팝업 타입</param>
    /// <param name="priority">우선순위 (기본값: Normal)</param>
    /// <param name="data">팝업에 전달할 데이터 (선택사항)</param>
    /// 
    /// <remarks>
    /// <b>팝업 타입별 데이터 파라미터:</b>
    /// <list type="bullet">
    /// <item><description><b>Item</b>: ItemData.ItemType (Hat, Face, Costume)</description></item>
    /// <item><description><b>DrawConfirm</b>: ItemData.ItemType (뽑을 아이템 종류)</description></item>
    /// <item><description><b>DrawResult</b>: ItemData (획득한 아이템)</description></item>
    /// <item><description><b>EvolvePopup</b>: WormData (진화한 웜)</description></item>
    /// <item><description><b>Die</b>: WormData (사망한 웜)</description></item>
    /// <item><description><b>GameType1Panel~4Panel</b>: int (게임 타입 인덱스)</description></item>
    /// <item><description><b>기타 팝업</b>: null (데이터 불필요)</description></item>
    /// </list>
    /// 
    /// <b>사용 예시:</b>
    /// <code>
    /// // 아이템 팝업 (Hat 탭으로 열기)
    /// PopupManager.Instance.OpenPopup(PopupType.Item, PopupPriority.Normal, ItemData.ItemType.Hat);
    /// 
    /// // 뽑기 확인 팝업
    /// PopupManager.Instance.OpenPopup(PopupType.DrawConfirm, PopupPriority.Normal, ItemData.ItemType.Costume);
    /// 
    /// // 웜 진화 팝업
    /// PopupManager.Instance.OpenPopup(PopupType.EvolvePopup, PopupPriority.High, wormData);
    /// 
    /// // 옵션 팝업 (데이터 없음)
    /// PopupManager.Instance.OpenPopup(PopupType.Option);
    /// </code>
    /// </remarks>
    public void OpenPopup(PopupType type, PopupPriority priority = PopupPriority.Normal, object data = null)
    {
        if (!isInitialized)
        {
            Debug.LogWarning("[PopupManager] 아직 초기화되지 않았습니다.");
            return;
        }

        GameObject popupObject = CreatePopupObject(type);
        if (popupObject == null)
        {
            Debug.LogError($"[PopupManager] 팝업 생성 실패: {type}");
            return;
        }

        // 데이터 설정
        SetupPopupData(popupObject, type, data);
        
        OpenPopupInternal(popupObject, type, priority);
    }

    private GameObject CreatePopupObject(PopupType type)
    {
        if (popupPrefabs.TryGetValue(type, out GameObject prefab))
        {
            Transform parent = GetParentForPopupType(type);
            return Instantiate(prefab, parent);
        }
        
        Debug.LogWarning($"[PopupManager] {type} 타입의 프리팹이 등록되지 않았습니다.");
        return null;
    }

    private Transform GetParentForPopupType(PopupType type)
    {
        return type switch
        {
            // GameUI 관련
            PopupType.GameType1Panel or PopupType.GameType2Panel or 
            PopupType.GameType3Panel or PopupType.GameType4Panel => gameUIParent,
            
            // 모든 팝업은 popupParent 사용
            _ => popupParent
        };
    }

    private void SetupPopupData(GameObject popupObject, PopupType type, object data)
    {
        switch (type)
        {
            case PopupType.EvolvePopup:
                if (data is WormData evolveWormData)
                {
                    SetupWormEvolvePopup(popupObject, evolveWormData);
                }
                break;
            case PopupType.Die:
                if (data is WormData dieWormData)
                {
                    SetupWormDiePopup(popupObject, dieWormData);
                }
                break;
            case PopupType.GameType1Panel:
            case PopupType.GameType2Panel:
            case PopupType.GameType3Panel:
            case PopupType.GameType4Panel:
                if (data is int gameTypeIndex)
                {
                    SetupGameTypePanel(popupObject, gameTypeIndex);
                }
                break;
            case PopupType.Item:
                if (data is ItemData.ItemType itemType)
                {
                    SetupItemPopup(popupObject, itemType);
                }
                else
                {
                    // 기본값으로 Hat 탭 설정
                    SetupItemPopup(popupObject, ItemData.ItemType.Hat);
                }
                break;
            case PopupType.DrawConfirm:
                if (data is ItemData.ItemType drawConfirmItemType)
                {
                    SetupDrawConfirmPopup(popupObject, drawConfirmItemType);
                }
                break;
            case PopupType.DrawResult:
                if (data is ItemData itemData)
                {
                    SetupDrawResultPopup(popupObject, itemData);
                }
                break;
            case PopupType.Achievement1:
                if (data is int achievement1Index)
                {
                    SetupAchievement1Popup(popupObject, achievement1Index);
                }
                break;
            case PopupType.Achievement2:
                if (data is int achievement2Index)
                {
                    SetupAchievement2Popup(popupObject, achievement2Index);
                }
                break;
        }
    }

    private void SetupAchievementPopup(GameObject popupObject, int achievementIndex)
    {
        // AchievementPopup 제거됨 - 단순화
        Debug.Log($"[PopupManager] AchievementPopup 제거됨: Achievement Index {achievementIndex}");
    }

    private void SetupAchievement1Popup(GameObject popupObject, int achievementIndex)
    {
        var popup = popupObject.GetComponent<AchievementUI>();
        if (popup != null)
        {
            popup.Initialize(achievementIndex);
        }
        else
        {
            Debug.LogWarning($"[PopupManager] AchievementUI 컴포넌트를 찾을 수 없습니다: {popupObject.name}");
        }
    }

    private void SetupAchievement2Popup(GameObject popupObject, int achievementIndex)
    {
        var popup = popupObject.GetComponent<AchievementUI>();
        if (popup != null)
        {
            popup.Initialize(achievementIndex, true); // Achievement2 전용
        }
        else
        {
            Debug.LogWarning($"[PopupManager] AchievementUI 컴포넌트를 찾을 수 없습니다: {popupObject.name}");
        }
    }

    private void SetupWormEvolvePopup(GameObject popupObject, WormData wormData)
    {
        var popup = popupObject.GetComponent<WormEvolvePopupUI>();
        if (popup != null)
        {
            // 진화 단계 정보는 WormManager에서 가져와야 함
            int fromStage = 0;
            int toStage = wormData.lifeStage;
            popup.OpenPopup(wormData, fromStage, toStage);
        }
    }

    private void SetupWormDiePopup(GameObject popupObject, WormData wormData)
    {
        var popup = popupObject.GetComponent<WormDiePopupUI>();
        if (popup != null)
        {
            popup.OpenPopup(wormData);
        }
    }

    private void SetupGameTypePanel(GameObject popupObject, int gameTypeIndex)
    {
        // GameTypePanel 설정 로직
        // GameTypeTab에 VerticalLayoutGroup 설정 등
        Debug.Log($"[PopupManager] GameTypePanel 설정 - Game Type Index: {gameTypeIndex}");
    }

    private void SetupItemPopup(GameObject popupObject, ItemData.ItemType itemPopupType)
    {
        // ItemSelectionPopupUI 제거됨 - 단순화
        Debug.Log($"[PopupManager] ItemSelectionPopupUI 제거됨: {itemPopupType}");
    }

    private void SetupDrawConfirmPopup(GameObject popupObject, ItemData.ItemType drawItemType)
    {
        var popup = popupObject.GetComponent<DrawConfirmPopupUI>();
        if (popup != null)
        {
            popup.Initialize(drawItemType);
        }
        else
        {
            Debug.LogWarning($"[PopupManager] DrawConfirmPopupUI 컴포넌트를 찾을 수 없습니다: {popupObject.name}");
        }
    }

    private void SetupDrawResultPopup(GameObject popupObject, ItemData itemData)
    {
        var popup = popupObject.GetComponent<DrawResultPopupUI>();
        if (popup != null)
        {
            popup.Initialize(itemData);
        }
        else
        {
            Debug.LogWarning($"[PopupManager] DrawResultPopupUI 컴포넌트를 찾을 수 없습니다: {popupObject.name}");
        }
    }

    private void OpenPopupInternal(GameObject popupObject, PopupType type, PopupPriority priority)
    {
        // 모달 팝업인 경우 다른 팝업들 비활성화
        bool isModal = priority >= PopupPriority.High;
        if (isModal)
        {
            DisableLowerPriorityPopups(priority);
        }

        // 팝업 정보 생성
        var popupInfo = new PopupInfo(type, priority, popupObject, isModal);
        popupInfoMap[popupObject] = popupInfo;
        activePopups.Add(popupInfo);
        popupStack.Push(popupInfo);

        // 팝업 활성화
        popupObject.SetActive(true);

        // 최대 팝업 수 제한
        if (activePopups.Count > maxConcurrentPopups)
        {
            var oldestPopup = activePopups.FirstOrDefault(p => p.priority == PopupPriority.Low);
            if (oldestPopup != null)
            {
                ClosePopup(oldestPopup.popupObject);
            }
        }

        Debug.Log($"[PopupManager] 팝업 열기: {type} (우선순위: {priority})");
    }

    private void DisableLowerPriorityPopups(PopupPriority currentPriority)
    {
        foreach (var popup in activePopups.ToList())
        {
            if (popup.priority < currentPriority)
            {
                popup.popupObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// 특정 타입의 팝업 닫기
    /// </summary>
    public void ClosePopup(PopupType type)
    {
        var popupToClose = activePopups.FirstOrDefault(p => p.type == type);
        if (popupToClose != null)
        {
            ClosePopup(popupToClose.popupObject);
        }
        else
        {
            Debug.LogWarning($"[PopupManager] 팝업 {type}을 찾을 수 없습니다.");
        }
    }

    /// <summary>
    /// 팝업 닫기
    /// </summary>
    public void ClosePopup(GameObject popupObject)
    {
        if (!isInitialized || popupObject == null) return;

        if (popupInfoMap.TryGetValue(popupObject, out PopupInfo popupInfo))
        {
            ClosePopupInternal(popupInfo);
        }
        else
        {
            popupObject.SetActive(false);
        }
    }



    private void ClosePopupInternal(PopupInfo popupInfo)
    {
        if (popupInfo == null || popupInfo.popupObject == null) return;

        // 팝업 비활성화 (null 체크 추가)
        popupInfo.popupObject.SetActive(false);
        
        // 콜백 실행
        popupInfo.onClose?.Invoke();
        
        // 리스트에서 제거
        activePopups.Remove(popupInfo);
        popupInfoMap.Remove(popupInfo.popupObject);
        
        // 모달 팝업이 닫힌 경우 다른 팝업들 재활성화
        if (popupInfo.isModal)
        {
            RestoreLowerPriorityPopups();
        }
        
        Debug.Log($"[PopupManager] 팝업 닫기: {popupInfo.type}");
    }

    private void RestoreLowerPriorityPopups()
    {
        foreach (var popup in activePopups)
        {
            if (!popup.isModal && popup.popupObject != null)
            {
                popup.popupObject.SetActive(true);
            }
        }
    }

    /// <summary>
    /// 모든 팝업 닫기
    /// </summary>
    public void CloseAllPopups()
    {
        if (!isInitialized) return;

        foreach (var popup in activePopups.ToList())
        {
            ClosePopupInternal(popup);
        }
        
        Debug.Log("[PopupManager] 모든 팝업 닫기");
    }



    public void ShowToast(string message, float duration = -1)
    {
        if (!isInitialized) return;

        GameObject toastObject = CreateToastObject(message);
        if (toastObject != null)
        {
            StartCoroutine(ShowToastCoroutine(toastObject, duration));
        }
    }

    private GameObject CreateToastObject(string message)
    {
        if (toastPrefab == null)
        {
            Debug.LogWarning("[PopupManager] 토스트 프리팹이 설정되지 않았습니다.");
            return null;
        }

        GameObject toastObject = Instantiate(toastPrefab, toastParent);
        
        // 메시지 설정
        var textComponent = toastObject.GetComponentInChildren<TMP_Text>();
        if (textComponent != null)
        {
            textComponent.text = message;
        }
        
        return toastObject;
    }

    private IEnumerator ShowToastCoroutine(GameObject toast, float customDuration = -1)
    {
        if (toast == null) yield break;

        toast.SetActive(true);
        float duration = customDuration > 0 ? customDuration : toastDuration;

        // 페이드 인
        CanvasGroup canvasGroup = toast.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = toast.AddComponent<CanvasGroup>();
        }

        float fadeInTime = 0.3f;
        float elapsed = 0f;
        
        while (elapsed < fadeInTime)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeInTime);
            yield return null;
        }
        
        canvasGroup.alpha = 1f;

        // 대기
        yield return new WaitForSeconds(duration);

        // 페이드 아웃
        elapsed = 0f;
        while (elapsed < toastFadeDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / toastFadeDuration);
            yield return null;
        }

        canvasGroup.alpha = 0f;
        toast.SetActive(false);
        
        // 메모리 정리
        if (toast.transform.parent == toastParent)
        {
            Destroy(toast);
        }
    }



    /// <summary>
    /// 진화 팝업 열기
    /// </summary>
    public void OpenEvolvePopup(WormData worm)
    {
        OpenPopup(PopupType.EvolvePopup, PopupPriority.High, worm);
    }

    /// <summary>
    /// 사망 팝업 열기
    /// </summary>
    public void OpenDiePopup(WormData worm)
    {
        OpenPopup(PopupType.Die, PopupPriority.Critical, worm);
    }

    /// <summary>
    /// 게임 타입 패널 열기
    /// </summary>
    public void OpenGameTypePanel(int gameType)
    {
        PopupType type = gameType switch
        {
            1 => PopupType.GameType1Panel,
            2 => PopupType.GameType2Panel,
            3 => PopupType.GameType3Panel,
            4 => PopupType.GameType4Panel,
            _ => PopupType.GameType1Panel
        };
        OpenPopup(type, PopupPriority.Normal);
    }

    /// <summary>
    /// 팝업 닫기 (하위 호환성을 위해 유지)
    /// </summary>
    public void CloseCustomPopup(PopupType customType)
    {
        ClosePopup(customType);
    }

    /// <summary>
    /// 아이템 뽑기 팝업 열기
    /// </summary>
    public void OpenItemDrawPopup(ItemData.ItemType itemType)
    {
        OpenPopup(PopupType.ItemDraw, PopupPriority.Normal, itemType);
    }

    /// <summary>
    /// 뽑기 확인 팝업 열기 (선택한 아이템 종류와 함께)
    /// </summary>
    public void OpenDrawConfirmPopup(ItemData.ItemType itemType)
    {
        CloseCustomPopup(PopupType.ItemDraw); // ItemDraw 팝업 닫기
        OpenPopup(PopupType.DrawConfirm, PopupPriority.Normal, itemType);
    }

    /// <summary>
    /// 뽑기 결과 팝업 열기 (획득한 아이템과 함께)
    /// </summary>
    public void OpenDrawResultPopup(ItemData itemData)
    {
        ClosePopup(PopupType.DrawConfirm); // DrawConfirm 팝업 닫기
        OpenPopup(PopupType.DrawResult, PopupPriority.Normal, itemData);
    }

    /// <summary>
    /// 아이템 뽑기 취소
    /// </summary>
    public void CancelItemDraw()
    {
        CloseCustomPopup(PopupType.ItemDraw);
    }

    /// <summary>
    /// 뽑기 확인 취소
    /// </summary>
    public void CancelDrawConfirm()
    {
        ClosePopup(PopupType.DrawConfirm);
    }

    /// <summary>
    /// 뽑기 결과 확인
    /// </summary>
    public void ConfirmDrawResult()
    {
        ClosePopup(PopupType.DrawResult);
    }

    /// <summary>
    /// 아이템 팝업 열기 (통합)
    /// </summary>
    public void OpenItemPopup(ItemData.ItemType itemType)
    {
        OpenPopup(PopupType.Item, PopupPriority.Normal, itemType);
    }

    /// <summary>
    /// Hat 탭으로 아이템 팝업 열기
    /// </summary>
    // 하위 호환 제거: OpenItemPopupWithHat/Face/Costume는 통합 OpenItemPopup으로 대체되었습니다.

    /// <summary>
    /// Face 탭으로 아이템 팝업 열기
    /// </summary>
    //

    /// <summary>
    /// Costume 탭으로 아이템 팝업 열기
    /// </summary>
    //

    /// <summary>
    /// 현재 활성 팝업 수 반환
    /// </summary>
    public int GetActivePopupCount()
    {
        return activePopups.Count;
    }

    /// <summary>
    /// 특정 타입의 팝업이 열려있는지 확인
    /// </summary>
    public bool IsPopupOpen(PopupType type)
    {
        return activePopups.Any(p => p.type == type);
    }

    /// <summary>
    /// 팝업 정보 가져오기
    /// </summary>
    public PopupInfo GetPopupInfo(GameObject popupObject)
    {
        return popupInfoMap.TryGetValue(popupObject, out PopupInfo info) ? info : null;
    }

    /// <summary>
    /// PopupManager 정보 반환
    /// </summary>
    public string GetPopupManagerInfo()
    {
        var info = new System.Text.StringBuilder();
        info.AppendLine("[PopupManager 정보]");
        info.AppendLine($"초기화됨: {isInitialized}");
        info.AppendLine($"활성 팝업 수: {activePopups.Count}");
        info.AppendLine($"최대 동시 팝업 수: {maxConcurrentPopups}");
        
        info.AppendLine("\n[부모 설정]");
        info.AppendLine($"GameUI Parent: {(gameUIParent != null ? gameUIParent.name : "null")}");
        info.AppendLine($"Popup Parent: {(popupParent != null ? popupParent.name : "null")}");
        info.AppendLine($"Toast Parent: {(toastParent != null ? toastParent.name : "null")}");
        
        info.AppendLine("\n[등록된 프리팹]");
        info.AppendLine($"총 등록된 프리팹: {popupPrefabs.Count}개");
        info.AppendLine($"GameUI 프리팹: {GetRegisteredPrefabCount(PopupType.GameType1Panel, PopupType.GameType2Panel, PopupType.GameType3Panel, PopupType.GameType4Panel)}개");
        info.AppendLine($"Popup 프리팹: {GetRegisteredPrefabCount(PopupType.Die, PopupType.DrawConfirm, PopupType.DrawResult, PopupType.EvolvePopup, PopupType.Item, PopupType.Acorn, PopupType.Age, PopupType.Diamond, PopupType.Medal, PopupType.Name, PopupType.Stages, PopupType.Generation, PopupType.EggFound, PopupType.GFC, PopupType.ItemDraw, PopupType.Option, PopupType.Stats, PopupType.GameTimePopup)}개");
        
        // 등록된 모든 프리팹 목록 출력
        info.AppendLine("\n[등록된 프리팹 목록]");
        foreach (var kvp in popupPrefabs)
        {
            info.AppendLine($"  {kvp.Key}: {kvp.Value.name}");
        }
        
        if (activePopups.Count > 0)
        {
            info.AppendLine("\n[활성 팝업]");
            foreach (var popup in activePopups)
            {
                info.AppendLine($"  {popup.type} (우선순위: {popup.priority})");
            }
        }

        return info.ToString();
    }

    private int GetRegisteredPrefabCount(params PopupType[] types)
    {
        int count = 0;
        foreach (var type in types)
        {
            if (popupPrefabs.ContainsKey(type))
                count++;
        }
        return count;
    }

    private void OnDestroy()
    {
        // 안전하게 모든 팝업 닫기
        if (activePopups != null)
        {
            foreach (var popup in activePopups.ToList())
            {
                if (popup != null && popup.popupObject != null)
                {
                    ClosePopupInternal(popup);
                }
            }
        }
    }
}
