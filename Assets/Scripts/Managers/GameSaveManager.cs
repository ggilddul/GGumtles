using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;
using System.Collections;
using System.Linq;
using GGumtles.Data;
using GGumtles.Managers;

namespace GGumtles.Managers
{
    public class GameSaveManager : MonoBehaviour
{
    public static GameSaveManager Instance { get; private set; }

    public GameSaveData currentSaveData;
    private string saveFilePath;
    private string backupFilePath;
    private const int MAX_BACKUP_COUNT = 3;

    [Header("자동 저장 설정")]
    [SerializeField] private bool enableAutoSave = true;
    [SerializeField] private float autoSaveInterval = 1f; // 1초마다 자동 저장
    private Coroutine autoSaveCoroutine;

    public delegate void OnGameSaveDataLoaded();
    public event OnGameSaveDataLoaded OnGameSaveDataLoadedEvent;

    private void Awake()
    {
        InitializeSingleton();
    }

    private void InitializeSingleton()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            saveFilePath = Path.Combine(Application.persistentDataPath, "gamesave.json");
            backupFilePath = Path.Combine(Application.persistentDataPath, "gamesave_backup.json");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 저장 데이터 초기화 및 로드
    /// </summary>
    public void Initialize()
    {
        LoadGame();
        OnGameSaveDataLoadedEvent?.Invoke();
        
        // 자동 저장 시작
        StartAutoSave();
    }

