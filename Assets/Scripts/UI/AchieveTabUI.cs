using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using GGumtles.Data;
using GGumtles.Managers;
using GGumtles.Utils;

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
        [SerializeField] private Color unlockedColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);    // 해금된 업적 색상
        [SerializeField] private Color lockedColor = new Color(0.9f, 0.9f, 0.9f, 1.0f); // 잠금된 업적 색상
        
        [Header("디버그")]
        [SerializeField] private bool enableDebugLogs = true;
        
        private List<AchievementButtonUI> activeButtons = new List<AchievementButtonUI>();
        private bool isInitialized = false;
        
        private void Start()
        {
            StartCoroutine(WaitForManagersAndInitialize());
        }
        
        /// <summary>
        /// 매니저들이 초기화될 때까지 대기한 후 업적 목록 초기화
        /// </summary>
        private System.Collections.IEnumerator WaitForManagersAndInitialize()
        {
            LogDebug("[AchieveTabUI] 매니저 초기화 대기 시작");
            
            // AchievementManager 인스턴스 대기
            while (AchievementManager.Instance == null)
            {
                LogDebug("[AchieveTabUI] AchievementManager.Instance 대기 중...");
                yield return null;
            }
            LogDebug("[AchieveTabUI] AchievementManager.Instance 확인됨");
            
            // AchievementManager 초기화 대기
            while (!AchievementManager.Instance.IsInitialized)
            {
                LogDebug("[AchieveTabUI] AchievementManager 초기화 대기 중...");
                yield return null;
            }
            LogDebug("[AchieveTabUI] AchievementManager 초기화 완료");
            
            // SpriteManager 인스턴스 대기
            while (SpriteManager.Instance == null)
            {
                LogDebug("[AchieveTabUI] SpriteManager.Instance 대기 중...");
                yield return null;
            }
            LogDebug("[AchieveTabUI] SpriteManager.Instance 확인됨");
            
            // 이제 안전하게 초기화
            if (!isInitialized)
            {
                InitializeAchievementList();
                isInitialized = true;
                LogDebug("[AchieveTabUI] 업적 목록 초기화 완료");
            }
        }
        
        /// <summary>
        /// 외부에서 강제 초기화할 수 있는 메서드
        /// </summary>
        public void ForceInitialize()
        {
            if (!isInitialized)
            {
                // GameObject가 비활성화된 경우 직접 초기화
                if (!gameObject.activeInHierarchy)
                {
                    InitializeAchievementList();
                    isInitialized = true;
                }
                else
                {
                    StartCoroutine(WaitForManagersAndInitialize());
                }
            }
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
            LogDebug("[AchieveTabUI] CreateAchievementList 시작");
            
            ClearExistingButtons();
            LogDebug("[AchieveTabUI] 기존 버튼들 제거 완료");
            
            if (AchievementManager.Instance == null)
            {
                Debug.LogWarning("[AchieveTabUI] AchievementManager 인스턴스가 없습니다.");
                return;
            }
            
            var definitions = AchievementManager.Instance.GetAllDefinitions();
            LogDebug($"[AchieveTabUI] AchievementManager에서 업적 정의 가져오기: {definitions?.Count ?? 0}개");
            
            if (definitions == null || definitions.Count == 0)
            {
                LogDebug("[AchieveTabUI] 업적 정의가 없습니다.");
                return;
            }
            
            // 각 업적 정의 상세 정보 출력
            for (int i = 0; i < definitions.Count; i++)
            {
                var definition = definitions[i];
                LogDebug($"[AchieveTabUI] 업적 {i+1}/{definitions.Count} 상세 정보:");
                LogDebug($"  - ID: {definition.achievementId}");
                LogDebug($"  - Title: {definition.achievementTitle}");
                LogDebug($"  - Description: {definition.achievementDescription}");
                LogDebug($"  - 해금 여부: {AchievementManager.Instance.IsUnlocked(definition.achievementId)}");
            }
            
            for (int i = 0; i < definitions.Count; i++)
            {
                var definition = definitions[i];
                LogDebug($"[AchieveTabUI] 업적 {i+1}/{definitions.Count} 생성 시작: {definition.achievementTitle} (ID: {definition.achievementId})");
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
            LogDebug($"[AchieveTabUI] CreateAchievementButton 시작:");
            LogDebug($"  - ID: {definition.achievementId}");
            LogDebug($"  - Title: {definition.achievementTitle}");
            LogDebug($"  - Description: {definition.achievementDescription}");
            LogDebug($"  - Index: {activeButtons.Count}");
            
            // 프리팹 인스턴스 생성
            LogDebug($"[AchieveTabUI] 프리팹 인스턴스 생성: {achievementButtonPrefab?.name}");
            if (achievementButtonPrefab == null)
            {
                Debug.LogError("[AchieveTabUI] achievementButtonPrefab이 null입니다! Inspector에서 AchButtonObject 프리팹을 할당하세요.");
                return;
            }
            GameObject buttonObj = Instantiate(achievementButtonPrefab, contentParent);
            LogDebug($"[AchieveTabUI] 프리팹 인스턴스 생성 완료: {buttonObj?.name}");
            
            // 프리팹 구조 디버깅
            LogDebug($"[AchieveTabUI] 프리팹 구조 확인:");
            LogDebug($"  - GameObject 이름: {buttonObj.name}");
            LogDebug($"  - 자식 개수: {buttonObj.transform.childCount}");
            for (int i = 0; i < buttonObj.transform.childCount; i++)
            {
                var child = buttonObj.transform.GetChild(i);
                LogDebug($"  - 자식[{i}]: {child.name}");
            }
            
            AchievementButtonUI buttonUI = buttonObj.GetComponent<AchievementButtonUI>();
            LogDebug($"[AchieveTabUI] AchievementButtonUI 컴포넌트 찾기: {buttonUI != null}");
            
            if (buttonUI == null)
            {
                Debug.LogError($"[AchieveTabUI] AchievementButtonUI 컴포넌트를 찾을 수 없습니다: {definition.achievementTitle}");
                Destroy(buttonObj);
                return;
            }
            
            // 해금 여부 확인
            bool isUnlocked = AchievementManager.Instance.IsUnlocked(definition.achievementId);
            LogDebug($"[AchieveTabUI] 해금 여부 확인: {isUnlocked} (ID: {definition.achievementId})");
            
            // 아이콘 가져오기 (SpriteManager에서)
            Sprite icon = null;
            if (SpriteManager.Instance != null)
            {
                icon = SpriteManager.Instance.GetAchievementSprite(definition.achievementId, isUnlocked);
                LogDebug($"[AchieveTabUI] 아이콘 가져오기: {icon != null} (ID: {definition.achievementId})");
            }
            else
            {
                LogDebug("[AchieveTabUI] SpriteManager.Instance가 null입니다");
            }
            
            // 버튼 초기화
            LogDebug($"[AchieveTabUI] 버튼 초기화 시작: {definition.achievementTitle} (ID: {definition.achievementId})");
            buttonUI.Initialize(definition, activeButtons.Count);
            LogDebug($"[AchieveTabUI] 버튼 초기화 완료: {definition.achievementTitle} (ID: {definition.achievementId})");

            // ReusableButton에 업적 ID를 액션 파라미터(문자열)로 주입하여 클릭 시 정확한 업적을 참조하도록 설정
            var reusable = buttonObj.GetComponent<ReusableButton>();
            if (reusable != null)
            {
                reusable.SetActionString(ButtonAction.OpenAchievementPopup, definition.achievementId);
            }
            
            // 색상 설정
            SetButtonColor(buttonUI, isUnlocked);
            LogDebug($"[AchieveTabUI] 색상 설정 완료: {isUnlocked}");
            
            activeButtons.Add(buttonUI);
            
            LogDebug($"[AchieveTabUI] 업적 버튼 생성 완료: {definition.achievementTitle} (ID: {definition.achievementId}, 해금: {isUnlocked})");
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
