using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class WormFamilyManager : MonoBehaviour
{
    public static WormFamilyManager Instance { get; private set; }

    [Header("가계도 UI 설정")]
    [SerializeField] private GameObject nonLeafWormPrefab;    // 기존 웜 정보용 프리팹
    [SerializeField] private Transform content;               // Content 오브젝트
    [SerializeField] private LeafWormUI leafWormUI;           // 현재 웜 (항상 화면 맨 아래에 위치해야 함)
    
    [Header("가계도 표시 설정")]
    // [SerializeField] private bool showFullFamilyTree = true;  // 전체 가계도 표시 (미사용)
    // [SerializeField] private int maxDisplayGenerations = 10;  // 최대 표시 세대 수 (미사용)
    [SerializeField] private bool showDeathStatus = true;     // 사망 상태 표시
    [SerializeField] private bool showLifeStage = true;       // 생명주기 표시
    [SerializeField] private bool showRarity = true;          // 희귀도 표시

    [Header("애니메이션 설정")]
    [SerializeField] private bool enableAnimations = true;    // 애니메이션 활성화
    // [SerializeField] private float animationDuration = 0.5f;  // 애니메이션 지속시간 (미사용)
    [SerializeField] private AnimationCurve animationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("디버그 설정")]
    [SerializeField] private bool enableDebugLogs = true;

    // 가계도 데이터 관리
    private Dictionary<int, WormNodeUI> generationNodes;  // 세대별 노드
    private List<WormData> familyHistory;                // 가계도 히스토리
    private WormData currentWorm;                        // 현재 벌레

    // 상태 관리
    private bool isInitialized = false;
    // private bool isUpdating = false;  // 미사용

    // 이벤트 정의
    public delegate void OnFamilyTreeUpdated(List<WormData> familyHistory);
    public event OnFamilyTreeUpdated OnFamilyTreeUpdatedEvent;

    public delegate void OnGenerationAdded(WormData newWorm, WormData previousWorm);
    public event OnGenerationAdded OnGenerationAddedEvent;

    // 프로퍼티
    public List<WormData> FamilyHistory => new List<WormData>(familyHistory);
    public int CurrentGeneration => currentWorm?.generation ?? 0;
    public int TotalGenerations => familyHistory?.Count ?? 0;
    public bool IsInitialized => isInitialized;

    private void Awake()
    {
        InitializeSingleton();
    }

    private void Start()
    {
        InitializeFamilySystem();
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

    private void InitializeFamilySystem()
    {
        try
        {
            generationNodes = new Dictionary<int, WormNodeUI>();
            familyHistory = new List<WormData>();
            currentWorm = null;
            isInitialized = true;

            LogDebug("[WormFamilyManager] 가계도 시스템 초기화 완료");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[WormFamilyManager] 초기화 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 가계도 초기화
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
                    if (worm != null && worm.IsValid)
                    {
                        AddWormToHistory(worm);
                    }
                }

                // 가장 최근 벌레를 현재 벌레로 설정
                currentWorm = familyHistory.LastOrDefault();
                UpdateFamilyTreeDisplay();
            }

            LogDebug($"[WormFamilyManager] {savedFamilyHistory?.Count ?? 0}개의 가계도 데이터 로드 완료");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[WormFamilyManager] 가계도 초기화 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 새 세대 추가
    /// </summary>
    public void AddGeneration(WormData newWormData)
    {
        if (!isInitialized)
        {
            Debug.LogWarning("[WormFamilyManager] 아직 초기화되지 않았습니다.");
            return;
        }

        if (newWormData == null || !newWormData.IsValid)
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

            // UI 업데이트
            UpdateFamilyTreeDisplay();

            LogDebug($"[WormFamilyManager] 새 세대 추가: {newWormData.DisplayName} (세대: {newWormData.generation})");

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
    /// 가계도 UI 업데이트
    /// </summary>
    private void UpdateFamilyTreeDisplay()
    {
        if (content == null || leafWormUI == null) return;

        try
        {
            // 기존 노드들 제거
            ClearExistingNodes();

            // 현재 벌레가 있는 경우에만 처리
            if (currentWorm != null)
            {
                // 이전 세대들의 노드 생성
                CreateAncestorNodes();

                // 현재 벌레를 LeafWorm으로 설정
                UpdateCurrentWormDisplay();
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[WormFamilyManager] UI 업데이트 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 기존 노드들 제거
    /// </summary>
    private void ClearExistingNodes()
    {
        // 기존 노드들 제거 (LeafWorm 제외)
        var existingNodes = content.GetComponentsInChildren<WormNodeUI>();
        foreach (var node in existingNodes)
        {
            if (node != null && node.gameObject != leafWormUI.gameObject)
            {
                DestroyImmediate(node.gameObject);
            }
        }

        generationNodes.Clear();
    }

    /// <summary>
    /// 조상 노드들 생성
    /// </summary>
    private void CreateAncestorNodes()
    {
        if (nonLeafWormPrefab == null) return;

        // 현재 세대보다 낮은 세대들의 노드 생성
        var ancestorWorms = familyHistory.Where(w => w.generation < currentWorm.generation).ToList();

        foreach (var ancestor in ancestorWorms)
        {
            CreateWormNode(ancestor);
        }
    }

    /// <summary>
    /// 벌레 노드 생성
    /// </summary>
    private void CreateWormNode(WormData wormData)
    {
        if (nonLeafWormPrefab == null || wormData == null) return;

        try
        {
            // 노드 생성
            GameObject node = Instantiate(nonLeafWormPrefab, content);
            node.name = $"WormNode_Gen{wormData.generation}";

            // WormNodeUI 컴포넌트 설정
            WormNodeUI nodeUI = node.GetComponent<WormNodeUI>();
            if (nodeUI != null)
            {
                nodeUI.SetData(wormData);
                nodeUI.SetDisplayOptions(showDeathStatus, showLifeStage, showRarity);
                
                // 세대별 노드 저장
                generationNodes[wormData.generation] = nodeUI;
            }

            // 위치 설정 (세대 순으로)
            node.transform.SetSiblingIndex(wormData.generation);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[WormFamilyManager] 노드 생성 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 현재 벌레 표시 업데이트
    /// </summary>
    private void UpdateCurrentWormDisplay()
    {
        if (leafWormUI == null || currentWorm == null) return;

        try
        {
            // LeafWorm 데이터 설정
            leafWormUI.SetData(currentWorm);
            leafWormUI.SetDisplayOptions(showDeathStatus, showLifeStage, showRarity);

            // LeafWorm을 맨 아래로 이동
            leafWormUI.transform.SetParent(content, false);
            leafWormUI.transform.SetSiblingIndex(currentWorm.generation);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[WormFamilyManager] 현재 벌레 표시 업데이트 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 특정 세대의 벌레 가져오기
    /// </summary>
    public WormData GetWormByGeneration(int generation)
    {
        return familyHistory.FirstOrDefault(w => w.generation == generation);
    }

    /// <summary>
    /// 특정 세대의 노드 UI 가져오기
    /// </summary>
    public WormNodeUI GetNodeByGeneration(int generation)
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
        info.AppendLine($"현재 벌레: {currentWorm?.DisplayName ?? "없음"}");
        
        if (familyHistory.Count > 0)
        {
            info.AppendLine();
            info.AppendLine("[세대별 벌레]");
            foreach (var worm in familyHistory.OrderBy(w => w.generation))
            {
                string status = worm.IsAlive ? "생존" : "사망";
                info.AppendLine($"세대 {worm.generation}: {worm.DisplayName} ({status})");
            }
        }

        return info.ToString();
    }

    /// <summary>
    /// 가계도 초기화
    /// </summary>
    public void ClearFamilyTree()
    {
        familyHistory.Clear();
        currentWorm = null;
        ClearExistingNodes();
        
        LogDebug("[WormFamilyManager] 가계도 초기화");
    }

    /// <summary>
    /// 표시 옵션 설정
    /// </summary>
    public void SetDisplayOptions(bool showDeath, bool showLifeStage, bool showRarity)
    {
        showDeathStatus = showDeath;
        this.showLifeStage = showLifeStage;
        this.showRarity = showRarity;
        
        // 모든 노드 업데이트
        UpdateAllNodeDisplays();
    }

    /// <summary>
    /// 모든 노드 표시 업데이트
    /// </summary>
    private void UpdateAllNodeDisplays()
    {
        // 조상 노드들 업데이트
        foreach (var node in generationNodes.Values)
        {
            if (node != null)
            {
                node.SetDisplayOptions(showDeathStatus, showLifeStage, showRarity);
            }
        }

        // 현재 벌레 노드 업데이트
        if (leafWormUI != null)
        {
            leafWormUI.SetDisplayOptions(showDeathStatus, showLifeStage, showRarity);
        }
    }

    /// <summary>
    /// 애니메이션 설정 변경
    /// </summary>
    public void SetAnimationEnabled(bool enabled)
    {
        enableAnimations = enabled;
        LogDebug($"[WormFamilyManager] 애니메이션 {(enabled ? "활성화" : "비활성화")}");
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
        // 이벤트 초기화
        OnFamilyTreeUpdatedEvent = null;
        OnGenerationAddedEvent = null;
    }
}
