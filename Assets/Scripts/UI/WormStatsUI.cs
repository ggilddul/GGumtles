using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using GGumtles.Managers;
using GGumtles.Data;

namespace GGumtles.UI
{
    public class WormStatsUI : MonoBehaviour
    {
        [Header("웜 기본 정보")]
        [SerializeField] private Image wormImage;
        [SerializeField] private TextMeshProUGUI wormNameText;

        [Header("웜 네비게이션")]
        [SerializeField] private Button prevWormButton;
        [SerializeField] private Button nextWormButton;

        [Header("웜 통계 정보")]
        [SerializeField] private TextMeshProUGUI totalEatCountText;
        [SerializeField] private TextMeshProUGUI totalDiamondCountText;
        [SerializeField] private TextMeshProUGUI totalShakeCountText;
        [SerializeField] private TextMeshProUGUI totalPlayCountText;
        [SerializeField] private TextMeshProUGUI achievementCountText;

        [Header("디버그")]
        [SerializeField] private bool enableDebugLogs = false;

        // 현재 표시 중인 웜 인덱스
        private int currentWormIndex = 0;
        private List<WormData> allWorms = new List<WormData>();

        private void Start()
        {
            InitializeUI();
            SubscribeToEvents();
            SetupButtonListeners();
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        /// <summary>
        /// UI 초기화
        /// </summary>
        private void InitializeUI()
        {
            if (WormManager.Instance != null && WormManager.Instance.IsInitialized)
            {
                LoadAllWorms();
                SetCurrentWormIndex();
                UpdateWormStats();
                UpdateNavigationButtons();
            }
            else
            {
                LogDebug("[WormStatsUI] WormManager가 아직 초기화되지 않았습니다.");
            }
        }

        /// <summary>
        /// 모든 웜 데이터 로드
        /// </summary>
        private void LoadAllWorms()
        {
            if (WormManager.Instance != null)
            {
                allWorms = WormManager.Instance.GetAllWorms();
                LogDebug($"[WormStatsUI] {allWorms.Count}개의 웜을 로드했습니다.");
            }
        }

        /// <summary>
        /// 현재 웜의 인덱스 설정
        /// </summary>
        private void SetCurrentWormIndex()
        {
            if (WormManager.Instance?.CurrentWorm == null || allWorms.Count == 0)
            {
                currentWormIndex = 0;
                return;
            }

            var currentWorm = WormManager.Instance.CurrentWorm;
            for (int i = 0; i < allWorms.Count; i++)
            {
                if (allWorms[i].wormId == currentWorm.wormId)
                {
                    currentWormIndex = i;
                    LogDebug($"[WormStatsUI] 현재 웜 인덱스 설정: {currentWorm.name} (인덱스: {i})");
                    return;
                }
            }

            // 현재 웜을 찾지 못한 경우 첫 번째 웜으로 설정
            currentWormIndex = 0;
            LogDebug("[WormStatsUI] 현재 웜을 찾지 못해 첫 번째 웜으로 설정");
        }

        /// <summary>
        /// 버튼 리스너 설정
        /// </summary>
        private void SetupButtonListeners()
        {
            if (prevWormButton != null)
                prevWormButton.onClick.AddListener(OnPrevWormButtonClicked);
            
            if (nextWormButton != null)
                nextWormButton.onClick.AddListener(OnNextWormButtonClicked);
        }

        /// <summary>
        /// 이벤트 구독
        /// </summary>
        private void SubscribeToEvents()
        {
            if (WormManager.Instance != null)
            {
                WormManager.Instance.OnCurrentWormChangedEvent += OnCurrentWormChanged;
                WormManager.Instance.OnWormEvolvedEvent += OnWormEvolved;
                WormManager.Instance.OnWormCreatedEvent += OnWormCreated;
            }
        }

        /// <summary>
        /// 이벤트 구독 해제
        /// </summary>
        private void UnsubscribeFromEvents()
        {
            if (WormManager.Instance != null)
            {
                WormManager.Instance.OnCurrentWormChangedEvent -= OnCurrentWormChanged;
                WormManager.Instance.OnWormEvolvedEvent -= OnWormEvolved;
                WormManager.Instance.OnWormCreatedEvent -= OnWormCreated;
            }
        }



        /// <summary>
        /// 웜 통계 업데이트
        /// </summary>
        public void UpdateWormStats()
        {
            if (allWorms == null || allWorms.Count == 0 || currentWormIndex >= allWorms.Count)
            {
                LogDebug("[WormStatsUI] 표시할 웜이 없습니다.");
                return;
            }

            var currentWorm = allWorms[currentWormIndex];
            var stats = currentWorm.statistics;

            // 웜 이미지 업데이트
            UpdateWormImage(currentWorm);

            // 웜 이름 업데이트
            if (wormNameText != null)
                wormNameText.text = currentWorm.name;

            // 통계 정보 업데이트
            if (totalEatCountText != null)
                totalEatCountText.text = stats.totalEatCount.ToString();

            if (totalDiamondCountText != null)
                totalDiamondCountText.text = stats.totalDiamondCount.ToString();
            
            if (totalShakeCountText != null)
                totalShakeCountText.text = stats.totalShakeCount.ToString();
            
            if (totalPlayCountText != null)
                totalPlayCountText.text = stats.totalPlayCount.ToString();
            
            if (achievementCountText != null)
                achievementCountText.text = stats.achievementCount.ToString();

            LogDebug($"[WormStatsUI] 웜 통계 업데이트 완료: {currentWorm.name}");
        }

        /// <summary>
        /// 웜 이미지 업데이트
        /// </summary>
        private void UpdateWormImage(WormData worm)
        {
            if (wormImage == null) return;

            // TODO: SpriteManager를 통해 웜 이미지를 가져오는 로직 구현
            // 현재는 기본 이미지나 웜의 생애주기에 따른 이미지 설정
            // SpriteManager.Instance?.GetWormSprite(worm) 등을 사용할 수 있음
            
            LogDebug($"[WormStatsUI] 웜 이미지 업데이트: {worm.name} (생애주기: {worm.lifeStage})");
        }

       

        /// <summary>
        /// 이전 웜 버튼 클릭
        /// </summary>
        private void OnPrevWormButtonClicked()
        {
            if (allWorms.Count == 0 || currentWormIndex <= 0) return;

            currentWormIndex--;
            UpdateWormStats();
            UpdateNavigationButtons();
            LogDebug($"[WormStatsUI] 이전 웜으로 이동: {allWorms[currentWormIndex].name}");
        }

        /// <summary>
        /// 다음 웜 버튼 클릭
        /// </summary>
        private void OnNextWormButtonClicked()
        {
            if (allWorms.Count == 0 || currentWormIndex >= allWorms.Count - 1) return;

            currentWormIndex++;
            UpdateWormStats();
            UpdateNavigationButtons();
            LogDebug($"[WormStatsUI] 다음 웜으로 이동: {allWorms[currentWormIndex].name}");
        }

        /// <summary>
        /// 네비게이션 버튼 상태 업데이트
        /// </summary>
        private void UpdateNavigationButtons()
        {
            // 이전 버튼: 첫 번째 웜이 아닐 때만 활성화
            if (prevWormButton != null)
                prevWormButton.interactable = allWorms.Count > 0 && currentWormIndex > 0;
            
            // 다음 버튼: 마지막 웜이 아닐 때만 활성화
            if (nextWormButton != null)
                nextWormButton.interactable = allWorms.Count > 0 && currentWormIndex < allWorms.Count - 1;
        }

        /// <summary>
        /// 현재 웜 변경 이벤트 핸들러
        /// </summary>
        private void OnCurrentWormChanged(WormData previousWorm, WormData newWorm)
        {
            LogDebug($"[WormStatsUI] 현재 웜 변경: {previousWorm?.name} → {newWorm?.name}");
            
            // 현재 웜이 변경되면 해당 웜의 인덱스를 찾아서 업데이트
            if (newWorm != null)
            {
                for (int i = 0; i < allWorms.Count; i++)
                {
                    if (allWorms[i].wormId == newWorm.wormId)
                    {
                        currentWormIndex = i;
                        break;
                    }
                }
                UpdateWormStats();
                UpdateNavigationButtons();
            }
        }

        /// <summary>
        /// 웜 진화 이벤트 핸들러
        /// </summary>
        private void OnWormEvolved(WormData worm, int fromStage, int toStage)
        {
            LogDebug($"[WormStatsUI] 웜 진화: {worm.name} ({fromStage} → {toStage})");
            UpdateWormStats();
        }

        /// <summary>
        /// 웜 생성 이벤트 핸들러
        /// </summary>
        private void OnWormCreated(WormData worm)
        {
            LogDebug($"[WormStatsUI] 새 웜 생성: {worm.name}");
            LoadAllWorms(); // 웜 리스트 다시 로드
            UpdateWormStats();
            UpdateNavigationButtons();
        }

        /// <summary>
        /// 수동으로 통계 새로고침 (외부에서 호출 가능)
        /// </summary>
        public void RefreshStats()
        {
            LoadAllWorms();
            SetCurrentWormIndex();
            UpdateWormStats();
            UpdateNavigationButtons();
        }

        /// <summary>
        /// 특정 통계만 업데이트
        /// </summary>
        public void UpdateSpecificStat(string statType, int value)
        {
            switch (statType.ToLower())
            {
                case "eat":
                    if (totalEatCountText != null)
                        totalEatCountText.text = value.ToString();
                    break;
                case "diamond":
                    if (totalDiamondCountText != null)
                        totalDiamondCountText.text = value.ToString();
                    break;
                case "shake":
                    if (totalShakeCountText != null)
                        totalShakeCountText.text = value.ToString();
                    break;
                case "play":
                    if (totalPlayCountText != null)
                        totalPlayCountText.text = value.ToString();
                    break;
                case "achievement":
                    if (achievementCountText != null)
                        achievementCountText.text = value.ToString();
                    break;
                default:
                    LogDebug($"[WormStatsUI] 알 수 없는 통계 타입: {statType}");
                    break;
            }
        }

        /// <summary>
        /// 특정 웜으로 직접 이동
        /// </summary>
        public void GoToWorm(int wormId)
        {
            for (int i = 0; i < allWorms.Count; i++)
            {
                if (allWorms[i].wormId == wormId)
                {
                    currentWormIndex = i;
                    UpdateWormStats();
                    UpdateNavigationButtons();
                    LogDebug($"[WormStatsUI] 웜으로 이동: {allWorms[i].name}");
                    return;
                }
            }
            LogDebug($"[WormStatsUI] 웜을 찾을 수 없습니다: {wormId}");
        }

        /// <summary>
        /// 현재 표시 중인 웜 데이터 반환
        /// </summary>
        public WormData GetCurrentDisplayedWorm()
        {
            if (allWorms != null && currentWormIndex >= 0 && currentWormIndex < allWorms.Count)
            {
                return allWorms[currentWormIndex];
            }
            return null;
        }

        /// <summary>
        /// 웜 리스트 새로고침 (외부에서 호출 가능)
        /// </summary>
        public void RefreshWormList()
        {
            LoadAllWorms();
            SetCurrentWormIndex();
            UpdateWormStats();
            UpdateNavigationButtons();
        }

        private void LogDebug(string message)
        {
            if (enableDebugLogs)
            {
                Debug.Log(message);
            }
        }
    }
}
