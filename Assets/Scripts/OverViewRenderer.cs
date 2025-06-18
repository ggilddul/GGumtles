using UnityEngine;
using UnityEngine.UI;

public class OverViewRenderer : MonoBehaviour
{
    public SpriteRenderer bodyRenderer;
    public SpriteRenderer faceRenderer;
    public SpriteRenderer hatRenderer;
    public SpriteRenderer costumeRenderer;
    
    public Camera renderCamera;          // 전용 렌더 카메라
    public RenderTexture renderTexture;  // 카메라에 연결된 렌더 텍스처

    public Sprite RenderOverviewSprite()
    {
        // 1. 현재 RenderTexture를 렌더링
        RenderTexture currentRT = RenderTexture.active;
        RenderTexture.active = renderTexture;

        renderCamera.Render();

        // 2. 텍스처로 읽어오기
        Texture2D tex = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.ARGB32, false);
        tex.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        tex.Apply();

        RenderTexture.active = currentRT;

        // 3. 텍스처를 Sprite로 변환
        Rect rect = new Rect(0, 0, tex.width, tex.height);
        Vector2 pivot = new Vector2(0.5f, 0.5f);
        Sprite sprite = Sprite.Create(tex, rect, pivot);

        return sprite;
    }

    public delegate void OnPartChanged(string partName, Sprite newSprite);

    public event OnPartChanged PartChanged;

    private void NotifyPartChanged(string partName, Sprite newSprite)
    {
        PartChanged?.Invoke(partName, newSprite);
    }

    public void RefreshOverview()
    {
        // 현재 착용 아이템 ID 가져오기
        string hatId = ItemManager.Instance.GetCurrentHatId();
        string faceId = ItemManager.Instance.GetCurrentFaceId();
        string costumeId = ItemManager.Instance.GetCurrentCostumeId();

        // 아이템 데이터에서 해당 스프라이트 찾기
        var hatItem = ItemManager.Instance.GetItemById(hatId);
        var faceItem = ItemManager.Instance.GetItemById(faceId);
        var costumeItem = ItemManager.Instance.GetItemById(costumeId);

        // 각 파츠 스프라이트 설정
        SetHatSprite(hatItem != null ? hatItem.sprite : null);
        SetFaceSprite(faceItem != null ? faceItem.sprite : null);
        SetCostumeSprite(costumeItem != null ? costumeItem.sprite : null);
    }


    public void SetBodySprite(Sprite sprite)
    {
        bodyRenderer.sprite = sprite;
    }

    public void SetFaceSprite(Sprite sprite)
    {
        faceRenderer.sprite = sprite;
        NotifyPartChanged("Face", sprite);
    }

    public void SetHatSprite(Sprite sprite)
    {
        hatRenderer.sprite = sprite;
        NotifyPartChanged("Hat", sprite);
    }

    public void SetCostumeSprite(Sprite sprite)
    {
        costumeRenderer.sprite = sprite;
        NotifyPartChanged("Costume", sprite);
    }
}
