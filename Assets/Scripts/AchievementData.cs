using UnityEngine;

[CreateAssetMenu(fileName = "AchievementData", menuName = "GGumtles/Achievement", order = 1)]
public class AchievementData : ScriptableObject
{
    public string id;                  // 고유 ID ("acorn_collector")
    public string title;              // 제목
    public string description;        // 설명
    public Sprite icon;
    // public string achieverName;
    // public Sprite achieverSprite;
}

[System.Serializable]
public class AchievementStatus
{
    public string id;
    public bool isUnlocked;
}