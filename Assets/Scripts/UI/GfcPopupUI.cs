using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using GGumtles.Data;
using GGumtles.Managers;
using GGumtles.Utils;

namespace GGumtles.UI
{
    /// <summary>
    /// GFC(Genealogy Family Chart) 팝업 UI 관리
    /// WormData 리스트를 받아서 가계도 노드들을 생성하고 표시
    /// </summary>
    public class GfcPopupUI : MonoBehaviour
    {
        [Header("UI 설정")]
        [SerializeField] private Transform contentParent;              // VerticalLayoutGroup을 가진 Content Transform
        [SerializeField] private GameObject gfcNodePrefab;             // GFC 노드 프리팹
        
        [Header("디버그 설정")]
        [SerializeField] private bool enableDebugLogs = true;
        
        // 데이터 관리
        private List<WormData> familyHistory = new List<WormData>();
        private Dictionary<int, GameObject> generationNodes = new Dictionary<int, GameObject>(); // 세대별 노드 오브젝트
        
        // 상태 관리
        private bool isInitialized = false;
        
        private void Awake()
        {
            InitializeComponents();
        }
        
        /// <summary>
        /// 컴포넌트 초기화
        /// </summary>
        private void InitializeComponents()
        {
            try
            {
                // Content Transform 자동 찾기
                if (contentParent == null)
                {
                    contentParent = FindContentTransform();
                    if (contentParent == null)
                    {
                        Debug.LogError("[GfcPopupUI] Content Transform을 찾을 수 없습니다.");
                        return;
                    }
                }
                
                isInitialized = true;
                LogDebug("[GfcPopupUI] 컴포넌트 초기화 완료");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[GfcPopupUI] 컴포넌트 초기화 중 오류: {ex.Message}");
            }
        }
        
