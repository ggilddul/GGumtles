using UnityEngine;
using GGumtles.Managers;

namespace GGumtles.Managers
{
    /// <summary>
    /// 미니게임 관리자 - GameUI와 MainUI 간의 전환 및 게임 타입 패널 관리
    /// </summary>
    public class MinigameManager : MonoBehaviour
{
    public static MinigameManager Instance { get; private set; }
    
    [Header("UI References")]
    [SerializeField] private GameObject mainUI;
    [SerializeField] private GameObject gameUI;
    
    [Header("Game Type Panel Prefabs")]
    [SerializeField] private GameObject gameType1PanelPrefab;
    [SerializeField] private GameObject gameType2PanelPrefab;
    [SerializeField] private GameObject gameType3PanelPrefab;
    [SerializeField] private GameObject gameType4PanelPrefab;
    
    private GameObject currentGamePanel;
    
    private void Awake()
    {
        // Singleton 패턴
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
    
    private void Start()
    {
        FindUIParents();
    }
    
    /// <summary>
    /// UI 부모 오브젝트들 자동 찾기
    /// </summary>
    private void FindUIParents()
    {
        if (mainUI == null)
        {
            mainUI = GameObject.Find("MainUI");
        }
        
        if (gameUI == null)
        {
            gameUI = GameObject.Find("GameUI");
        }
        
        if (mainUI == null)
        {
            Debug.LogError("[MinigameManager] MainUI를 찾을 수 없습니다!");
        }
        
        if (gameUI == null)
        {
            Debug.LogError("[MinigameManager] GameUI를 찾을 수 없습니다!");
        }
    }
    
    /// <summary>
    /// 게임 시작 (ReusableButton에서 호출)
    /// </summary>
    /// <param name="gameType">게임 타입 (1-4)</param>
    public void StartGame(int gameType)
    {
        if (gameType < 1 || gameType > 4)
        {
            Debug.LogError($"[MinigameManager] 잘못된 게임 타입: {gameType} (1-4 범위여야 함)");
            return;
        }
        
        Debug.Log($"[MinigameManager] 게임 시작 - GameType: {gameType}");
        
        // MainUI 비활성화
        if (mainUI != null)
        {
            mainUI.SetActive(false);
            Debug.Log("[MinigameManager] MainUI 비활성화");
        }
        
        // GameUI 활성화
        if (gameUI != null)
        {
            gameUI.SetActive(true);
            Debug.Log("[MinigameManager] GameUI 활성화");
        }
        
        // TopBar를 GameState로 변경
        if (TopBarManager.Instance != null)
        {
            TopBarManager.Instance.SetTopBarType(TopBarManager.TopBarType.GameState);
            Debug.Log("[MinigameManager] TopBar를 GameState로 변경");
        }
        
        // 해당 게임 타입 패널 생성
        CreateGameTypePanel(gameType);
    }
    
    /// <summary>
    /// 게임 종료 - MainUI로 돌아가기 (ReusableButton에서 호출)
    /// </summary>
    public void QuitGame()
    {
        Debug.Log("[MinigameManager] 게임 종료 - MainUI로 돌아가기");
        
        // 현재 활성화된 게임 패널 제거
        DestroyCurrentGamePanel();
        
        // 게임 저장
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SaveGameData();
            Debug.Log("[MinigameManager] 게임 종료 시 자동 저장 완료");
        }
        
        // GameUI 비활성화
        if (gameUI != null)
        {
            gameUI.SetActive(false);
            Debug.Log("[MinigameManager] GameUI 비활성화");
        }
        
        // MainUI 활성화
        if (mainUI != null)
        {
            mainUI.SetActive(true);
            Debug.Log("[MinigameManager] MainUI 활성화");
        }
        
        // TopBar를 NonGameState로 변경
        if (TopBarManager.Instance != null)
        {
            TopBarManager.Instance.SetTopBarType(TopBarManager.TopBarType.NonGameState);
            Debug.Log("[MinigameManager] TopBar를 NonGameState로 변경");
        }
    }
    
    /// <summary>
    /// 게임 타입 패널 생성
    /// </summary>
    /// <param name="gameType">게임 타입 (1-4)</param>
    private void CreateGameTypePanel(int gameType)
    {
        // 기존 게임 패널 제거
        DestroyCurrentGamePanel();
        
        // 해당 게임 타입 패널 프리팹 찾기
        GameObject gameTypePanelPrefab = GetGameTypePanelPrefab(gameType);
        
        if (gameTypePanelPrefab != null && gameUI != null)
        {
            // GameUI 밑에 instantiate
            currentGamePanel = Instantiate(gameTypePanelPrefab, gameUI.transform);
            Debug.Log($"[MinigameManager] GameType{gameType}Panel instantiate 완료");
        }
        else
        {
            Debug.LogError($"[MinigameManager] GameType{gameType}Panel 프리팹을 찾을 수 없습니다!");
        }
    }
    
    /// <summary>
    /// 현재 활성화된 게임 패널 제거
    /// </summary>
    private void DestroyCurrentGamePanel()
    {
        if (currentGamePanel != null)
        {
            Destroy(currentGamePanel);
            currentGamePanel = null;
            Debug.Log("[MinigameManager] 현재 게임 패널 제거 완료");
        }
    }
    
    /// <summary>
    /// 게임 타입에 해당하는 프리팹 가져오기
    /// </summary>
    /// <param name="gameType">게임 타입 (1-4)</param>
    /// <returns>해당 게임 타입 프리팹</returns>
    private GameObject GetGameTypePanelPrefab(int gameType)
    {
        GameObject prefab = gameType switch
        {
            1 => gameType1PanelPrefab,
            2 => gameType2PanelPrefab,
            3 => gameType3PanelPrefab,
            4 => gameType4PanelPrefab,
            _ => null
        };
        
        if (prefab == null)
        {
            Debug.LogError($"[MinigameManager] GameType{gameType}Panel 프리팹이 Inspector에서 할당되지 않았습니다!");
            Debug.LogError("MinigameManager Inspector에서 해당 게임 타입 프리팹을 할당해주세요.");
        }
        
        return prefab;
    }
    
    /// <summary>
    /// 현재 게임이 실행 중인지 확인
    /// </summary>
    /// <returns>게임 실행 중 여부</returns>
    public bool IsGameRunning()
    {
        return currentGamePanel != null;
    }
    
    /// <summary>
    /// 현재 게임 타입 가져오기
    /// </summary>
    /// <returns>현재 게임 타입 (게임이 실행 중이 아니면 -1)</returns>
    public int GetCurrentGameType()
    {
        if (currentGamePanel == null) return -1;
        
        // 게임 패널 이름에서 타입 추출 (예: "GameType1Panel" -> 1)
        string panelName = currentGamePanel.name;
        if (panelName.Contains("GameType") && panelName.Contains("Panel"))
        {
            string typeStr = panelName.Replace("GameType", "").Replace("Panel", "").Replace("(Clone)", "");
            if (int.TryParse(typeStr, out int gameType))
            {
                return gameType;
            }
        }
        
        return -1;
    }
    }
}
