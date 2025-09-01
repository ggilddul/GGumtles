using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using GGumtles.UI;

namespace GGumtles.UI
{
    /// <summary>
    /// 재사용 가능한 레이아웃 그룹 컴포넌트
    /// VerticalLayoutGroup, HorizontalLayoutGroup, GridLayoutGroup을 지원
    /// </summary>
    public class ReusableLayoutGroup : UIBase
    {
        [Header("레이아웃 타입")]
        [SerializeField] private LayoutType layoutType = LayoutType.Vertical;
        
        [Header("레이아웃 컴포넌트")]
        [SerializeField] private VerticalLayoutGroup verticalLayout;
        [SerializeField] private HorizontalLayoutGroup horizontalLayout;
        [SerializeField] private GridLayoutGroup gridLayout;

        [Header("공통 설정")]
        [SerializeField] private float spacing = 5f;
        [SerializeField] private RectOffset padding = new RectOffset(10, 10, 10, 10);
        [SerializeField] private TextAnchor childAlignment = TextAnchor.UpperCenter;
        [SerializeField] private bool childControlHeight = true;
        [SerializeField] private bool childControlWidth = true;
        [SerializeField] private bool childForceExpandHeight = false;
        [SerializeField] private bool childForceExpandWidth = false;

        [Header("그리드 설정")]
        [SerializeField] private Vector2 cellSize = new Vector2(100f, 100f);
        [SerializeField] private Vector2 spacingGrid = new Vector2(5f, 5f);
        [SerializeField] private GridLayoutGroup.Constraint constraint = GridLayoutGroup.Constraint.Flexible;
        [SerializeField] private int constraintCount = 3;

        // [Header("디버그")] // 필드와 함께 주석 처리
        // [SerializeField] private new bool enableDebugLogs = false; // 사용되지 않음

        // 레이아웃 타입 열거형
        public enum LayoutType
        {
            Vertical,
            Horizontal,
            Grid
        }

        // 상태 관리
        private List<GameObject> layoutItems = new List<GameObject>();
        private new bool isInitialized = false;

        // 이벤트 정의
        public delegate void OnLayoutItemAdded(GameObject item);
        public delegate void OnLayoutItemRemoved(GameObject item);
        public delegate void OnLayoutChanged();
        
        public event OnLayoutItemAdded OnLayoutItemAddedEvent;
        public event OnLayoutItemRemoved OnLayoutItemRemovedEvent;
        public event OnLayoutChanged OnLayoutChangedEvent;

        // 프로퍼티
        public LayoutType CurrentLayoutType => layoutType;
        public int ItemCount => layoutItems.Count;
        public RectTransform LayoutTransform => transform as RectTransform;

        protected override void AutoFindComponents()
        {
            // 기존 레이아웃 컴포넌트들 찾기
            verticalLayout = GetComponent<VerticalLayoutGroup>();
            horizontalLayout = GetComponent<HorizontalLayoutGroup>();
            gridLayout = GetComponent<GridLayoutGroup>();
        }

        protected override void SetupDefaultSettings()
        {
            // 레이아웃 타입에 따라 컴포넌트 설정
            SetupLayoutByType();
            isInitialized = true;
        }

        /// <summary>
        /// 레이아웃 타입에 따라 컴포넌트 설정
        /// </summary>
        private void SetupLayoutByType()
        {
            // 모든 레이아웃 컴포넌트 비활성화
            if (verticalLayout != null) verticalLayout.enabled = false;
            if (horizontalLayout != null) horizontalLayout.enabled = false;
            if (gridLayout != null) gridLayout.enabled = false;

            switch (layoutType)
            {
                case LayoutType.Vertical:
                    SetupVerticalLayout();
                    break;
                case LayoutType.Horizontal:
                    SetupHorizontalLayout();
                    break;
                case LayoutType.Grid:
                    SetupGridLayout();
                    break;
            }
        }

        /// <summary>
        /// 수직 레이아웃 설정
        /// </summary>
        private void SetupVerticalLayout()
        {
            if (verticalLayout == null)
            {
                verticalLayout = gameObject.AddComponent<VerticalLayoutGroup>();
            }

            verticalLayout.enabled = true;
            verticalLayout.spacing = spacing;
            verticalLayout.padding = padding;
            verticalLayout.childAlignment = childAlignment;
            verticalLayout.childControlHeight = childControlHeight;
            verticalLayout.childControlWidth = childControlWidth;
            verticalLayout.childForceExpandHeight = childForceExpandHeight;
            verticalLayout.childForceExpandWidth = childForceExpandWidth;

            LogDebug("[ReusableLayoutGroup] 수직 레이아웃 설정 완료");
        }

