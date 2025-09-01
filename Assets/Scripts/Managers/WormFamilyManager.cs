using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using TMPro;
using GGumtles.UI;

/// <summary>
/// 벌레 가계도 관리 매니저
/// GFC(Genealogy Family Chart) 팝업에서 벌레 가족사를 관리하고 표시
/// GFCNode 프리팹을 content에 붙이고 UI 요소들을 동적으로 연결
/// </summary>
public class WormFamilyManager : MonoBehaviour
{
    public static WormFamilyManager Instance { get; private set; }

    [Header("GFC 팝업 설정")]
    [SerializeField] private GameObject gfcPopupPrefab;        // GFC 팝업 프리팹
    [SerializeField] private Transform gfcPopupParent;         // GFC 팝업이 생성될 부모 Transform
    
    [Header("GFC 노드 설정")]
    [SerializeField] private GameObject gfcNodePrefab;         // GFC 노드 프리팹
    
    [Header("디버그 설정")]
    [SerializeField] private bool enableDebugLogs = true;

    // 가계도 데이터 관리
    private List<WormData> familyHistory = new List<WormData>();  // 가계도 히스토리
    private WormData currentWorm;                                 // 현재 벌레

    // UI 관리
    private GameObject currentGfcPopup;                           // 현재 열린 GFC 팝업
    private Transform contentTransform;                           // VerticalLayoutGroup을 가진 Content Transform
    private Dictionary<int, GameObject> generationNodes = new Dictionary<int, GameObject>(); // 세대별 노드 오브젝트

    // 상태 관리
    private bool isInitialized = false;
    private bool isPopupOpen = false;

    // 이벤트 정의
    public delegate void OnFamilyTreeUpdated(List<WormData> familyHistory);
    public event OnFamilyTreeUpdated OnFamilyTreeUpdatedEvent;

    public delegate void OnGenerationAdded(WormData newWorm, WormData previousWorm);
    public event OnGenerationAdded OnGenerationAddedEvent;

    public delegate void OnGfcPopupOpened();
    public event OnGfcPopupOpened OnGfcPopupOpenedEvent;

    public delegate void OnGfcPopupClosed();
    public event OnGfcPopupClosed OnGfcPopupClosedEvent;

    // 프로퍼티
    public List<WormData> FamilyHistory => new List<WormData>(familyHistory);
    public int CurrentGeneration => currentWorm?.generation ?? 0;
    public int TotalGenerations => familyHistory?.Count ?? 0;
    public bool IsInitialized => isInitialized;
    public bool IsPopupOpen => isPopupOpen;

    #region Unity 생명주기

    private void Awake()
    {
        InitializeSingleton();
    }

    #endregion

    #region 초기화

    /// <summary>
    /// 싱글톤 초기화
    /// </summary>
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

