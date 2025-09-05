using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WormDiePopupUI : MonoBehaviour
{
    [Header("UI 요소")]
    [SerializeField] private Image wormImage;              // 벌레 이미지
    [SerializeField] private TMP_Text ageText;             // 나이 텍스트

    [Header("디버그")]
    [SerializeField] private bool enableDebugLogs = false;

    // 데이터
    private WormData currentWorm;                          // 현재 벌레 데이터

    // 프로퍼티
    public WormData CurrentWorm => currentWorm;

    /// <summary>
    /// 사망 팝업 초기화
    /// </summary>
    public void Initialize(WormData worm)
    {
        if (worm == null)
        {
            Debug.LogWarning("[WormDiePopupUI] null 벌레 데이터가 전달되었습니다.");
            return;
        }

        try
        {
            currentWorm = worm;
            UpdateUI();

            LogDebug($"[WormDiePopupUI] 사망 팝업 초기화: {worm.name}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[WormDiePopupUI] 팝업 초기화 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 팝업 닫기
    /// </summary>
    public void ClosePopup()
    {
        Destroy(gameObject);
        LogDebug("[WormDiePopupUI] 사망 팝업 닫기");
    }

    /// <summary>
    /// UI 업데이트
    /// </summary>
    private void UpdateUI()
    {
        if (currentWorm == null) return;

        try
        {
            // 나이 텍스트
            if (ageText != null)
            {
                ageText.text = $"나이: {FormatAge(currentWorm.age)}";
            }

            // 벌레 이미지
            if (wormImage != null)
            {
                wormImage.sprite = GetLifeStageSprite(currentWorm.lifeStage);
                wormImage.color = Color.gray; // 사망 시 회색조
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[WormDiePopupUI] UI 업데이트 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 생명주기 스프라이트 가져오기
    /// </summary>
    private Sprite GetLifeStageSprite(int stage)
    {
        try
        {
            return SpriteManager.Instance?.GetLifeStageSprite(stage);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[WormDiePopupUI] 생명주기 스프라이트 로드 중 오류: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// 나이 포맷팅
    /// </summary>
    public static string FormatAge(int ageInMinutes)
    {
        try
        {
            int days = ageInMinutes / 1440;           // 1일 = 1440분
            int hours = (ageInMinutes % 1440) / 60;    // 나머지에서 시간 추출
            int minutes = ageInMinutes % 60;           // 나머지에서 분 추출

            var parts = new System.Collections.Generic.List<string>();
            if (days > 0) parts.Add($"{days}일");
            if (hours > 0) parts.Add($"{hours}시간");
            if (minutes > 0 || parts.Count == 0) parts.Add($"{minutes}분");

            return string.Join(" ", parts);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[WormDiePopupUI] 나이 포맷팅 중 오류: {ex.Message}");
            return "알 수 없음";
        }
    }

    private void LogDebug(string message)
    {
        if (enableDebugLogs)
        {
            Debug.Log(message);
        }
    }
}