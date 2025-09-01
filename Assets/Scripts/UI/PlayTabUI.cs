using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace GGumtles.UI
{
    /// <summary>
    /// 게임 플레이 탭 UI 관리
    /// GamePanel을 포함한 게임 관련 UI 요소들을 관리
    /// </summary>
    public class PlayTabUI : MonoBehaviour
    {
        [Header("UI 설정")]
        [SerializeField] private Transform contentParent;              // Content Transform
        [SerializeField] private GameObject gamePanelPrefab;          // GamePanel 프리팹
        
        [Header("디버그")]
        [SerializeField] private bool enableDebugLogs = false;
        
        private GamePanel activeGamePanel;
        
        private void Start()
        {
            InitializePlayTab();
        }
        
        /// <summary>
        /// 플레이 탭 초기화
        /// </summary>
        public void InitializePlayTab()
        {
            try
            {
                if (contentParent == null)
                {
                    Debug.LogError("[PlayTabUI] Content Parent가 설정되지 않았습니다.");
                    return;
                }
                
                if (gamePanelPrefab == null)
                {
                    Debug.LogError("[PlayTabUI] Game Panel Prefab이 설정되지 않았습니다.");
                    return;
                }
                
                CreateGamePanel();
                LogDebug("[PlayTabUI] 플레이 탭 초기화 완료");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[PlayTabUI] 플레이 탭 초기화 중 오류: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 게임 패널 생성
        /// </summary>
        public void CreateGamePanel()
        {
            ClearExistingGamePanel();
            
            try
            {
                // 프리팹 인스턴스 생성
                GameObject panelObj = Instantiate(gamePanelPrefab, contentParent);
                activeGamePanel = panelObj.GetComponent<GamePanel>();
                
                if (activeGamePanel == null)
                {
                    Debug.LogError("[PlayTabUI] GamePanel 컴포넌트를 찾을 수 없습니다.");
                    Destroy(panelObj);
                    return;
                }
                
                // 레이아웃 업데이트
                LayoutRebuilder.ForceRebuildLayoutImmediate(contentParent as RectTransform);
                
                LogDebug("[PlayTabUI] 게임 패널 생성 완료");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[PlayTabUI] 게임 패널 생성 중 오류: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 기존 게임 패널 정리
        /// </summary>
        private void ClearExistingGamePanel()
        {
            if (activeGamePanel != null)
            {
                Destroy(activeGamePanel.gameObject);
                activeGamePanel = null;
            }
        }
        
        /// <summary>
        /// 게임 패널 새로고침
        /// </summary>
        public void RefreshGamePanel()
        {
            CreateGamePanel();
        }
        
        /// <summary>
        /// 활성 게임 패널 반환
        /// </summary>
        public GamePanel GetActiveGamePanel()
        {
            return activeGamePanel;
        }
        
        /// <summary>
        /// 게임 패널 활성화/비활성화
        /// </summary>
        public void SetGamePanelActive(bool active)
        {
            if (activeGamePanel != null)
            {
                activeGamePanel.gameObject.SetActive(active);
            }
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
        
        private void OnDestroy()
        {
            ClearExistingGamePanel();
        }
    }
}
