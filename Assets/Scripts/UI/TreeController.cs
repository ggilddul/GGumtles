using UnityEngine;
using System.Collections;

public class TreeController : MonoBehaviour
{
    [Header("드롭 확률 설정")]
    [SerializeField] private float acornOdd = 0.002f;
    [SerializeField] private float diamondOdd = 0.002f;
    [SerializeField] private float oddIncreaseAmount = 0.002f;
    [SerializeField] private float maxAcornOdd = 0.1f; // 최대 도토리 확률

    [Header("프리팹 설정")]
    [SerializeField] private GameObject acornPrefab;
    [SerializeField] private GameObject diamondPrefab;

    [Header("드롭 위치 설정")]
    [SerializeField] private Transform dropOrigin; // 중심점 (보통 나무 아래)
    [SerializeField] private Transform parentTransform;
    [SerializeField] private float dropRangeX = 200f; // X축 랜덤 범위
    [SerializeField] private float dropRangeY = 20f;  // Y축 초기 랜덤 오프셋

    [Header("나무 흔들기 설정")]
    [SerializeField] private bool enableShakeAnimation = true;
    [SerializeField] private float shakeDuration = 0.5f;
    [SerializeField] private float shakeIntensity = 5f;
    [SerializeField] private int shakeCount = 3;

    [Header("드롭 효과")]
    [SerializeField] private bool enableDropSound = true;
    [SerializeField] private bool enableDropParticles = true;
    [SerializeField] private GameObject dropParticlePrefab;
    [SerializeField] private float dropDelay = 0.2f; // 드롭 지연 시간

    [Header("디버그")]
    [SerializeField] private bool enableDebugLogs = false;

    // 상태 관리
    private bool isShaking = false;
    private Coroutine shakeCoroutine;
    private Vector3 originalPosition;

    // 이벤트 정의
    public delegate void OnTreeShake();
    public delegate void OnItemDropped(GameObject item, Vector3 position);
    public delegate void OnAcornDropped();
    public delegate void OnDiamondDropped();
    public event OnTreeShake OnTreeShakeEvent;
    public event OnItemDropped OnItemDroppedEvent;
    public event OnAcornDropped OnAcornDroppedEvent;
    public event OnDiamondDropped OnDiamondDroppedEvent;

    // 프로퍼티
    public float AcornOdd => acornOdd;
    public float DiamondOdd => diamondOdd;
    public bool IsShaking => isShaking;
    public Vector3 DropOriginPosition => dropOrigin != null ? dropOrigin.position : transform.position;

    private void Start()
    {
        InitializeTreeController();
    }

