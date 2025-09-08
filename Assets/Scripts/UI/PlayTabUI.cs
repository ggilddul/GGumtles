using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using GGumtles.Managers;

namespace GGumtles.UI
{
    /// <summary>
    /// 게임 플레이 탭 UI 관리
    /// MinigameManager를 통해 게임 관련 UI 요소들을 관리
    /// </summary>
    public class PlayTabUI : MonoBehaviour
{
    [Header("UI 설정")]
    [SerializeField] private Transform contentParent;              // Content Transform
    
    [Header("디버그")]
    [SerializeField] private bool enableDebugLogs = false;
        
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
                
                LogDebug("[PlayTabUI] 플레이 탭 초기화 완료 - MinigameManager 사용");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[PlayTabUI] 플레이 탭 초기화 중 오류: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 게임 패널 생성 (MinigameManager 사용)
        /// </summary>
        public void CreateGamePanel()
        {
            LogDebug("[PlayTabUI] CreateGamePanel 호출됨 - MinigameManager에서 처리");
            // MinigameManager가 게임 패널을 관리하므로 여기서는 로그만 출력
        }
        
        /// <summary>
        /// 게임 패널 새로고침 (MinigameManager 사용)
        /// </summary>
        public void RefreshGamePanel()
        {
            LogDebug("[PlayTabUI] RefreshGamePanel 호출됨 - MinigameManager에서 처리");
            // MinigameManager가 게임 패널을 관리하므로 여기서는 로그만 출력
        }
        
        /// <summary>
        /// 현재 게임이 실행 중인지 확인
        /// </summary>
        public bool IsGameRunning()
        {
            return MinigameManager.Instance != null && MinigameManager.Instance.IsGameRunning();
        }
        
        /// <summary>
        /// 현재 게임 타입 가져오기
        /// </summary>
        public int GetCurrentGameType()
        {
            return MinigameManager.Instance != null ? MinigameManager.Instance.GetCurrentGameType() : -1;
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
            // MinigameManager가 게임 패널을 관리하므로 여기서는 특별한 정리 작업 불필요
            LogDebug("[PlayTabUI] PlayTabUI 파괴됨");
        }
    }
}