        /// <summary>
        /// GFC 팝업 초기화 (WormData 리스트로 가계도 표시)
        /// </summary>
        /// <param name="wormDataList">가계도에 표시할 WormData 리스트</param>
        public void Initialize(List<WormData> wormDataList)
        {
            if (!isInitialized)
            {
                Debug.LogWarning("[GfcPopupUI] 아직 초기화되지 않았습니다.");
                return;
            }
            
            try
            {
                LogDebug($"[GfcPopupUI] Initialize 호출됨 - 벌레 수: {wormDataList?.Count ?? 0}");
                
                // 기존 노드들 제거
                ClearWormNodes();
                
                // 새로운 데이터 설정
                familyHistory = wormDataList ?? new List<WormData>();
                
                // 가계도 노드들 생성
                CreateAllWormNodes();
                
                LogDebug($"[GfcPopupUI] 가계도 초기화 완료 - {familyHistory.Count}개 벌레");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[GfcPopupUI] 초기화 중 오류: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 가계도 업데이트
        /// </summary>
        /// <param name="newWormDataList">새로운 WormData 리스트</param>
        public void UpdateFamilyTree(List<WormData> newWormDataList)
        {
            if (!isInitialized) return;
            
            try
            {
                LogDebug($"[GfcPopupUI] 가계도 업데이트 - 새 벌레 수: {newWormDataList?.Count ?? 0}");
                
                // 기존 노드들 제거
                ClearWormNodes();
                
                // 새로운 데이터로 업데이트
                familyHistory = newWormDataList ?? new List<WormData>();
                
                // 가계도 노드들 재생성
                CreateAllWormNodes();
                
                LogDebug($"[GfcPopupUI] 가계도 업데이트 완료");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[GfcPopupUI] 가계도 업데이트 중 오류: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 모든 벌레 노드 생성
        /// </summary>
        private void CreateAllWormNodes()
        {
            if (contentParent == null)
            {
                Debug.LogError("[GfcPopupUI] contentParent가 null입니다!");
                return;
            }

            if (familyHistory == null || familyHistory.Count == 0)
            {
                LogDebug("[GfcPopupUI] familyHistory가 비어있습니다!");
                return;
            }

            if (gfcNodePrefab == null)
            {
                Debug.LogError("[GfcPopupUI] gfcNodePrefab이 null입니다!");
                return;
            }

            try
            {
                // 모든 벌레에 대해 노드 생성
                foreach (var worm in familyHistory)
                {
                    LogDebug($"[GfcPopupUI] 벌레 노드 생성 시도: {worm.name} (세대: {worm.generation})");
                    CreateWormNode(worm);
                }

                LogDebug($"[GfcPopupUI] {familyHistory.Count}개의 벌레 노드 생성 완료");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[GfcPopupUI] 벌레 노드 생성 중 오류: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 벌레 노드 생성 및 UI 연결
        /// </summary>
        private void CreateWormNode(WormData wormData)
        {
            try
            {
                LogDebug($"[GfcPopupUI] GFC 노드 프리팹 인스턴스 생성 시도...");
                
                // GFC 노드 프리팹 인스턴스 생성
                GameObject node = Instantiate(gfcNodePrefab, contentParent);
                if (node == null)
                {
                    Debug.LogError("[GfcPopupUI] GFC 노드 프리팹 인스턴스 생성 실패");
                    return;
                }

                LogDebug($"[GfcPopupUI] GFC 노드 프리팹 인스턴스 생성 성공: {node.name}");
                
                node.name = $"GFCNode_Gen{wormData.generation}";

                // UI 요소들 연결
                LogDebug($"[GfcPopupUI] UI 요소들 연결 시도...");
                ConnectNodeUIElements(node, wormData);

                // 세대별 노드 저장
                generationNodes[wormData.generation] = node;

                // 위치 설정 (세대 순으로)
                node.transform.SetSiblingIndex(wormData.generation);

                LogDebug($"[GfcPopupUI] 벌레 노드 생성 완료: {wormData.name} (세대: {wormData.generation})");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[GfcPopupUI] 벌레 노드 생성 중 오류: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 노드 UI 요소들 연결 (WormUI 활용)
        /// </summary>
        private void ConnectNodeUIElements(GameObject node, WormData wormData)
        {
            try
            {
                // WormUI 컴포넌트가 있는지 확인
                WormUI wormUI = node.GetComponent<WormUI>();
                if (wormUI != null)
                {
                    // WormUI를 사용하여 UI 설정
                    wormUI.SetWormData(wormData);
                    LogDebug($"[GfcPopupUI] WormUI를 사용하여 UI 설정 완료: {wormData.name}");
                    return;
                }
                
                // WormUI가 없는 경우 기존 방식 사용
                LogDebug($"[GfcPopupUI] WormUI 컴포넌트가 없어서 수동 설정을 사용합니다.");
                
                // 모든 TextMeshProUGUI 컴포넌트 찾기
                TextMeshProUGUI[] textComponents = node.GetComponentsInChildren<TextMeshProUGUI>();
                
                // 이름 텍스트 연결 (첫 번째 텍스트 컴포넌트)
                if (textComponents.Length > 0)
                {
                    textComponents[0].text = wormData.name;
                    LogDebug($"[GfcPopupUI] 이름 텍스트 설정: {wormData.name}");
                }
                else
                {
                    Debug.LogWarning("[GfcPopupUI] 이름 텍스트 컴포넌트를 찾을 수 없습니다.");
                }

                // 나이 텍스트 연결 (두 번째 텍스트 컴포넌트)
                if (textComponents.Length > 1)
                {
                    textComponents[1].text = $"나이: {wormData.age}";
                    LogDebug($"[GfcPopupUI] 나이 텍스트 설정: {wormData.age}");
                }
                else
                {
                    Debug.LogWarning("[GfcPopupUI] 나이 텍스트 컴포넌트를 찾을 수 없습니다.");
                }

                // 세대 텍스트 연결 (세 번째 텍스트 컴포넌트)
                if (textComponents.Length > 2)
                {
                    textComponents[2].text = $"세대: {wormData.generation}";
                    LogDebug($"[GfcPopupUI] 세대 텍스트 설정: {wormData.generation}");
                }
                else
                {
                    Debug.LogWarning("[GfcPopupUI] 세대 텍스트 컴포넌트를 찾을 수 없습니다.");
                }

                // 벌레 이미지 연결
                Image wormImage = node.GetComponentInChildren<Image>();
                if (wormImage != null)
                {
                    // SpriteManager를 통해 벌레 스프라이트 가져오기
                    if (SpriteManager.Instance != null)
                    {
                        var completedWormSprite = SpriteManager.Instance.CreateCompletedWormSprite(wormData);
                        if (completedWormSprite != null)
                        {
                            wormImage.sprite = completedWormSprite.sprite;
                            // Life Stage Scale 적용
                            wormImage.rectTransform.localScale = Vector3.one * completedWormSprite.scale;
                            LogDebug($"[GfcPopupUI] 벌레 이미지 설정 완료: {wormData.name}, Scale: {completedWormSprite.scale}");
                        }
                        else
                        {
                            Debug.LogWarning($"[GfcPopupUI] 벌레 스프라이트를 찾을 수 없습니다: {wormData.name}");
                        }
                    }
                    else
                    {
                        Debug.LogWarning("[GfcPopupUI] SpriteManager.Instance가 null입니다.");
                    }
                }
                else
                {
                    Debug.LogWarning("[GfcPopupUI] 벌레 이미지 컴포넌트를 찾을 수 없습니다.");
                }

                LogDebug($"[GfcPopupUI] 수동 UI 요소 연결 완료: {wormData.name}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[GfcPopupUI] UI 요소 연결 중 오류: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 모든 벌레 노드 제거
        /// </summary>
        private void ClearWormNodes()
        {
            try
            {
                foreach (var kvp in generationNodes)
                {
                    if (kvp.Value != null)
                    {
                        Destroy(kvp.Value);
                    }
                }
                generationNodes.Clear();
                
                LogDebug("[GfcPopupUI] 모든 벌레 노드 제거 완료");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[GfcPopupUI] 벌레 노드 제거 중 오류: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Content Transform 찾기
        /// </summary>
        private Transform FindContentTransform()
        {
            // VerticalLayoutGroup을 가진 Transform 찾기
            VerticalLayoutGroup layoutGroup = GetComponentInChildren<VerticalLayoutGroup>();
            if (layoutGroup != null)
            {
                return layoutGroup.transform;
            }
            
            // Content라는 이름의 Transform 찾기
            Transform content = transform.Find("Content");
            if (content != null)
            {
                return content;
            }
            
            // ScrollView 내부의 Content 찾기
            ScrollRect scrollRect = GetComponentInChildren<ScrollRect>();
            if (scrollRect != null && scrollRect.content != null)
            {
                return scrollRect.content;
            }
            
            Debug.LogError("[GfcPopupUI] Content Transform을 찾을 수 없습니다.");
            return null;
        }
        
        /// <summary>
        /// 팝업 닫기
        /// </summary>
        public void ClosePopup()
        {
            try
            {
                // 노드들 정리
                ClearWormNodes();
                
                // 팝업 닫기
                if (PopupManager.Instance != null)
                {
                    PopupManager.Instance.CloseCustomPopup(PopupManager.PopupType.GFC);
                }
                else
                {
                    Destroy(gameObject);
                }
                
                LogDebug("[GfcPopupUI] 팝업 닫기 완료");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[GfcPopupUI] 팝업 닫기 중 오류: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 디버그 로그 출력
        /// </summary>
        private void LogDebug(string message)
        {
            if (enableDebugLogs)
            {
                Debug.Log(message);
            }
        }
        
        private void OnDestroy()
        {
            LogDebug("[GfcPopupUI] GfcPopupUI 파괴됨");
        }
    }
}
