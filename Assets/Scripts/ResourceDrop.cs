    using UnityEngine;

    public class ResourceDrop : MonoBehaviour
    {
        public float fallSpeed = 300f; // 픽셀/초 기준
        public float fallDistance = 300f; // 총 낙하 거리

        private Vector3 startPosition;
        private float fallenDistance = 0f;

        private void Start()
        {
            startPosition = transform.position;
        }

        private void Update()
        {
            float move = fallSpeed * Time.deltaTime;
            transform.Translate(Vector3.down * move);
            fallenDistance += move;

            if (fallenDistance >= fallDistance)
            {
                enabled = false; // 낙하 멈춤 (이후 클릭 대기 상태로 전환됨)
            }
        }
    }
