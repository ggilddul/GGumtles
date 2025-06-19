using UnityEngine;
using System.Collections.Generic;

public class SpriteManager : MonoBehaviour
{
    public static SpriteManager Instance { get; private set; }

    [Header("LifeStage Sprites")]
    public Sprite[] lifeStageSprites;

    public enum MapType { TypeA, TypeB }
    public enum MapPhase { Day, Sunset, Night }

    [System.Serializable]
    public class MapSpriteSet
    {
        public MapType mapType;
        public Sprite daySprite;
        public Sprite sunsetSprite;
        public Sprite nightSprite;
    }

    [Header("Map Sprites")]
    public List<MapSpriteSet> mapSpriteSets;

    private Dictionary<MapType, Dictionary<MapPhase, Sprite>> mapSpriteTable;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeMapSpriteTable();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeMapSpriteTable()
    {
        mapSpriteTable = new Dictionary<MapType, Dictionary<MapPhase, Sprite>>();

        foreach (var set in mapSpriteSets)
        {
            var phaseDict = new Dictionary<MapPhase, Sprite>
            {
                { MapPhase.Day, set.daySprite },
                { MapPhase.Sunset, set.sunsetSprite },
                { MapPhase.Night, set.nightSprite }
            };

            mapSpriteTable[set.mapType] = phaseDict;
        }
    }

    public Sprite GetLifeStageSprite(int stage)
    {
        return (stage >= 0 && stage < lifeStageSprites.Length) ? lifeStageSprites[stage] : null;
    }

    public Sprite GetMapSprite(MapType mapType, MapPhase phase)
    {
        if (mapSpriteTable.TryGetValue(mapType, out var phaseDict) &&
            phaseDict.TryGetValue(phase, out var sprite))
        {
            return sprite;
        }

        Debug.LogWarning($"[SpriteManager] 스프라이트를 찾을 수 없습니다: {mapType}, {phase}");
        return null;
    }
}
