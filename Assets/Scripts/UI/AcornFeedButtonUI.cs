using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using GGumtles.Managers;

namespace GGumtles.UI
{
    /// <summary>
    /// AcornFeed 버튼 UI 관리
    /// - 클릭 시 AcornFeedSpawner.SpawnAcorn() 실행
    /// - 30초 쿨다운: 버튼/이미지 비활성화, 텍스트로 카운트다운 표시
    /// - 30초 후 원상복구
    /// </summary>
    public class AcornFeedButtonUI : MonoBehaviour
    {
        [Header("UI 컴포넌트")]
        [SerializeField] private Button acornFeedButton;
        [SerializeField] private Image acornButtonImage;
        [SerializeField] private TextMeshProUGUI acornButtonText;

        [Header("스폰 설정")]
        [SerializeField] private AcornFeedSpawner acornFeedSpawner;
        [SerializeField] private GameObject acornToast; // 도토리 부족 시 표시할 Toast

        [Header("쿨다운 설정")]
        [SerializeField] private float cooldownDuration = 30f;

        [Header("디버그")]
        [SerializeField] private bool enableDebugLogs = false;

        private bool isOnCooldown = false;
        private Coroutine cooldownCoroutine;
        private float remainingCooldownTime = 0f; // 남은 쿨다운 시간 저장
        private bool wasOnCooldownBeforeDisable = false; // 비활성화 전 쿨다운 상태 저장

        private void Awake()
        {
            InitializeComponents();
        }

        private void Start()
        {
            SetupButtonEvents();
            SetInitialState();
        }

        private void InitializeComponents()
        {
            // 자동 컴포넌트 찾기
            if (acornFeedButton == null)
                acornFeedButton = GetComponent<Button>();

            if (acornButtonImage == null)
                acornButtonImage = GetComponent<Image>();

            if (acornButtonText == null)
                acornButtonText = GetComponentInChildren<TextMeshProUGUI>();

            if (acornFeedSpawner == null)
                acornFeedSpawner = FindFirstObjectByType<AcornFeedSpawner>();

            LogDebug("[AcornFeedButtonUI] 컴포넌트 초기화 완료");
        }

        private void SetupButtonEvents()
        {
            if (acornFeedButton != null)
            {
                acornFeedButton.onClick.RemoveAllListeners();
                acornFeedButton.onClick.AddListener(OnAcornFeedButtonClicked);
                LogDebug("[AcornFeedButtonUI] 버튼 이벤트 설정 완료");
            }
        }

        private void SetInitialState()
        {
            // 초기 상태: 버튼 활성화, 이미지 활성화, 텍스트 비활성화
            SetButtonActive(true);
            SetImageActive(true);
            SetTextActive(false);
        }

        private void OnAcornFeedButtonClicked()
        {
            if (isOnCooldown)
            {
                LogDebug("[AcornFeedButtonUI] 쿨다운 중이므로 클릭 무시");
                return;
            }

            LogDebug("[AcornFeedButtonUI] AcornFeed 버튼 클릭됨");

            // 도토리 차감 시도
            if (GameManager.Instance != null)
            {
                if (GameManager.Instance.acornCount > 0)
                {
                    // 도토리 차감
                    GameManager.Instance.UseAcorn();
                    
                    // AcornFeedSpawner 실행
                    if (acornFeedSpawner != null)
                    {
                        acornFeedSpawner.SpawnAcorn();
                        LogDebug("[AcornFeedButtonUI] SpawnAcorn() 실행됨");
                    }
                    else
                    {
                        Debug.LogWarning("[AcornFeedButtonUI] AcornFeedSpawner가 연결되지 않았습니다!");
                    }

                    // 쿨다운 시작
                    StartCooldown();
                }
                else
                {
                    // 도토리 부족 - Toast 표시
                    ShowAcornToast();
                    LogDebug("[AcornFeedButtonUI] 도토리 부족 - Toast 표시");
                }
            }
            else
            {
                Debug.LogWarning("[AcornFeedButtonUI] GameManager가 없습니다!");
            }
        }

        private void StartCooldown()
        {
            if (cooldownCoroutine != null)
            {
                StopCoroutine(cooldownCoroutine);
            }

            isOnCooldown = true;
            cooldownCoroutine = StartCoroutine(CooldownCoroutine());
        }

        private IEnumerator CooldownCoroutine()
        {
            LogDebug($"[AcornFeedButtonUI] 쿨다운 시작: {cooldownDuration}초");

            // 쿨다운 상태로 전환
            SetButtonActive(false);
            SetImageActive(false);
            SetTextActive(true);

            remainingCooldownTime = cooldownDuration;

            while (remainingCooldownTime > 0f)
            {
                // 카운트다운 텍스트 업데이트
                UpdateCountdownText(remainingCooldownTime);

                yield return new WaitForSeconds(1f);
                remainingCooldownTime -= 1f;
            }

            // 쿨다운 완료
            EndCooldown();
        }

        private void UpdateCountdownText(float remainingTime)
        {
            if (acornButtonText != null)
            {
                int seconds = Mathf.CeilToInt(remainingTime);
                acornButtonText.text = $"{seconds}";
                LogDebug($"[AcornFeedButtonUI] 카운트다운: {seconds}");
            }
        }