    /// <summary>
    /// 저장 데이터 로드 또는 신규 생성
    /// </summary>
    private void LoadGame()
    {
        try
        {
            if (File.Exists(saveFilePath))
            {
                // 파일 크기 확인 (손상된 파일 감지)
                FileInfo fileInfo = new FileInfo(saveFilePath);
                if (fileInfo.Length == 0)
                {
                    Debug.LogWarning("[GameSaveManager] 저장 파일이 비어있습니다. 백업에서 복원 시도");
                    if (!TryLoadFromBackup())
                    {
                        CreateNewSaveData();
                    }
                    return;
                }
                
                string json = File.ReadAllText(saveFilePath);
                
                // JSON 유효성 검사
                if (string.IsNullOrEmpty(json) || json.Trim().Length == 0)
                {
                    Debug.LogWarning("[GameSaveManager] 저장 파일 내용이 비어있습니다. 백업에서 복원 시도");
                    if (!TryLoadFromBackup())
                    {
                        CreateNewSaveData();
                    }
                    return;
                }
                
                currentSaveData = JsonUtility.FromJson<GameSaveData>(json);
                
                // 데이터 검증
                if (ValidateSaveData(currentSaveData))
                {
                    Debug.Log("[GameSaveManager] 기존 저장 데이터 로드 완료");
                }
                else
                {
                    Debug.LogWarning("[GameSaveManager] 저장 데이터 검증 실패. 백업에서 복원 시도");
                    if (!TryLoadFromBackup())
                    {
                        CreateNewSaveData();
                    }
                }
            }
            else
            {
                Debug.Log("[GameSaveManager] 저장 데이터 없음. 새로 생성합니다.");
                CreateNewSaveData();
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[GameSaveManager] 로드 중 오류: {ex.Message}");
            Debug.LogError($"[GameSaveManager] 손상된 파일 삭제 시도: {saveFilePath}");
            
            // 손상된 파일 삭제
            try
            {
                if (File.Exists(saveFilePath))
                {
                    File.Delete(saveFilePath);
                    Debug.Log("[GameSaveManager] 손상된 파일 삭제 완료");
                }
            }
            catch (Exception deleteEx)
            {
                Debug.LogError($"[GameSaveManager] 파일 삭제 실패: {deleteEx.Message}");
            }
            
            if (!TryLoadFromBackup())
            {
                CreateNewSaveData();
            }
        }

        InitializeAchievementSystem();
    }

    /// <summary>
    /// 저장 데이터 유효성 검사
    /// </summary>
    private bool ValidateSaveData(GameSaveData data)
    {
        if (data == null) return false;
        
        // 기본 데이터 검증
        if (data.acornCount < 0 || data.diamondCount < 0) return false;
        if (data.selectedMapIndex < 0) return false;
        if (!System.Enum.IsDefined(typeof(GameSaveData.AudioOption), data.sfxOption)) return false;
        if (!System.Enum.IsDefined(typeof(GameSaveData.AudioOption), data.bgmOption)) return false;
        
        // 리스트 null 체크
        if (data.wormList == null || data.ownedItemIds == null || data.unlockedAchIds == null) return false;
        
        return true;
    }

    /// <summary>
    /// 백업에서 데이터 복원 시도
    /// </summary>
    private bool TryLoadFromBackup()
    {
        try
        {
            if (File.Exists(backupFilePath))
            {
                string json = File.ReadAllText(backupFilePath);
                currentSaveData = JsonUtility.FromJson<GameSaveData>(json);
                
                if (ValidateSaveData(currentSaveData))
                {
                    Debug.Log("[GameSaveManager] 백업에서 데이터 복원 성공");
                    return true;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[GameSaveManager] 백업 복원 실패: {ex.Message}");
        }
        
        return false;
    }

    /// <summary>
    /// 새로운 저장 데이터 생성
    /// </summary>
    private void CreateNewSaveData()
    {
        currentSaveData = new GameSaveData();
        InitializeNewSaveData();
    }

    /// <summary>
    /// 업적 시스템 초기화
    /// </summary>
    private void InitializeAchievementSystem()
    {
        if (AchievementManager.Instance != null)
        {
            // AchievementManager가 초기화되지 않은 경우 먼저 초기화
            if (!AchievementManager.Instance.IsInitialized)
            {
                AchievementManager.Instance.Initialize();
            }
            
            if (currentSaveData.unlockedAchIds != null)
            {
                // 새로운 Initialize 메서드 사용 (두 매개변수 모두 전달)
                AchievementManager.Instance.Initialize(currentSaveData.unlockedAchIds, currentSaveData.achievementWormIds);
            }
        }
        else
        {
            Debug.LogWarning("[GameSaveManager] AchievementManager 인스턴스가 없습니다.");
        }
    }


    /// <summary>
    /// 신규 저장 데이터 기본값 설정
    /// </summary>
    private void InitializeNewSaveData()
    {
        currentSaveData.totalPlayTime = 0f;
        currentSaveData.acornCount = 0;
        currentSaveData.diamondCount = 0;
        currentSaveData.wormList = new List<WormData>();
        currentSaveData.ownedItemIds = new List<string>();
        currentSaveData.selectedMapIndex = 0;

        // 업적 아이디 리스트 초기화 (빈 리스트)
        currentSaveData.unlockedAchIds = new List<string>();
        currentSaveData.achievementWormIds = new List<AchievementWormData>();

        currentSaveData.sfxOption = GameSaveData.AudioOption.High;
        currentSaveData.bgmOption = GameSaveData.AudioOption.High;

        // 초기 아이템 추가 (ItemManager 초기화 후에 처리)
        // ItemManager가 초기화되지 않은 상태에서는 건너뛰고, 나중에 LoadingManager에서 처리

        // SaveGame() 호출 제거 - 무한 루프 방지
        // LoadingManager에서 모든 초기화가 완료된 후 SaveGame이 호출됨
    }

    /// <summary>
    /// 현재 저장 데이터를 파일로 저장
    /// </summary>
    public void SaveGame()
    {
        if (currentSaveData == null)
        {
            Debug.LogWarning("[GameSaveManager] 저장 실패: currentSaveData가 null입니다.");
            return;
        }

        // 업적 상태 갱신
        if (AchievementManager.Instance != null)
        {
            currentSaveData.unlockedAchIds = GetUnlockedAchIds();
            currentSaveData.achievementWormIds = GetAchievementWormIds();
        }

        // 웜 데이터 갱신
        if (WormManager.Instance != null && WormManager.Instance.IsInitialized)
        {
            currentSaveData.wormList = WormManager.Instance.GetAllWorms();
            Debug.Log($"[GameSaveManager] 웜 데이터 갱신 - 웜 수: {currentSaveData.wormList.Count}");
        }
        else
        {
            // WormManager가 초기화되지 않은 경우 빈 리스트로 설정
            currentSaveData.wormList = new List<WormData>();
            Debug.Log("[GameSaveManager] WormManager가 초기화되지 않음 - 빈 웜 리스트로 설정");
        }

        // 아이템 데이터 갱신
        if (ItemManager.Instance != null && ItemManager.Instance.IsInitialized)
        {
            currentSaveData.ownedItemIds = ItemManager.Instance.GetOwnedItems().Select(item => item.itemId).ToList();
            
            // 착용 아이템 정보 저장
            var equippedItems = ItemManager.Instance.GetEquippedItems();
            currentSaveData.equippedHatId = equippedItems.hatId;
            currentSaveData.equippedFaceId = equippedItems.faceId;
            currentSaveData.equippedCostumeId = equippedItems.costumeId;
            
            Debug.Log($"[GameSaveManager] 아이템 데이터 갱신 - 아이템 수: {currentSaveData.ownedItemIds.Count}");
        }
        else
        {
            // ItemManager가 초기화되지 않은 경우 빈 리스트로 설정
            currentSaveData.ownedItemIds = new List<string>();
            currentSaveData.equippedHatId = "";
            currentSaveData.equippedFaceId = "";
            currentSaveData.equippedCostumeId = "";
            Debug.Log("[GameSaveManager] ItemManager가 초기화되지 않음 - 빈 아이템 리스트로 설정");
        }

        StartCoroutine(SaveGameAsync());
    }

    /// <summary>
    /// 비동기 저장 처리
    /// </summary>
    private IEnumerator SaveGameAsync()
    {
        yield return new WaitForEndOfFrame(); // UI 블로킹 방지

        try
        {
            // 백업 생성
            if (File.Exists(saveFilePath))
            {
                File.Copy(saveFilePath, backupFilePath, true);
            }

            string json = JsonUtility.ToJson(currentSaveData, true);
            File.WriteAllText(saveFilePath, json);
            
            Debug.Log("[GameSaveManager] 저장 완료");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[GameSaveManager] 저장 중 오류: {ex.Message}");
            
            // 백업에서 복원 시도
            if (File.Exists(backupFilePath))
            {
                try
                {
                    File.Copy(backupFilePath, saveFilePath, true);
                    Debug.Log("[GameSaveManager] 백업에서 복원 완료");
                }
                catch (Exception restoreEx)
                {
                    Debug.LogError($"[GameSaveManager] 백업 복원 실패: {restoreEx.Message}");
                }
            }
        }
    }

    /// <summary>
    /// 현재 해금된 업적 ID 리스트 반환
    /// </summary>
    public List<string> GetUnlockedAchIds()
    {
        List<string> unlockedIds = new List<string>();

        if (AchievementManager.Instance == null)
        {
            Debug.LogWarning("[GameSaveManager] AchievementManager 인스턴스가 null입니다.");
            return unlockedIds;
        }

        var definitions = AchievementManager.Instance.GetAllDefinitions();
        foreach (var def in definitions)
        {
            if (AchievementManager.Instance.IsUnlocked(def.achievementId))
            {
                unlockedIds.Add(def.achievementId);
            }
        }

        return unlockedIds;
    }

    /// <summary>
    /// 달성 웜 ID 리스트 반환
    /// </summary>
    public List<AchievementWormData> GetAchievementWormIds()
    {
        if (AchievementManager.Instance == null)
        {
            Debug.LogWarning("[GameSaveManager] AchievementManager 인스턴스가 null입니다.");
            return new List<AchievementWormData>();
        }

        try
        {
            // AchievementManager에서 직접 List<AchievementWormData> 반환
            return AchievementManager.Instance.GetAchievementWormIds();
        }
        catch (Exception ex)
        {
            Debug.LogError($"[GameSaveManager] 달성 웜 ID 가져오기 중 오류: {ex.Message}");
            return new List<AchievementWormData>();
        }
    }

    /// <summary>
    /// 현재 다이아몬드 개수 반환
    /// </summary>
    public int GetDiamondCount()
    {
        return currentSaveData != null ? currentSaveData.diamondCount : 0;
    }

    /// <summary>
    /// 저장된 벌레 데이터 반환
    /// </summary>
    public List<WormData> GetWormData()
    {
        var wormList = currentSaveData?.wormList ?? new List<WormData>();
        Debug.Log($"[GameSaveManager] GetWormData 호출 - 웜 수: {wormList.Count}, currentSaveData null: {currentSaveData == null}");
        
        if (currentSaveData != null && currentSaveData.wormList != null)
        {
            for (int i = 0; i < currentSaveData.wormList.Count; i++)
            {
                var worm = currentSaveData.wormList[i];
                Debug.Log($"[GameSaveManager] 웜 {i}: ID={worm?.wormId}, Name={worm?.name}, Generation={worm?.generation}");
            }
        }
        
        return wormList;
    }

    /// <summary>
    /// 저장 데이터 백업 생성
    /// </summary>
    public void CreateBackup()
    {
        try
        {
            if (File.Exists(saveFilePath))
            {
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string backupPath = Path.Combine(Application.persistentDataPath, $"gamesave_backup_{timestamp}.json");
                File.Copy(saveFilePath, backupPath, true);
                Debug.Log($"[GameSaveManager] 백업 생성 완료: {backupPath}");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[GameSaveManager] 백업 생성 실패: {ex.Message}");
        }
    }

    /// <summary>
    /// 자동 저장 시작
    /// </summary>
    private void StartAutoSave()
    {
        if (!enableAutoSave) return;
        
        StopAutoSave(); // 기존 자동 저장 중지
        
        autoSaveCoroutine = StartCoroutine(AutoSaveCoroutine());
        Debug.Log($"[GameSaveManager] 자동 저장 시작 - 간격: {autoSaveInterval}초");
    }

    /// <summary>
    /// 자동 저장 중지
    /// </summary>
    private void StopAutoSave()
    {
        if (autoSaveCoroutine != null)
        {
            StopCoroutine(autoSaveCoroutine);
            autoSaveCoroutine = null;
            Debug.Log("[GameSaveManager] 자동 저장 중지");
        }
    }

    /// <summary>
    /// 자동 저장 코루틴
    /// </summary>
    private IEnumerator AutoSaveCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(autoSaveInterval);
            
            if (currentSaveData != null)
            {
                SaveGame();
                Debug.Log($"[GameSaveManager] 자동 저장 완료 - {DateTime.Now:HH:mm:ss}");
            }
        }
    }

    /// <summary>
    /// 자동 저장 설정 변경
    /// </summary>
    public void SetAutoSave(bool enabled, float interval = 1f)
    {
        enableAutoSave = enabled;
        autoSaveInterval = interval;
        
        if (enabled)
        {
            StartAutoSave();
        }
        else
        {
            StopAutoSave();
        }
        
        Debug.Log($"[GameSaveManager] 자동 저장 설정 변경 - 활성화: {enabled}, 간격: {interval}초");
    }

    /// <summary>
    /// 게임 종료 시 강제 저장
    /// </summary>
    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            Debug.Log("[GameSaveManager] 게임 일시정지 - 강제 저장");
            SaveGame();
        }
    }

    /// <summary>
    /// 게임 포커스 잃을 때 강제 저장
    /// </summary>
    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
        {
            Debug.Log("[GameSaveManager] 게임 포커스 상실 - 강제 저장");
            SaveGame();
        }
    }

    /// <summary>
    /// 게임 종료 시 강제 저장
    /// </summary>
    private void OnApplicationQuit()
    {
        Debug.Log("[GameSaveManager] 게임 종료 - 강제 저장");
        StopAutoSave();
        SaveGame();
    }

    /// <summary>
    /// 보유 아이템 ID 목록 반환
    /// </summary>
    public List<string> GetOwnedItemIds()
    {
        return currentSaveData?.ownedItemIds ?? new List<string>();
    }

    /// <summary>
    /// 오브젝트 파괴 시 자동 저장 중지
    /// </summary>
    private void OnDestroy()
    {
        StopAutoSave();
    }
    }
}
