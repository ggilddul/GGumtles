using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AchievementButtonUI : MonoBehaviour
{
    public Image iconImage;
    public TMP_Text titleText;

    private int achievementIndex;


    public void Set(AchievementData definition, Sprite iconSprite, int index, AchievementStatus status)
    {
        achievementIndex = index;

        titleText.text = definition.title;
        iconImage.sprite = iconSprite;
        iconImage.color = status.isUnlocked ? Color.white : new Color(0.8f, 0.8f, 0.8f, 1f);

        // 버튼 클릭 이벤트 등록
        GetComponent<Button>().onClick.RemoveAllListeners();
        GetComponent<Button>().onClick.AddListener(() =>
        {
            AudioManager.Instance.PlayButtonSound(0);
            PopupManager.Instance.ShowAchievementPopup(achievementIndex);
        });
    }

}
