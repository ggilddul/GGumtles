using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 웜 정보를 표시하는 통합 UI 클래스
/// LeafWormUI와 WormNodeUI를 대체합니다.
/// </summary>
public class WormUI : MonoBehaviour
{
    [Header("웜 이미지")]
    [SerializeField] private Image wormImage;               // 웜 생명주기 이미지
    
    [Header("텍스트 요소")]
    [SerializeField] private TMP_Text nameText;             // 이름 텍스트
    [SerializeField] private TMP_Text ageText;              // 나이 텍스트
    [SerializeField] private TMP_Text generationText;       // 세대 텍스트 (선택사항)
    
    [Header("디버그")]
    [SerializeField] private bool enableDebugLogs = false;
    
    // 데이터 및 상태
    private WormData currentWormData;
    
    // 프로퍼티
    public WormData CurrentWormData => currentWormData;
    
    private void Awake()
    {
        InitializeComponents();
    }
    
    private void InitializeComponents()
    {
        try
        {
            LogDebug("[WormUI] 컴포넌트 초기화 완료");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[WormUI] 컴포넌트 초기화 중 오류: {ex.Message}");
        }
    }
    
    /// <summary>
    /// 웜 데이터 설정
    /// </summary>
    public void SetWormData(WormData wormData)
    {
        if (wormData == null)
        {
            Debug.LogWarning("[WormUI] null 웜 데이터가 전달되었습니다.");
            return;
        }
        
        try
        {
            currentWormData = wormData;
            UpdateUI();
            LogDebug($"[WormUI] 웜 데이터 설정 완료: {wormData.name}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[WormUI] 웜 데이터 설정 중 오류: {ex.Message}");
        }
    }
    
    /// <summary>
    /// UI 전체 업데이트
    /// </summary>
    public void UpdateUI()
    {
        if (currentWormData == null) return;
        
        try
        {
            UpdateWormImage();
            UpdateTexts();
            
            LogDebug($"[WormUI] UI 업데이트 완료: {currentWormData.name}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[WormUI] UI 업데이트 중 오류: {ex.Message}");
        }
    }
    
    /// <summary>
    /// 웜 이미지 업데이트 (WormData 기반 렌더링)
    /// </summary>
    private void UpdateWormImage()
    {
        try
        {
            if (wormImage != null)
            {
                // WormData에서 생명주기 스프라이트 가져오기
                Sprite wormSprite = GetLifeStageSprite(currentWormData.lifeStage);
                wormImage.sprite = wormSprite;
                
                // 사망 상태에 따른 색상 조정
                if (!currentWormData.isAlive)
                {
                    wormImage.color = Color.gray;
                }
                else
                {
                    wormImage.color = Color.white;
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[WormUI] 웜 이미지 업데이트 중 오류: {ex.Message}");
        }
    }
    
    /// <summary>
    /// 텍스트 업데이트
    /// </summary>
    private void UpdateTexts()
    {
        try
        {
            // 이름
            if (nameText != null)
                nameText.text = currentWormData.name;
            
            // 나이
            if (ageText != null)
                ageText.text = FormatAge(currentWormData.age);
            
            // 세대 (있는 경우에만)
            if (generationText != null)
                generationText.text = $"세대 {currentWormData.generation}";
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[WormUI] 텍스트 업데이트 중 오류: {ex.Message}");
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
            Debug.LogError($"[WormUI] 생명주기 스프라이트 로드 중 오류: {ex.Message}");
            return null;
        }
    }
    
    /// <summary>
    /// 나이 포맷팅
    /// </summary>
    private string FormatAge(int ageInMinutes)
    {
        try
        {
            if (ageInMinutes < 60)
                return $"{ageInMinutes}분";
            else if (ageInMinutes < 1440) // 24시간
                return $"{ageInMinutes / 60}시간 {ageInMinutes % 60}분";
            else
                return $"{ageInMinutes / 1440}일 {(ageInMinutes % 1440) / 60}시간";
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[WormUI] 나이 포맷팅 중 오류: {ex.Message}");
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
    
    private void OnDestroy()
    {
        // 정리 작업 없음
    }
}
