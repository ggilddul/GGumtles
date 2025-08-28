using UnityEngine;
using UnityEngine.UI;

public class PickAcorn : MonoBehaviour
{
    [Header("UI 컴포넌트")]
    [SerializeField] private Image acornImage;
    [SerializeField] private Button pickButton;
    [SerializeField] private GameObject glowEffect;

    [Header("설정")]
    [SerializeField] private bool enableSound = true;
    [SerializeField] private bool enableDebugLogs = false;
    [SerializeField] private bool enableParticleEffect = true;
    [SerializeField] private int acornValue = 1; // 획득할 도토리 수

    [Header("애니메이션")]
    [SerializeField] private bool enableAnimations = true;
    [SerializeField] private float pickAnimationDuration = 0.5f;
    [SerializeField] private AnimationCurve pickCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("파티클 효과")]
    [SerializeField] private GameObject particlePrefab;
    [SerializeField] private Transform particleSpawnPoint;

    // 상태 관리
    private bool isPicked = false;
    private bool isAnimating = false;
    private Coroutine pickAnimationCoroutine;

    // 이벤트 정의
    public delegate void OnAcornPicked(int amount);
    public delegate void OnAcornPickStarted();
    public event OnAcornPicked OnAcornPickedEvent;
    public event OnAcornPickStarted OnAcornPickStartedEvent;

    // 프로퍼티
    public bool IsPicked => isPicked;
    public bool IsAnimating => isAnimating;
    public int AcornValue => acornValue;

    private void Awake()
    {
        InitializePickAcorn();
    }

    private void InitializePickAcorn()
    {
        try
        {
            // 자동으로 컴포넌트 찾기
            if (acornImage == null)
                acornImage = GetComponentInChildren<Image>();

            if (pickButton == null)
                pickButton = GetComponent<Button>();

            if (pickButton == null)
                pickButton = gameObject.AddComponent<Button>();

            SetupUI();

            LogDebug("[PickAcorn] 도토리 수집기 초기화 완료");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[PickAcorn] 초기화 중 오류: {ex.Message}");
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

            LogDebug("[PickAcorn] UI 설정 완료");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[PickAcorn] UI 설정 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 도토리 수집
    /// </summary>
    public void Pick()
    {
        if (isPicked || isAnimating) return;

        try
        {
            OnAcornPickStartedEvent?.Invoke();

            if (enableAnimations)
            {
                StartPickAnimation();
            }
            else
            {
                ExecutePick();
            }

            LogDebug($"[PickAcorn] 도토리 수집 시작 - 수량: {acornValue}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[PickAcorn] 도토리 수집 중 오류: {ex.Message}");
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

            // 도토리 획득
            if (GameManager.Instance != null)
            {
                GameManager.Instance.diamondCount += acornValue;
            }

            // 파티클 효과
            if (enableParticleEffect)
            {
                SpawnPickupParticles();
            }

            // 상태 변경
            SetPickedState(true);

            // 이벤트 발생
            OnAcornPickedEvent?.Invoke(acornValue);

            // 오브젝트 제거
            Destroy(gameObject, 0.1f);

            LogDebug($"[PickAcorn] 도토리 수집 완료 - 획득: {acornValue}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[PickAcorn] 수집 실행 중 오류: {ex.Message}");
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

            // 스케일 애니메이션 (커졌다가 작아짐)
            float scaleMultiplier = 1f + Mathf.Sin(progress * Mathf.PI) * 0.3f;
            transform.localScale = originalScale * scaleMultiplier;

            // 위치 애니메이션 (위로 올라가면서 회전)
            Vector3 newPosition = originalPosition + Vector3.up * curveValue * 2f;
            transform.position = newPosition;
            transform.Rotate(0f, 360f * curveValue, 0f);

            yield return null;
        }

        // 파티클 효과
        if (enableParticleEffect)
        {
            SpawnPickupParticles();
        }

        // 도토리 획득
        if (GameManager.Instance != null)
        {
            GameManager.Instance.diamondCount += acornValue;
        }

        // 상태 변경
        SetPickedState(true);

        // 이벤트 발생
        OnAcornPickedEvent?.Invoke(acornValue);

        // 오브젝트 제거
        Destroy(gameObject);

        isAnimating = false;
        pickAnimationCoroutine = null;
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
                Destroy(particle, 2f);
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[PickAcorn] 파티클 생성 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 수집 상태 설정
    /// </summary>
    private void SetPickedState(bool picked)
    {
        isPicked = picked;

        // UI 업데이트
        if (acornImage != null)
        {
            acornImage.color = picked ? new Color(1f, 1f, 1f, 0.5f) : Color.white;
        }

        if (glowEffect != null)
        {
            glowEffect.SetActive(!picked);
        }

        if (pickButton != null)
        {
            pickButton.interactable = !picked;
        }
    }

    /// <summary>
    /// 도토리 수량 설정
    /// </summary>
    public void SetAcornValue(int value)
    {
        acornValue = Mathf.Max(1, value);
        LogDebug($"[PickAcorn] 도토리 수량 설정: {acornValue}");
    }

    /// <summary>
    /// 애니메이션 활성화/비활성화
    /// </summary>
    public void SetAnimationEnabled(bool enabled)
    {
        enableAnimations = enabled;
        LogDebug($"[PickAcorn] 애니메이션 {(enabled ? "활성화" : "비활성화")}");
    }

    /// <summary>
    /// 사운드 활성화/비활성화
    /// </summary>
    public void SetSoundEnabled(bool enabled)
    {
        enableSound = enabled;
        LogDebug($"[PickAcorn] 사운드 {(enabled ? "활성화" : "비활성화")}");
    }

    /// <summary>
    /// 파티클 효과 활성화/비활성화
    /// </summary>
    public void SetParticleEffectEnabled(bool enabled)
    {
        enableParticleEffect = enabled;
        LogDebug($"[PickAcorn] 파티클 효과 {(enabled ? "활성화" : "비활성화")}");
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

            SetPickedState(false);
            LogDebug("[PickAcorn] 상태 리셋 완료");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[PickAcorn] 상태 리셋 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 도토리 수집기 정보 반환
    /// </summary>
    public string GetPickAcornInfo()
    {
        var info = new System.Text.StringBuilder();
        info.AppendLine($"[PickAcorn 정보]");
        info.AppendLine($"수집됨: {isPicked}");
        info.AppendLine($"애니메이션 중: {isAnimating}");
        info.AppendLine($"도토리 수량: {acornValue}");
        info.AppendLine($"애니메이션: {(enableAnimations ? "활성화" : "비활성화")}");
        info.AppendLine($"사운드: {(enableSound ? "활성화" : "비활성화")}");
        info.AppendLine($"파티클 효과: {(enableParticleEffect ? "활성화" : "비활성화")}");

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

        // 이벤트 구독 해제
        OnAcornPickedEvent = null;
        OnAcornPickStartedEvent = null;
    }
}