        /// <summary>
        /// 수평 레이아웃 설정
        /// </summary>
        private void SetupHorizontalLayout()
        {
            if (horizontalLayout == null)
            {
                horizontalLayout = gameObject.AddComponent<HorizontalLayoutGroup>();
            }

            horizontalLayout.enabled = true;
            horizontalLayout.spacing = spacing;
            horizontalLayout.padding = padding;
            horizontalLayout.childAlignment = childAlignment;
            horizontalLayout.childControlHeight = childControlHeight;
            horizontalLayout.childControlWidth = childControlWidth;
            horizontalLayout.childForceExpandHeight = childForceExpandHeight;
            horizontalLayout.childForceExpandWidth = childForceExpandWidth;

            LogDebug("[ReusableLayoutGroup] 수평 레이아웃 설정 완료");
        }

        /// <summary>
        /// 그리드 레이아웃 설정
        /// </summary>
        private void SetupGridLayout()
        {
            if (gridLayout == null)
            {
                gridLayout = gameObject.AddComponent<GridLayoutGroup>();
            }

            gridLayout.enabled = true;
            gridLayout.cellSize = cellSize;
            gridLayout.spacing = spacingGrid;
            gridLayout.padding = padding;
            gridLayout.childAlignment = childAlignment;
            gridLayout.constraint = constraint;
            gridLayout.constraintCount = constraintCount;

            LogDebug("[ReusableLayoutGroup] 그리드 레이아웃 설정 완료");
        }

        /// <summary>
        /// 레이아웃 타입 변경
        /// </summary>
        public void SetLayoutType(LayoutType newType)
        {
            if (layoutType != newType)
            {
                layoutType = newType;
                SetupLayoutByType();
                OnLayoutChangedEvent?.Invoke();
                LogDebug($"[ReusableLayoutGroup] 레이아웃 타입 변경: {newType}");
            }
        }

