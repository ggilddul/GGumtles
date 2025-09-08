using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GGumtles.UI;
using System.Collections;
using System.Collections.Generic;
using GGumtles.Managers;

namespace GGumtles.Managers
{
    /// <summary>
    /// 하단 바 UI를 관리하는 매니저
    /// TabBar와 ADBar 중 어떤 것을 표시할지 결정하는 스위처 역할
    /// </summary>
    public class BottomBarManager : MonoBehaviour
    {
        public static BottomBarManager Instance { get; private set; }

        [Header("UI 참조")]
        [SerializeField] private Transform bottomBarParent;
        [SerializeField] private Transform tabBarContainer;
        [SerializeField] private Transform adBarContainer;

        [Header("바 타입 설정")]
        [SerializeField] private BarType currentBarType = BarType.TabBar;
        [SerializeField] private bool enableBarAnimations = true;
        [SerializeField] private float barTransitionDuration = 0.3f;


        [Header("디버그")]
        [SerializeField] private bool enableDebugLogs = false;

        // 바 타입 열거형
        public enum BarType
        {
            TabBar,     // 탭 바 (웜, 아이템, 업적, 설정)
            ADBar       // 광고 바
        }

        // 이벤트 정의
        public delegate void OnBarTypeChanged(BarType fromType, BarType toType);
        public event OnBarTypeChanged OnBarTypeChangedEvent;



        public delegate void OnBottomBarStateChanged(bool isVisible);
        public event OnBottomBarStateChanged OnBottomBarStateChangedEvent;

        // 상태 관리
        private BarType previousBarType = BarType.TabBar;
        private bool isInitialized = false;
        private bool isVisible = true;
        private bool isTransitioning = false;

        // 프로퍼티
        public BarType CurrentBarType => currentBarType;
        public bool IsVisible => isVisible;
        public bool IsTransitioning => isTransitioning;

        private void Awake()
        {
            InitializeSingleton();
        }

        public void Initialize()
        {
            InitializeBottomBar();
        }

