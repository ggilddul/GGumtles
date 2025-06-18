using UnityEngine;

public class WormFamilyManager : MonoBehaviour
{
    public GameObject nonLeafWormPrefab;    // 기존 웜 정보용 프리팹
    public Transform content;               // Content 오브젝트
    public LeafWormUI leafWormUI;           // 현재 웜 (항상 화면 맨 아래에 위치해야 함)

    public void AddGeneration(WormData newWormData)
    {
        // 1. 현재 LeafWorm의 데이터 백업
        WormData previousData = leafWormUI.GetCurrentData();

        // 2. 조상 노드 생성 및 설정
        GameObject node = Instantiate(nonLeafWormPrefab);
        node.name = $"WormNode_Gen{previousData.gen}";

        WormNodeUI nodeUI = node.GetComponent<WormNodeUI>();
        nodeUI.SetData(previousData);

        // 3. LeafWorm이 Content의 첫 번째 자식인지 보장
        leafWormUI.transform.SetParent(content, false);
        leafWormUI.transform.SetSiblingIndex(0);  // 항상 맨 아래 (Hierarchy 상)

        // 4. 새 노드는 LeafWorm 바로 뒤에 삽입 (실제 화면상 한 단계 위)
        node.transform.SetParent(content, false);
        node.transform.SetSiblingIndex(1);

        // 5. LeafWorm에 새로운 웜 데이터 설정
        leafWormUI.SetData(newWormData);
    }

}
