using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GGumtles.Data;
using GGumtles.Utils;

namespace GGumtles.UI
{
    public class WormEvolvePopupUI : MonoBehaviour
{
    [Header("UI 요소")]
    [SerializeField] private TMP_Text beforeText;          // 진화 전 텍스트
    [SerializeField] private TMP_Text afterText;           // 진화 후 텍스트
    [SerializeField] private TMP_Text ageText;             // 나이 텍스트

    [Header("이미지")]
    [SerializeField] private Image afterImage;             // 진화 후 이미지

    [Header("디버그")]
    [SerializeField] private bool enableDebugLogs = false;

    // 데이터
    private WormData currentWorm;                          // 현재 벌레 데이터
    private int previousLifeStage;                         // 이전 생명주기
    private int currentLifeStage;                          // 현재 생명주기

    // 프로퍼티
    public WormData CurrentWorm => currentWorm;
    public int PreviousLifeStage => previousLifeStage;
    public int CurrentLifeStage => currentLifeStage;

    /// <summary>
    /// 진화 팝업 초기화
    /// </summary>
    public void Initialize(WormData worm, int fromStage, int toStage)
    {
        if (worm == null)
        {
            Debug.LogWarning("[WormEvolvePopupUI] null 벌레 데이터가 전달되었습니다.");
            return;
        }

        try
        {
            currentWorm = worm;
            previousLifeStage = fromStage;
            currentLifeStage = toStage;

            UpdateUI();

            LogDebug($"[WormEvolvePopupUI] 진화 팝업 초기화: {worm.name} ({fromStage} → {toStage})");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[WormEvolvePopupUI] 팝업 초기화 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 팝업 닫기
    /// </summary>
    public void ClosePopup()
    {
        Destroy(gameObject);
        LogDebug("[WormEvolvePopupUI] 진화 팝업 닫기");
    }

    /// <summary>
    /// UI 업데이트
    /// </summary>
    private void UpdateUI()
    {
        if (currentWorm == null) return;

        try
        {
            UpdateTexts();
            UpdateImages();
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[WormEvolvePopupUI] UI 업데이트 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 텍스트 업데이트
    /// </summary>
    private void UpdateTexts()
    {
        try
        {
            // 나이 텍스트
            if (ageText != null)
            {
                ageText.text = FormatAge(currentWorm.age);
            }

            // 진화 전 텍스트
            if (beforeText != null)
            {
                beforeText.text = GetLifeStageName(previousLifeStage);
            }

            // 진화 후 텍스트
            if (afterText != null)
            {
                afterText.text = GetLifeStageName(currentLifeStage);
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[WormEvolvePopupUI] 텍스트 업데이트 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 이미지 업데이트
    /// </summary>
    private void UpdateImages()
    {
        try
        {
            // 진화 후 이미지만 표시
            if (afterImage != null)
            {
                afterImage.sprite = GetLifeStageSprite(currentLifeStage);
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[WormEvolvePopupUI] 이미지 업데이트 중 오류: {ex.Message}");
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
            Debug.LogError($"[WormEvolvePopupUI] 생명주기 스프라이트 로드 중 오류: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// 생명주기 이름 가져오기
    /// </summary>
    private string GetLifeStageName(int stage)
    {
        switch (stage)
        {
            case 0: return "알";
            case 1: return "제 1 유충기";
            case 2: return "제 2 유충기";
            case 3: return "제 3 유충기";
            case 4: return "제 4 유충기";
            case 5: return "성체";
            case 6: return "사망";
            default: return "알 수 없음";
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
            Debug.LogError($"[WormEvolvePopupUI] 나이 포맷팅 중 오류: {ex.Message}");
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
}