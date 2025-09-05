using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class WormManager : MonoBehaviour
{
    public static WormManager Instance { get; private set; }

    [Header("벌레 생성 설정")]
    [SerializeField] private bool enableAutoEvolution = false; // 분 단위 이벤트로 대체
    [SerializeField] private bool enableAutoDeath = true;
    // [SerializeField] private float evolutionCheckInterval = 60f; // 미사용 (호환)

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
    private bool isSubscribedToGameTime = false;

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

    public void Initialize()
    {
        InitializeWormSystem();
    }

    private void Update()
    {
        // 진화는 GameManager의 분 변경 이벤트에서 처리함
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
            // 게임 시간 이벤트 구독 시도
            TrySubscribeToGameTime();
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
                    if (worm != null && worm.wormId >= 0)
                    {
                        AddWorm(worm);
                    }
                }

                // 가장 최근 벌레를 현재 벌레로 설정
                SetCurrentWorm(wormList.LastOrDefault());
                LogDebug($"[WormManager] {savedWormList.Count}개의 저장된 벌레 로드 완료");
            }
            else
            {
                // 저장된 벌레가 없으면 EggFound 팝업 띄우기
                LogDebug("[WormManager] 저장된 벌레가 없습니다. EggFound 팝업을 띄웁니다.");
                ShowEggFoundPopupForFirstTime();
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[WormManager] 초기화 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 최초 접속 시 EggFound 팝업 표시
    /// </summary>
    private void ShowEggFoundPopupForFirstTime()
    {
        try
        {
            // PopupManager가 준비될 때까지 대기
            if (PopupManager.Instance == null)
            {
                StartCoroutine(WaitForPopupManagerAndShowEggFound());
            }
            else
            {
                // EggFound 팝업 열기
                PopupManager.Instance.OpenPopup(PopupManager.PopupType.EggFound, PopupManager.PopupPriority.Critical);
                LogDebug("[WormManager] 최초 접속 - EggFound 팝업 표시");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[WormManager] EggFound 팝업 표시 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// PopupManager가 준비될 때까지 대기 후 EggFound 팝업 표시
    /// </summary>
    private System.Collections.IEnumerator WaitForPopupManagerAndShowEggFound()
    {
        while (PopupManager.Instance == null)
        {
            yield return new WaitForSeconds(0.1f);
        }

        // EggFound 팝업 열기
        PopupManager.Instance.OpenPopup(PopupManager.PopupType.EggFound, PopupManager.PopupPriority.Critical);
        LogDebug("[WormManager] 최초 접속 - EggFound 팝업 표시 (지연)");
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
            // 새 벌레 생성
            var newWorm = new WormData();
            newWorm.wormId = nextWormId;
            newWorm.name = GenerateRandomWormName();
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

            LogDebug($"[WormManager] 새 벌레 생성: {newWorm.name} (세대: {generation})");

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
    }

    /// <summary>
    /// 벌레 추가
    /// </summary>
    private void AddWorm(WormData worm)
    {
        if (worm == null || worm.wormId < 0) return;

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

                    LogDebug($"[WormManager] 현재 벌레 변경: {previousWorm?.name ?? "없음"} → {currentWorm.name}");

        // 이벤트 발생
        OnCurrentWormChangedEvent?.Invoke(previousWorm, currentWorm);
    }

    /// <summary>
    /// 벌레 부모 설정
    /// </summary>
    private void SetWormParents(WormData child, WormData parent)
    {
        if (child == null || parent == null) return;

        // 부모-자식 관계는 단순화하여 제거
        // child.parentId1 = parent.wormId;
        // child.parentId2 = -1;
        // parent.AddChild(child.wormId);

                    LogDebug($"[WormManager] 부모-자식 관계 설정: {parent.name} → {child.name}");
    }

    /// <summary>
    /// 진화 체크
    /// </summary>
    public void CheckEvolution()
    {
        // 하위 호환: 분 증가 없이 단계만 재평가
        IncrementAgeAndEvaluate(0);
    }

    /// <summary>
    /// 나이에 따른 생명주기 단계 계산
    /// </summary>
    private int GetLifeStageByAge(int age, int lifespan = 2880)
    {
        // 진화 임계값(분): 0(Egg) → 1:30 → 2:120 → 3:360 → 4:720 → 5:1440 → 6:수명
        if (age >= lifespan)
        {
            return 6; // 사망
        }
        else if (age >= 1440)
        {
            return 5;
        }
        else if (age >= 720)
        {
            return 4;
        }
        else if (age >= 360)
        {
            return 3;
        }
        else if (age >= 120)
        {
            return 2;
        }
        else if (age >= 30)
        {
            return 1;
        }
        else
        {
            return 0; // 알
        }
    }

    /// <summary>
    /// 특정 벌레의 진화 체크
    /// </summary>
    private void CheckEvolutionForWorm(WormData worm)
    {
        if (worm == null || !worm.isAlive) return;

        try
        {
            int previousStage = worm.lifeStage;
            if (previousStage == 6) return; // 이미 최종 단계

            // 나이에 따른 진화 체크
            int newStage = GetLifeStageByAge(worm.age, worm.lifespan);
            if (newStage > previousStage)
            {
                worm.lifeStage = newStage;
                HandleWormEvolved(worm, previousStage, newStage);
                LogDebug($"[WormManager] 벌레 진화: {worm.name} ({previousStage} → {newStage})");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[WormManager] 벌레 진화 체크 중 오류: {ex.Message}");
        }
    }

    private void IncrementAgeAndEvaluate(int minutes)
    {
        if (currentWorm == null || !currentWorm.isAlive) return;

        try
        {
            int previousStage = currentWorm.lifeStage;
            if (previousStage == 6) return;

            if (minutes > 0)
            {
                currentWorm.age += minutes;
            }

            // 수명 보정 및 고정 임계값 기반 단계 계산
            int lifespan = Mathf.Max(1, currentWorm.lifespan);
            int age = Mathf.Max(0, currentWorm.age);

            // 진화 임계값(분): 0(Egg) → 1:30 → 2:120 → 3:360 → 4:720 → 5:1440 → 6:수명
            int newStage = 0;
            if (age >= lifespan)
            {
                newStage = 6; // 사망
            }
            else if (age >= 1440)
            {
                newStage = 5;
            }
            else if (age >= 720)
            {
                newStage = 4;
            }
            else if (age >= 360)
            {
                newStage = 3;
            }
            else if (age >= 120)
            {
                newStage = 2;
            }
            else if (age >= 30)
            {
                newStage = 1;
            }

            if (newStage != previousStage)
            {
                currentWorm.lifeStage = newStage;

                if (newStage == 6)
                {
                    currentWorm.isAlive = false;
                    HandleWormDied(currentWorm, "자연사");
                }
                else
                {
                    HandleWormEvolved(currentWorm, previousStage, newStage);
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[WormManager] 단계 평가 중 오류: {ex.Message}");
        }
    }

    private void TrySubscribeToGameTime()
    {
        if (isSubscribedToGameTime) return;
        if (GameManager.Instance == null)
        {
            StartCoroutine(WaitAndSubscribeGameTime());
            return;
        }

        GameManager.Instance.OnGameTimeChangedEvent += OnGameTimeChanged;
        isSubscribedToGameTime = true;
    }

    private System.Collections.IEnumerator WaitAndSubscribeGameTime()
    {
        while (GameManager.Instance == null)
        {
            yield return null;
        }
        GameManager.Instance.OnGameTimeChangedEvent += OnGameTimeChanged;
        isSubscribedToGameTime = true;
    }

    private void OnGameTimeChanged(int hour, int minute, string ampm)
    {
        // 분 변경 이벤트마다 1분 증가로 가정 (GameManager에서 분 변화 시 호출됨)
        IncrementAgeAndEvaluate(1);
    }

    /// <summary>
    /// 벌레 진화 시 호출
    /// </summary>
    private void HandleWormEvolved(WormData worm, int fromStage, int toStage)
    {
        LogDebug($"[WormManager] 벌레 진화: {worm.name} ({fromStage} → {toStage})");
        
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
                    LogDebug($"[WormManager] 벌레 사망: {worm.name} (사유: {reason})");

        // 이벤트 발생
        OnWormDiedEvent?.Invoke(worm, reason);

        // 팝업 표시
        PopupManager.Instance?.OpenDiePopup(worm);

        // 사운드 재생
        AudioManager.Instance?.PlaySFX(AudioManager.SFXType.Error);

        // 자동 새 벌레 생성 비활성화 - 사용자가 WormDieConfirm 버튼을 눌러야 EggFound 팝업이 열림
        // if (enableAutoDeath)
        // {
        //     CreateNewWorm(worm.generation + 1);
        // }
    }

    /// <summary>
    /// 모든 벌레의 나이 증가
    /// </summary>
    public void AgeAllWorms()
    {
        if (!isInitialized)
        {
            Debug.LogWarning("[WormManager] 아직 초기화되지 않았습니다.");
            return;
        }

        try
        {
            var wormsToAge = new List<WormData>(wormDictionary.Values);
            
            foreach (var worm in wormsToAge)
            {
                if (worm != null)
                {
                    worm.age++;
                    LogDebug($"[WormManager] 벌레 나이 증가: {worm.name} -> {worm.age}일");
                    
                    // 진화 체크 (개별 벌레에 대해)
                    CheckEvolutionForWorm(worm);
                }
            }
            
            // 데이터 저장 (GameSaveManager를 통해)
            if (GameSaveManager.Instance != null)
            {
                GameSaveManager.Instance.SaveGame();
            }
            
            LogDebug($"[WormManager] 모든 벌레 나이 증가 완료: {wormsToAge.Count}마리");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[WormManager] 벌레 나이 증가 중 오류: {ex.Message}");
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
        return wormList.Where(w => w.isAlive).ToList();
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

                    LogDebug($"[WormManager] 벌레 제거: {worm.name}");

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
        stats.AppendLine($"현재 벌레: {currentWorm?.name ?? "없음"}");
        
        if (currentWorm != null)
        {
            stats.AppendLine($"현재 세대: {currentWorm.generation}");
            stats.AppendLine($"현재 생명주기: {currentWorm.lifeStage}/6");
            stats.AppendLine($"나이: {currentWorm.age / 1440f:F1}일");
            stats.AppendLine($"수명: {currentWorm.lifespan / 1440f:F1}일");
        }

        return stats.ToString();
    }

    /// <summary>
    /// 현재 웜에게 먹이주기
    /// </summary>
    public void FeedWorm()
    {
        if (currentWorm == null)
        {
            Debug.LogWarning("[WormManager] 현재 웜이 없습니다.");
            return;
        }

        try
        {
            // 웜의 통계 업데이트
            currentWorm.statistics.totalEatCount++;
            
            // 사운드 재생
            AudioManager.Instance?.PlaySFX(AudioManager.SFXType.EarnItem);
            
            if (enableDebugLogs)
                Debug.Log($"[WormManager] 웜 먹이주기 완료: {currentWorm.name}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[WormManager] 웜 먹이주기 중 오류: {ex.Message}");
        }
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

        if (isSubscribedToGameTime && GameManager.Instance != null)
        {
            GameManager.Instance.OnGameTimeChangedEvent -= OnGameTimeChanged;
            isSubscribedToGameTime = false;
        }
    }
}

