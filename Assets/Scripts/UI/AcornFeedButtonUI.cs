using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

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

        [Header("쿨다운 설정")]
        [SerializeField] private float cooldownDuration = 30f;

        [Header("디버그")]
        [SerializeField] private bool enableDebugLogs = false;

        private bool isOnCooldown = false;
        private Coroutine cooldownCoroutine;

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

            float remainingTime = cooldownDuration;

            while (remainingTime > 0f)
            {
                // 카운트다운 텍스트 업데이트
                UpdateCountdownText(remainingTime);

                yield return new WaitForSeconds(1f);
                remainingTime -= 1f;
            }

            // 쿨다운 완료
            EndCooldown();
        }

        private void UpdateCountdownText(float remainingTime)
        {
            if (acornButtonText != null)
            {
                int seconds = Mathf.CeilToInt(remainingTime);
                acornButtonText.text = $"{seconds}초";
                LogDebug($"[AcornFeedButtonUI] 카운트다운: {seconds}초");
            }
        }

        private void EndCooldown()
        {
            LogDebug("[AcornFeedButtonUI] 쿨다운 완료");

            isOnCooldown = false;
            cooldownCoroutine = null;

            // 원상복구
            SetButtonActive(true);
            SetImageActive(true);
            SetTextActive(false);
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
            
            // 정확한 남은 시간 계산은 코루틴 내부에서 관리
            // 여기서는 대략적인 값만 반환
            return cooldownDuration;
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
            if (cooldownCoroutine != null)
            {
                StopCoroutine(cooldownCoroutine);
            }
        }
    }
}
