using UnityEngine;
using System.Collections.Generic;
using GGumtles.UI;

namespace GGumtles.UI
{
    /// <summary>
    /// UI 프리팹 관리 및 재사용 시스템
    /// 오브젝트 풀링과 프리팹 인스턴스 관리를 담당
    /// </summary>
    public class UIPrefabManager : MonoBehaviour
    {
        public static UIPrefabManager Instance { get; private set; }

            [Header("UI 프리팹 설정")]
    [SerializeField] private GameObject buttonPrefab;
    [SerializeField] private GameObject panelPrefab;
    [SerializeField] private GameObject toastPrefab;
    [SerializeField] private GameObject loadingPrefab;
    [SerializeField] private GameObject progressBarPrefab;
    [SerializeField] private GameObject tooltipPrefab;
    [SerializeField] private GameObject scrollViewPrefab;
    [SerializeField] private GameObject layoutGroupPrefab;

        [Header("풀링 설정")]
        [SerializeField] private bool enableObjectPooling = true;
        [SerializeField] private int defaultPoolSize = 10;
        [SerializeField] private int maxPoolSize = 50;

        [Header("디버그")]
        [SerializeField] private bool enableDebugLogs = false;

        // 프리팹 캐시
        private Dictionary<string, GameObject> prefabCache = new Dictionary<string, GameObject>();
        
        // 오브젝트 풀
        private Dictionary<string, Queue<GameObject>> objectPools = new Dictionary<string, Queue<GameObject>>();
        private Dictionary<string, List<GameObject>> activeObjects = new Dictionary<string, List<GameObject>>();

        // 상태 관리
        private bool isInitialized = false;

        private void Awake()
        {
            InitializeSingleton();
        }

        private void Start()
        {
            InitializeUIPrefabManager();
        }

        private void InitializeSingleton()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void InitializeUIPrefabManager()
        {
            try
            {
                InitializePrefabCache();
                InitializeObjectPools();
                isInitialized = true;
                
                LogDebug("[UIPrefabManager] UI 프리팹 매니저 초기화 완료");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[UIPrefabManager] 초기화 중 오류: {ex.Message}");
            }
        }

        /// <summary>
        /// 프리팹 캐시 초기화
        /// </summary>
        private void InitializePrefabCache()
        {
            prefabCache.Clear();
            
            if (buttonPrefab != null)
                prefabCache["Button"] = buttonPrefab;
            if (panelPrefab != null)
                prefabCache["Panel"] = panelPrefab;
            if (toastPrefab != null)
                prefabCache["Toast"] = toastPrefab;
            if (loadingPrefab != null)
                prefabCache["Loading"] = loadingPrefab;
            if (progressBarPrefab != null)
                prefabCache["ProgressBar"] = progressBarPrefab;
                    if (tooltipPrefab != null)
            prefabCache["Tooltip"] = tooltipPrefab;
        if (scrollViewPrefab != null)
            prefabCache["ScrollView"] = scrollViewPrefab;
        if (layoutGroupPrefab != null)
            prefabCache["LayoutGroup"] = layoutGroupPrefab;
        }

        /// <summary>
        /// 오브젝트 풀 초기화
        /// </summary>
        private void InitializeObjectPools()
        {
            if (!enableObjectPooling) return;

            foreach (var kvp in prefabCache)
            {
                string prefabName = kvp.Key;
                GameObject prefab = kvp.Value;
                
                objectPools[prefabName] = new Queue<GameObject>();
                activeObjects[prefabName] = new List<GameObject>();

                // 기본 풀 크기만큼 미리 생성
                for (int i = 0; i < defaultPoolSize; i++)
                {
                    GameObject obj = Instantiate(prefab, transform);
                    obj.SetActive(false);
                    objectPools[prefabName].Enqueue(obj);
                }
            }
        }