                    private void InitializeSingleton()
        {
            if (Instance == null)
            {
                Instance = this;
                // UI 요소는 Canvas 하위에 있어야 하므로 Canvas를 DontDestroyOnLoad로 설정
                Canvas parentCanvas = GetComponentInParent<Canvas>();
                if (parentCanvas != null)
                {
                    Debug.Log("[BottomBarManager] Canvas를 DontDestroyOnLoad로 설정합니다.");
                    DontDestroyOnLoad(parentCanvas.gameObject);
                }
                else
                {
                    Debug.LogWarning("[BottomBarManager] Canvas를 찾을 수 없습니다. 직접 DontDestroyOnLoad를 설정합니다.");
                    DontDestroyOnLoad(gameObject);
                }
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// BottomBar 초기화
        /// </summary>
        private void InitializeBottomBar()
        {
            try
            {
                AutoFindReferences();
                SetupBarContainers();
                ShowCurrentBar();
                
                isInitialized = true;
                LogDebug("[BottomBarManager] 초기화 완료");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[BottomBarManager] 초기화 중 오류: {ex.Message}");
            }
        }

        /// <summary>
        /// 참조 자동 찾기
        /// </summary>
        private void AutoFindReferences()
        {
            if (bottomBarParent == null)
                bottomBarParent = transform;

            if (tabBarContainer == null)
                tabBarContainer = transform.Find("TabBar") ?? transform;

            if (adBarContainer == null)
                adBarContainer = transform.Find("ADBar") ?? transform;
        }

        /// <summary>
        /// 바 컨테이너들 설정
        /// </summary>
        private void SetupBarContainers()
        {
            // TabBar와 ADBar 컨테이너 초기 설정
            if (tabBarContainer != null)
            {
                tabBarContainer.gameObject.SetActive(false);
            }
            
            if (adBarContainer != null)
            {
                adBarContainer.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// 현재 바 표시
        /// </summary>
        private void ShowCurrentBar()
        {
            if (isTransitioning) return;

            if (enableBarAnimations)
            {
                StartCoroutine(TransitionToBar(currentBarType));
            }
            else
            {
                SwitchToBarImmediate(currentBarType);
            }
        }

        /// <summary>
        /// 바 타입 전환
        /// </summary>
        public void SwitchToBar(BarType barType)
        {
            if (currentBarType == barType || isTransitioning) return;

            previousBarType = currentBarType;
            currentBarType = barType;

            if (enableBarAnimations)
            {
                StartCoroutine(TransitionToBar(barType));
            }
            else
            {
                SwitchToBarImmediate(barType);
            }

            OnBarTypeChangedEvent?.Invoke(previousBarType, currentBarType);
            LogDebug($"[BottomBarManager] 바 전환: {previousBarType} → {currentBarType}");
        }

        /// <summary>
        /// 즉시 바 전환 (애니메이션 없음)
        /// </summary>
        private void SwitchToBarImmediate(BarType barType)
        {
            // 모든 바 숨기기
            if (tabBarContainer != null) tabBarContainer.gameObject.SetActive(false);
            if (adBarContainer != null) adBarContainer.gameObject.SetActive(false);

            // 선택된 바만 표시
            switch (barType)
            {
                case BarType.TabBar:
                    if (tabBarContainer != null) tabBarContainer.gameObject.SetActive(true);
                    break;
                case BarType.ADBar:
                    if (adBarContainer != null) adBarContainer.gameObject.SetActive(true);
                    break;
            }
        }

        /// <summary>
        /// 애니메이션과 함께 바 전환
        /// </summary>
        private IEnumerator TransitionToBar(BarType barType)
        {
            isTransitioning = true;

            // 페이드 아웃
            yield return StartCoroutine(FadeOutCurrentBar());

            // 바 전환
            SwitchToBarImmediate(barType);

            // 페이드 인
            yield return StartCoroutine(FadeInNewBar());

            isTransitioning = false;
        }

        /// <summary>
        /// 현재 바 페이드 아웃
        /// </summary>
        private IEnumerator FadeOutCurrentBar()
        {
            Transform currentBar = GetCurrentBarTransform();
            if (currentBar == null) yield break;

            CanvasGroup canvasGroup = currentBar.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = currentBar.gameObject.AddComponent<CanvasGroup>();
            }

            float elapsed = 0f;
            while (elapsed < barTransitionDuration)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / barTransitionDuration);
                yield return null;
            }

            canvasGroup.alpha = 0f;
        }

        /// <summary>
        /// 새 바 페이드 인
        /// </summary>
        private IEnumerator FadeInNewBar()
        {
            Transform newBar = GetCurrentBarTransform();
            if (newBar == null) yield break;

            CanvasGroup canvasGroup = newBar.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = newBar.gameObject.AddComponent<CanvasGroup>();
            }

            canvasGroup.alpha = 0f;

            float elapsed = 0f;
            while (elapsed < barTransitionDuration)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / barTransitionDuration);
                yield return null;
            }

            canvasGroup.alpha = 1f;
        }

        /// <summary>
        /// 현재 바 Transform 가져오기
        /// </summary>
        private Transform GetCurrentBarTransform()
        {
            switch (currentBarType)
            {
                case BarType.TabBar:
                    return tabBarContainer;
                case BarType.ADBar:
                    return adBarContainer;
                default:
                    return null;
            }
        }



        /// <summary>
        /// TabBar로 전환
        /// </summary>
        public void ShowTabBar()
        {
            SwitchToBar(BarType.TabBar);
        }



        /// <summary>
        /// 이전 바로 되돌리기
        /// </summary>
        public void SwitchToPreviousBar()
        {
            SwitchToBar(previousBarType);
        }



        /// <summary>
        /// 하단 바 표시/숨김
        /// </summary>
        public void SetVisible(bool visible)
        {
            if (isVisible == visible) return;

            isVisible = visible;
            gameObject.SetActive(visible);
            OnBottomBarStateChangedEvent?.Invoke(visible);
            
            LogDebug($"[BottomBarManager] 하단 바 {(visible ? "표시" : "숨김")}");
        }



        /// <summary>
        /// BottomBar 정보 반환
        /// </summary>
        public string GetBottomBarInfo()
        {
            var info = new System.Text.StringBuilder();
            info.AppendLine($"[BottomBar 정보]");
            info.AppendLine($"초기화됨: {isInitialized}");
            info.AppendLine($"표시됨: {isVisible}");
            info.AppendLine($"현재 바 타입: {currentBarType}");
            info.AppendLine($"이전 바 타입: {previousBarType}");
            info.AppendLine($"전환 중: {isTransitioning}");
            info.AppendLine($"애니메이션: {enableBarAnimations}");

            return info.ToString();
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
            // 이벤트 구독 해제는 여기서 처리
            LogDebug("[BottomBarManager] 파괴됨");
        }
    }
}
