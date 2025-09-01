using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using GGumtles.UI;

namespace GGumtles.UI
{
    /// <summary>
    /// 재사용 가능한 스크롤 뷰 컴포넌트
    /// ScrollRect와 VerticalLayoutGroup을 결합한 범용 스크롤 뷰
    /// </summary>
    public class ReusableScrollView : UIBase
    {
        [Header("스크롤 뷰 컴포넌트")]
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private RectTransform viewport;
        [SerializeField] private RectTransform content;
        [SerializeField] private VerticalLayoutGroup layoutGroup;
        [SerializeField] private ContentSizeFitter contentSizeFitter;

        [Header("스크롤 설정")]
        [SerializeField] private bool enableHorizontalScroll = false;
        [SerializeField] private bool enableVerticalScroll = true;
        [SerializeField] private float scrollSensitivity = 1f;
        [SerializeField] private float scrollDecelerationRate = 0.135f;

        [Header("레이아웃 설정")]
        [SerializeField] private float spacing = 5f;
        [SerializeField] private RectOffset padding = new RectOffset(10, 10, 10, 10);
        [SerializeField] private TextAnchor childAlignment = TextAnchor.UpperCenter;
        [SerializeField] private bool childControlHeight = true;
        [SerializeField] private bool childControlWidth = true;

        [Header("스크롤바 설정")]
        [SerializeField] private Scrollbar verticalScrollbar;
        [SerializeField] private Scrollbar horizontalScrollbar;
        [SerializeField] private bool showVerticalScrollbar = true;
        [SerializeField] private bool showHorizontalScrollbar = false;

        // [Header("디버그")] // 필드와 함께 주석 처리
        // [SerializeField] private new bool enableDebugLogs = false; // 사용되지 않음

        // 상태 관리
        private List<GameObject> contentItems = new List<GameObject>();
        private new bool isInitialized = false;

        // 이벤트 정의
        public delegate void OnScrollValueChanged(Vector2 scrollValue);
        public delegate void OnContentItemAdded(GameObject item);
        public delegate void OnContentItemRemoved(GameObject item);
        
        public event OnScrollValueChanged OnScrollValueChangedEvent;
        public event OnContentItemAdded OnContentItemAddedEvent;
        public event OnContentItemRemoved OnContentItemRemovedEvent;

        // 프로퍼티
        public ScrollRect ScrollRect => scrollRect;
        public RectTransform Content => content;
        public int ItemCount => contentItems.Count;
        public bool IsScrolling => scrollRect.velocity.magnitude > 0.1f;

        protected override void AutoFindComponents()
        {
            if (scrollRect == null)
                scrollRect = GetComponent<ScrollRect>();
            if (viewport == null)
                viewport = transform.Find("Viewport")?.GetComponent<RectTransform>();
            if (content == null)
                content = transform.Find("Viewport/Content")?.GetComponent<RectTransform>();
            if (layoutGroup == null)
                layoutGroup = content?.GetComponent<VerticalLayoutGroup>();
            if (contentSizeFitter == null)
                contentSizeFitter = content?.GetComponent<ContentSizeFitter>();
            if (verticalScrollbar == null)
                verticalScrollbar = transform.Find("Scrollbar Vertical")?.GetComponent<Scrollbar>();
            if (horizontalScrollbar == null)
                horizontalScrollbar = transform.Find("Scrollbar Horizontal")?.GetComponent<Scrollbar>();
        }

        protected override void SetupDefaultSettings()
        {
            if (scrollRect != null)
            {
                scrollRect.horizontal = enableHorizontalScroll;
                scrollRect.vertical = enableVerticalScroll;
                scrollRect.scrollSensitivity = scrollSensitivity;
                scrollRect.decelerationRate = scrollDecelerationRate;
                scrollRect.onValueChanged.AddListener(OnScrollValueChangedHandler);
            }

            if (layoutGroup != null)
            {
                layoutGroup.spacing = spacing;
                layoutGroup.padding = padding;
                layoutGroup.childAlignment = childAlignment;
                layoutGroup.childControlHeight = childControlHeight;
                layoutGroup.childControlWidth = childControlWidth;
            }

            if (contentSizeFitter != null)
            {
                contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            }

            // 스크롤바 설정
            if (verticalScrollbar != null)
            {
                verticalScrollbar.gameObject.SetActive(showVerticalScrollbar);
            }
            if (horizontalScrollbar != null)
            {
                horizontalScrollbar.gameObject.SetActive(showHorizontalScrollbar);
            }

            isInitialized = true;
        }

