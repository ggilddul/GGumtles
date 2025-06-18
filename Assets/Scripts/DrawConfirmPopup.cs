using TMPro;
using UnityEngine;

public class DrawConfirmPopup : MonoBehaviour
{
    public TMP_Text diamondCountText;

    public void Show()
    {
        gameObject.SetActive(true);

        // 현재 다이아몬드 수 GameManager에서 받아와 텍스트에 표시
        int currentDiamonds = GameManager.Instance.diamondCount;
        diamondCountText.text = $"보유 다이아몬드 수 : {currentDiamonds}";
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