        /// <summary>
        /// 아이템 추가
        /// </summary>
        public GameObject AddItem(GameObject itemPrefab)
        {
            if (!isInitialized)
            {
                Debug.LogError("[ReusableLayoutGroup] 아직 초기화되지 않았습니다.");
                return null;
            }

            try
            {
                GameObject newItem = Instantiate(itemPrefab, transform);
                layoutItems.Add(newItem);
                
                OnLayoutItemAddedEvent?.Invoke(newItem);
                LogDebug($"[ReusableLayoutGroup] 아이템 추가됨. 총 개수: {layoutItems.Count}");
                
                return newItem;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[ReusableLayoutGroup] 아이템 추가 중 오류: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 아이템 제거
        /// </summary>
        public void RemoveItem(GameObject item)
        {
            if (layoutItems.Contains(item))
            {
                layoutItems.Remove(item);
                Destroy(item);
                
                OnLayoutItemRemovedEvent?.Invoke(item);
                LogDebug($"[ReusableLayoutGroup] 아이템 제거됨. 총 개수: {layoutItems.Count}");
            }
        }

        /// <summary>
        /// 모든 아이템 제거
        /// </summary>
        public void ClearAllItems()
        {
            foreach (var item in layoutItems)
            {
                if (item != null)
                {
                    Destroy(item);
                }
            }
            
            layoutItems.Clear();
            LogDebug("[ReusableLayoutGroup] 모든 아이템 제거됨");
        }

        /// <summary>
        /// 간격 설정
        /// </summary>
        public void SetSpacing(float newSpacing)
        {
            spacing = newSpacing;
            
            if (verticalLayout != null && verticalLayout.enabled)
            {
                verticalLayout.spacing = newSpacing;
            }
            if (horizontalLayout != null && horizontalLayout.enabled)
            {
                horizontalLayout.spacing = newSpacing;
            }
            
            LogDebug($"[ReusableLayoutGroup] 간격 설정: {newSpacing}");
        }

        /// <summary>
        /// 패딩 설정
        /// </summary>
        public void SetPadding(RectOffset newPadding)
        {
            padding = newPadding;
            
            if (verticalLayout != null && verticalLayout.enabled)
            {
                verticalLayout.padding = newPadding;
            }
            if (horizontalLayout != null && horizontalLayout.enabled)
            {
                horizontalLayout.padding = newPadding;
            }
            if (gridLayout != null && gridLayout.enabled)
            {
                gridLayout.padding = newPadding;
            }
            
            LogDebug($"[ReusableLayoutGroup] 패딩 설정: {newPadding}");
        }

        /// <summary>
        /// 자식 정렬 설정
        /// </summary>
        public void SetChildAlignment(TextAnchor alignment)
        {
            childAlignment = alignment;
            
            if (verticalLayout != null && verticalLayout.enabled)
            {
                verticalLayout.childAlignment = alignment;
            }
            if (horizontalLayout != null && horizontalLayout.enabled)
            {
                horizontalLayout.childAlignment = alignment;
            }
            if (gridLayout != null && gridLayout.enabled)
            {
                gridLayout.childAlignment = alignment;
            }
            
            LogDebug($"[ReusableLayoutGroup] 자식 정렬 설정: {alignment}");
        }

        /// <summary>
        /// 그리드 셀 크기 설정
        /// </summary>
        public void SetCellSize(Vector2 size)
        {
            cellSize = size;
            
            if (gridLayout != null && gridLayout.enabled)
            {
                gridLayout.cellSize = size;
            }
            
            LogDebug($"[ReusableLayoutGroup] 셀 크기 설정: {size}");
        }

        /// <summary>
        /// 그리드 간격 설정
        /// </summary>
        public void SetGridSpacing(Vector2 gridSpacing)
        {
            spacingGrid = gridSpacing;
            
            if (gridLayout != null && gridLayout.enabled)
            {
                gridLayout.spacing = gridSpacing;
            }
            
            LogDebug($"[ReusableLayoutGroup] 그리드 간격 설정: {gridSpacing}");
        }

        /// <summary>
        /// 그리드 제약 설정
        /// </summary>
        public void SetGridConstraint(GridLayoutGroup.Constraint newConstraint, int count)
        {
            constraint = newConstraint;
            constraintCount = count;
            
            if (gridLayout != null && gridLayout.enabled)
            {
                gridLayout.constraint = newConstraint;
                gridLayout.constraintCount = count;
            }
            
            LogDebug($"[ReusableLayoutGroup] 그리드 제약 설정: {newConstraint}, 개수: {count}");
        }

        /// <summary>
        /// 자식 크기 제어 설정
        /// </summary>
        public void SetChildControl(bool controlWidth, bool controlHeight, bool forceExpandWidth = false, bool forceExpandHeight = false)
        {
            childControlWidth = controlWidth;
            childControlHeight = controlHeight;
            childForceExpandWidth = forceExpandWidth;
            childForceExpandHeight = forceExpandHeight;
            
            if (verticalLayout != null && verticalLayout.enabled)
            {
                verticalLayout.childControlWidth = controlWidth;
                verticalLayout.childControlHeight = controlHeight;
                verticalLayout.childForceExpandWidth = forceExpandWidth;
                verticalLayout.childForceExpandHeight = forceExpandHeight;
            }
            if (horizontalLayout != null && horizontalLayout.enabled)
            {
                horizontalLayout.childControlWidth = controlWidth;
                horizontalLayout.childControlHeight = controlHeight;
                horizontalLayout.childForceExpandWidth = forceExpandWidth;
                horizontalLayout.childForceExpandHeight = forceExpandHeight;
            }
            
            LogDebug($"[ReusableLayoutGroup] 자식 제어 설정 - 너비: {controlWidth}, 높이: {controlHeight}");
        }

        /// <summary>
        /// 레이아웃 새로고침
        /// </summary>
        public void RefreshLayout()
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);
            LogDebug("[ReusableLayoutGroup] 레이아웃 새로고침 완료");
        }

        /// <summary>
        /// 레이아웃 그룹 정보 반환
        /// </summary>
        public string GetLayoutGroupInfo()
        {
            var info = new System.Text.StringBuilder();
            info.AppendLine($"[ReusableLayoutGroup 정보]");
            info.AppendLine($"레이아웃 타입: {layoutType}");
            info.AppendLine($"아이템 개수: {layoutItems.Count}");
            info.AppendLine($"간격: {spacing}");
            info.AppendLine($"자식 정렬: {childAlignment}");
            info.AppendLine($"자식 너비 제어: {childControlWidth}");
            info.AppendLine($"자식 높이 제어: {childControlHeight}");

            if (layoutType == LayoutType.Grid)
            {
                info.AppendLine($"셀 크기: {cellSize}");
                info.AppendLine($"그리드 간격: {spacingGrid}");
                info.AppendLine($"제약: {constraint}");
                info.AppendLine($"제약 개수: {constraintCount}");
            }

            return info.ToString();
        }

        protected override void OnDestroy()
        {
            // 이벤트 구독 해제
            OnLayoutItemAddedEvent = null;
            OnLayoutItemRemovedEvent = null;
            OnLayoutChangedEvent = null;

            base.OnDestroy();
        }
    }
}
