using UnityEngine;

namespace GGumtles.UI
{
    /// <summary>
    /// AcornFeed 버튼에서 호출: 도토리를 X 범위 내에서 DropParent 아래로 생성하고, AcornFeedDrop 초기화
    /// </summary>
    public class AcornFeedSpawner : MonoBehaviour
    {
        [Header("프리팹")]
        [SerializeField] private GameObject feedAcornPrefab; // AcornFeedDrop 포함 프리팹

        [Header("스폰 설정")]
        [SerializeField] private Transform dropOrigin;   // 시작 높이/위치 기준
        [SerializeField] private Transform dropParent;   // 부모(캔버스/컨테이너)
        [SerializeField] private Vector2 xRange = new Vector2(-480f, 480f); // 로컬 X 범위

        public void SpawnAcorn()
        {
            if (feedAcornPrefab == null || dropOrigin == null)
            {
                Debug.LogWarning("[AcornFeedSpawner] 프리팹 또는 dropOrigin 미지정");
                return;
            }

            float x = Random.Range(xRange.x, xRange.y);
            Vector3 startPos = dropOrigin.position + new Vector3(x, 0f, 0f);
            Transform parent = dropParent != null ? dropParent : dropOrigin;

            var go = Instantiate(feedAcornPrefab, startPos, Quaternion.identity, parent);
            var drop = go.GetComponent<AcornFeedDrop>();
            if (drop != null)
            {
                drop.Initialize(startPos);
            }
        }

        

        
    }
}


