using UnityEngine;
using UnityEngine.UI;
using GGumtles.Managers;

namespace GGumtles.UI
{
    public class PickAcorn : MonoBehaviour
    {
        [Header("프리팹")]
        [SerializeField] private Button acornCollectButtonPrefab;

        [Header("설정")]
        [SerializeField] private int acornValue = 1; // 획득할 도토리 수
        [SerializeField] private bool enableDebugLogs = false;

        // 상태 관리
        private bool isPicked = false;

        // 이벤트 정의
        public delegate void OnAcornPicked(int amount);
        public event OnAcornPicked OnAcornPickedEvent;

        // 프로퍼티
        public bool IsPicked => isPicked;
        public int AcornValue => acornValue;

        private void Awake()
        {
            InitializePickAcorn();
        }

        private void InitializePickAcorn()
        {
            try
            {
                // Button 컴포넌트가 없으면 추가
                Button pickButton = GetComponent<Button>();
                if (pickButton == null)
                {
                    pickButton = gameObject.AddComponent<Button>();
                }

                // 버튼 이벤트 설정
                pickButton.onClick.RemoveAllListeners();
                pickButton.onClick.AddListener(Pick);

                LogDebug("[PickAcorn] 도토리 수집기 초기화 완료");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[PickAcorn] 초기화 중 오류: {ex.Message}");
            }
        }

        /// <summary>
        /// 도토리 수집
        /// </summary>
        public void Pick()
        {
            if (isPicked) return;

            try
            {
                // 사운드 재생
                AudioManager.Instance?.PlaySFX(AudioManager.SFXType.EarnItem);

                // 도토리 획득
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.PickAcorn();
                }

                // 상태 변경
                isPicked = true;

                // 이벤트 발생
                OnAcornPickedEvent?.Invoke(acornValue);

                // 오브젝트 제거
                Destroy(gameObject);

                LogDebug($"[PickAcorn] 도토리 수집 완료 - 획득: {acornValue}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[PickAcorn] 도토리 수집 중 오류: {ex.Message}");
            }
        }

        /// <summary>
        /// 도토리 수량 설정
        /// </summary>
        public void SetAcornValue(int value)
        {
            acornValue = Mathf.Max(1, value);
            LogDebug($"[PickAcorn] 도토리 수량 설정: {acornValue}");
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
            // 이벤트 구독 해제
            OnAcornPickedEvent = null;
        }
    }
}