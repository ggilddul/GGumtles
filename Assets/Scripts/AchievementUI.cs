using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AchievementUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text titleText;

    public void Set(AchievementData definition, AchievementStatus status)
    {
        titleText.text = definition.title;
        iconImage.sprite = definition.icon;

        iconImage.color = status.isUnlocked ? Color.white : new Color(0.8f, 0.8f, 0.8f, 1f);
    }
}