        /// <summary>
        /// 스크롤 값 변경 이벤트 핸들러
        /// </summary>
        private void OnScrollValueChangedHandler(Vector2 scrollValue)
        {
            OnScrollValueChangedEvent?.Invoke(scrollValue);
            LogDebug($"[ReusableScrollView] 스크롤 값 변경: {scrollValue}");
        }

        /// <summary>
        /// 아이템 추가
        /// </summary>
        public GameObject AddItem(GameObject itemPrefab)
        {
            if (!isInitialized || content == null)
            {
                Debug.LogError("[ReusableScrollView] 아직 초기화되지 않았습니다.");
                return null;
            }

            try
            {
                GameObject newItem = Instantiate(itemPrefab, content);
                contentItems.Add(newItem);
                
                OnContentItemAddedEvent?.Invoke(newItem);
                LogDebug($"[ReusableScrollView] 아이템 추가됨. 총 개수: {contentItems.Count}");
                
                return newItem;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[ReusableScrollView] 아이템 추가 중 오류: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 아이템 제거
        /// </summary>
        public void RemoveItem(GameObject item)
        {
            if (contentItems.Contains(item))
            {
                contentItems.Remove(item);
                Destroy(item);
                
                OnContentItemRemovedEvent?.Invoke(item);
                LogDebug($"[ReusableScrollView] 아이템 제거됨. 총 개수: {contentItems.Count}");
            }
        }

        /// <summary>
        /// 모든 아이템 제거
        /// </summary>
        public void ClearAllItems()
        {
            foreach (var item in contentItems)
            {
                if (item != null)
                {
                    Destroy(item);
                }
            }
            
            contentItems.Clear();
            LogDebug("[ReusableScrollView] 모든 아이템 제거됨");
        }

        /// <summary>
        /// 스크롤 위치 설정
        /// </summary>
        public void SetScrollPosition(Vector2 position)
        {
            if (scrollRect != null)
            {
                scrollRect.normalizedPosition = position;
                LogDebug($"[ReusableScrollView] 스크롤 위치 설정: {position}");
            }
        }

        /// <summary>
        /// 맨 위로 스크롤
        /// </summary>
        public void ScrollToTop()
        {
            SetScrollPosition(new Vector2(0f, 1f));
        }

        /// <summary>
        /// 맨 아래로 스크롤
        /// </summary>
        public void ScrollToBottom()
        {
            SetScrollPosition(new Vector2(0f, 0f));
        }

        /// <summary>
        /// 특정 아이템으로 스크롤
        /// </summary>
        public void ScrollToItem(int itemIndex)
        {
            if (itemIndex < 0 || itemIndex >= contentItems.Count)
            {
                Debug.LogWarning($"[ReusableScrollView] 유효하지 않은 아이템 인덱스: {itemIndex}");
                return;
            }

            // 간단한 계산 (정확한 위치 계산은 더 복잡함)
            float normalizedPosition = 1f - ((float)itemIndex / contentItems.Count);
            SetScrollPosition(new Vector2(0f, normalizedPosition));
        }

        /// <summary>
        /// 스크롤 활성화/비활성화
        /// </summary>
        public void SetScrollEnabled(bool enabled)
        {
            if (scrollRect != null)
            {
                scrollRect.enabled = enabled;
                LogDebug($"[ReusableScrollView] 스크롤 {(enabled ? "활성화" : "비활성화")}");
            }
        }

        /// <summary>
        /// 수직 스크롤 활성화/비활성화
        /// </summary>
        public void SetVerticalScrollEnabled(bool enabled)
        {
            if (scrollRect != null)
            {
                scrollRect.vertical = enabled;
                enableVerticalScroll = enabled;
                
                if (verticalScrollbar != null)
                {
                    verticalScrollbar.gameObject.SetActive(enabled && showVerticalScrollbar);
                }
                
                LogDebug($"[ReusableScrollView] 수직 스크롤 {(enabled ? "활성화" : "비활성화")}");
            }
        }

        /// <summary>
        /// 수평 스크롤 활성화/비활성화
        /// </summary>
        public void SetHorizontalScrollEnabled(bool enabled)
        {
            if (scrollRect != null)
            {
                scrollRect.horizontal = enabled;
                enableHorizontalScroll = enabled;
                
                if (horizontalScrollbar != null)
                {
                    horizontalScrollbar.gameObject.SetActive(enabled && showHorizontalScrollbar);
                }
                
                LogDebug($"[ReusableScrollView] 수평 스크롤 {(enabled ? "활성화" : "비활성화")}");
            }
        }

        /// <summary>
        /// 스크롤 감도 설정
        /// </summary>
        public void SetScrollSensitivity(float sensitivity)
        {
            if (scrollRect != null)
            {
                scrollRect.scrollSensitivity = sensitivity;
                scrollSensitivity = sensitivity;
                LogDebug($"[ReusableScrollView] 스크롤 감도 설정: {sensitivity}");
            }
        }

        /// <summary>
        /// 레이아웃 간격 설정
        /// </summary>
        public void SetSpacing(float newSpacing)
        {
            if (layoutGroup != null)
            {
                layoutGroup.spacing = newSpacing;
                spacing = newSpacing;
                LogDebug($"[ReusableScrollView] 간격 설정: {newSpacing}");
            }
        }

        /// <summary>
        /// 패딩 설정
        /// </summary>
        public void SetPadding(RectOffset newPadding)
        {
            if (layoutGroup != null)
            {
                layoutGroup.padding = newPadding;
                padding = newPadding;
                LogDebug($"[ReusableScrollView] 패딩 설정: {newPadding}");
            }
        }

        /// <summary>
        /// 스크롤바 표시 설정
        /// </summary>
        public void SetScrollbarVisibility(bool showVertical, bool showHorizontal)
        {
            showVerticalScrollbar = showVertical;
            showHorizontalScrollbar = showHorizontal;
            
            if (verticalScrollbar != null)
            {
                verticalScrollbar.gameObject.SetActive(showVertical && enableVerticalScroll);
            }
            if (horizontalScrollbar != null)
            {
                horizontalScrollbar.gameObject.SetActive(showHorizontal && enableHorizontalScroll);
            }
            
            LogDebug($"[ReusableScrollView] 스크롤바 표시 - 수직: {showVertical}, 수평: {showHorizontal}");
        }

        /// <summary>
        /// 스크롤 뷰 정보 반환
        /// </summary>
        public string GetScrollViewInfo()
        {
            var info = new System.Text.StringBuilder();
            info.AppendLine($"[ReusableScrollView 정보]");
            info.AppendLine($"초기화됨: {isInitialized}");
            info.AppendLine($"아이템 개수: {contentItems.Count}");
            info.AppendLine($"수직 스크롤: {(enableVerticalScroll ? "활성화" : "비활성화")}");
            info.AppendLine($"수평 스크롤: {(enableHorizontalScroll ? "활성화" : "비활성화")}");
            info.AppendLine($"스크롤 감도: {scrollSensitivity}");
            info.AppendLine($"간격: {spacing}");
            info.AppendLine($"스크롤 중: {IsScrolling}");

            return info.ToString();
        }

        protected override void OnDestroy()
        {
            if (scrollRect != null)
            {
                scrollRect.onValueChanged.RemoveListener(OnScrollValueChangedHandler);
            }

            // 이벤트 구독 해제
            OnScrollValueChangedEvent = null;
            OnContentItemAddedEvent = null;
            OnContentItemRemovedEvent = null;

            base.OnDestroy();
        }
    }
}
