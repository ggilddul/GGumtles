using System.Collections.Generic;
using UnityEngine;

namespace GGumtles.Data
{
    [System.Serializable]
    public class AchievementWormData
{
    public string achievementId;
    public int wormId;
    
    public AchievementWormData(string achievementId, int wormId)
    {
        this.achievementId = achievementId;
        this.wormId = wormId;
    }
}

[System.Serializable]
public class GameSaveData
{
    // 옵션 enum 정의 (0~4)
    public enum AudioOption
    {
        Off = 0,
        Low = 1,
        Medium = 2,
        High = 3,
        Max = 4
    }

    [Header("플레이어 정보")]
    public int userId = 0;
    public float totalPlayTime = 0f;

    [Header("게임 재화")]
    public int acornCount = 0;
    public int diamondCount = 0;

    [Header("게임 진행")]
    public List<WormData> wormList = new List<WormData>();
    public List<string> ownedItemIds = new List<string>();
    public List<string> unlockedAchIds = new List<string>();
    public List<AchievementWormData> achievementWormIds = new List<AchievementWormData>(); // 달성 웜 ID 저장
    public int selectedMapIndex = 0;
    
    [Header("착용 아이템")]
    public string equippedHatId = "";
    public string equippedFaceId = "";
    public string equippedCostumeId = "";

    [Header("설정")]
    public AudioOption sfxOption = AudioOption.Medium;
    public AudioOption bgmOption = AudioOption.Medium;
    }
}