        /// <summary>
        /// UI 요소 생성 (풀링 사용)
        /// </summary>
        public GameObject CreateUIElement(string prefabName, Transform parent = null)
        {
            if (!isInitialized)
            {
                Debug.LogWarning("[UIPrefabManager] 아직 초기화되지 않았습니다.");
                return null;
            }

            try
            {
                if (enableObjectPooling && objectPools.ContainsKey(prefabName))
                {
                    return GetFromPool(prefabName, parent);
                }
                else
                {
                    return CreateNewInstance(prefabName, parent);
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[UIPrefabManager] UI 요소 생성 중 오류: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 풀에서 오브젝트 가져오기
        /// </summary>
        private GameObject GetFromPool(string prefabName, Transform parent)
        {
            Queue<GameObject> pool = objectPools[prefabName];
            GameObject obj;

            if (pool.Count > 0)
            {
                obj = pool.Dequeue();
            }
            else
            {
                // 풀이 비어있으면 새로 생성
                obj = CreateNewInstance(prefabName, parent);
                if (obj != null)
                {
                    activeObjects[prefabName].Add(obj);
                }
                return obj;
            }

            // 부모 설정
            if (parent != null)
            {
                obj.transform.SetParent(parent, false);
            }
            else
            {
                obj.transform.SetParent(transform, false);
            }

            obj.SetActive(true);
            activeObjects[prefabName].Add(obj);

            LogDebug($"[UIPrefabManager] 풀에서 {prefabName} 가져옴 (남은 풀: {pool.Count})");
            return obj;
        }

        /// <summary>
        /// 새 인스턴스 생성
        /// </summary>
        private GameObject CreateNewInstance(string prefabName, Transform parent)
        {
            if (!prefabCache.ContainsKey(prefabName))
            {
                Debug.LogError($"[UIPrefabManager] {prefabName} 프리팹이 등록되지 않았습니다.");
                return null;
            }

            GameObject prefab = prefabCache[prefabName];
            GameObject obj = Instantiate(prefab, parent);
            
            if (enableObjectPooling && activeObjects.ContainsKey(prefabName))
            {
                activeObjects[prefabName].Add(obj);
            }

            LogDebug($"[UIPrefabManager] 새 {prefabName} 인스턴스 생성");
            return obj;
        }

        /// <summary>
        /// UI 요소 반환 (풀로 회수)
        /// </summary>
        public void ReturnUIElement(GameObject uiElement, string prefabName)
        {
            if (!isInitialized || !enableObjectPooling)
            {
                Destroy(uiElement);
                return;
            }

            try
            {
                if (objectPools.ContainsKey(prefabName))
                {
                    ReturnToPool(uiElement, prefabName);
                }
                else
                {
                    Destroy(uiElement);
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[UIPrefabManager] UI 요소 반환 중 오류: {ex.Message}");
                Destroy(uiElement);
            }
        }

        /// <summary>
        /// 풀로 오브젝트 반환
        /// </summary>
        private void ReturnToPool(GameObject obj, string prefabName)
        {
            if (obj == null) return;

            // 활성 오브젝트 목록에서 제거
            if (activeObjects.ContainsKey(prefabName))
            {
                activeObjects[prefabName].Remove(obj);
            }

            // 풀 크기 제한 확인
            Queue<GameObject> pool = objectPools[prefabName];
            if (pool.Count < maxPoolSize)
            {
                obj.SetActive(false);
                obj.transform.SetParent(transform);
                pool.Enqueue(obj);
                LogDebug($"[UIPrefabManager] {prefabName} 풀로 반환 (풀 크기: {pool.Count})");
            }
            else
            {
                Destroy(obj);
                LogDebug($"[UIPrefabManager] {prefabName} 풀 크기 초과로 제거");
            }
        }

        /// <summary>
        /// 모든 활성 UI 요소 반환
        /// </summary>
        public void ReturnAllUIElements()
        {
            if (!enableObjectPooling) return;

            foreach (var kvp in activeObjects)
            {
                string prefabName = kvp.Key;
                List<GameObject> activeList = new List<GameObject>(kvp.Value);

                foreach (GameObject obj in activeList)
                {
                    if (obj != null)
                    {
                        ReturnUIElement(obj, prefabName);
                    }
                }
            }

            LogDebug("[UIPrefabManager] 모든 UI 요소 반환 완료");
        }

        /// <summary>
        /// 특정 타입의 모든 UI 요소 반환
        /// </summary>
        public void ReturnAllUIElementsOfType(string prefabName)
        {
            if (!enableObjectPooling || !activeObjects.ContainsKey(prefabName)) return;

            List<GameObject> activeList = new List<GameObject>(activeObjects[prefabName]);

            foreach (GameObject obj in activeList)
            {
                if (obj != null)
                {
                    ReturnUIElement(obj, prefabName);
                }
            }

            LogDebug($"[UIPrefabManager] {prefabName} 타입 모든 UI 요소 반환 완료");
        }

        /// <summary>
        /// 프리팹 등록
        /// </summary>
        public void RegisterPrefab(string name, GameObject prefab)
        {
            if (prefab == null)
            {
                Debug.LogError($"[UIPrefabManager] {name} 프리팹이 null입니다.");
                return;
            }

            prefabCache[name] = prefab;
            
            if (enableObjectPooling && !objectPools.ContainsKey(name))
            {
                objectPools[name] = new Queue<GameObject>();
                activeObjects[name] = new List<GameObject>();
            }

            LogDebug($"[UIPrefabManager] {name} 프리팹 등록 완료");
        }

        /// <summary>
        /// 프리팹 해제
        /// </summary>
        public void UnregisterPrefab(string name)
        {
            if (prefabCache.ContainsKey(name))
            {
                prefabCache.Remove(name);
                
                if (enableObjectPooling)
                {
                    // 활성 오브젝트들 제거
                    if (activeObjects.ContainsKey(name))
                    {
                        foreach (GameObject obj in activeObjects[name])
                        {
                            if (obj != null)
                            {
                                Destroy(obj);
                            }
                        }
                        activeObjects.Remove(name);
                    }

                    // 풀 오브젝트들 제거
                    if (objectPools.ContainsKey(name))
                    {
                        while (objectPools[name].Count > 0)
                        {
                            GameObject obj = objectPools[name].Dequeue();
                            if (obj != null)
                            {
                                Destroy(obj);
                            }
                        }
                        objectPools.Remove(name);
                    }
                }

                LogDebug($"[UIPrefabManager] {name} 프리팹 해제 완료");
            }
        }

        /// <summary>
        /// 풀링 활성화/비활성화
        /// </summary>
        public void SetObjectPoolingEnabled(bool enabled)
        {
            enableObjectPooling = enabled;
            LogDebug($"[UIPrefabManager] 오브젝트 풀링 {(enabled ? "활성화" : "비활성화")}");
        }

        /// <summary>
        /// 풀 크기 설정
        /// </summary>
        public void SetPoolSize(int defaultSize, int maxSize)
        {
            defaultPoolSize = Mathf.Max(1, defaultSize);
            maxPoolSize = Mathf.Max(defaultPoolSize, maxSize);
            LogDebug($"[UIPrefabManager] 풀 크기 설정 - 기본: {defaultPoolSize}, 최대: {maxPoolSize}");
        }

        /// <summary>
        /// UI 프리팹 매니저 정보 반환
        /// </summary>
        public string GetUIPrefabManagerInfo()
        {
            var info = new System.Text.StringBuilder();
            info.AppendLine($"[UIPrefabManager 정보]");
            info.AppendLine($"초기화됨: {isInitialized}");
            info.AppendLine($"등록된 프리팹 수: {prefabCache.Count}");
            info.AppendLine($"오브젝트 풀링: {(enableObjectPooling ? "활성화" : "비활성화")}");
            info.AppendLine($"기본 풀 크기: {defaultPoolSize}");
            info.AppendLine($"최대 풀 크기: {maxPoolSize}");

            if (enableObjectPooling)
            {
                info.AppendLine("\n[풀 상태]");
                foreach (var kvp in objectPools)
                {
                    string prefabName = kvp.Key;
                    int poolSize = kvp.Value.Count;
                    int activeCount = activeObjects.ContainsKey(prefabName) ? activeObjects[prefabName].Count : 0;
                    info.AppendLine($"  {prefabName}: 풀 {poolSize}개, 활성 {activeCount}개");
                }
            }

            return info.ToString();
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
            // 모든 활성 오브젝트 정리
            ReturnAllUIElements();
        }
    }
}
