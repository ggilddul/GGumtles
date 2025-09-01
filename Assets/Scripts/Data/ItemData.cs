using UnityEngine;

[CreateAssetMenu(fileName = "ItemData", menuName = "GGumtles/Item Data")]
public class ItemData : ScriptableObject
{
    [Header("기본 정보")]
    public string itemId = "";
    public string itemName = "";
    [TextArea(3, 5)]
    public string itemDescription = "";
    public ItemType itemType = ItemType.Hat;
    
    [Header("시각적 요소")]
    public Sprite itemSprite;              // 아이템 스프라이트
    
    [Header("상태")]
    public bool isOwned = false;           // 보유 여부

    // 열거형 정의
    public enum ItemType
    {
        Hat,
        Face,
        Costume
    }
}
