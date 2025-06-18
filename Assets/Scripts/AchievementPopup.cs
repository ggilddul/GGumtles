using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AchievementPopup : MonoBehaviour
{
    [SerializeField] private TMP_Text titleTextType1;
    [SerializeField] private TMP_Text titleTextType2;


    public void Setup(string title, bool isAchieved)
    {
        if (isAchieved)
        {
            titleTextType2.text = title;
        }
        else
        {
            titleTextType1.text = title;
        }
    }
}
