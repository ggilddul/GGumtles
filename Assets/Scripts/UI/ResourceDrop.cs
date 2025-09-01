using UnityEngine;
using UnityEngine.UI;

public class ResourceDrop : MonoBehaviour
{
    [Header("낙하 설정")]
    [SerializeField] private float fallSpeed = 300f; // 초기 속도 (픽셀/초)
    [SerializeField] private float fallDistance = 300f; // 총 낙하 거리
    [SerializeField] private float gravity = 980f; // 중력 가속도 (픽셀/초²)
    [SerializeField] private float bounceHeight = 50f; // 바운스 높이
    [SerializeField] private int maxBounces = 2; // 최대 바운스 횟수

    [Header("애니메이션")]
    [SerializeField] private bool enableRotation = true;
    [SerializeField] private float rotationSpeed = 180f; // 회전 속도 (도/초)
    [SerializeField] private bool enableScaleAnimation = true;
    [SerializeField] private float scaleAnimationDuration = 0.3f;

    [Header("효과")]
    [SerializeField] private bool enableDropSound = true;
    [SerializeField] private bool enableParticleEffect = true;
    [SerializeField] private GameObject dropParticlePrefab;
    [SerializeField] private GameObject bounceParticlePrefab;

    [Header("디버그")]
    [SerializeField] private bool enableDebugLogs = false;

    // 물리 상태
    private Vector3 startPosition;
    private Vector3 currentVelocity;
    private float fallenDistance = 0f;
    private int bounceCount = 0;
    private bool isFalling = true;
    private bool hasLanded = false;

    // 애니메이션 상태
    private Coroutine scaleAnimationCoroutine;

    // 이벤트 정의
    public delegate void OnResourceLanded();
    public delegate void OnResourceBounced(int bounceCount);
    public event OnResourceLanded OnResourceLandedEvent;
    public event OnResourceBounced OnResourceBouncedEvent;

    // 프로퍼티
    public bool IsFalling => isFalling;
    public bool HasLanded => hasLanded;
    public float FallenDistance => fallenDistance;
    public int BounceCount => bounceCount;
    public Vector3 CurrentVelocity => currentVelocity;

    private void Start()
    {
        InitializeResourceDrop();
    }

    private void InitializeResourceDrop()
    {
        try
        {
            startPosition = transform.position;
            currentVelocity = Vector3.down * fallSpeed;

            // 초기 스케일 애니메이션
            if (enableScaleAnimation)
            {
                StartScaleAnimation();
            }

            LogDebug("[ResourceDrop] 자원 낙하 초기화 완료");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[ResourceDrop] 초기화 중 오류: {ex.Message}");
        }
    }

