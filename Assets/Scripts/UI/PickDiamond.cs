using UnityEngine;
using UnityEngine.UI;
using GGumtles.Managers;

namespace GGumtles.UI
{
    public class PickDiamond : MonoBehaviour
    {
        [Header("프리팹")]
        [SerializeField] private Button diamondCollectButtonPrefab;

        [Header("설정")]
        [SerializeField] private int diamondValue = 1; // 획득할 다이아몬드 수
        [SerializeField] private bool enableDebugLogs = false;

        // 상태 관리
        private bool isPicked = false;

        // 이벤트 정의
        public delegate void OnDiamondPicked(int amount);
        public event OnDiamondPicked OnDiamondPickedEvent;

        // 프로퍼티
        public bool IsPicked => isPicked;
        public int DiamondValue => diamondValue;

        private void Awake()
        {
            InitializePickDiamond();
        }

        private void InitializePickDiamond()
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

                LogDebug("[PickDiamond] 다이아몬드 수집기 초기화 완료");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[PickDiamond] 초기화 중 오류: {ex.Message}");
            }
        }

        /// <summary>
        /// 다이아몬드 수집
        /// </summary>
        public void Pick()
        {
            if (isPicked) return;

            try
            {
                // 사운드 재생
                AudioManager.Instance?.PlaySFX(AudioManager.SFXType.EarnItem);

                // 다이아몬드 획득
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.diamondCount++;
                }

                // 상태 변경
                isPicked = true;

                // 이벤트 발생
                OnDiamondPickedEvent?.Invoke(diamondValue);

                // 오브젝트 제거
                Destroy(gameObject);

                LogDebug($"[PickDiamond] 다이아몬드 수집 완료 - 획득: {diamondValue}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[PickDiamond] 다이아몬드 수집 중 오류: {ex.Message}");
            }
        }

        /// <summary>
        /// 다이아몬드 수량 설정
        /// </summary>
        public void SetDiamondValue(int value)
        {
            diamondValue = Mathf.Max(1, value);
            LogDebug($"[PickDiamond] 다이아몬드 수량 설정: {diamondValue}");
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
            OnDiamondPickedEvent = null;
        }
    }
}