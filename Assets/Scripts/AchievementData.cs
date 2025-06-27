using UnityEngine;

[CreateAssetMenu(fileName = "AchievementData", menuName = "GGumtles/Achievement Data")]
public class AchievementData : ScriptableObject
{
    public string ach_id;                   // 고유 ID ("Ach_00_Acorn_Collector")
    public string ach_title;                // 제목 ("도토리 부자")
    public string ach_description;          // 조건 설명 ("도토리 5개를 모으세요!")
    public string ach_condition;            // 조건 수식 ("GameManager.Instance.acornCount >= 5")

    // public string achieverName = null;
    
    // public Sprite achieverSprite = null;
}

[System.Serializable]
public class AchievementStatus
{
    public string ach_id;
    public bool isUnlocked;
}