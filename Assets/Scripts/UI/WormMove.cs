using UnityEngine;
using UnityEngine.UI;
using GGumtles.Managers;
using GGumtles.Utils;
using GGumtles.Data;
using System.Collections;

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
        [SerializeField] private Vector2 moveSpeedRange = new Vector2(40f, 80f); // px/sec (속도 범위 조정)
        [SerializeField] private Vector2 moveDurationRange = new Vector2(2f, 4f); // sec
        [SerializeField] private Vector2 idleDurationRange = new Vector2(1f, 2.5f); // sec

        [Header("이동 범위 설정")]
        [SerializeField] private Vector2 xRange = new Vector2(-400f, 400f); // X축 이동 범위
        [SerializeField] private Vector2 yRange = new Vector2(-100f, 0f); // Y축 이동 범위

        [Header("도토리 감지 설정")]
        [SerializeField] private float detectionRange = 50f; // 도토리 감지 범위
        [SerializeField] private float moveToAcornSpeed = 100f; // 도토리로 이동할 때의 속도 (속도 조정)
        [SerializeField] private float searchInterval = 2f; // 도토리 탐색 간격 (초)
        [SerializeField] private float searchRange = 300f; // 도토리 탐색 범위
        
        [Header("디버그")]
        [SerializeField] private bool enableDebugLogs = false;

        private enum MoveState { Idle, Move, MoveToAcorn }
        private MoveState state = MoveState.Idle;
        private float stateTimer = 0f;
        private float currentSpeed = 0f;
        private Vector3 targetPosition;
        
        // 도토리 관련 변수
        private Vector3? acornPosition = null;
        private GameObject targetAcorn = null;
        private float searchTimer = 0f; // 도토리 탐색 타이머

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
            
            // 탐색 타이머 초기화
            searchTimer = searchInterval;
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
            
            // 도토리 탐색 (주기적으로 실행)
            UpdateAcornSearch(Time.deltaTime);
            
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
            else if (state == MoveState.Move)
            {
                // 목표 방향으로 이동 (X, Y 모두 이동)
                Vector3 pos = selfRect.anchoredPosition;
                Vector3 direction = (targetPosition - pos).normalized;
                
                // currentSpeed에 이미 boostFactor가 적용되어 있으므로 추가 적용하지 않음
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
            else if (state == MoveState.MoveToAcorn)
            {
                // 도토리로 이동
                if (acornPosition.HasValue && targetAcorn != null)
                {
                    Vector3 pos = selfRect.anchoredPosition;
                    Vector3 direction = (acornPosition.Value - pos).normalized;
                    
                    // moveToAcornSpeed에 boostFactor 적용
                    float boostFactor = GameManager.Instance != null ? GameManager.Instance.BoostFactor : 1f;
                    float finalSpeed = moveToAcornSpeed * boostFactor;
                    
                    pos += direction * finalSpeed * dt;
                    selfRect.anchoredPosition = pos;

                    // X 방향에 따라 반전
                    SetFacing(direction.x > 0f);

                    // 도토리에 도달했는지 확인
                    float distance = Vector3.Distance(pos, acornPosition.Value);
                    if (distance < detectionRange)
                    {
                        // 도토리와 충돌 - 도토리 파괴 및 GameManager에 알림
                        OnAcornReached();
                    }
                }
                else
                {
                    // 도토리가 사라졌으면 일반 상태로 복귀
                    EnterIdle();
                }
            }
        }

        private void EnterIdle()
        {
            state = MoveState.Idle;
            float boostFactor = GameManager.Instance != null ? GameManager.Instance.BoostFactor : 1f;
            stateTimer = Random.Range(idleDurationRange.x, idleDurationRange.y) / boostFactor;
            if (enableDebugLogs) Debug.Log($"[WormMove] Idle - boostFactor: {boostFactor}, stateTimer: {stateTimer}");
        }

        private void EnterMove()
        {
            state = MoveState.Move;
            float boostFactor = GameManager.Instance != null ? GameManager.Instance.BoostFactor : 1f;
            stateTimer = Random.Range(moveDurationRange.x, moveDurationRange.y) / boostFactor;
            currentSpeed = Random.Range(moveSpeedRange.x, moveSpeedRange.y) * boostFactor;
            targetPosition = GetRandomPointInsideBoundary();
            if (enableDebugLogs) Debug.Log($"[WormMove] Move → targetX={targetPosition.x:F1}, speed={currentSpeed:F0}, boostFactor={boostFactor}");
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

        /// <summary>
        /// 도토리 탐색 업데이트 (주기적으로 실행)
        /// </summary>
        private void UpdateAcornSearch(float deltaTime)
        {
            // 이미 도토리를 향해 이동 중이면 탐색하지 않음
            if (state == MoveState.MoveToAcorn) return;
            
            searchTimer -= deltaTime;
            if (searchTimer <= 0f)
            {
                SearchForAcorns();
                searchTimer = searchInterval; // 타이머 리셋
            }
        }

        /// <summary>
        /// 주변에서 인식 가능한 도토리를 찾는 함수
        /// </summary>
        private void SearchForAcorns()
        {
            Vector3 currentPos = selfRect.anchoredPosition;
            
            // 모든 AcornFeedDrop 컴포넌트를 찾아서 탐색 범위 내의 인식 가능한 도토리 확인
            AcornFeedDrop[] acorns = FindObjectsByType<AcornFeedDrop>(FindObjectsSortMode.None);
            AcornFeedDrop nearestAcorn = null;
            float nearestDistance = float.MaxValue;
            
            foreach (var acorn in acorns)
            {
                // 인식 가능한 상태인지 확인
                if (!acorn.IsDetectable) continue;
                
                // 도토리의 월드 좌표를 Worm 부모 기준 로컬 좌표로 변환
                Vector3 acornLocalPos = selfRect.parent != null
                    ? selfRect.parent.InverseTransformPoint(acorn.transform.position)
                    : acorn.transform.position;
                
                // 탐색 범위 내에 있는지 확인 (동일 좌표계에서 계산)
                float distance = Vector3.Distance(currentPos, acornLocalPos);
                if (distance <= searchRange && distance < nearestDistance)
                {
                    nearestAcorn = acorn;
                    nearestDistance = distance;
                }
            }
            
            // 가장 가까운 도토리를 찾았으면 이동 시작
            if (nearestAcorn != null)
            {
                StartMoveToAcorn(nearestAcorn);
                LogDebug($"[WormMove] 도토리 발견! 거리: {nearestDistance:F1}px");
            }
        }

        /// <summary>
        /// 도토리로 이동 시작
        /// </summary>
        private void StartMoveToAcorn(AcornFeedDrop acorn)
        {
            targetAcorn = acorn.gameObject;
            // 도토리의 월드 좌표를 Worm 부모 기준 로컬 좌표로 변환하여 저장
            acornPosition = selfRect.parent != null
                ? selfRect.parent.InverseTransformPoint(acorn.transform.position)
                : acorn.transform.position;
            state = MoveState.MoveToAcorn;
            LogDebug($"[WormMove] 도토리로 이동 시작! 위치: {acornPosition}");
        }

        /// <summary>
        /// 도토리에 도달했을 때 호출되는 함수
        /// </summary>
        private void OnAcornReached()
        {
            LogDebug("[WormMove] 도토리에 도달!");
            
            // 도토리가 여전히 인식 가능한 상태인지 확인
            if (targetAcorn != null)
            {
                var acornComponent = targetAcorn.GetComponent<AcornFeedDrop>();
                if (acornComponent != null && acornComponent.IsDetectable)
                {
                    // 먹는 소리 재생
                    if (AudioManager.Instance != null)
                    {
                        AudioManager.Instance.PlaySFX(AudioManager.SFXType.EatingSound);
                    }

                    WormManager.Instance.CurrentWorm.statistics.totalEatCount++;

                    // 도토리 파괴
                    Destroy(targetAcorn);
                    targetAcorn = null;
                    
                    // GameManager에 timeScale 조정 요청
                    if (GameManager.Instance != null)
                    {
                        GameManager.Instance.StartSpeedBoost();
                    }
                    
                    LogDebug("[WormMove] 도토리 섭취 완료!");
                }
                else
                {
                    LogDebug("[WormMove] 도토리가 이미 다른 Worm에 의해 섭취됨");
                }
            }
            
            // 상태 초기화
            acornPosition = null;
            EnterIdle();
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


