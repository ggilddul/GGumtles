using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace GGumtles.UI
{
    /// <summary>
    /// 업적 목록을 동적으로 생성하고 관리하는 매니저
    /// </summary>
    public class AchieveTabUI : MonoBehaviour
    {
        [Header("UI 설정")]
        [SerializeField] private Transform contentParent;              // Content Transform
        [SerializeField] private GameObject achievementButtonPrefab;   // AchievementButtonUI 프리팹
        
        [Header("색상 설정")]
        [SerializeField] private Color unlockedColor = Color.white;    // 해금된 업적 색상
        [SerializeField] private Color lockedColor = new Color(0.8f, 0.8f, 0.8f, 1f); // 잠금된 업적 색상
        
        [Header("디버그")]
        [SerializeField] private bool enableDebugLogs = false;
        
        private List<AchievementButtonUI> activeButtons = new List<AchievementButtonUI>();
        
        private void Start()
        {
            InitializeAchievementList();
        }
        
        /// <summary>
        /// 업적 목록 초기화
        /// </summary>
        public void InitializeAchievementList()
        {
            try
            {
                if (contentParent == null)
                {
                    Debug.LogError("[AchieveTabUI] Content Parent가 설정되지 않았습니다.");
                    return;
                }
                
                if (achievementButtonPrefab == null)
                {
                    Debug.LogError("[AchieveTabUI] Achievement Button Prefab이 설정되지 않았습니다.");
                    return;
                }
                
                CreateAchievementList();
                LogDebug("[AchieveTabUI] 업적 목록 초기화 완료");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[AchieveTabUI] 업적 목록 초기화 중 오류: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 업적 목록 생성
        /// </summary>
        public void CreateAchievementList()
        {
            ClearExistingButtons();
            
            if (AchievementManager.Instance == null)
            {
                Debug.LogWarning("[AchieveTabUI] AchievementManager 인스턴스가 없습니다.");
                return;
            }
            
            var definitions = AchievementManager.Instance.GetAllDefinitions();
            
            foreach (var definition in definitions)
            {
                CreateAchievementButton(definition);
            }
            
            // 레이아웃 업데이트
            LayoutRebuilder.ForceRebuildLayoutImmediate(contentParent as RectTransform);
            
            LogDebug($"[AchieveTabUI] {activeButtons.Count}개의 업적 버튼 생성 완료");
        }
        
        /// <summary>
        /// 개별 업적 버튼 생성
        /// </summary>
        private void CreateAchievementButton(AchievementData definition)
        {
            try
            {
                // 프리팹 인스턴스 생성
                GameObject buttonObj = Instantiate(achievementButtonPrefab, contentParent);
                AchievementButtonUI buttonUI = buttonObj.GetComponent<AchievementButtonUI>();
                
                if (buttonUI == null)
                {
                    Debug.LogError($"[AchieveTabUI] AchievementButtonUI 컴포넌트를 찾을 수 없습니다: {definition.achievementTitle}");
                    Destroy(buttonObj);
                    return;
                }
                
                // 해금 여부 확인
                bool isUnlocked = AchievementManager.Instance.IsUnlocked(definition.achievementId);
                
                // 아이콘 가져오기 (SpriteManager에서)
                Sprite icon = null;
                if (SpriteManager.Instance != null)
                {
                    icon = SpriteManager.Instance.GetAchievementSprite(definition.achievementId, isUnlocked);
                }
                
                // 버튼 설정
                buttonUI.Set(definition, icon, activeButtons.Count, isUnlocked);
                
                // 색상 설정
                SetButtonColor(buttonUI, isUnlocked);
                
                // 클릭 이벤트 연결
                buttonUI.OnAchievementButtonClickedEvent += OnAchievementButtonClicked;
                
                activeButtons.Add(buttonUI);
                
                LogDebug($"[AchieveTabUI] 업적 버튼 생성: {definition.achievementTitle} (해금: {isUnlocked})");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[AchieveTabUI] 업적 버튼 생성 중 오류: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 버튼 색상 설정
        /// </summary>
        private void SetButtonColor(AchievementButtonUI buttonUI, bool isUnlocked)
        {
            try
            {
                // 버튼의 Image 컴포넌트 찾기
                Image buttonImage = buttonUI.GetComponent<Image>();
                if (buttonImage != null)
                {
                    buttonImage.color = isUnlocked ? unlockedColor : lockedColor;
                }
                
                // 자식 오브젝트들의 색상도 설정
                Image[] childImages = buttonUI.GetComponentsInChildren<Image>();
                foreach (var childImage in childImages)
                {
                    if (childImage != buttonImage) // 버튼 자체는 제외
                    {
                        childImage.color = isUnlocked ? unlockedColor : lockedColor;
                    }
                }
                
                // 텍스트 색상도 설정
                TMPro.TextMeshProUGUI[] texts = buttonUI.GetComponentsInChildren<TMPro.TextMeshProUGUI>();
                foreach (var text in texts)
                {
                    text.color = isUnlocked ? unlockedColor : lockedColor;
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[AchieveTabUI] 버튼 색상 설정 중 오류: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 업적 버튼 클릭 처리
        /// </summary>
        private void OnAchievementButtonClicked(AchievementButtonUI button, int index)
        {
            try
            {
                LogDebug($"[AchieveTabUI] 업적 버튼 클릭: {button.AchievementData?.achievementTitle}");
                
                // 여기에 클릭 시 동작 추가 (예: 상세 정보 팝업 표시)
                // PopupManager.Instance?.OpenAchievementDetailPopup(button.AchievementData);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[AchieveTabUI] 업적 버튼 클릭 처리 중 오류: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 기존 버튼들 제거
        /// </summary>
        private void ClearExistingButtons()
        {
            try
            {
                foreach (var button in activeButtons)
                {
                    if (button != null)
                    {
                        button.OnAchievementButtonClickedEvent -= OnAchievementButtonClicked;
                        Destroy(button.gameObject);
                    }
                }
                activeButtons.Clear();
                
                LogDebug("[AchieveTabUI] 기존 업적 버튼들 제거 완료");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[AchieveTabUI] 기존 버튼 제거 중 오류: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 업적 목록 새로고침
        /// </summary>
        public void RefreshAchievementList()
        {
            CreateAchievementList();
        }
        
        /// <summary>
        /// 특정 업적 버튼 색상 업데이트
        /// </summary>
        public void UpdateAchievementButtonColor(string achievementId)
        {
            try
            {
                foreach (var button in activeButtons)
                {
                    if (button.AchievementData?.achievementId == achievementId)
                    {
                        bool isUnlocked = AchievementManager.Instance.IsUnlocked(achievementId);
                        SetButtonColor(button, isUnlocked);
                        LogDebug($"[AchieveTabUI] 업적 색상 업데이트: {achievementId} (해금: {isUnlocked})");
                        break;
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[AchieveTabUI] 업적 버튼 색상 업데이트 중 오류: {ex.Message}");
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
            ClearExistingButtons();
        }
    }
}