        private void EndCooldown()
        {
            LogDebug("[AcornFeedButtonUI] 쿨다운 완료");

            isOnCooldown = false;
            cooldownCoroutine = null;
            remainingCooldownTime = 0f;
            wasOnCooldownBeforeDisable = false;

            // 원상복구
            SetButtonActive(true);
            SetImageActive(true);
            SetTextActive(false);

            // 쿨다운 완료 후 TopBar 강제 업데이트 (count 동기화)
            if (TopBarManager.Instance != null)
            {
                TopBarManager.Instance.ForceUpdateAllCounts();
                LogDebug("[AcornFeedButtonUI] 쿨다운 완료 후 TopBar 강제 업데이트");
            }
        }

        private void SetButtonActive(bool active)
        {
            if (acornFeedButton != null)
            {
                acornFeedButton.interactable = active;
            }
        }

        private void SetImageActive(bool active)
        {
            if (acornButtonImage != null)
            {
                acornButtonImage.enabled = active;
            }
        }

        private void SetTextActive(bool active)
        {
            if (acornButtonText != null)
            {
                acornButtonText.gameObject.SetActive(active);
            }
        }

        /// <summary>
        /// 쿨다운 강제 종료 (디버그/테스트용)
        /// </summary>
        public void ForceEndCooldown()
        {
            if (cooldownCoroutine != null)
            {
                StopCoroutine(cooldownCoroutine);
                cooldownCoroutine = null;
            }

            EndCooldown();
            LogDebug("[AcornFeedButtonUI] 쿨다운 강제 종료됨");
        }

        /// <summary>
        /// 쿨다운 시간 설정
        /// </summary>
        public void SetCooldownDuration(float duration)
        {
            cooldownDuration = Mathf.Max(1f, duration);
            LogDebug($"[AcornFeedButtonUI] 쿨다운 시간 설정: {cooldownDuration}초");
        }

        /// <summary>
        /// 현재 쿨다운 상태 확인
        /// </summary>
        public bool IsOnCooldown => isOnCooldown;

        /// <summary>
        /// 남은 쿨다운 시간 확인
        /// </summary>
        public float GetRemainingCooldownTime()
        {
            if (!isOnCooldown) return 0f;
            return remainingCooldownTime;
        }

        /// <summary>
        /// 도토리 부족 Toast 표시
        /// </summary>
        private void ShowAcornToast()
        {
            if (acornToast != null)
            {
                acornToast.SetActive(true);
                LogDebug("[AcornFeedButtonUI] 도토리 부족 Toast 활성화");
            }
            else
            {
                Debug.LogWarning("[AcornFeedButtonUI] acornToast가 연결되지 않았습니다!");
            }
        }

        private void LogDebug(string message)
        {
            if (enableDebugLogs)
            {
                Debug.Log(message);
            }
        }

        /// <summary>
        /// 쿨다운 상태 저장 (탭 이동 시 호출)
        /// </summary>
        public void SaveCooldownState()
        {
            if (isOnCooldown)
            {
                wasOnCooldownBeforeDisable = true;
                LogDebug($"[AcornFeedButtonUI] 쿨다운 상태 저장 - 남은 시간: {remainingCooldownTime}초");
            }
        }

        /// <summary>
        /// 쿨다운 상태 복원 (탭 복귀 시 호출)
        /// </summary>
        public void RestoreCooldownState()
        {
            if (wasOnCooldownBeforeDisable && remainingCooldownTime > 0f)
            {
                LogDebug($"[AcornFeedButtonUI] 쿨다운 상태 복원 - 남은 시간: {remainingCooldownTime}초");
                
                // 기존 코루틴 중지
                if (cooldownCoroutine != null)
                {
                    StopCoroutine(cooldownCoroutine);
                }
                
                // 쿨다운 재시작
                isOnCooldown = true;
                cooldownCoroutine = StartCoroutine(RestoreCooldownCoroutine());
            }
        }

        /// <summary>
        /// 쿨다운 복원 코루틴
        /// </summary>
        private IEnumerator RestoreCooldownCoroutine()
        {
            LogDebug($"[AcornFeedButtonUI] 쿨다운 복원 시작: {remainingCooldownTime}초");

            // 쿨다운 상태로 전환
            SetButtonActive(false);
            SetImageActive(false);
            SetTextActive(true);

            while (remainingCooldownTime > 0f)
            {
                // 카운트다운 텍스트 업데이트
                UpdateCountdownText(remainingCooldownTime);

                yield return new WaitForSeconds(1f);
                remainingCooldownTime -= 1f;
            }

            // 쿨다운 완료
            EndCooldown();
        }

        /// <summary>
        /// 쿨다운 상태 초기화 (미니게임 종료 시 호출)
        /// </summary>
        public void ResetCooldownState()
        {
            wasOnCooldownBeforeDisable = false;
            LogDebug("[AcornFeedButtonUI] 쿨다운 상태 초기화");
        }

        private void OnDestroy()
        {
            if (cooldownCoroutine != null)
            {
                StopCoroutine(cooldownCoroutine);
            }
        }
    }
}
