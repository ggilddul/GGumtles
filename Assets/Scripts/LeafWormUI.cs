using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LeafWormUI : MonoBehaviour
{
    public Image overviewImage;        // 웜의 전체 외형 썸네일을 보여줄 UI 이미지
    public TMP_Text nameText;          // 이름 텍스트
    public TMP_Text ageText;           // 나이 텍스트

    private WormData data;             // 현재 웜 데이터
    public OverViewRenderer overviewRenderer;  // 렌더링용 오브젝트 참조

    public void SetData(WormData newData)
    {
        data = newData;

        // 외형 정보 설정
        bool isDead = (data.lifeStage == 6);

        // 죽었으면 꾸미기 제거
        overviewRenderer.SetHatSprite(isDead ? null : GetItemSprite(data.hatItemId));
        overviewRenderer.SetFaceSprite(isDead ? null : GetItemSprite(data.faceItemId));
        overviewRenderer.SetCostumeSprite(isDead ? null : GetItemSprite(data.costumeItemId));
        overviewRenderer.SetBodySprite(GetLifeStageSprite(data.lifeStage));

        // 썸네일 스프라이트로 변환
        overviewImage.sprite = overviewRenderer.RenderOverviewSprite();

        // 텍스트 갱신
        nameText.text = data.name;
        ageText.text = FormatAge(data.age);
    }

    private Sprite GetItemSprite(string itemId)
    {
        var item = ItemManager.Instance.GetItemById(itemId);
        return item != null ? item.sprite : null;
    }

    private Sprite GetLifeStageSprite(int stage)
    {
        return SpriteManager.Instance.GetLifeStageSprite(stage);
    }

    private string FormatAge(int age)
    {
        int days = age / 1440;
        int hours = (age % 1440) / 60;

        if (days > 0)
            return $"{days}일 {hours}시간";
        else if (hours > 0)
            return $"{hours}시간";
        else
            return $"{age}분";
    }

    public WormData GetCurrentData()
    {
        return data;
    }
}
