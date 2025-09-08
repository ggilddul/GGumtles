using UnityEngine;
using UnityEngine.UI;
using GGumtles.Managers;
using GGumtles.Utils;
using GGumtles.Data;

namespace GGumtles.UI
{
    /// <summary>
    /// 홈 탭에서 웜이 경계 내에서 좌/우 이동/대기하며 표시되도록 하는 컴포넌트
    /// - 경계(RectTransform) 내에서만 이동
    /// - 이동 방향에 따라 좌/우 반전
    /// - 대기 상태/이동 상태를 랜덤하게 반복
    /// - SpriteManager를 통해 Life Stage Scale 및 아이템 포함 스프라이트 적용
    /// </summary>
    public class WormMove : MonoBehaviour
    {
        [Header("필수 컴포넌트")]
        [SerializeField] private Image wormImage;                  // UI 이미지

        [Header("이동 설정")]
        [SerializeField] private Vector2 moveSpeedRange = new Vector2(80f, 140f); // px/sec
        [SerializeField] private Vector2 moveDurationRange = new Vector2(2f, 4f); // sec
        [SerializeField] private Vector2 idleDurationRange = new Vector2(1f, 2.5f); // sec

        [Header("이동 범위 설정")]
        [SerializeField] private Vector2 xRange = new Vector2(-400f, 400f); // X축 이동 범위
        [SerializeField] private Vector2 yRange = new Vector2(-100f, 0f); // Y축 이동 범위

        [Header("디버그")]
        [SerializeField] private bool enableDebugLogs = false;

        private enum MoveState { Idle, Move }
        private MoveState state = MoveState.Idle;
        private float stateTimer = 0f;
        private float currentSpeed = 0f;
        private Vector3 targetPosition;

        // 렌더/스케일 관리
        private float baseScale = 1f; // life stage scale 값 보관
        private bool facingRight = true;

        private RectTransform selfRect;

        private void Awake()
        {
            selfRect = GetComponent<RectTransform>();
            if (wormImage == null)
                wormImage = GetComponent<Image>();
        }

        private void OnEnable()
        {
            SubscribeEvents();
            RefreshSprite();
            EnterIdle();
        }

        private void OnDisable()
        {
            UnsubscribeEvents();
        }

        private void Update()
        {
            // 알상태에서는 움직이지 않음
            if (IsEggStage())
            {
                return;
            }
            
            UpdateState(Time.deltaTime);
            ClampInsideBoundary();
        }

        private void UpdateState(float dt)
        {
            stateTimer -= dt;
            if (state == MoveState.Idle)
            {
                if (stateTimer <= 0f)
                {
                    EnterMove();
                }
            }
            else // Move
            {
                // 목표 방향으로 이동 (X, Y 모두 이동)
                Vector3 pos = selfRect.anchoredPosition;
                Vector3 direction = (targetPosition - pos).normalized;
                pos += direction * currentSpeed * dt;
                selfRect.anchoredPosition = pos;

                // X 방향에 따라 반전
                SetFacing(direction.x > 0f);

                // 도착 또는 시간 만료 시 대기 전환
                float distance = Vector3.Distance(pos, targetPosition);
                if (distance < 5f || stateTimer <= 0f)
                {
                    EnterIdle();
                }
            }
        }

        private void EnterIdle()
        {
            state = MoveState.Idle;
            stateTimer = Random.Range(idleDurationRange.x, idleDurationRange.y);
            if (enableDebugLogs) Debug.Log("[WormMove] Idle");
        }

        private void EnterMove()
        {
            state = MoveState.Move;
            stateTimer = Random.Range(moveDurationRange.x, moveDurationRange.y);
            currentSpeed = Random.Range(moveSpeedRange.x, moveSpeedRange.y);
            targetPosition = GetRandomPointInsideBoundary();
            if (enableDebugLogs) Debug.Log($"[WormMove] Move → targetX={targetPosition.x:F1}, speed={currentSpeed:F0}");
        }

        private void SetFacing(bool toRight)
        {
            if (facingRight == toRight) return;
            facingRight = toRight;
            Vector3 scale = Vector3.one * baseScale;
            // 기본 스프라이트가 '왼쪽'을 보고 있으므로,
            // 오른쪽을 보게 하려면 X를 음수로 반전해야 한다.
            scale.x *= facingRight ? -1f : 1f;
            selfRect.localScale = scale;
        }

        private void ClampInsideBoundary()
        {
            // 웜의 크기를 고려한 여백 계산
            Vector2 wormSize = selfRect.sizeDelta * baseScale;
            float halfWidth = wormSize.x * 0.5f;
            float halfHeight = wormSize.y * 0.5f;
            
            // 인스펙터 설정에 따른 범위에서 클램핑
            Vector2 currentPos = selfRect.anchoredPosition;
            float clampedX = Mathf.Clamp(currentPos.x, xRange.x + halfWidth, xRange.y - halfWidth);
            float clampedY = Mathf.Clamp(currentPos.y, yRange.x + halfHeight, yRange.y - halfHeight);
            
            // 위치가 변경되었을 때만 업데이트
            if (Mathf.Abs(currentPos.x - clampedX) > 0.1f || Mathf.Abs(currentPos.y - clampedY) > 0.1f)
            {
                selfRect.anchoredPosition = new Vector2(clampedX, clampedY);
                LogDebug($"[WormMove] 경계 클램핑: ({currentPos.x:F1}, {currentPos.y:F1}) → ({clampedX:F1}, {clampedY:F1})");
            }
        }

