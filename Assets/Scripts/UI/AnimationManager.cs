using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AnimationManager : MonoBehaviour
{
    [Header("애니메이터")]
    [SerializeField] private Animator animator;

    [Header("설정")]
    [SerializeField] private bool enableDebugLogs = false;
    [SerializeField] private bool enableAnimationEvents = true;

    [Header("애니메이션 설정")]
    [SerializeField] private float defaultAnimationSpeed = 1f;
    [SerializeField] private bool pauseOnGamePause = true;

    // 애니메이션 상태 관리
    private Dictionary<string, bool> animationStates = new Dictionary<string, bool>();
    private Coroutine currentAnimationCoroutine;

    // 이벤트 정의
    public delegate void OnAnimationStarted(string animationName);
    public delegate void OnAnimationCompleted(string animationName);
    public delegate void OnAnimationEvent(string eventName);
    public event OnAnimationStarted OnAnimationStartedEvent;
    public event OnAnimationCompleted OnAnimationCompletedEvent;
    public event OnAnimationEvent OnAnimationEventEvent;

    // Singleton 인스턴스
    public static AnimationManager Instance { get; private set; }

    // 프로퍼티
    public Animator Animator => animator;
    public bool IsPlaying => animator != null && animator.GetCurrentAnimatorStateInfo(0).length > 0;
    public float CurrentAnimationTime => animator != null ? animator.GetCurrentAnimatorStateInfo(0).normalizedTime : 0f;

    private void Awake()
    {
        InitializeSingleton();
        InitializeAnimationManager();
    }

    private void InitializeSingleton()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LogDebug("[AnimationManager] Singleton 초기화 완료");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeAnimationManager()
    {
        try
        {
            // 자동으로 Animator 찾기
            if (animator == null)
            {
                animator = GetComponent<Animator>();
                if (animator == null)
                {
                    animator = GetComponentInChildren<Animator>();
                }
            }

            // 애니메이션 속도 설정
            if (animator != null)
            {
                animator.speed = defaultAnimationSpeed;
            }

            // 애니메이션 상태 초기화
            InitializeAnimationStates();

            LogDebug("[AnimationManager] 애니메이션 매니저 초기화 완료");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[AnimationManager] 초기화 중 오류: {ex.Message}");
        }
    }

    private void InitializeAnimationStates()
    {
        if (animator == null) return;

        try
        {
            // 기본 애니메이션 상태들 초기화
            string[] defaultStates = { "Idle", "ShakeTree", "DropAcorn", "DropDiamond", "ThrowAcorn", "HatchEgg" };
            
            foreach (string state in defaultStates)
            {
                animationStates[state] = false;
            }

            LogDebug("[AnimationManager] 애니메이션 상태 초기화 완료");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[AnimationManager] 애니메이션 상태 초기화 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 나무 흔들기 애니메이션
    /// </summary>
    public void ShakeTree()
    {
        PlayAnimation("ShakeTree");
        LogDebug("[AnimationManager] 나무 흔들기 애니메이션 시작");
    }

    /// <summary>
    /// 도토리 떨어뜨리기 애니메이션
    /// </summary>
    public void DropAcorn()
    {
        PlayAnimation("DropAcorn");
        LogDebug("[AnimationManager] 도토리 떨어뜨리기 애니메이션 시작");
    }

    /// <summary>
    /// 다이아몬드 떨어뜨리기 애니메이션
    /// </summary>
    public void DropDiamond()
    {
        PlayAnimation("DropDiamond");
        LogDebug("[AnimationManager] 다이아몬드 떨어뜨리기 애니메이션 시작");
    }

    /// <summary>
    /// 도토리 던지기 애니메이션
    /// </summary>
    public void PlayThrowAcornAnimation()
    {
        PlayAnimation("ThrowAcorn");
        LogDebug("[AnimationManager] 도토리 던지기 애니메이션 시작");
    }

    /// <summary>
    /// 알 부화 애니메이션
    /// </summary>
    public void HatchEgg()
    {
        PlayAnimation("HatchEgg");
        LogDebug("[AnimationManager] 알 부화 애니메이션 시작");
    }

    /// <summary>
    /// 벌레 진화 애니메이션
    /// </summary>
    public void EvolveWorm()
    {
        PlayAnimation("EvolveWorm");
        LogDebug("[AnimationManager] 벌레 진화 애니메이션 시작");
    }

    /// <summary>
    /// 벌레 사망 애니메이션
    /// </summary>
    public void WormDie()
    {
        PlayAnimation("WormDie");
        LogDebug("[AnimationManager] 벌레 사망 애니메이션 시작");
    }

    /// <summary>
    /// 업적 해금 애니메이션
    /// </summary>
    public void UnlockAchievement()
    {
        PlayAnimation("UnlockAchievement");
        LogDebug("[AnimationManager] 업적 해금 애니메이션 시작");
    }

    /// <summary>
    /// 일반 애니메이션 재생
    /// </summary>
    public void PlayAnimation(string animationName)
    {
        try
        {
            if (animator == null)
            {
                Debug.LogWarning("[AnimationManager] Animator가 없습니다.");
                return;
            }

            // 애니메이션 상태 업데이트
            animationStates[animationName] = true;

            // 트리거 설정
            animator.SetTrigger(animationName);

            // 이벤트 발생
            OnAnimationStartedEvent?.Invoke(animationName);

            // 애니메이션 완료 감지
            StartCoroutine(WaitForAnimationComplete(animationName));

            LogDebug($"[AnimationManager] 애니메이션 재생: {animationName}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[AnimationManager] 애니메이션 재생 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 애니메이션 완료 대기
    /// </summary>
    private IEnumerator WaitForAnimationComplete(string animationName)
    {
        if (animator == null) yield break;

        // 현재 애니메이션 상태 정보 가져오기
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        
        // 애니메이션이 끝날 때까지 대기
        while (stateInfo.IsName(animationName) && stateInfo.normalizedTime < 1.0f)
        {
            yield return null;
            stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        }

        // 애니메이션 상태 업데이트
        animationStates[animationName] = false;

        // 완료 이벤트 발생
        OnAnimationCompletedEvent?.Invoke(animationName);

        LogDebug($"[AnimationManager] 애니메이션 완료: {animationName}");
    }

    /// <summary>
    /// 애니메이션 속도 설정
    /// </summary>
    public void SetAnimationSpeed(float speed)
    {
        try
        {
            if (animator != null)
            {
                animator.speed = Mathf.Clamp(speed, 0f, 3f);
                LogDebug($"[AnimationManager] 애니메이션 속도 설정: {speed}");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[AnimationManager] 애니메이션 속도 설정 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 애니메이션 일시정지/재개
    /// </summary>
    public void SetAnimationPaused(bool paused)
    {
        try
        {
            if (animator != null)
            {
                animator.speed = paused ? 0f : defaultAnimationSpeed;
                LogDebug($"[AnimationManager] 애니메이션 {(paused ? "일시정지" : "재개")}");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[AnimationManager] 애니메이션 일시정지 설정 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 특정 애니메이션 상태 확인
    /// </summary>
    public bool IsAnimationPlaying(string animationName)
    {
        try
        {
            if (animator == null) return false;

            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            return stateInfo.IsName(animationName) && stateInfo.normalizedTime < 1.0f;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[AnimationManager] 애니메이션 상태 확인 중 오류: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// 모든 애니메이션 정지
    /// </summary>
    public void StopAllAnimations()
    {
        try
        {
            if (animator != null)
            {
                animator.SetTrigger("Stop");
                animator.speed = 0f;
                
                // 모든 애니메이션 상태 초기화
                foreach (string key in animationStates.Keys)
                {
                    animationStates[key] = false;
                }

                LogDebug("[AnimationManager] 모든 애니메이션 정지");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[AnimationManager] 애니메이션 정지 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 애니메이션 이벤트 호출 (Animator에서 호출)
    /// </summary>
    public void HandleAnimationEvent(string eventName)
    {
        if (!enableAnimationEvents) return;

        try
        {
            OnAnimationEventEvent?.Invoke(eventName);
            LogDebug($"[AnimationManager] 애니메이션 이벤트: {eventName}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[AnimationManager] 애니메이션 이벤트 처리 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 애니메이션 매니저 정보 반환
    /// </summary>
    public string GetAnimationInfo()
    {
        var info = new System.Text.StringBuilder();
        info.AppendLine($"[AnimationManager 정보]");
        info.AppendLine($"Animator: {(animator != null ? "연결됨" : "연결 안됨")}");
        info.AppendLine($"현재 재생 중: {IsPlaying}");
        info.AppendLine($"애니메이션 속도: {animator?.speed ?? 0f}");
        info.AppendLine($"이벤트 활성화: {(enableAnimationEvents ? "활성화" : "비활성화")}");

        return info.ToString();
    }

    private void LogDebug(string message)
    {
        if (enableDebugLogs)
        {
            Debug.Log(message);
        }
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseOnGamePause)
        {
            SetAnimationPaused(pauseStatus);
        }
    }

    private void OnDestroy()
    {
        if (currentAnimationCoroutine != null)
        {
            StopCoroutine(currentAnimationCoroutine);
        }

        // 이벤트 구독 해제
        OnAnimationStartedEvent = null;
        OnAnimationCompletedEvent = null;
        OnAnimationEventEvent = null;
    }
}
