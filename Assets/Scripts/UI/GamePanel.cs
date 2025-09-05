using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 게임 패널 관리 - PlayTab에서 미니게임으로 전환
/// </summary>
public class GamePanel : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button gamePlayButton;
    
    [Header("Game Type Selection")]
    [SerializeField] private int gameTypeIndex = 1; // 1, 2, 3, 4 중 선택
    
    [Header("UI Parents")]
    [SerializeField] private GameObject mainUI;
    [SerializeField] private GameObject gameUI;
    
    private void Start()
    {
        SetupButton();
        FindUIParents();
    }
    
    /// <summary>
    /// 버튼 설정
    /// </summary>
    private void SetupButton()
    {
        // GamePlayButton 자동 찾기
        if (gamePlayButton == null)
        {
            gamePlayButton = GetComponentInChildren<Button>();
            if (gamePlayButton != null)
            {
                Debug.Log("[GamePanel] GamePlayButton 자동 찾기 완료");
            }
            else
            {
                Debug.LogError("[GamePanel] GamePlayButton을 찾을 수 없습니다!");
                return;
            }
        }
        
        if (gamePlayButton != null)
        {
            gamePlayButton.onClick.AddListener(OnGamePlayButtonClicked);
            Debug.Log($"[GamePanel] GamePlayButton 설정 완료 - GameType: {gameTypeIndex}");
        }
        else
        {
            Debug.LogError("[GamePanel] GamePlayButton이 설정되지 않았습니다!");
        }
    }
    
    /// <summary>
    /// GamePlayButton 클릭 시 호출
    /// </summary>
    private void OnGamePlayButtonClicked()
    {
        StartGame(gameTypeIndex);
    }
    
    /// <summary>
    /// 게임 시작 (ReusableButton에서 호출)
    /// </summary>
    public void StartGame(int gameType = -1)
    {
        // 게임 타입이 지정되지 않았으면 기본값 사용
        if (gameType > 0)
        {
            gameTypeIndex = gameType;
        }
        
        Debug.Log($"[GamePanel] 게임 시작 - GameType: {gameTypeIndex}");
        
        // MainUI 전체 비활성화
        if (mainUI != null)
        {
            mainUI.SetActive(false);
            Debug.Log("[GamePanel] MainUI 비활성화");
        }
        
        // GameUI 활성화
        if (gameUI != null)
        {
            gameUI.SetActive(true);
            Debug.Log("[GamePanel] GameUI 활성화");
        }
        
        // TopBar를 GameState로 변경
        if (TopBarManager.Instance != null)
        {
            TopBarManager.Instance.SetTopBarType(TopBarManager.TopBarType.GameState);
            Debug.Log("[GamePanel] TopBar를 GameState로 변경");
        }
        
        // GameUI 밑에 해당 게임 타입 패널 instantiate
        InstantiateGameTypePanel();
    }
    
    /// <summary>
    /// 게임 타입 설정 (Inspector에서 설정)
    /// </summary>
    public void SetGameType(int gameType)
    {
        if (gameType >= 1 && gameType <= 4)
        {
            gameTypeIndex = gameType;
            Debug.Log($"[GamePanel] 게임 타입 설정: {gameTypeIndex}");
        }
        else
        {
            Debug.LogError($"[GamePanel] 잘못된 게임 타입: {gameType} (1-4 범위여야 함)");
        }
    }
    
    /// <summary>
    /// GameUI 밑에 게임 타입 패널 instantiate
    /// </summary>
    private void InstantiateGameTypePanel()
    {
        if (gameUI == null)
        {
            Debug.LogError("[GamePanel] GameUI가 null입니다!");
            return;
        }
        
        // 기존 게임 타입 패널들 제거
        ClearExistingGameTypePanels();
        
        // 해당 게임 타입 패널 프리팹 찾기
        GameObject gameTypePanelPrefab = FindGameTypePanelPrefab(gameTypeIndex);
        
        if (gameTypePanelPrefab != null)
        {
            // GameUI 밑에 instantiate
            GameObject instantiatedPanel = Instantiate(gameTypePanelPrefab, gameUI.transform);
            Debug.Log($"[GamePanel] GameType{gameTypeIndex}Panel instantiate 완료");
        }
        else
        {
            Debug.LogError($"[GamePanel] GameType{gameTypeIndex}Panel 프리팹을 찾을 수 없습니다!");
        }
    }
    
    /// <summary>
    /// 기존 게임 타입 패널들 제거
    /// </summary>
    private void ClearExistingGameTypePanels()
    {
        if (gameUI == null) return;
        
        // GameUI 밑의 모든 GameType 패널들 제거
        for (int i = gameUI.transform.childCount - 1; i >= 0; i--)
        {
            Transform child = gameUI.transform.GetChild(i);
            if (child.name.Contains("GameType") && child.name.Contains("Panel"))
            {
                DestroyImmediate(child.gameObject);
                Debug.Log($"[GamePanel] 기존 패널 제거: {child.name}");
            }
        }
    }
    
    /// <summary>
    /// 게임 타입 패널 프리팹 찾기
    /// </summary>
    private GameObject FindGameTypePanelPrefab(int gameType)
    {
        string prefabName = $"GameType{gameType}Panel";
        
        // Resources 폴더에서 찾기
        GameObject resourcePrefab = Resources.Load<GameObject>($"Prefabs/{prefabName}");
        
        if (resourcePrefab == null)
        {
            Debug.LogError($"[GamePanel] {prefabName} 프리팹을 찾을 수 없습니다!");
        }
        
        return resourcePrefab;
    }
    
    /// <summary>
    /// UI 부모 자동 찾기
    /// </summary>
    private void FindUIParents()
    {
        // MainUI 찾기
        if (mainUI == null)
        {
            mainUI = GameObject.Find("MainUI");
            if (mainUI != null)
            {
                Debug.Log("[GamePanel] MainUI 자동 찾기 완료");
            }
            else
            {
                Debug.LogWarning("[GamePanel] MainUI를 찾을 수 없습니다!");
            }
        }
        
        // GameUI 찾기
        if (gameUI == null)
        {
            gameUI = GameObject.Find("GameUI");
            if (gameUI != null)
            {
                Debug.Log("[GamePanel] GameUI 자동 찾기 완료");
            }
            else
            {
                Debug.LogWarning("[GamePanel] GameUI를 찾을 수 없습니다!");
            }
        }
    }
    
    /// <summary>
    /// UI 부모 수동 설정
    /// </summary>
    public void SetUIParents(GameObject main, GameObject game)
    {
        mainUI = main;
        gameUI = game;
        Debug.Log("[GamePanel] UI 부모 수동 설정 완료");
    }
    
    /// <summary>
    /// 게임에서 MainUI로 돌아가기
    /// </summary>
    public void ReturnToMainUI()
    {
        // 게임 종료 시 자동 저장
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SaveGameData();
            Debug.Log("[GamePanel] 게임 종료 시 자동 저장 완료");
        }
        
        // GameUI 비활성화
        if (gameUI != null)
        {
            gameUI.SetActive(false);
            Debug.Log("[GamePanel] GameUI 비활성화");
        }
        
        // MainUI 활성화
        if (mainUI != null)
        {
            mainUI.SetActive(true);
            Debug.Log("[GamePanel] MainUI 활성화");
        }
        
        // TopBar를 NonGameState로 변경
        if (TopBarManager.Instance != null)
        {
            TopBarManager.Instance.SetTopBarType(TopBarManager.TopBarType.NonGameState);
            Debug.Log("[GamePanel] TopBar를 NonGameState로 변경");
        }
        
        // GameUI 밑의 모든 게임 타입 패널들 제거
        ClearExistingGameTypePanels();
        Debug.Log("[GamePanel] 모든 게임 타입 패널 제거 완료");
    }
}
