using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GGumtles.Data;
using GGumtles.Managers;
using GGumtles.Utils;

namespace GGumtles.UI
{
    public class AchievementButtonUI : MonoBehaviour
{
    [Header("UI 컴포넌트")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private Button button;
    
    // 자동 찾기용 필드
    private bool componentsInitialized = false;

    [Header("디버그")]
    [SerializeField] private bool enableDebugLogs = true;

    // 데이터
    private int achievementIndex;
    private AchievementData achievementData;
    private bool isUnlocked = false;

    // 프로퍼티
    public int AchievementIndex => achievementIndex;
    public AchievementData AchievementData => achievementData;
    public bool IsUnlocked => isUnlocked;

    /// <summary>
    /// 강제 텍스트 업데이트 (개선된 프리팹 구조에 맞게 수정)
    /// </summary>
    public void ForceUpdateText()
    {
        if (achievementData == null) return;

        LogDebug($"[AchievementButtonUI] ForceUpdateText 호출 - 제목: {achievementData.achievementTitle}");
        
        // AchNameText 하위 요소에서 직접 찾기
        if (titleText != null)
        {
            titleText.text = achievementData.achievementTitle;
            LogDebug($"[AchievementButtonUI] titleText에 강제 설정: '{titleText.text}'");
        }
        else
        {
            // titleText가 null인 경우 다시 찾기 시도
            Transform titleTransform = transform.Find("AchNameText");
            if (titleTransform != null)
            {
                titleText = titleTransform.GetComponent<TMP_Text>();
                if (titleText != null)
                {
                    titleText.text = achievementData.achievementTitle;
                    LogDebug($"[AchievementButtonUI] AchNameText에서 찾아서 설정: '{titleText.text}'");
                }
            }
            else
            {
                Debug.LogError($"[AchievementButtonUI] AchNameText를 찾을 수 없습니다!");
            }
        }
        
        LogDebug($"[AchievementButtonUI] ForceUpdateText 완료");
    }

    /// <summary>
    /// 업적 버튼 초기화
    /// </summary>
    public void Initialize(AchievementData definition, int index)
    {
        try
        {
            LogDebug($"[AchievementButtonUI] Initialize 시작:");
            LogDebug($"  - Title: {definition?.achievementTitle}");
            LogDebug($"  - ID: {definition?.achievementId}");
            LogDebug($"  - Description: {definition?.achievementDescription}");
            LogDebug($"  - Index: {index}");
            
            // 컴포넌트 자동 찾기 (Instantiate된 경우)
            LogDebug($"[AchievementButtonUI] 컴포넌트 초기화 시작");
            InitializeComponents();
            LogDebug($"[AchievementButtonUI] 컴포넌트 초기화 완료");
            
            achievementIndex = index;
            achievementData = definition;
            LogDebug($"[AchievementButtonUI] 데이터 저장 완료:");
            LogDebug($"  - achievementIndex: {achievementIndex}");
            LogDebug($"  - achievementData.achievementId: {achievementData?.achievementId}");
            LogDebug($"  - achievementData.achievementTitle: {achievementData?.achievementTitle}");
            
            // AchievementManager에서 해금 여부 가져오기
            var achievementManager = AchievementManager.Instance;
            if (achievementManager != null)
            {
                isUnlocked = achievementManager.IsUnlocked(definition.achievementId);
                LogDebug($"[AchievementButtonUI] 해금 여부 확인: {isUnlocked}");
            }
            else
            {
                LogDebug("[AchievementButtonUI] AchievementManager.Instance가 null입니다");
            }
            
            // UI 업데이트
            LogDebug($"[AchievementButtonUI] UI 업데이트 시작");
            UpdateUI();
            LogDebug($"[AchievementButtonUI] UI 업데이트 완료");
            
            // 강제 텍스트 업데이트 (안전장치)
            LogDebug($"[AchievementButtonUI] 강제 텍스트 업데이트 시작");
            ForceUpdateText();
            LogDebug($"[AchievementButtonUI] 강제 텍스트 업데이트 완료");
            
            // 버튼 이벤트 설정
            LogDebug($"[AchievementButtonUI] 버튼 이벤트 설정 시작");
            SetupButtonEvents();
            LogDebug($"[AchievementButtonUI] 버튼 이벤트 설정 완료");
            
            LogDebug($"[AchievementButtonUI] 업적 버튼 초기화 완료: {definition?.achievementTitle}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[AchievementButtonUI] 업적 버튼 초기화 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 컴포넌트 자동 찾기 (개선된 프리팹 구조에 맞게 수정)
    /// </summary>
    private void InitializeComponents()
    {
        if (componentsInitialized) return;
        
        try
        {
            LogDebug($"[AchievementButtonUI] 컴포넌트 초기화 시작 - GameObject: {gameObject.name}");
            
            // 1. titleText 찾기 (AchNameText 하위 요소)
            if (titleText == null)
            {
                Transform titleTransform = transform.Find("AchNameText");
                if (titleTransform != null)
                {
                    titleText = titleTransform.GetComponent<TMP_Text>();
                    LogDebug($"[AchievementButtonUI] AchNameText 찾기: {titleText != null}");
                }
                else
                {
                    LogDebug($"[AchievementButtonUI] AchNameText Transform을 찾을 수 없음");
                }
            }
            
            // 2. iconImage 찾기 (IconImage 하위 요소)
            if (iconImage == null)
            {
                Transform iconTransform = transform.Find("IconImage");
                if (iconTransform != null)
                {
                    iconImage = iconTransform.GetComponent<Image>();
                    LogDebug($"[AchievementButtonUI] IconImage 찾기: {iconImage != null}");
                }
                else
                {
                    LogDebug($"[AchievementButtonUI] IconImage Transform을 찾을 수 없음");
                }
            }
            
            // 3. button 찾기 (Button 하위 요소)
            if (button == null)
            {
                Transform buttonTransform = transform.Find("Button");
                if (buttonTransform != null)
                {
                    button = buttonTransform.GetComponent<Button>();
                    LogDebug($"[AchievementButtonUI] Button 찾기: {button != null}");
                }
                else
                {
                    LogDebug($"[AchievementButtonUI] Button Transform을 찾을 수 없음");
                }
            }
            
            componentsInitialized = true;
            LogDebug($"[AchievementButtonUI] 컴포넌트 초기화 완료 - titleText: {titleText != null}, iconImage: {iconImage != null}, button: {button != null}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[AchievementButtonUI] 컴포넌트 초기화 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// UI 업데이트
    /// </summary>
    private void UpdateUI()
    {
        LogDebug($"[AchievementButtonUI] UpdateUI 시작");
        
        if (achievementData == null) 
        {
            LogDebug("[AchievementButtonUI] achievementData가 null입니다");
            return;
        }

        LogDebug($"[AchievementButtonUI] achievementData 확인: {achievementData.achievementTitle}");

        // 제목 텍스트 설정
        LogDebug($"[AchievementButtonUI] titleText 확인: {titleText != null}");
        if (titleText != null)
        {
            LogDebug($"[AchievementButtonUI] titleText GameObject: {titleText.gameObject.name}");
            LogDebug($"[AchievementButtonUI] titleText 현재 값: '{titleText.text}'");
            titleText.text = achievementData.achievementTitle;
            LogDebug($"[AchievementButtonUI] titleText 설정 후 값: '{titleText.text}'");
            LogDebug($"[AchievementButtonUI] 제목 텍스트 설정 완료: {achievementData.achievementTitle}");
        }
        else
        {
            Debug.LogError($"[AchievementButtonUI] titleText가 null입니다! AchNameText 하위 요소를 확인하세요.");
        }

        // 아이콘 이미지
        LogDebug($"[AchievementButtonUI] iconImage 확인: {iconImage != null}");
        if (iconImage != null)
        {
            // SpriteManager에서 아이콘 가져오기
            LogDebug($"[AchievementButtonUI] SpriteManager 확인: {SpriteManager.Instance != null}");
            Sprite iconSprite = null;
            if (SpriteManager.Instance != null)
            {
                LogDebug($"[AchievementButtonUI] GetAchievementSprite 호출: {achievementData.achievementId}");
                iconSprite = SpriteManager.Instance.GetAchievementSprite(achievementData.achievementId);
                LogDebug($"[AchievementButtonUI] GetAchievementSprite 결과: {iconSprite != null}");
            }
            else
            {
                LogDebug("[AchievementButtonUI] SpriteManager.Instance가 null입니다");
            }
            
            iconImage.sprite = iconSprite;
            iconImage.color = isUnlocked ? Color.white : new Color(0.8f, 0.8f, 0.8f, 1f);
            LogDebug($"[AchievementButtonUI] 아이콘 이미지 설정 완료: sprite={iconSprite != null}, color={iconImage.color}");
        }
        else
        {
            Debug.LogWarning($"[AchievementButtonUI] iconImage가 null입니다. Inspector에서 할당하거나 자식 오브젝트에 Image를 추가하세요.");
        }
        
        LogDebug($"[AchievementButtonUI] UpdateUI 완료");
    }

    /// <summary>
    /// 버튼 이벤트 설정
    /// </summary>
    private void SetupButtonEvents()
    {
        if (button == null) return;

        try
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnButtonClicked);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[AchievementButtonUI] 버튼 이벤트 설정 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 버튼 클릭 이벤트
    /// </summary>
    private void OnButtonClicked()
    {
        try
        {
            LogDebug($"[AchievementButtonUI] OnButtonClicked 시작:");
            LogDebug($"  - achievementData: {achievementData?.achievementTitle}");
            LogDebug($"  - achievementId: {achievementData?.achievementId}");
            LogDebug($"  - achievementIndex: {achievementIndex}");
            LogDebug($"  - 저장된 isUnlocked: {isUnlocked}");
            
            // 사운드 재생
            AudioManager.Instance?.PlayButtonSound(0);

            // 업적 팝업 열기 (해금 상태에 따라 구분)
            if (achievementData != null)
            {
                // 실시간으로 해금 상태 확인
                bool currentUnlocked = false;
                if (AchievementManager.Instance != null)
                {
                    currentUnlocked = AchievementManager.Instance.IsUnlocked(achievementData.achievementId);
                }
                
                // 팝업 열기 (단순화)
                if (currentUnlocked)
                {
                    // 해금된 업적: Achievement2 팝업
                    PopupManager.Instance?.OpenPopup(PopupManager.PopupType.Achievement2, PopupManager.PopupPriority.Normal, achievementIndex);
                }
                else
                {
                    // 잠금된 업적: Achievement1 팝업
                    PopupManager.Instance?.OpenPopup(PopupManager.PopupType.Achievement1, PopupManager.PopupPriority.Normal, achievementIndex);
                }
            }

            LogDebug($"[AchievementButtonUI] 업적 버튼 클릭 완료: {achievementData?.achievementTitle}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[AchievementButtonUI] 버튼 클릭 처리 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 업적 상태 업데이트
    /// </summary>
    public void UpdateStatus(bool unlocked)
    {
        try
        {
            isUnlocked = unlocked;
            
            if (iconImage != null)
            {
                iconImage.color = unlocked ? Color.white : new Color(0.8f, 0.8f, 0.8f, 1f);
            }

            LogDebug($"[AchievementButtonUI] 업적 상태 업데이트: {achievementData?.achievementTitle} - {unlocked}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[AchievementButtonUI] 상태 업데이트 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 버튼 활성화/비활성화
    /// </summary>
    public void SetInteractable(bool interactable)
    {
        if (button != null)
        {
            button.interactable = interactable;
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
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
        }
    }
    }
}