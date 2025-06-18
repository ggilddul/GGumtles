using UnityEngine;

public class TreeController : MonoBehaviour
{
    [Header("Drop Odds")]
    public float acornOdd = 0.002f;
    public float diamondOdd = 0.002f;
    public float oddIncreaseAmount = 0.002f;

    [Header("Prefabs")]
    public GameObject acornPrefab;
    public GameObject diamondPrefab;

    [Header("Drop Settings")]

    public Transform dropOrigin; // 중심점 (보통 나무 아래)
    public Transform parentTransform;
    public float dropRangeX = 200f; // X축 랜덤 범위
    public float dropRangeY = 20f;  // Y축 초기 랜덤 오프셋 (선택)

    private Vector3 GetRandomDropPosition()
    {
        float offsetX = Random.Range(-dropRangeX, dropRangeX);
        float offsetY = Random.Range(-dropRangeY, dropRangeY);
        return dropOrigin.position + new Vector3(offsetX, offsetY, 0f);
    }

    [Header("SFX")]
    public AudioClip itemDropSfx;

    private void ItemDropSound()
    {
        if (itemDropSfx != null)
            AudioManager.Instance?.PlaySFX(itemDropSfx);
    }

    public void ShakeTree()
    {
        // 도토리 드롭 확률
        if (Random.value < acornOdd)
        {
            DropItem(acornPrefab);
            acornOdd = 0.002f; // 초기화
        }
        else
        {
            acornOdd += oddIncreaseAmount;
        }

        // 다이아몬드 드롭 확률
        if (Random.value < diamondOdd)
        {
            DropItem(diamondPrefab);
        }
    }

    private void DropItem(GameObject itemPrefab)
    {
        Instantiate(itemPrefab, GetRandomDropPosition(), Quaternion.identity, parentTransform);
        ItemDropSound();
    }
}
