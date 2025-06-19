using UnityEngine;

[CreateAssetMenu(fileName = "ItemData", menuName = "GGumtles/Item Data")]
public class ItemData : ScriptableObject
{
    public enum ItemType { Hat, Face, Costume }

    public ItemType type;
    public string itemId;
    public string itemName;
    public string description;
    public Sprite sprite;
    public Vector2 positionOffset;
    public bool isVisible = true;
}