    /// <summary>
    /// 매니저 초기화
    /// </summary>
    public void Initialize()
    {
        try
        {
            ValidateComponents();
            InitializeFamilySystem();
            
            isInitialized = true;
            
            LogDebug("[WormFamilyManager] 초기화 완료");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[WormFamilyManager] 초기화 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 컴포넌트 검증
    /// </summary>
    private void ValidateComponents()
    {
        if (gfcPopupPrefab == null)
        {
            Debug.LogError("[WormFamilyManager] GFC 팝업 프리팹이 설정되지 않았습니다.");
        }

        if (gfcNodePrefab == null)
        {
            Debug.LogError("[WormFamilyManager] GFC 노드 프리팹이 설정되지 않았습니다.");
        }

        if (gfcPopupParent == null)
        {
            Debug.LogWarning("[WormFamilyManager] GFC 팝업 부모가 설정되지 않았습니다. 기본값을 사용합니다.");
            gfcPopupParent = transform;
        }
    }

    /// <summary>
    /// 가계도 시스템 초기화
    /// </summary>
    private void InitializeFamilySystem()
    {
        familyHistory.Clear();
        generationNodes.Clear();
        currentWorm = null;
        
        LogDebug("[WormFamilyManager] 가계도 시스템 초기화 완료");
    }

    #endregion

    #region 가계도 데이터 관리

    /// <summary>
    /// 저장된 가계도 데이터로 초기화
    /// </summary>
    public void InitializeFamilyTree(List<WormData> savedFamilyHistory)
    {
        if (!isInitialized)
        {
            Debug.LogWarning("[WormFamilyManager] 아직 초기화되지 않았습니다.");
            return;
        }

        try
        {
            ClearFamilyTree();

            if (savedFamilyHistory != null && savedFamilyHistory.Count > 0)
            {
                foreach (var worm in savedFamilyHistory)
                {
                    if (worm != null && worm.wormId >= 0)
                    {
                        AddWormToHistory(worm);
                    }
                }

                // 가장 최근 벌레를 현재 벌레로 설정
                currentWorm = familyHistory.LastOrDefault();
                
                LogDebug($"[WormFamilyManager] {savedFamilyHistory.Count}개의 가계도 데이터 로드 완료");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[WormFamilyManager] 가계도 초기화 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 새 세대 추가 (새 벌레 생성 시 호출)
    /// </summary>
    public void AddGeneration(WormData newWormData)
    {
        if (!isInitialized)
        {
            Debug.LogWarning("[WormFamilyManager] 아직 초기화되지 않았습니다.");
            return;
        }

        if (newWormData == null || newWormData.wormId < 0)
        {
            Debug.LogError("[WormFamilyManager] 유효하지 않은 벌레 데이터입니다.");
            return;
        }

        try
        {
            // 이전 벌레 백업
            WormData previousWorm = currentWorm;

            // 새 벌레를 히스토리에 추가
            AddWormToHistory(newWormData);
            currentWorm = newWormData;

            // 팝업이 열려있다면 UI 업데이트
            if (isPopupOpen)
            {
                CreateWormNode(newWormData);
            }

            LogDebug($"[WormFamilyManager] 새 세대 추가: {newWormData.name} (세대: {newWormData.generation})");

            // 이벤트 발생
            OnGenerationAddedEvent?.Invoke(newWormData, previousWorm);
            OnFamilyTreeUpdatedEvent?.Invoke(FamilyHistory);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[WormFamilyManager] 세대 추가 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 벌레를 히스토리에 추가
    /// </summary>
    private void AddWormToHistory(WormData worm)
    {
        if (worm == null) return;

        // 중복 제거 (같은 세대가 있는 경우)
        var existingWorm = familyHistory.FirstOrDefault(w => w.generation == worm.generation);
        if (existingWorm != null)
        {
            familyHistory.Remove(existingWorm);
        }

        familyHistory.Add(worm);
        
        // 세대 순으로 정렬
        familyHistory = familyHistory.OrderBy(w => w.generation).ToList();
    }

    /// <summary>
    /// 가계도 초기화
    /// </summary>
    public void ClearFamilyTree()
    {
        familyHistory.Clear();
        currentWorm = null;
        
        // 팝업이 열려있다면 UI도 초기화
        if (isPopupOpen)
        {
            ClearWormNodes();
        }
        
        LogDebug("[WormFamilyManager] 가계도 초기화");
    }

    #endregion

    #region GFC 팝업 관리

    /// <summary>
    /// GFC 팝업 열기
    /// </summary>
    public void OpenGfcPopup()
    {
        if (!isInitialized)
        {
            Debug.LogWarning("[WormFamilyManager] 아직 초기화되지 않았습니다.");
            return;
        }

        if (isPopupOpen)
        {
            Debug.LogWarning("[WormFamilyManager] GFC 팝업이 이미 열려있습니다.");
            return;
        }

        try
        {
            // GFC 팝업 생성
            currentGfcPopup = Instantiate(gfcPopupPrefab, gfcPopupParent);
            
            // Content Transform 찾기
            contentTransform = FindContentTransform(currentGfcPopup);
            if (contentTransform == null)
            {
                Debug.LogError("[WormFamilyManager] Content Transform을 찾을 수 없습니다.");
                return;
            }

            isPopupOpen = true;

            // 기존 가계도 노드들 생성
            CreateAllWormNodes();

            LogDebug("[WormFamilyManager] GFC 팝업 열기 완료");
            
            // 이벤트 발생
            OnGfcPopupOpenedEvent?.Invoke();
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[WormFamilyManager] GFC 팝업 열기 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// GFC 팝업 닫기
    /// </summary>
    public void CloseGfcPopup()
    {
        if (!isPopupOpen)
        {
            Debug.LogWarning("[WormFamilyManager] GFC 팝업이 열려있지 않습니다.");
            return;
        }

        try
        {
            // 팝업 제거
            if (currentGfcPopup != null)
            {
                Destroy(currentGfcPopup);
                currentGfcPopup = null;
            }

            // UI 정리
            ClearWormNodes();
            contentTransform = null;
            isPopupOpen = false;

            LogDebug("[WormFamilyManager] GFC 팝업 닫기 완료");
            
            // 이벤트 발생
            OnGfcPopupClosedEvent?.Invoke();
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[WormFamilyManager] GFC 팝업 닫기 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// Content Transform 찾기
    /// </summary>
    private Transform FindContentTransform(GameObject popup)
    {
        // "Content"라는 이름의 자식 오브젝트 찾기
        Transform content = popup.transform.Find("Content");
        if (content != null)
        {
            // VerticalLayoutGroup 컴포넌트 확인
            if (content.GetComponent<VerticalLayoutGroup>() != null)
            {
                return content;
            }
        }

        // 다른 일반적인 이름들도 시도
        string[] possibleNames = { "ScrollView/Viewport/Content", "ScrollView/Content", "Viewport/Content" };
        foreach (string path in possibleNames)
        {
            Transform found = popup.transform.Find(path);
            if (found != null && found.GetComponent<VerticalLayoutGroup>() != null)
            {
                return found;
            }
        }

        return null;
    }

    #endregion

    #region GFC 노드 관리

    /// <summary>
    /// 모든 벌레 노드 생성
    /// </summary>
    private void CreateAllWormNodes()
    {
        if (contentTransform == null) return;

        try
        {
            // 기존 노드들 제거
            ClearWormNodes();

            // 모든 벌레에 대해 노드 생성
            foreach (var worm in familyHistory)
            {
                CreateWormNode(worm);
            }

            LogDebug($"[WormFamilyManager] {familyHistory.Count}개의 벌레 노드 생성 완료");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[WormFamilyManager] 벌레 노드 생성 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 벌레 노드 생성 및 UI 연결
    /// </summary>
    private void CreateWormNode(WormData wormData)
    {
        if (wormData == null || contentTransform == null) return;

        try
        {
            // GFC 노드 프리팹 인스턴스 생성
            GameObject node = Instantiate(gfcNodePrefab, contentTransform);
            if (node == null)
            {
                Debug.LogError("[WormFamilyManager] GFC 노드 프리팹 인스턴스 생성 실패");
                return;
            }

            node.name = $"GFCNode_Gen{wormData.generation}";

            // UI 요소들 연결
            ConnectNodeUIElements(node, wormData);

            // 세대별 노드 저장
            generationNodes[wormData.generation] = node;

            // 위치 설정 (세대 순으로)
            node.transform.SetSiblingIndex(wormData.generation);

            LogDebug($"[WormFamilyManager] 벌레 노드 생성: {wormData.name} (세대: {wormData.generation})");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[WormFamilyManager] 벌레 노드 생성 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 노드의 UI 요소들을 벌레 데이터와 연결
    /// </summary>
    private void ConnectNodeUIElements(GameObject node, WormData wormData)
    {
        if (node == null || wormData == null) return;

        try
        {
            // GFCWormImage 연결
            Transform wormImageTransform = node.transform.Find("GFCWormImage");
            if (wormImageTransform != null)
            {
                Image wormImage = wormImageTransform.GetComponent<Image>();
                if (wormImage != null)
                {
                    // SpriteManager를 통해 벌레 이미지 가져오기
                    Sprite wormSprite = SpriteManager.Instance?.GetLifeStageSprite(wormData.lifeStage);
                    if (wormSprite != null)
                    {
                        wormImage.sprite = wormSprite;
                    }
                    else
                    {
                        Debug.LogWarning($"[WormFamilyManager] 벌레 이미지를 찾을 수 없습니다: {wormData.lifeStage}");
                    }
                }
            }

            // GFCGenText 연결
            Transform genTextTransform = node.transform.Find("GFCGenText");
            if (genTextTransform != null)
            {
                TextMeshProUGUI genText = genTextTransform.GetComponent<TextMeshProUGUI>();
                if (genText != null)
                {
                    genText.text = $"세대 {wormData.generation}";
                    genText.color = Color.black;
                }
            }

            // GFCNameText 연결
            Transform nameTextTransform = node.transform.Find("GFCNameText");
            if (nameTextTransform != null)
            {
                TextMeshProUGUI nameText = nameTextTransform.GetComponent<TextMeshProUGUI>();
                if (nameText != null)
                {
                    nameText.text = wormData.name;
                    nameText.color = Color.black;
                }
            }

            // GFCAgeText 연결
            Transform ageTextTransform = node.transform.Find("GFCAgeText");
            if (ageTextTransform != null)
            {
                TextMeshProUGUI ageText = ageTextTransform.GetComponent<TextMeshProUGUI>();
                if (ageText != null)
                {
                    if (wormData.lifeStage == 6)
                    {
                        // 사망한 벌레는 "사망" 표시
                        ageText.text = "사망";
                        ageText.color = Color.black;
                    }
                    else
                    {
                        // 나이 계산 (초를 일로 변환)
                        int ageInDays = Mathf.FloorToInt(wormData.age / 86400f); // 86400초 = 1일
                        ageText.text = $"{ageInDays}일";
                        ageText.color = Color.black;
                    }
                }
            }

            LogDebug($"[WormFamilyManager] UI 요소 연결 완료: {wormData.name}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[WormFamilyManager] UI 요소 연결 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 벌레 노드들 제거
    /// </summary>
    private void ClearWormNodes()
    {
        if (contentTransform == null) return;

        try
        {
            // 기존 노드들 제거
            var existingNodes = contentTransform.GetComponentsInChildren<Transform>();
            foreach (var node in existingNodes)
            {
                if (node != null && node.gameObject != null && node != contentTransform)
                {
                    Destroy(node.gameObject);
                }
            }

            generationNodes.Clear();
            
            LogDebug("[WormFamilyManager] 벌레 노드들 제거 완료");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[WormFamilyManager] 벌레 노드 제거 중 오류: {ex.Message}");
        }
    }

    #endregion

    #region 유틸리티 메서드

    /// <summary>
    /// 특정 세대의 벌레 가져오기
    /// </summary>
    public WormData GetWormByGeneration(int generation)
    {
        return familyHistory.FirstOrDefault(w => w.generation == generation);
    }

    /// <summary>
    /// 특정 세대의 노드 오브젝트 가져오기
    /// </summary>
    public GameObject GetNodeByGeneration(int generation)
    {
        return generationNodes.TryGetValue(generation, out var node) ? node : null;
    }

    /// <summary>
    /// 가계도 정보 반환
    /// </summary>
    public string GetFamilyTreeInfo()
    {
        if (!isInitialized) return "가계도 시스템이 초기화되지 않았습니다.";

        var info = new System.Text.StringBuilder();
        info.AppendLine($"[가계도 정보]");
        info.AppendLine($"총 세대 수: {TotalGenerations}");
        info.AppendLine($"현재 세대: {CurrentGeneration}");
        info.AppendLine($"현재 벌레: {currentWorm?.name ?? "없음"}");
        info.AppendLine($"팝업 상태: {(isPopupOpen ? "열림" : "닫힘")}");
        
        if (familyHistory.Count > 0)
        {
            info.AppendLine();
            info.AppendLine("[세대별 벌레]");
            foreach (var worm in familyHistory.OrderBy(w => w.generation))
            {
                string status = worm.isAlive ? "생존" : "사망";
                info.AppendLine($"세대 {worm.generation}: {worm.name} ({status})");
            }
        }

        return info.ToString();
    }

    /// <summary>
    /// 디버그 로그 출력
    /// </summary>
    private void LogDebug(string message)
    {
        if (enableDebugLogs)
        {
            Debug.Log(message);
        }
    }

    #endregion

    #region 이벤트 정리

    private void OnDestroy()
    {
        // 이벤트 초기화
        OnFamilyTreeUpdatedEvent = null;
        OnGenerationAddedEvent = null;
        OnGfcPopupOpenedEvent = null;
        OnGfcPopupClosedEvent = null;
    }

    #endregion
}
