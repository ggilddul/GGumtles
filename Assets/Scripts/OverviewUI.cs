using UnityEngine;

public class OverviewUI : MonoBehaviour
{
    public OverViewRenderer overViewRenderer;

    private void OnEnable()
    {
        overViewRenderer.PartChanged += OnPartChanged;
    }

    private void OnDisable()
    {
        overViewRenderer.PartChanged -= OnPartChanged;
    }

    private void OnPartChanged(string partName, Sprite newSprite)
    {
        Debug.Log($"{partName} 스프라이트가 변경됨!");
        // 예: 툴팁 갱신, 하이라이트 효과 등 처리
    }
}
