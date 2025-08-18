using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace GGumtles.UI
{
    /// <summary>
    /// 모든 UI 요소의 기본 클래스
    /// 공통 기능과 인터페이스를 제공
    /// </summary>
    public abstract class UIBase : MonoBehaviour
    {
        [Header("기본 설정")]
        [SerializeField] protected bool enableAnimations = true;
        [SerializeField] protected bool enableSound = true;
        [SerializeField] protected bool enableDebugLogs = false;

        [Header("애니메이션 설정")]
        [SerializeField] protected float animationDuration = 0.3f;
        [SerializeField] protected AnimationCurve animationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [Header("사운드 설정")]
        [SerializeField] protected AudioManager.SFXType showSound = AudioManager.SFXType.Button;
        [SerializeField] protected AudioManager.SFXType hideSound = AudioManager.SFXType.Button;

        // 상태 관리
        protected bool isInitialized = false;
        protected bool isVisible = false;
        protected bool isAnimating = false;
        protected Coroutine animationCoroutine;

        // 이벤트 정의
        public delegate void OnUIShown();
        public delegate void OnUIHidden();
        public delegate void OnUIAnimationComplete();
        
        public event OnUIShown OnUIShownEvent;
        public event OnUIHidden OnUIHiddenEvent;
        public event OnUIAnimationComplete OnUIAnimationCompleteEvent;

        // 프로퍼티
        public bool IsVisible => isVisible;
        public bool IsAnimating => isAnimating;
        public bool IsInitialized => isInitialized;

        protected virtual void Awake()
        {
            InitializeUI();
        }

        protected virtual void Start()
        {
            SetupUI();
        }

        /// <summary>
        /// UI 초기화
        /// </summary>
        protected virtual void InitializeUI()
        {
            try
            {
                // 자동 컴포넌트 찾기
                AutoFindComponents();
                
                // 기본 설정
                SetupDefaultSettings();
                
                isInitialized = true;
                LogDebug($"[{GetType().Name}] 초기화 완료");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[{GetType().Name}] 초기화 중 오류: {ex.Message}");
            }
        }

        /// <summary>
        /// 자동 컴포넌트 찾기
        /// </summary>
        protected virtual void AutoFindComponents()
        {
            // 하위 클래스에서 구현
        }

        /// <summary>
        /// 기본 설정
        /// </summary>
        protected virtual void SetupDefaultSettings()
        {
            // 하위 클래스에서 구현
        }

        /// <summary>
        /// UI 설정
        /// </summary>
        protected virtual void SetupUI()
        {
            // 하위 클래스에서 구현
        }

        /// <summary>
        /// UI 표시
        /// </summary>
        public virtual void Show()
        {
            if (isVisible) return;

            try
            {
                if (enableAnimations)
                {
                    StartShowAnimation();
                }
                else
                {
                    ShowImmediate();
                }

                // 사운드 재생
                if (enableSound)
                {
                    AudioManager.Instance?.PlaySFX(showSound);
                }

                OnUIShownEvent?.Invoke();
                LogDebug($"[{GetType().Name}] 표시됨");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[{GetType().Name}] 표시 중 오류: {ex.Message}");
            }
        }

        /// <summary>
        /// UI 숨김
        /// </summary>
        public virtual void Hide()
        {
            if (!isVisible) return;

            try
            {
                if (enableAnimations)
                {
                    StartHideAnimation();
                }
                else
                {
                    HideImmediate();
                }

                // 사운드 재생
                if (enableSound)
                {
                    AudioManager.Instance?.PlaySFX(hideSound);
                }

                OnUIHiddenEvent?.Invoke();
                LogDebug($"[{GetType().Name}] 숨겨짐");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[{GetType().Name}] 숨김 중 오류: {ex.Message}");
            }
        }

        /// <summary>
        /// 즉시 표시
        /// </summary>
        public virtual void ShowImmediate()
        {
            gameObject.SetActive(true);
            isVisible = true;
            OnUIShownEvent?.Invoke();
        }

        /// <summary>
        /// 즉시 숨김
        /// </summary>
        public virtual void HideImmediate()
        {
            gameObject.SetActive(false);
            isVisible = false;
            OnUIHiddenEvent?.Invoke();
        }

        /// <summary>
        /// 표시 애니메이션 시작
        /// </summary>
        protected virtual void StartShowAnimation()
        {
            if (animationCoroutine != null)
            {
                StopCoroutine(animationCoroutine);
            }
            animationCoroutine = StartCoroutine(ShowAnimationCoroutine());
        }

        /// <summary>
        /// 숨김 애니메이션 시작
        /// </summary>
        protected virtual void StartHideAnimation()
        {
            if (animationCoroutine != null)
            {
                StopCoroutine(animationCoroutine);
            }
            animationCoroutine = StartCoroutine(HideAnimationCoroutine());
        }

        /// <summary>
        /// 표시 애니메이션 코루틴
        /// </summary>
        protected virtual IEnumerator ShowAnimationCoroutine()
        {
            isAnimating = true;
            gameObject.SetActive(true);

            // 기본 페이드 인 애니메이션
            CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                float elapsed = 0f;

                while (elapsed < animationDuration)
                {
                    elapsed += Time.deltaTime;
                    float progress = elapsed / animationDuration;
                    float curveValue = animationCurve.Evaluate(progress);
                    
                    canvasGroup.alpha = curveValue;
                    yield return null;
                }

                canvasGroup.alpha = 1f;
            }

            isVisible = true;
            isAnimating = false;
            animationCoroutine = null;
            OnUIAnimationCompleteEvent?.Invoke();
        }

        /// <summary>
        /// 숨김 애니메이션 코루틴
        /// </summary>
        protected virtual IEnumerator HideAnimationCoroutine()
        {
            isAnimating = true;

            // 기본 페이드 아웃 애니메이션
            CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                float elapsed = 0f;

                while (elapsed < animationDuration)
                {
                    elapsed += Time.deltaTime;
                    float progress = elapsed / animationDuration;
                    float curveValue = animationCurve.Evaluate(progress);
                    
                    canvasGroup.alpha = 1f - curveValue;
                    yield return null;
                }

                canvasGroup.alpha = 0f;
            }

            gameObject.SetActive(false);
            isVisible = false;
            isAnimating = false;
            animationCoroutine = null;
            OnUIAnimationCompleteEvent?.Invoke();
        }

        /// <summary>
        /// 애니메이션 활성화/비활성화
        /// </summary>
        public virtual void SetAnimationEnabled(bool enabled)
        {
            enableAnimations = enabled;
            LogDebug($"[{GetType().Name}] 애니메이션 {(enabled ? "활성화" : "비활성화")}");
        }

        /// <summary>
        /// 사운드 활성화/비활성화
        /// </summary>
        public virtual void SetSoundEnabled(bool enabled)
        {
            enableSound = enabled;
            LogDebug($"[{GetType().Name}] 사운드 {(enabled ? "활성화" : "비활성화")}");
        }

        /// <summary>
        /// 애니메이션 지속시간 설정
        /// </summary>
        public virtual void SetAnimationDuration(float duration)
        {
            animationDuration = Mathf.Max(0f, duration);
            LogDebug($"[{GetType().Name}] 애니메이션 지속시간 설정: {duration}");
        }

        /// <summary>
        /// UI 정보 반환
        /// </summary>
        public virtual string GetUIInfo()
        {
            var info = new System.Text.StringBuilder();
            info.AppendLine($"[{GetType().Name} 정보]");
            info.AppendLine($"초기화됨: {isInitialized}");
            info.AppendLine($"표시됨: {isVisible}");
            info.AppendLine($"애니메이션 중: {isAnimating}");
            info.AppendLine($"애니메이션: {(enableAnimations ? "활성화" : "비활성화")}");
            info.AppendLine($"사운드: {(enableSound ? "활성화" : "비활성화")}");

            return info.ToString();
        }

        protected virtual void LogDebug(string message)
        {
            if (enableDebugLogs)
            {
                Debug.Log(message);
            }
        }

        protected virtual void OnDestroy()
        {
            if (animationCoroutine != null)
            {
                StopCoroutine(animationCoroutine);
            }

            // 이벤트 구독 해제
            OnUIShownEvent = null;
            OnUIHiddenEvent = null;
            OnUIAnimationCompleteEvent = null;
        }
    }
}
