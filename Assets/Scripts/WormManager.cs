using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class WormManager : MonoBehaviour
{
    public static WormManager Instance { get; private set; }

    [Header("벌레 생성 설정")]
    [SerializeField] private bool enableAutoEvolution = true;
    [SerializeField] private bool enableAutoDeath = true;
    [SerializeField] private float evolutionCheckInterval = 1f; // 진화 체크 간격 (초)

    [Header("디버그 설정")]
    [SerializeField] private bool enableDebugLogs = true;
    [SerializeField] private bool showEvolutionNotifications = true;

    // 이름 생성용 배열
    private readonly string[] adjectives = new string[]
    {
        "반짝이는", "말랑말랑한", "재빠른", "작은", "따뜻한",
        "귀여운", "푸른", "부드러운", "신나는", "조용한",
        "빛나는", "무시무시한", "행복한", "장난꾸러기", "포근한",
        "대단한", "상큼한", "졸린", "용감한", "엄청난"
    };

    private readonly string[] nouns = new string[]
    {
        "꿈틀이", "꼬물이", "말랑이", "쫄랑이", "비비미",
        "꼬마", "토실이", "무지개", "나비", "별빛",
        "토끼", "햇살", "방울", "파랑새", "풍선",
        "별똥별", "구름", "사탕", "바람", "도토리"
    };

    // 벌레 관리
    private Dictionary<int, WormData> wormDictionary;
    private List<WormData> wormList;
    private WormData currentWorm;
    private int nextWormId = 1;

    // 상태 관리
    private bool isInitialized = false;
    private bool isProcessing = false;
    private float lastEvolutionCheck = 0f;

    // 이벤트 정의
    public delegate void OnWormCreated(WormData worm);
    public event OnWormCreated OnWormCreatedEvent;

    public delegate void OnWormEvolved(WormData worm, int fromStage, int toStage);
    public event OnWormEvolved OnWormEvolvedEvent;

    public delegate void OnWormDied(WormData worm, string reason);
    public event OnWormDied OnWormDiedEvent;

    public delegate void OnCurrentWormChanged(WormData previousWorm, WormData newWorm);
    public event OnCurrentWormChanged OnCurrentWormChangedEvent;

    // 프로퍼티
    public WormData CurrentWorm => currentWorm;
    public int TotalWorms => wormDictionary?.Count ?? 0;
    public bool HasWorms => TotalWorms > 0;
    public bool IsInitialized => isInitialized;

    private void Awake()
    {
        InitializeSingleton();
    }

    private void Start()
    {
        InitializeWormSystem();
    }

    private void Update()
    {
        if (!isInitialized || isProcessing) return;

        // 주기적 진화 체크
        if (enableAutoEvolution && Time.time - lastEvolutionCheck >= evolutionCheckInterval)
        {
            CheckEvolution();
            lastEvolutionCheck = Time.time;
        }
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

    private void InitializeWormSystem()
    {
        try
        {
            wormDictionary = new Dictionary<int, WormData>();
            wormList = new List<WormData>();
            currentWorm = null;
            nextWormId = 1;
            isInitialized = true;

            LogDebug("[WormManager] 벌레 시스템 초기화 완료");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[WormManager] 초기화 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 저장된 벌레 목록으로 초기화
    /// </summary>
    public void Initialize(List<WormData> savedWormList)
    {
        if (!isInitialized)
        {
            Debug.LogWarning("[WormManager] 아직 초기화되지 않았습니다.");
            return;
        }

        try
        {
            ClearAllWorms();

            if (savedWormList != null && savedWormList.Count > 0)
            {
                foreach (var worm in savedWormList)
                {
                    if (worm != null && worm.IsValid)
                    {
                        AddWorm(worm);
                    }
                }

                // 가장 최근 벌레를 현재 벌레로 설정
                SetCurrentWorm(wormList.LastOrDefault());
            }

            LogDebug($"[WormManager] {savedWormList?.Count ?? 0}개의 저장된 벌레 로드 완료");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[WormManager] 초기화 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 랜덤 벌레 이름 생성
    /// </summary>
    public string GenerateRandomWormName()
    {
        string adjective = RandomManager.GetRandomElement(adjectives);
        string noun = RandomManager.GetRandomElement(nouns);
        return $"{adjective} {noun}";
    }

    /// <summary>
    /// 새 벌레 생성
    /// </summary>
    public WormData CreateNewWorm(int generation = 1)
    {
        if (!isInitialized)
        {
            Debug.LogWarning("[WormManager] 아직 초기화되지 않았습니다.");
            return null;
        }

        try
        {
            isProcessing = true;

            // 새 벌레 생성
            var newWorm = new WormData(nextWormId, GenerateRandomWormName());
            newWorm.generation = generation;
            newWorm.lifespan = RandomManager.GenerateWormLifespan();

            // 기본 아이템 설정
            newWorm.hatItemId = "100";
            newWorm.faceItemId = "200";
            newWorm.costumeItemId = "300";

            // 벌레 추가
            AddWorm(newWorm);

            // 현재 벌레로 설정
            SetCurrentWorm(newWorm);

            // 가족 관계 설정
            if (generation > 1 && currentWorm != null)
            {
                SetWormParents(newWorm, currentWorm);
            }

            LogDebug($"[WormManager] 새 벌레 생성: {newWorm.DisplayName} (세대: {generation})");

            // 이벤트 발생
            OnWormCreatedEvent?.Invoke(newWorm);

            // 팝업 표시 (EggPopup는 현재 구현되지 않음)
            // PopupManager.Instance?.OpenEggPopup();

            return newWorm;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[WormManager] 벌레 생성 중 오류: {ex.Message}");
            return null;
        }
        finally
        {
            isProcessing = false;
        }
    }

    /// <summary>
    /// 벌레 추가
    /// </summary>
    private void AddWorm(WormData worm)
    {
        if (worm == null || !worm.IsValid) return;

        wormDictionary[worm.wormId] = worm;
        wormList.Add(worm);

        // 다음 ID 업데이트
        if (worm.wormId >= nextWormId)
        {
            nextWormId = worm.wormId + 1;
        }
    }

    /// <summary>
    /// 현재 벌레 설정
    /// </summary>
    public void SetCurrentWorm(WormData worm)
    {
        if (worm == null || !wormDictionary.ContainsKey(worm.wormId))
        {
            Debug.LogWarning("[WormManager] 유효하지 않은 벌레를 현재 벌레로 설정할 수 없습니다.");
            return;
        }

        var previousWorm = currentWorm;
        currentWorm = worm;

        LogDebug($"[WormManager] 현재 벌레 변경: {previousWorm?.DisplayName ?? "없음"} → {currentWorm.DisplayName}");

        // 이벤트 발생
        OnCurrentWormChangedEvent?.Invoke(previousWorm, currentWorm);
    }

    /// <summary>
    /// 벌레 부모 설정
    /// </summary>
    private void SetWormParents(WormData child, WormData parent)
    {
        if (child == null || parent == null) return;

        child.parentId1 = parent.wormId;
        child.parentId2 = -1; // 단일 부모 (필요시 수정)
        parent.AddChild(child.wormId);

        LogDebug($"[WormManager] 부모-자식 관계 설정: {parent.DisplayName} → {child.DisplayName}");
    }

    /// <summary>
    /// 진화 체크
    /// </summary>
    public void CheckEvolution()
    {
        if (currentWorm == null || !currentWorm.IsAlive) return;

        try
        {
            int previousStage = currentWorm.lifeStage;
            
            // 나이 증가 (1분)
            currentWorm.AgeUp(1);

            // 생명주기 변경 확인
            if (currentWorm.lifeStage != previousStage)
            {
                HandleWormEvolved(currentWorm, previousStage, currentWorm.lifeStage);
            }

            // 사망 확인
            if (currentWorm.IsDead)
            {
                HandleWormDied(currentWorm, "자연사");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[WormManager] 진화 체크 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 벌레 진화 시 호출
    /// </summary>
    private void HandleWormEvolved(WormData worm, int fromStage, int toStage)
    {
        LogDebug($"[WormManager] 벌레 진화: {worm.DisplayName} ({fromStage} → {toStage})");

        // 이벤트 발생
        OnWormEvolvedEvent?.Invoke(worm, fromStage, toStage);

        // 알림 표시
        if (showEvolutionNotifications)
        {
            PopupManager.Instance?.OpenEvolvePopup(worm);
        }

        // 사운드 재생
        AudioManager.Instance?.PlayButtonSound(6);
    }

    /// <summary>
    /// 벌레 사망 시 호출
    /// </summary>
    private void HandleWormDied(WormData worm, string reason)
    {
        LogDebug($"[WormManager] 벌레 사망: {worm.DisplayName} (사유: {reason})");

        // 이벤트 발생
        OnWormDiedEvent?.Invoke(worm, reason);

        // 팝업 표시
        PopupManager.Instance?.OpenDiePopup(worm);

        // 새 벌레 생성 (세대 증가)
        if (enableAutoDeath)
        {
            CreateNewWorm(worm.generation + 1);
        }
    }

    /// <summary>
    /// 벌레 ID로 벌레 가져오기
    /// </summary>
    public WormData GetWormById(int wormId)
    {
        return wormDictionary.TryGetValue(wormId, out var worm) ? worm : null;
    }

    /// <summary>
    /// 현재 벌레 가져오기
    /// </summary>
    public WormData GetCurrentWorm()
    {
        return currentWorm;
    }

    /// <summary>
    /// 모든 벌레 가져오기
    /// </summary>
    public List<WormData> GetAllWorms()
    {
        return new List<WormData>(wormList);
    }

    /// <summary>
    /// 특정 세대의 벌레들 가져오기
    /// </summary>
    public List<WormData> GetWormsByGeneration(int generation)
    {
        return wormList.Where(w => w.generation == generation).ToList();
    }

    /// <summary>
    /// 살아있는 벌레들 가져오기
    /// </summary>
    public List<WormData> GetAliveWorms()
    {
        return wormList.Where(w => w.IsAlive).ToList();
    }

    /// <summary>
    /// 벌레 제거
    /// </summary>
    public bool RemoveWorm(int wormId)
    {
        if (!wormDictionary.TryGetValue(wormId, out var worm))
        {
            return false;
        }

        wormDictionary.Remove(wormId);
        wormList.Remove(worm);

        // 현재 벌레가 제거된 경우 다른 벌레로 변경
        if (currentWorm?.wormId == wormId)
        {
            SetCurrentWorm(wormList.LastOrDefault());
        }

        LogDebug($"[WormManager] 벌레 제거: {worm.DisplayName}");

        return true;
    }

    /// <summary>
    /// 모든 벌레 제거
    /// </summary>
    public void ClearAllWorms()
    {
        wormDictionary.Clear();
        wormList.Clear();
        currentWorm = null;
        nextWormId = 1;

        LogDebug("[WormManager] 모든 벌레 제거");
    }

    /// <summary>
    /// 자동 진화 설정 변경
    /// </summary>
    public void SetAutoEvolution(bool enabled)
    {
        enableAutoEvolution = enabled;
        LogDebug($"[WormManager] 자동 진화 {(enabled ? "활성화" : "비활성화")}");
    }

    /// <summary>
    /// 자동 사망 설정 변경
    /// </summary>
    public void SetAutoDeath(bool enabled)
    {
        enableAutoDeath = enabled;
        LogDebug($"[WormManager] 자동 사망 {(enabled ? "활성화" : "비활성화")}");
    }

    /// <summary>
    /// 벌레 통계 정보 반환
    /// </summary>
    public string GetWormStatistics()
    {
        if (!isInitialized) return "벌레 시스템이 초기화되지 않았습니다.";

        var stats = new System.Text.StringBuilder();
        stats.AppendLine($"[벌레 통계]");
        stats.AppendLine($"총 벌레 수: {TotalWorms}마리");
        stats.AppendLine($"살아있는 벌레: {GetAliveWorms().Count}마리");
        stats.AppendLine($"현재 벌레: {currentWorm?.DisplayName ?? "없음"}");
        
        if (currentWorm != null)
        {
            stats.AppendLine($"현재 세대: {currentWorm.generation}");
            stats.AppendLine($"현재 생명주기: {currentWorm.lifeStage}/6");
            stats.AppendLine($"나이: {currentWorm.AgeInDays:F1}일");
            stats.AppendLine($"수명: {currentWorm.lifespan / 1440f:F1}일");
        }

        return stats.ToString();
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
        OnWormCreatedEvent = null;
        OnWormEvolvedEvent = null;
        OnWormDiedEvent = null;
        OnCurrentWormChangedEvent = null;
    }
}

