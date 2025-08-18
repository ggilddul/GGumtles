using UnityEngine;
using UnityEngine.UI;

public class PickDiamond : MonoBehaviour
{
    [Header("UI 컴포넌트")]
    [SerializeField] private Image diamondImage;
    [SerializeField] private Button pickButton;
    [SerializeField] private GameObject glowEffect;
    [SerializeField] private GameObject sparkleEffect;

    [Header("설정")]
    [SerializeField] private bool enableSound = true;
    [SerializeField] private bool enableDebugLogs = false;
    [SerializeField] private bool enableParticleEffect = true;
    [SerializeField] private int diamondValue = 1; // 획득할 다이아몬드 수

    [Header("애니메이션")]
    [SerializeField] private bool enableAnimations = true;
    [SerializeField] private float pickAnimationDuration = 0.8f;
    [SerializeField] private AnimationCurve pickCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("파티클 효과")]
    [SerializeField] private GameObject particlePrefab;
    [SerializeField] private Transform particleSpawnPoint;
    [SerializeField] private GameObject diamondSparklePrefab;

    [Header("특수 효과")]
    [SerializeField] private bool enableSparkleAnimation = true;
    [SerializeField] private float sparkleInterval = 0.3f;

    // 상태 관리
    private bool isPicked = false;
    private bool isAnimating = false;
    private Coroutine pickAnimationCoroutine;
    private Coroutine sparkleCoroutine;

    // 이벤트 정의
    public delegate void OnDiamondPicked(int amount);
    public delegate void OnDiamondPickStarted();
    public event OnDiamondPicked OnDiamondPickedEvent;
    public event OnDiamondPickStarted OnDiamondPickStartedEvent;

    // 프로퍼티
    public bool IsPicked => isPicked;
    public bool IsAnimating => isAnimating;
    public int DiamondValue => diamondValue;

    private void Awake()
    {
        InitializePickDiamond();
    }

    private void Start()
    {
        if (enableSparkleAnimation)
        {
            StartSparkleAnimation();
        }
    }

    private void InitializePickDiamond()
    {
        try
        {
            // 자동으로 컴포넌트 찾기
            if (diamondImage == null)
                diamondImage = GetComponentInChildren<Image>();

            if (pickButton == null)
                pickButton = GetComponent<Button>();

            if (pickButton == null)
                pickButton = gameObject.AddComponent<Button>();

            SetupUI();

            LogDebug("[PickDiamond] 다이아몬드 수집기 초기화 완료");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[PickDiamond] 초기화 중 오류: {ex.Message}");
        }
    }

    private void SetupUI()
    {
        try
        {
            // 버튼 이벤트 설정
            if (pickButton != null)
            {
                pickButton.onClick.RemoveAllListeners();
                pickButton.onClick.AddListener(Pick);
            }

            // 초기 상태 설정
            SetPickedState(false);

            LogDebug("[PickDiamond] UI 설정 완료");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[PickDiamond] UI 설정 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 다이아몬드 수집
    /// </summary>
    public void Pick()
    {
        if (isPicked || isAnimating) return;

        try
        {
            OnDiamondPickStartedEvent?.Invoke();

            if (enableAnimations)
            {
                StartPickAnimation();
            }
            else
            {
                ExecutePick();
            }

            LogDebug($"[PickDiamond] 다이아몬드 수집 시작 - 수량: {diamondValue}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[PickDiamond] 다이아몬드 수집 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 수집 실행
    /// </summary>
    private void ExecutePick()
    {
        try
        {
            // 사운드 재생
            if (enableSound)
            {
                AudioManager.Instance?.PlaySFX(AudioManager.SFXType.EarnItem);
            }

            // 다이아몬드 획득
            if (GameManager.Instance != null)
            {
                GameManager.Instance.diamondCount += diamondValue;
            }

            // 파티클 효과
            if (enableParticleEffect)
            {
                SpawnPickupParticles();
            }

            // 상태 변경
            SetPickedState(true);

            // 이벤트 발생
            OnDiamondPickedEvent?.Invoke(diamondValue);

            // 오브젝트 제거
            Destroy(gameObject, 0.1f);

            LogDebug($"[PickDiamond] 다이아몬드 수집 완료 - 획득: {diamondValue}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[PickDiamond] 수집 실행 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 수집 애니메이션 시작
    /// </summary>
    private void StartPickAnimation()
    {
        if (pickAnimationCoroutine != null)
        {
            StopCoroutine(pickAnimationCoroutine);
        }

        pickAnimationCoroutine = StartCoroutine(PickAnimationCoroutine());
    }

    /// <summary>
    /// 수집 애니메이션 코루틴
    /// </summary>
    private System.Collections.IEnumerator PickAnimationCoroutine()
    {
        isAnimating = true;

        // 사운드 재생
        if (enableSound)
        {
            AudioManager.Instance?.PlaySFX(AudioManager.SFXType.EarnItem);
        }

        float elapsed = 0f;
        Vector3 originalScale = transform.localScale;
        Vector3 originalPosition = transform.position;

        while (elapsed < pickAnimationDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / pickAnimationDuration;
            float curveValue = pickCurve.Evaluate(progress);

            // 스케일 애니메이션 (더 화려한 효과)
            float scaleMultiplier = 1f + Mathf.Sin(progress * Mathf.PI * 2) * 0.4f;
            transform.localScale = originalScale * scaleMultiplier;

            // 위치 애니메이션 (위로 올라가면서 회전)
            Vector3 newPosition = originalPosition + Vector3.up * curveValue * 3f;
            transform.position = newPosition;
            transform.Rotate(0f, 720f * curveValue, 0f); // 2바퀴 회전

            // 색상 애니메이션 (반짝이는 효과)
            if (diamondImage != null)
            {
                Color originalColor = Color.white;
                Color sparkleColor = new Color(1f, 1f, 0.8f, 1f); // 황금빛
                diamondImage.color = Color.Lerp(originalColor, sparkleColor, Mathf.Sin(progress * Mathf.PI * 4));
            }

            yield return null;
        }

        // 파티클 효과
        if (enableParticleEffect)
        {
            SpawnPickupParticles();
        }

        // 다이아몬드 획득
        if (GameManager.Instance != null)
        {
            GameManager.Instance.diamondCount += diamondValue;
        }

        // 상태 변경
        SetPickedState(true);

        // 이벤트 발생
        OnDiamondPickedEvent?.Invoke(diamondValue);

        // 오브젝트 제거
        Destroy(gameObject);

        isAnimating = false;
        pickAnimationCoroutine = null;
    }

    /// <summary>
    /// 반짝임 애니메이션 시작
    /// </summary>
    private void StartSparkleAnimation()
    {
        if (sparkleCoroutine != null)
        {
            StopCoroutine(sparkleCoroutine);
        }

        sparkleCoroutine = StartCoroutine(SparkleAnimationCoroutine());
    }

    /// <summary>
    /// 반짝임 애니메이션 코루틴
    /// </summary>
    private System.Collections.IEnumerator SparkleAnimationCoroutine()
    {
        while (!isPicked)
        {
            // 반짝임 효과 생성
            if (sparkleEffect != null)
            {
                sparkleEffect.SetActive(true);
                yield return new WaitForSeconds(0.1f);
                sparkleEffect.SetActive(false);
            }

            // 다이아몬드 스파클 파티클 생성
            if (diamondSparklePrefab != null)
            {
                Vector3 spawnPosition = transform.position + Random.insideUnitSphere * 0.5f;
                GameObject sparkle = Instantiate(diamondSparklePrefab, spawnPosition, Quaternion.identity);
                Destroy(sparkle, 1f);
            }

            yield return new WaitForSeconds(sparkleInterval);
        }
    }

    /// <summary>
    /// 수집 파티클 생성
    /// </summary>
    private void SpawnPickupParticles()
    {
        try
        {
            if (particlePrefab != null)
            {
                Vector3 spawnPosition = particleSpawnPoint != null ? 
                    particleSpawnPoint.position : transform.position;

                GameObject particle = Instantiate(particlePrefab, spawnPosition, Quaternion.identity);
                
                // 파티클 자동 제거
                Destroy(particle, 3f);
            }

            // 추가 다이아몬드 파티클 효과
            if (diamondSparklePrefab != null)
            {
                for (int i = 0; i < 5; i++)
                {
                    Vector3 randomPosition = transform.position + Random.insideUnitSphere * 1f;
                    GameObject sparkle = Instantiate(diamondSparklePrefab, randomPosition, Quaternion.identity);
                    Destroy(sparkle, 2f);
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[PickDiamond] 파티클 생성 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 수집 상태 설정
    /// </summary>
    private void SetPickedState(bool picked)
    {
        isPicked = picked;

        // UI 업데이트
        if (diamondImage != null)
        {
            diamondImage.color = picked ? new Color(1f, 1f, 1f, 0.5f) : Color.white;
        }

        if (glowEffect != null)
        {
            glowEffect.SetActive(!picked);
        }

        if (sparkleEffect != null)
        {
            sparkleEffect.SetActive(!picked);
        }

        if (pickButton != null)
        {
            pickButton.interactable = !picked;
        }

        // 반짝임 애니메이션 정지
        if (picked && sparkleCoroutine != null)
        {
            StopCoroutine(sparkleCoroutine);
            sparkleCoroutine = null;
        }
    }

    /// <summary>
    /// 다이아몬드 수량 설정
    /// </summary>
    public void SetDiamondValue(int value)
    {
        diamondValue = Mathf.Max(1, value);
        LogDebug($"[PickDiamond] 다이아몬드 수량 설정: {diamondValue}");
    }

    /// <summary>
    /// 애니메이션 활성화/비활성화
    /// </summary>
    public void SetAnimationEnabled(bool enabled)
    {
        enableAnimations = enabled;
        LogDebug($"[PickDiamond] 애니메이션 {(enabled ? "활성화" : "비활성화")}");
    }

    /// <summary>
    /// 사운드 활성화/비활성화
    /// </summary>
    public void SetSoundEnabled(bool enabled)
    {
        enableSound = enabled;
        LogDebug($"[PickDiamond] 사운드 {(enabled ? "활성화" : "비활성화")}");
    }

    /// <summary>
    /// 파티클 효과 활성화/비활성화
    /// </summary>
    public void SetParticleEffectEnabled(bool enabled)
    {
        enableParticleEffect = enabled;
        LogDebug($"[PickDiamond] 파티클 효과 {(enabled ? "활성화" : "비활성화")}");
    }

    /// <summary>
    /// 반짝임 애니메이션 활성화/비활성화
    /// </summary>
    public void SetSparkleAnimationEnabled(bool enabled)
    {
        enableSparkleAnimation = enabled;
        
        if (enabled && !isPicked)
        {
            StartSparkleAnimation();
        }
        else if (!enabled && sparkleCoroutine != null)
        {
            StopCoroutine(sparkleCoroutine);
            sparkleCoroutine = null;
        }

        LogDebug($"[PickDiamond] 반짝임 애니메이션 {(enabled ? "활성화" : "비활성화")}");
    }

    /// <summary>
    /// 수집 가능 상태로 리셋
    /// </summary>
    public void Reset()
    {
        try
        {
            isPicked = false;
            isAnimating = false;

            if (pickAnimationCoroutine != null)
            {
                StopCoroutine(pickAnimationCoroutine);
                pickAnimationCoroutine = null;
            }

            if (sparkleCoroutine != null)
            {
                StopCoroutine(sparkleCoroutine);
                sparkleCoroutine = null;
            }

            SetPickedState(false);

            if (enableSparkleAnimation)
            {
                StartSparkleAnimation();
            }

            LogDebug("[PickDiamond] 상태 리셋 완료");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[PickDiamond] 상태 리셋 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 다이아몬드 수집기 정보 반환
    /// </summary>
    public string GetPickDiamondInfo()
    {
        var info = new System.Text.StringBuilder();
        info.AppendLine($"[PickDiamond 정보]");
        info.AppendLine($"수집됨: {isPicked}");
        info.AppendLine($"애니메이션 중: {isAnimating}");
        info.AppendLine($"다이아몬드 수량: {diamondValue}");
        info.AppendLine($"애니메이션: {(enableAnimations ? "활성화" : "비활성화")}");
        info.AppendLine($"사운드: {(enableSound ? "활성화" : "비활성화")}");
        info.AppendLine($"파티클 효과: {(enableParticleEffect ? "활성화" : "비활성화")}");
        info.AppendLine($"반짝임 애니메이션: {(enableSparkleAnimation ? "활성화" : "비활성화")}");

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
        if (pickAnimationCoroutine != null)
        {
            StopCoroutine(pickAnimationCoroutine);
        }

        if (sparkleCoroutine != null)
        {
            StopCoroutine(sparkleCoroutine);
        }

        // 이벤트 구독 해제
        OnDiamondPickedEvent = null;
        OnDiamondPickStartedEvent = null;
    }
}
