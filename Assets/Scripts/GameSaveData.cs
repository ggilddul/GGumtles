using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameSaveData
{
    private static GameSaveData _instance;

    public static GameSaveData Instance
    {
        get
        {
            if (_instance == null)
                _instance = new GameSaveData();  // 최초 접근 시 기본값 생성
            return _instance;
        }
        set => _instance = value; // GameSaveManager가 Load 시 세팅
    }

    public float totalPlayTime;
    public int acornCount;
    public int diamondCount;
    public List<WormData> wormList = new();
    public List<ItemData> ownedItems = new();
    public List<int> unlockedAchievements = new();
    public int selectedMapIndex;
    public int sfxOption = 2;
    public int bgmOption = 2;

}
