using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class MapManager : MonoBehaviour
{
    public enum BackgroundType
    {
        SpriteRenderer,
        UIImage
    }

    [Header("설정")]
    public BackgroundType backgroundType;

    [Header("Background Components")]
    public SpriteRenderer backgroundRenderer;
    public Image backgroundImage;

    [Header("맵 타입 리스트")]
    public List<SpriteManager.MapType> mapTypes = new List<SpriteManager.MapType>();

    // 현재 선택된 맵 인덱스
    private int currentMapIndex = 0;

    /// <summary>
    /// 외부에서 현재 선택된 맵 타입 요청
    /// </summary>
    public SpriteManager.MapType GetCurrentMapType()
    {
        if (currentMapIndex >= 0 && currentMapIndex < mapTypes.Count)
            return mapTypes[currentMapIndex];
        return SpriteManager.MapType.TypeA; // 기본값
    }

    /// <summary>
    /// 외부에서 맵 변경 요청
    /// </summary>
    
    public SpriteManager.MapType currentMapType = SpriteManager.MapType.TypeA;

    public void ChangeMap(SpriteManager.MapType newMapType)
    {
        currentMapType = newMapType;
        LoadingManager.Instance.ShowLoading(LoadingManager.LoadingType.MapChange);
        GameManager.Instance.ForceUpdateMapBackground(); // 예: 스프라이트 새로 적용
        TabManager.Instance.OpenTab(2);
        LoadingManager.Instance.HideLoading(LoadingManager.LoadingType.MapChange);
    }
    
    public void ChangeMapByIndex(int index)
    {
        if (index >= 0 && index < mapTypes.Count)
        {
            currentMapIndex = index;
            ChangeMap(mapTypes[index]);
        }
        else
        {
            Debug.LogWarning($"[MapManager] 잘못된 맵 인덱스: {index}");
        }
    }

    public void UpdateMapBackground(Sprite newSprite)
    {
        if (newSprite == null)
        {
            Debug.LogWarning("MapManager: newSprite가 null입니다.");
            return;
        }

        switch (backgroundType)
        {
            case BackgroundType.SpriteRenderer:
                if (backgroundRenderer != null)
                    backgroundRenderer.sprite = newSprite;
                break;

            case BackgroundType.UIImage:
                if (backgroundImage != null)
                    backgroundImage.sprite = newSprite;
                break;
        }
    }

    public int GetCurrentMapIndex() => currentMapIndex;
    public void SetMapIndex(int index) => currentMapIndex = index;
}
