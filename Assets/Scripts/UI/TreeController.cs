using UnityEngine;
using System.Collections;
using GGumtles.Managers;

namespace GGumtles.UI
{
    public class TreeController : MonoBehaviour
{
    public static TreeController Instance { get; private set; }

    [Header("드롭 확률")]
    [SerializeField] private float acornOdd = 0.002f;
    [SerializeField] private float diamondOdd = 0.001f;
    [SerializeField] private float oddIncreaseAmount = 0.002f;

    [Header("프리팹")]
    [SerializeField] private GameObject acornPrefab;
    [SerializeField] private GameObject diamondPrefab;

    [Header("드롭 위치")]
    [SerializeField] private Transform dropOrigin;
    [SerializeField] private Transform dropParent; // 드롭 프리팹 부모 (예: TreeButton/전용 컨테이너)
    [SerializeField] private float dropRangeX = 200f;
    [SerializeField] private float dropRangeY = 20f;

    [Header("디버그")]
    [SerializeField] private bool enableDebugLogs = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (dropOrigin == null)
            dropOrigin = transform;
    }

    /// <summary>
    /// 나무 흔들기
    /// </summary>
    public void ShakeTree()
    {   
        // 현재 웜이 있으면 통계 업데이트
        if (WormManager.Instance?.CurrentWorm != null)
        {
            WormManager.Instance.CurrentWorm.statistics.totalShakeCount++;
        }
        
        // 아이템 드롭 처리
        ProcessItemDrops();
    }

    /// <summary>
    /// 아이템 드롭 처리
    /// </summary>
    private void ProcessItemDrops()
    {
        // 도토리 드롭 확률 체크
        if (Random.value < acornOdd)
        {
            DropAcorn();
            acornOdd = 0.002f; // 확률 초기화
        }
        else
        {
            // 확률 증가
            acornOdd += oddIncreaseAmount;
        }

        // 다이아몬드 드롭 확률 체크
        if (Random.value < diamondOdd)
        {
            DropDiamond();
        }
    }

    /// <summary>
    /// 도토리 드롭
    /// </summary>
    private void DropAcorn()
    {
        if (acornPrefab == null) return;

        Vector3 dropPosition = GetRandomDropPosition();
        Instantiate(acornPrefab, dropPosition, Quaternion.identity, dropParent != null ? dropParent : dropOrigin);
        
        LogDebug("[TreeController] 도토리 드롭");
    }

    /// <summary>
    /// 다이아몬드 드롭
    /// </summary>
    private void DropDiamond()
    {
        if (diamondPrefab == null) return;

        Vector3 dropPosition = GetRandomDropPosition();
        Instantiate(diamondPrefab, dropPosition, Quaternion.identity, dropParent != null ? dropParent : dropOrigin);
        
        LogDebug("[TreeController] 다이아몬드 드롭");
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

    private void LogDebug(string message)
    {
        if (enableDebugLogs)
        {
            Debug.Log(message);
        }
    }
    }
}
