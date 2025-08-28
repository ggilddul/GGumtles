using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using System.Linq;

public class PopupManager : MonoBehaviour
{
    public static PopupManager Instance { get; private set; }

    [Header("팝업 프리팹")]
    [SerializeField] private GameObject achievementPopupPrefab;
    [SerializeField] private GameObject wormEvolvePopupPrefab;
    [SerializeField] private GameObject wormDiePopupPrefab;
    [SerializeField] private GameObject toastPrefab;
    
    [Header("커스텀 팝업")]
    [SerializeField] private List<GameObject> customPopups;
    [SerializeField] private List<GameObject> customToasts;
    
    [Header("설정")]
    [SerializeField] private Transform popupParent;
    [SerializeField] private Transform toastParent;
    [SerializeField] private int maxConcurrentPopups = 3;
    [SerializeField] private float toastDuration = 2f;
    [SerializeField] private float toastFadeDuration = 0.5f;

    // 팝업 타입 열거형
    public enum PopupType
    {
        Achievement,
        WormEvolve,
        WormDie,
        Hat,
        Face,
        Costume,
        Egg,
        Custom,
        ItemSelection,
        DrawConfirm
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

    private void Start()
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
        // 프리팹 등록
        if (achievementPopupPrefab != null)
            popupPrefabs[PopupType.Achievement] = achievementPopupPrefab;
        if (wormEvolvePopupPrefab != null)
            popupPrefabs[PopupType.WormEvolve] = wormEvolvePopupPrefab;
        if (wormDiePopupPrefab != null)
            popupPrefabs[PopupType.WormDie] = wormDiePopupPrefab;
    }

    private void SetupParents()
    {
        // 부모 설정이 없는 경우 자동 생성
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
    /// 팝업 열기
    /// </summary>
    public void OpenPopup(int index, PopupPriority priority = PopupPriority.Normal)
    {
        if (!isInitialized)
        {
            Debug.LogWarning("[PopupManager] 아직 초기화되지 않았습니다.");
            return;
        }

        if (index < 0 || index >= customPopups.Count)
        {
            Debug.LogError($"[PopupManager] 잘못된 팝업 인덱스: {index}");
            return;
        }

        GameObject popupObject = customPopups[index];
        if (popupObject == null)
        {
            Debug.LogError($"[PopupManager] 팝업 객체가 null입니다: 인덱스 {index}");
            return;
        }

        OpenPopupInternal(popupObject, PopupType.Custom, priority);
    }

    /// <summary>
    /// 팝업 타입별 열기
    /// </summary>
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
            return Instantiate(prefab, popupParent);
        }
        
        Debug.LogWarning($"[PopupManager] {type} 타입의 프리팹이 등록되지 않았습니다.");
        return null;
    }

    private void SetupPopupData(GameObject popupObject, PopupType type, object data)
    {
        switch (type)
        {
            case PopupType.Achievement:
                if (data is int achievementIndex)
                {
                    SetupAchievementPopup(popupObject, achievementIndex);
                }
                break;
            case PopupType.WormEvolve:
                if (data is WormData evolveWormData)
                {
                    SetupWormEvolvePopup(popupObject, evolveWormData);
                }
                break;
            case PopupType.WormDie:
                if (data is WormData dieWormData)
                {
                    SetupWormDiePopup(popupObject, dieWormData);
                }
                break;
        }
    }

    private void SetupAchievementPopup(GameObject popupObject, int achievementIndex)
    {
        var achievementManager = AchievementManager.Instance;
        if (achievementManager == null) return;

        var definitions = achievementManager.GetAllDefinitions();
        if (achievementIndex < 0 || achievementIndex >= definitions.Count) return;

        var definition = definitions[achievementIndex];
        var status = achievementManager.GetStatusById(definition.ach_id);
        bool isUnlocked = status != null && status.isUnlocked;

        var popup = popupObject.GetComponent<AchievementPopup>();
        if (popup != null)
        {
            popup.Setup(definition.ach_title, isUnlocked);
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

    public void ClosePopup(int index)
    {
        if (!isInitialized || index < 0 || index >= customPopups.Count) return;

        GameObject popupObject = customPopups[index];
        ClosePopup(popupObject);
    }

    private void ClosePopupInternal(PopupInfo popupInfo)
    {
        // 팝업 비활성화
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
            if (!popup.isModal)
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

    /// <summary>
    /// 토스트 메시지 표시
    /// </summary>
    public void ShowToast(int index)
    {
        if (!isInitialized || index < 0 || index >= customToasts.Count) return;

        GameObject toastObject = customToasts[index];
        if (toastObject != null)
        {
            StartCoroutine(ShowToastCoroutine(toastObject));
        }
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
    /// 업적 팝업 표시 (기존 호환성)
    /// </summary>
    public void ShowAchievementPopup(int index)
    {
        OpenPopup(PopupType.Achievement, PopupPriority.High, index);
    }

    /// <summary>
    /// 벌레 진화 팝업 표시
    /// </summary>
    public void OpenEvolvePopup(WormData worm)
    {
        OpenPopup(PopupType.WormEvolve, PopupPriority.High, worm);
    }

    /// <summary>
    /// 벌레 사망 팝업 표시
    /// </summary>
    public void OpenDiePopup(WormData worm)
    {
        OpenPopup(PopupType.WormDie, PopupPriority.Critical, worm);
    }

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

    private void OnDestroy()
    {
        CloseAllPopups();
    }
}
