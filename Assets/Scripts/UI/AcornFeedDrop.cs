using UnityEngine;
using GGumtles.Managers;

namespace GGumtles.UI
{
    /// <summary>
    /// AcornFeed 버튼으로 소환되는 전용 도토리 낙하
    /// - 초기 속도 0
    /// - 중력 가속도 적용
    /// - 최대 바운스 2회
    /// - 최소/최대 낙하 시간 내에서 착지 (낙하 시간에 대응하여 낙하 거리 유도)
    /// </summary>
    public class AcornFeedDrop : MonoBehaviour
    {
        [Header("낙하 설정")]
        [SerializeField] private float minFallTime = 0.8f;
        [SerializeField] private float maxFallTime = 1.4f;

        [Header("물리")]
        [SerializeField] private float gravity = 980f; // px/s^2
        [SerializeField] private int maxBounces = 2;

        [Header("효과")]
        [SerializeField] private bool enableSound = true;
        [SerializeField] private GameObject bounceParticle;
        [SerializeField] private GameObject landParticle;

        private float targetY;
        private float elapsed;
        private float fallDuration;
        private Vector3 startPos;
        private Vector3 velocity; // 초기 0
        private int bounceCount;
        private bool landed;

        public void Initialize(Vector3 startPosition)
        {
            transform.position = startPosition;
            startPos = startPosition;
            fallDuration = Random.Range(minFallTime, maxFallTime);
            // 낙하 시간에 대응하는 낙하 거리 유도: s = 1/2 * g * t^2
            float fallDistance = 0.5f * gravity * fallDuration * fallDuration;
            targetY = startPosition.y - fallDistance;
            elapsed = 0f;
            velocity = Vector3.zero;
            bounceCount = 0;
            landed = false;
        }

        private void Update()
        {
            if (landed) return;

            elapsed += Time.deltaTime;

            // 시간 기반 목표 보정: 지정된 시간 내 바닥 도달을 보장하기 위해 보간 + 중력
            // 1) 중력에 의한 속도 증가
            velocity += Vector3.down * gravity * Time.deltaTime;

            // 2) 위치 업데이트
            transform.position += velocity * Time.deltaTime;

            // 바닥 도달 체크
            if (transform.position.y <= targetY + 0.01f)
            {
                HandleGroundContact();
            }
        }

        private void HandleGroundContact()
        {
            // 정확한 바닥 위치 정렬
            transform.position = new Vector3(transform.position.x, targetY, transform.position.z);

            if (bounceCount < maxBounces && Mathf.Abs(velocity.y) > 200f)
            {
                // 반사(감쇠)
                velocity.y = Mathf.Abs(velocity.y) * 0.3f;
                bounceCount++;
                if (enableSound) AudioManager.Instance?.PlaySFX(AudioManager.SFXType.ItemDrop);
                if (bounceParticle != null) Instantiate(bounceParticle, transform.position, Quaternion.identity);
            }
            else
            {
                landed = true;
                velocity = Vector3.zero;
                if (enableSound) AudioManager.Instance?.PlaySFX(AudioManager.SFXType.ItemDrop);
                if (landParticle != null) Instantiate(landParticle, transform.position, Quaternion.identity);
            }
        }
    }
}