    private void Update()
    {
        if (!isFalling) return;

        try
        {
            UpdateFalling();
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[ResourceDrop] 낙하 업데이트 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 낙하 업데이트
    /// </summary>
    private void UpdateFalling()
    {
        // 중력 적용 (가속 운동)
        currentVelocity += Vector3.down * gravity * Time.deltaTime;

        // 위치 업데이트
        Vector3 movement = currentVelocity * Time.deltaTime;
        transform.Translate(movement);

        // 낙하 거리 계산
        fallenDistance = startPosition.y - transform.position.y;

        // 회전 애니메이션
        if (enableRotation)
        {
            transform.Rotate(0f, rotationSpeed * Time.deltaTime, 0f);
        }

        // 바닥 충돌 체크
        if (transform.position.y <= startPosition.y - fallDistance)
        {
            HandleLanding();
        }
    }

    /// <summary>
    /// 착지 처리
    /// </summary>
    private void HandleLanding()
    {
        try
        {
            // 정확한 착지 위치로 조정
            Vector3 landPosition = new Vector3(
                transform.position.x,
                startPosition.y - fallDistance,
                transform.position.z
            );
            transform.position = landPosition;

            // 바운스 처리
            if (bounceCount < maxBounces && Mathf.Abs(currentVelocity.y) > 100f)
            {
                HandleBounce();
            }
            else
            {
                FinalizeLanding();
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[ResourceDrop] 착지 처리 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 바운스 처리
    /// </summary>
    private void HandleBounce()
    {
        try
        {
            bounceCount++;

            // 바운스 속도 계산 (에너지 손실 포함)
            float bounceVelocity = Mathf.Abs(currentVelocity.y) * 0.6f; // 40% 에너지 손실
            currentVelocity.y = bounceVelocity;

            // 바운스 파티클 효과
            if (enableParticleEffect && bounceParticlePrefab != null)
            {
                Instantiate(bounceParticlePrefab, transform.position, Quaternion.identity);
            }

            // 바운스 사운드
            if (enableDropSound)
            {
                AudioManager.Instance?.PlaySFX(AudioManager.SFXType.ItemDrop);
            }

            OnResourceBouncedEvent?.Invoke(bounceCount);

            LogDebug($"[ResourceDrop] 바운스 {bounceCount}회");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[ResourceDrop] 바운스 처리 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 최종 착지 처리
    /// </summary>
    private void FinalizeLanding()
    {
        try
        {
            isFalling = false;
            hasLanded = true;
            currentVelocity = Vector3.zero;

            // 착지 파티클 효과
            if (enableParticleEffect && dropParticlePrefab != null)
            {
                Instantiate(dropParticlePrefab, transform.position, Quaternion.identity);
            }

            // 착지 사운드
            if (enableDropSound)
            {
                AudioManager.Instance?.PlaySFX(AudioManager.SFXType.ItemDrop);
            }

            // 착지 후 스케일 애니메이션
            if (enableScaleAnimation)
            {
                StartLandingScaleAnimation();
            }

            OnResourceLandedEvent?.Invoke();

            LogDebug("[ResourceDrop] 최종 착지 완료");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[ResourceDrop] 최종 착지 처리 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 초기 스케일 애니메이션
    /// </summary>
    private void StartScaleAnimation()
    {
        if (scaleAnimationCoroutine != null)
        {
            StopCoroutine(scaleAnimationCoroutine);
        }

        scaleAnimationCoroutine = StartCoroutine(ScaleAnimationCoroutine(true));
    }

    /// <summary>
    /// 착지 스케일 애니메이션
    /// </summary>
    private void StartLandingScaleAnimation()
    {
        if (scaleAnimationCoroutine != null)
        {
            StopCoroutine(scaleAnimationCoroutine);
        }

        scaleAnimationCoroutine = StartCoroutine(ScaleAnimationCoroutine(false));
    }

    /// <summary>
    /// 스케일 애니메이션 코루틴
    /// </summary>
    private System.Collections.IEnumerator ScaleAnimationCoroutine(bool isStart)
    {
        Vector3 originalScale = transform.localScale;
        Vector3 targetScale = isStart ? originalScale * 1.2f : originalScale * 0.9f;
        Vector3 finalScale = isStart ? originalScale : originalScale;

        float elapsed = 0f;
        float duration = scaleAnimationDuration;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            float curveValue = Mathf.Sin(progress * Mathf.PI);

            transform.localScale = Vector3.Lerp(originalScale, targetScale, curveValue);
            yield return null;
        }

        // 최종 스케일로 조정
        transform.localScale = finalScale;

        scaleAnimationCoroutine = null;
    }

    /// <summary>
    /// 낙하 속도 설정
    /// </summary>
    public void SetFallSpeed(float speed)
    {
        fallSpeed = Mathf.Max(0f, speed);
        currentVelocity = Vector3.down * fallSpeed;
        LogDebug($"[ResourceDrop] 낙하 속도 설정: {fallSpeed}");
    }

    /// <summary>
    /// 중력 설정
    /// </summary>
    public void SetGravity(float gravityValue)
    {
        gravity = Mathf.Max(0f, gravityValue);
        LogDebug($"[ResourceDrop] 중력 설정: {gravity}");
    }

    /// <summary>
    /// 낙하 거리 설정
    /// </summary>
    public void SetFallDistance(float distance)
    {
        fallDistance = Mathf.Max(0f, distance);
        LogDebug($"[ResourceDrop] 낙하 거리 설정: {fallDistance}");
    }

    /// <summary>
    /// 바운스 설정
    /// </summary>
    public void SetBounceSettings(float height, int maxBounceCount)
    {
        bounceHeight = Mathf.Max(0f, height);
        maxBounces = Mathf.Max(0, maxBounceCount);
        LogDebug($"[ResourceDrop] 바운스 설정 - 높이: {bounceHeight}, 최대 횟수: {maxBounces}");
    }

    /// <summary>
    /// 회전 애니메이션 활성화/비활성화
    /// </summary>
    public void SetRotationEnabled(bool enabled)
    {
        enableRotation = enabled;
        LogDebug($"[ResourceDrop] 회전 애니메이션 {(enabled ? "활성화" : "비활성화")}");
    }

    /// <summary>
    /// 스케일 애니메이션 활성화/비활성화
    /// </summary>
    public void SetScaleAnimationEnabled(bool enabled)
    {
        enableScaleAnimation = enabled;
        LogDebug($"[ResourceDrop] 스케일 애니메이션 {(enabled ? "활성화" : "비활성화")}");
    }

    /// <summary>
    /// 사운드 활성화/비활성화
    /// </summary>
    public void SetSoundEnabled(bool enabled)
    {
        enableDropSound = enabled;
        LogDebug($"[ResourceDrop] 사운드 {(enabled ? "활성화" : "비활성화")}");
    }

    /// <summary>
    /// 파티클 효과 활성화/비활성화
    /// </summary>
    public void SetParticleEffectEnabled(bool enabled)
    {
        enableParticleEffect = enabled;
        LogDebug($"[ResourceDrop] 파티클 효과 {(enabled ? "활성화" : "비활성화")}");
    }

    /// <summary>
    /// 낙하 상태 리셋
    /// </summary>
    public void ResetDrop()
    {
        try
        {
            transform.position = startPosition;
            currentVelocity = Vector3.down * fallSpeed;
            fallenDistance = 0f;
            bounceCount = 0;
            isFalling = true;
            hasLanded = false;

            if (scaleAnimationCoroutine != null)
            {
                StopCoroutine(scaleAnimationCoroutine);
                scaleAnimationCoroutine = null;
            }

            LogDebug("[ResourceDrop] 낙하 상태 리셋 완료");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[ResourceDrop] 상태 리셋 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 자원 낙하 정보 반환
    /// </summary>
    public string GetResourceDropInfo()
    {
        var info = new System.Text.StringBuilder();
        info.AppendLine($"[ResourceDrop 정보]");
        info.AppendLine($"낙하 중: {isFalling}");
        info.AppendLine($"착지됨: {hasLanded}");
        info.AppendLine($"낙하 거리: {fallenDistance:F1}");
        info.AppendLine($"바운스 횟수: {bounceCount}");
        info.AppendLine($"현재 속도: {currentVelocity.magnitude:F1}");
        info.AppendLine($"회전 애니메이션: {(enableRotation ? "활성화" : "비활성화")}");
        info.AppendLine($"스케일 애니메이션: {(enableScaleAnimation ? "활성화" : "비활성화")}");

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
        if (scaleAnimationCoroutine != null)
        {
            StopCoroutine(scaleAnimationCoroutine);
        }

        // 이벤트 구독 해제
        OnResourceLandedEvent = null;
        OnResourceBouncedEvent = null;
    }
}