    private void InitializeTreeController()
    {
        try
        {
            originalPosition = transform.position;

            // dropOrigin이 설정되지 않은 경우 현재 위치 사용
            if (dropOrigin == null)
            {
                dropOrigin = transform;
            }

            // parentTransform이 설정되지 않은 경우 현재 트랜스폼 사용
            if (parentTransform == null)
            {
                parentTransform = transform;
            }

            LogDebug("[TreeController] 나무 컨트롤러 초기화 완료");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[TreeController] 초기화 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 나무 흔들기 (공개 메서드)
    /// </summary>
    public void ShakeTree()
    {
        if (isShaking) return;

        try
        {
            StartShakeTree();
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[TreeController] 나무 흔들기 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 나무 흔들기 시작
    /// </summary>
    private void StartShakeTree()
    {
        isShaking = true;
        OnTreeShakeEvent?.Invoke();

        // 나무 흔들기 사운드 재생
        AudioManager.Instance?.PlaySFX(AudioManager.SFXType.ShakeTree);

        // 흔들기 애니메이션 시작
        if (enableShakeAnimation)
        {
            if (shakeCoroutine != null)
            {
                StopCoroutine(shakeCoroutine);
            }
            shakeCoroutine = StartCoroutine(ShakeTreeCoroutine());
        }

        // 아이템 드롭 처리
        StartCoroutine(ProcessItemDrops());

        LogDebug("[TreeController] 나무 흔들기 시작");
    }

    /// <summary>
    /// 나무 흔들기 애니메이션 코루틴
    /// </summary>
    private IEnumerator ShakeTreeCoroutine()
    {
        Vector3 startPosition = transform.position;
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / shakeDuration;

            // 사인파를 이용한 흔들기 효과
            float shakeX = Mathf.Sin(progress * shakeCount * Mathf.PI * 2) * shakeIntensity;
            float shakeY = Mathf.Sin(progress * shakeCount * Mathf.PI * 2 + Mathf.PI * 0.5f) * shakeIntensity * 0.5f;

            transform.position = startPosition + new Vector3(shakeX, shakeY, 0f);

            yield return null;
        }

        // 원래 위치로 복원
        transform.position = startPosition;
        isShaking = false;
        shakeCoroutine = null;

        LogDebug("[TreeController] 나무 흔들기 완료");
    }

    /// <summary>
    /// 아이템 드롭 처리
    /// </summary>
    private IEnumerator ProcessItemDrops()
    {
        // 드롭 지연
        yield return new WaitForSeconds(dropDelay);

        bool acornDropped = false;
        bool diamondDropped = false;

        // 도토리 드롭 확률 체크
        if (Random.value < acornOdd)
        {
            DropAcorn();
            acornDropped = true;
            acornOdd = 0.002f; // 확률 초기화
        }
        else
        {
            // 확률 증가 (최대값 제한)
            acornOdd = Mathf.Min(acornOdd + oddIncreaseAmount, maxAcornOdd);
        }

        // 다이아몬드 드롭 확률 체크
        if (Random.value < diamondOdd)
        {
            DropDiamond();
            diamondDropped = true;
        }

        // 드롭 결과 로그
        if (acornDropped || diamondDropped)
        {
            LogDebug($"[TreeController] 아이템 드롭 - 도토리: {acornDropped}, 다이아몬드: {diamondDropped}");
        }
    }

    /// <summary>
    /// 도토리 드롭
    /// </summary>
    private void DropAcorn()
    {
        if (acornPrefab == null)
        {
            Debug.LogWarning("[TreeController] 도토리 프리팹이 설정되지 않았습니다.");
            return;
        }

        Vector3 dropPosition = GetRandomDropPosition();
        GameObject acorn = Instantiate(acornPrefab, dropPosition, Quaternion.identity, parentTransform);

        // ResourceDrop 컴포넌트가 있다면 초기화
        var resourceDrop = acorn.GetComponent<ResourceDrop>();
        if (resourceDrop != null)
        {
            resourceDrop.ResetDrop();
        }

        OnAcornDroppedEvent?.Invoke();
        OnItemDroppedEvent?.Invoke(acorn, dropPosition);

        PlayDropEffects(dropPosition);
    }

    /// <summary>
    /// 다이아몬드 드롭
    /// </summary>
    private void DropDiamond()
    {
        if (diamondPrefab == null)
        {
            Debug.LogWarning("[TreeController] 다이아몬드 프리팹이 설정되지 않았습니다.");
            return;
        }

        Vector3 dropPosition = GetRandomDropPosition();
        GameObject diamond = Instantiate(diamondPrefab, dropPosition, Quaternion.identity, parentTransform);

        // ResourceDrop 컴포넌트가 있다면 초기화
        var resourceDrop = diamond.GetComponent<ResourceDrop>();
        if (resourceDrop != null)
        {
            resourceDrop.ResetDrop();
        }

        OnDiamondDroppedEvent?.Invoke();
        OnItemDroppedEvent?.Invoke(diamond, dropPosition);

        PlayDropEffects(dropPosition);
    }

    /// <summary>
    /// 랜덤 드롭 위치 계산
    /// </summary>
    private Vector3 GetRandomDropPosition()
    {
        float offsetX = Random.Range(-dropRangeX, dropRangeX);
        float offsetY = Random.Range(-dropRangeY, dropRangeY);
        return dropOrigin.position + new Vector3(offsetX, offsetY, 0f);
    }

    /// <summary>
    /// 드롭 효과 재생
    /// </summary>
    private void PlayDropEffects(Vector3 position)
    {
        // 드롭 사운드
        if (enableDropSound)
        {
            AudioManager.Instance?.PlaySFX(AudioManager.SFXType.ItemDrop);
        }

        // 드롭 파티클
        if (enableDropParticles && dropParticlePrefab != null)
        {
            Instantiate(dropParticlePrefab, position, Quaternion.identity);
        }
    }

    /// <summary>
    /// 도토리 확률 설정
    /// </summary>
    public void SetAcornOdd(float odd)
    {
        acornOdd = Mathf.Clamp(odd, 0f, maxAcornOdd);
        LogDebug($"[TreeController] 도토리 확률 설정: {acornOdd}");
    }

    /// <summary>
    /// 다이아몬드 확률 설정
    /// </summary>
    public void SetDiamondOdd(float odd)
    {
        diamondOdd = Mathf.Clamp(odd, 0f, 1f);
        LogDebug($"[TreeController] 다이아몬드 확률 설정: {diamondOdd}");
    }

    /// <summary>
    /// 확률 증가량 설정
    /// </summary>
    public void SetOddIncreaseAmount(float amount)
    {
        oddIncreaseAmount = Mathf.Max(0f, amount);
        LogDebug($"[TreeController] 확률 증가량 설정: {oddIncreaseAmount}");
    }

    /// <summary>
    /// 최대 도토리 확률 설정
    /// </summary>
    public void SetMaxAcornOdd(float maxOdd)
    {
        maxAcornOdd = Mathf.Clamp(maxOdd, 0f, 1f);
        LogDebug($"[TreeController] 최대 도토리 확률 설정: {maxAcornOdd}");
    }

    /// <summary>
    /// 드롭 범위 설정
    /// </summary>
    public void SetDropRange(float rangeX, float rangeY)
    {
        dropRangeX = Mathf.Max(0f, rangeX);
        dropRangeY = Mathf.Max(0f, rangeY);
        LogDebug($"[TreeController] 드롭 범위 설정 - X: {dropRangeX}, Y: {dropRangeY}");
    }

    /// <summary>
    /// 흔들기 애니메이션 설정
    /// </summary>
    public void SetShakeSettings(float duration, float intensity, int count)
    {
        shakeDuration = Mathf.Max(0f, duration);
        shakeIntensity = Mathf.Max(0f, intensity);
        shakeCount = Mathf.Max(1, count);
        LogDebug($"[TreeController] 흔들기 설정 - 지속시간: {shakeDuration}, 강도: {shakeIntensity}, 횟수: {shakeCount}");
    }

    /// <summary>
    /// 흔들기 애니메이션 활성화/비활성화
    /// </summary>
    public void SetShakeAnimationEnabled(bool enabled)
    {
        enableShakeAnimation = enabled;
        LogDebug($"[TreeController] 흔들기 애니메이션 {(enabled ? "활성화" : "비활성화")}");
    }

    /// <summary>
    /// 드롭 사운드 활성화/비활성화
    /// </summary>
    public void SetDropSoundEnabled(bool enabled)
    {
        enableDropSound = enabled;
        LogDebug($"[TreeController] 드롭 사운드 {(enabled ? "활성화" : "비활성화")}");
    }

    /// <summary>
    /// 드롭 파티클 활성화/비활성화
    /// </summary>
    public void SetDropParticleEnabled(bool enabled)
    {
        enableDropParticles = enabled;
        LogDebug($"[TreeController] 드롭 파티클 {(enabled ? "활성화" : "비활성화")}");
    }

    /// <summary>
    /// 확률 초기화
    /// </summary>
    public void ResetOdds()
    {
        acornOdd = 0.002f;
        diamondOdd = 0.002f;
        LogDebug("[TreeController] 확률 초기화 완료");
    }

    /// <summary>
    /// 나무 컨트롤러 정보 반환
    /// </summary>
    public string GetTreeControllerInfo()
    {
        var info = new System.Text.StringBuilder();
        info.AppendLine($"[TreeController 정보]");
        info.AppendLine($"도토리 확률: {acornOdd:F4}");
        info.AppendLine($"다이아몬드 확률: {diamondOdd:F4}");
        info.AppendLine($"확률 증가량: {oddIncreaseAmount:F4}");
        info.AppendLine($"최대 도토리 확률: {maxAcornOdd:F4}");
        info.AppendLine($"흔들기 중: {isShaking}");
        info.AppendLine($"드롭 범위 X: {dropRangeX}");
        info.AppendLine($"드롭 범위 Y: {dropRangeY}");
        info.AppendLine($"흔들기 애니메이션: {(enableShakeAnimation ? "활성화" : "비활성화")}");
        info.AppendLine($"드롭 사운드: {(enableDropSound ? "활성화" : "비활성화")}");
        info.AppendLine($"드롭 파티클: {(enableDropParticles ? "활성화" : "비활성화")}");

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
        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
        }

        // 이벤트 구독 해제
        OnTreeShakeEvent = null;
        OnItemDroppedEvent = null;
        OnAcornDroppedEvent = null;
        OnDiamondDroppedEvent = null;
    }
}