        private Vector3 GetRandomPointInsideBoundary()
        {
            // 웜의 크기를 고려한 여백 계산
            Vector2 wormSize = selfRect.sizeDelta * baseScale;
            float halfWidth = wormSize.x * 0.5f;
            float halfHeight = wormSize.y * 0.5f;
            
            // 인스펙터 설정에 따른 범위에서 랜덤 위치 생성
            float x = Random.Range(xRange.x + halfWidth, xRange.y - halfWidth);
            float y = Random.Range(yRange.x + halfHeight, yRange.y - halfHeight);
            
            LogDebug($"[WormMove] 랜덤 목표 위치 생성: ({x:F1}, {y:F1}) [X범위: {xRange.x:F1}~{xRange.y:F1}, Y범위: {yRange.x:F1}~{yRange.y:F1}]");
            return new Vector3(x, y, 0f);
        }


        private void RefreshSprite()
        {
            var worm = WormManager.Instance?.GetCurrentWorm();
            if (worm == null || SpriteManager.Instance == null)
            {
                if (wormImage != null) wormImage.enabled = false;
                return;
            }

            var completed = SpriteManager.Instance.CreateCompletedWormSprite(worm);
            if (completed != null && completed.sprite != null)
            {
                baseScale = Mathf.Max(0.01f, completed.scale);
                if (wormImage != null)
                {
                    wormImage.sprite = completed.sprite;
                    wormImage.enabled = true;
                }

                // 좌우 반전 포함 스케일 적용
                Vector3 scale = Vector3.one * baseScale;
                // 기본 스프라이트는 왼쪽을 봄 → 오른쪽일 때만 반전
                scale.x *= facingRight ? -1f : 1f;
                selfRect.localScale = scale;
            }
            else if (wormImage != null)
            {
                wormImage.enabled = false;
            }
        }

        private void SubscribeEvents()
        {
            if (WormManager.Instance != null)
            {
                WormManager.Instance.OnCurrentWormChangedEvent += OnWormChanged;
                WormManager.Instance.OnWormEvolvedEvent += OnWormEvolved;
            }
            if (ItemManager.Instance != null)
            {
                ItemManager.Instance.OnItemEquippedEvent += OnItemChanged;
                ItemManager.Instance.OnItemUnequippedEvent += OnItemChanged;
            }
        }

        private void UnsubscribeEvents()
        {
            if (WormManager.Instance != null)
            {
                WormManager.Instance.OnCurrentWormChangedEvent -= OnWormChanged;
                WormManager.Instance.OnWormEvolvedEvent -= OnWormEvolved;
            }
            if (ItemManager.Instance != null)
            {
                ItemManager.Instance.OnItemEquippedEvent -= OnItemChanged;
                ItemManager.Instance.OnItemUnequippedEvent -= OnItemChanged;
            }
        }

        private void OnWormChanged(WormData prev, WormData curr)
        {
            RefreshSprite();
        }

        private void OnWormEvolved(WormData worm, int fromStage, int toStage)
        {
            RefreshSprite();
        }

        private void OnItemChanged(string itemId, ItemData.ItemType type)
        {
            RefreshSprite();
        }

        /// <summary>
        /// 현재 웜이 알상태인지 확인
        /// </summary>
        private bool IsEggStage()
        {
            var worm = WormManager.Instance?.GetCurrentWorm();
            if (worm == null) return false;
            
            // LifeStage.Egg (0)인지 확인
            return worm.lifeStage == 0;
        }

        private void LogDebug(string message)
        {
            if (enableDebugLogs)
            {
                Debug.Log(message);
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (selfRect == null) return;
            
            // 인스펙터 설정에 따른 이동 범위를 Gizmo로 표시
            Vector3 a = new Vector3(xRange.x, yRange.y, 0f);
            Vector3 b = new Vector3(xRange.y, yRange.y, 0f);
            Vector3 c = new Vector3(xRange.y, yRange.x, 0f);
            Vector3 d = new Vector3(xRange.x, yRange.x, 0f);
            
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(selfRect.TransformPoint(a), selfRect.TransformPoint(b));
            Gizmos.DrawLine(selfRect.TransformPoint(b), selfRect.TransformPoint(c));
            Gizmos.DrawLine(selfRect.TransformPoint(c), selfRect.TransformPoint(d));
            Gizmos.DrawLine(selfRect.TransformPoint(d), selfRect.TransformPoint(a));
        }
    }
}


