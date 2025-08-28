using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class OverViewRenderer : MonoBehaviour
{
    [Header("렌더러 컴포넌트")]
    [SerializeField] private SpriteRenderer bodyRenderer;
    [SerializeField] private SpriteRenderer faceRenderer;
    [SerializeField] private SpriteRenderer hatRenderer;
    [SerializeField] private SpriteRenderer costumeRenderer;
    
    [Header("렌더링 설정")]
    [SerializeField] private Camera renderCamera;          // 전용 렌더 카메라
    [SerializeField] private RenderTexture renderTexture;  // 카메라에 연결된 렌더 텍스처
    [SerializeField] private int renderWidth = 256;        // 렌더 텍스처 너비
    [SerializeField] private int renderHeight = 256;       // 렌더 텍스처 높비
    [SerializeField] private bool enableCaching = true;    // 스프라이트 캐싱 활성화

    [Header("디버그")]
    [SerializeField] private bool enableDebugLogs = false;

    // 캐싱 시스템
    private Dictionary<string, Sprite> spriteCache;
    private string currentRenderHash;
    private Sprite lastRenderedSprite;

    // 이벤트 정의
    public delegate void OnPartChanged(string partName, Sprite newSprite);
    public event OnPartChanged PartChanged;

    public delegate void OnOverviewRendered(Sprite overviewSprite);
    public event OnOverviewRendered OnOverviewRenderedEvent;

    private void Awake()
    {
        InitializeRenderer();
    }

    private void InitializeRenderer()
    {
        try
        {
            // 캐시 초기화
            spriteCache = new Dictionary<string, Sprite>();

            // 렌더 텍스처 초기화
            InitializeRenderTexture();

            // 렌더 카메라 설정
            SetupRenderCamera();

            LogDebug("[OverViewRenderer] 렌더러 초기화 완료");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[OverViewRenderer] 초기화 중 오류: {ex.Message}");
        }
    }

    private void InitializeRenderTexture()
    {
        try
        {
            if (renderTexture == null)
            {
                renderTexture = new RenderTexture(renderWidth, renderHeight, 24);
                renderTexture.Create();
            }

            if (renderCamera != null)
            {
                renderCamera.targetTexture = renderTexture;
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[OverViewRenderer] 렌더 텍스처 초기화 중 오류: {ex.Message}");
        }
    }

    private void SetupRenderCamera()
    {
        try
        {
            if (renderCamera == null)
            {
                // 자식 오브젝트에서 카메라 찾기
                renderCamera = GetComponentInChildren<Camera>();
            }

            if (renderCamera == null)
            {
                Debug.LogWarning("[OverViewRenderer] 렌더 카메라를 찾을 수 없습니다.");
                return;
            }

            // 카메라 설정
            renderCamera.clearFlags = CameraClearFlags.SolidColor;
            renderCamera.backgroundColor = Color.clear;
            renderCamera.cullingMask = LayerMask.GetMask("OverviewRender");
            renderCamera.orthographic = true;
            renderCamera.orthographicSize = 1f;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[OverViewRenderer] 렌더 카메라 설정 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 오버뷰 스프라이트 렌더링
    /// </summary>
    public Sprite RenderOverviewSprite()
    {
        try
        {
            // 현재 상태의 해시 생성
            string currentHash = GenerateRenderHash();

            // 캐시된 스프라이트가 있는지 확인
            if (enableCaching && spriteCache.TryGetValue(currentHash, out Sprite cachedSprite))
            {
                LogDebug("[OverViewRenderer] 캐시된 스프라이트 사용");
                return cachedSprite;
            }

            // 새로운 스프라이트 렌더링
            Sprite newSprite = RenderNewSprite();

            // 캐시에 저장
            if (enableCaching && newSprite != null)
            {
                spriteCache[currentHash] = newSprite;
                currentRenderHash = currentHash;
                lastRenderedSprite = newSprite;

                // 캐시 크기 제한 (메모리 관리)
                if (spriteCache.Count > 10)
                {
                    CleanupOldCache();
                }
            }

            // 이벤트 발생
            OnOverviewRenderedEvent?.Invoke(newSprite);

            LogDebug("[OverViewRenderer] 새로운 오버뷰 스프라이트 렌더링 완료");
            return newSprite;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[OverViewRenderer] 오버뷰 스프라이트 렌더링 중 오류: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// 렌더 해시 생성
    /// </summary>
    private string GenerateRenderHash()
    {
        try
        {
            var hash = new System.Text.StringBuilder();
            
            // 각 파츠의 스프라이트 정보를 해시에 포함
            hash.Append($"B:{GetSpriteHash(bodyRenderer?.sprite)}");
            hash.Append($"F:{GetSpriteHash(faceRenderer?.sprite)}");
            hash.Append($"H:{GetSpriteHash(hatRenderer?.sprite)}");
            hash.Append($"C:{GetSpriteHash(costumeRenderer?.sprite)}");

            return hash.ToString();
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[OverViewRenderer] 렌더 해시 생성 중 오류: {ex.Message}");
            return "error";
        }
    }

    /// <summary>
    /// 스프라이트 해시 가져오기
    /// </summary>
    private string GetSpriteHash(Sprite sprite)
    {
        if (sprite == null) return "null";
        return sprite.name ?? "unnamed";
    }

    /// <summary>
    /// 새로운 스프라이트 렌더링
    /// </summary>
    private Sprite RenderNewSprite()
    {
        try
        {
            if (renderCamera == null || renderTexture == null)
            {
                Debug.LogWarning("[OverViewRenderer] 렌더 카메라 또는 텍스처가 설정되지 않았습니다.");
                return null;
            }

            // 1. 현재 RenderTexture 백업
            RenderTexture currentRT = RenderTexture.active;
            RenderTexture.active = renderTexture;

            // 2. 카메라 렌더링
            renderCamera.Render();

            // 3. 텍스처로 읽어오기
            Texture2D tex = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.ARGB32, false);
            tex.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            tex.Apply();

            // 4. RenderTexture 복원
            RenderTexture.active = currentRT;

            // 5. 텍스처를 Sprite로 변환
            Rect rect = new Rect(0, 0, tex.width, tex.height);
            Vector2 pivot = new Vector2(0.5f, 0.5f);
            Sprite sprite = Sprite.Create(tex, rect, pivot);

            return sprite;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[OverViewRenderer] 새 스프라이트 렌더링 중 오류: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// 오래된 캐시 정리
    /// </summary>
    private void CleanupOldCache()
    {
        try
        {
            if (spriteCache.Count <= 5) return; // 최소 5개는 유지

            var keysToRemove = new List<string>();
            int removeCount = spriteCache.Count - 5;

            foreach (var kvp in spriteCache)
            {
                if (keysToRemove.Count >= removeCount) break;
                
                // 현재 해시가 아닌 것들만 제거 대상으로 선택
                if (kvp.Key != currentRenderHash)
                {
                    keysToRemove.Add(kvp.Key);
                }
            }

            foreach (var key in keysToRemove)
            {
                if (spriteCache.TryGetValue(key, out Sprite sprite))
                {
                    if (sprite != null && sprite.texture != null)
                    {
                        DestroyImmediate(sprite.texture);
                    }
                    spriteCache.Remove(key);
                }
            }

            LogDebug($"[OverViewRenderer] 캐시 정리 완료: {removeCount}개 제거");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[OverViewRenderer] 캐시 정리 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 오버뷰 새로고침
    /// </summary>
    public void RefreshOverview()
    {
        try
        {
            // 현재 착용 아이템 ID 가져오기
            string hatId = GetEquippedItemId(ItemData.ItemType.Hat);
            string faceId = GetEquippedItemId(ItemData.ItemType.Face);
            string costumeId = GetEquippedItemId(ItemData.ItemType.Costume);

            // 아이템 데이터에서 해당 스프라이트 찾기
            var hatItem = ItemManager.Instance?.GetItemById(hatId);
            var faceItem = ItemManager.Instance?.GetItemById(faceId);
            var costumeItem = ItemManager.Instance?.GetItemById(costumeId);

            // 각 파츠 스프라이트 설정
            SetHatSprite(hatItem?.sprite);
            SetFaceSprite(faceItem?.sprite);
            SetCostumeSprite(costumeItem?.sprite);

            LogDebug("[OverViewRenderer] 오버뷰 새로고침 완료");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[OverViewRenderer] 오버뷰 새로고침 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 착용된 아이템 ID 가져오기
    /// </summary>
    private string GetEquippedItemId(ItemData.ItemType itemType)
    {
        try
        {
            if (ItemManager.Instance == null) return "";

            return ItemManager.Instance.GetEquippedItemId(itemType);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[OverViewRenderer] 착용 아이템 ID 가져오기 중 오류: {ex.Message}");
            return "";
        }
    }

    /// <summary>
    /// 몸체 스프라이트 설정
    /// </summary>
    public void SetBodySprite(Sprite sprite)
    {
        try
        {
            if (bodyRenderer != null)
            {
                bodyRenderer.sprite = sprite;
                NotifyPartChanged("Body", sprite);
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[OverViewRenderer] 몸체 스프라이트 설정 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 얼굴 스프라이트 설정
    /// </summary>
    public void SetFaceSprite(Sprite sprite)
    {
        try
        {
            if (faceRenderer != null)
            {
                faceRenderer.sprite = sprite;
                NotifyPartChanged("Face", sprite);
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[OverViewRenderer] 얼굴 스프라이트 설정 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 모자 스프라이트 설정
    /// </summary>
    public void SetHatSprite(Sprite sprite)
    {
        try
        {
            if (hatRenderer != null)
            {
                hatRenderer.sprite = sprite;
                NotifyPartChanged("Hat", sprite);
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[OverViewRenderer] 모자 스프라이트 설정 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 의상 스프라이트 설정
    /// </summary>
    public void SetCostumeSprite(Sprite sprite)
    {
        try
        {
            if (costumeRenderer != null)
            {
                costumeRenderer.sprite = sprite;
                NotifyPartChanged("Costume", sprite);
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[OverViewRenderer] 의상 스프라이트 설정 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 파츠 변경 알림
    /// </summary>
    private void NotifyPartChanged(string partName, Sprite newSprite)
    {
        try
        {
            PartChanged?.Invoke(partName, newSprite);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[OverViewRenderer] 파츠 변경 알림 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 캐시 활성화/비활성화
    /// </summary>
    public void SetCachingEnabled(bool enabled)
    {
        enableCaching = enabled;
        LogDebug($"[OverViewRenderer] 캐싱 {(enabled ? "활성화" : "비활성화")}");
    }

    /// <summary>
    /// 캐시 정리
    /// </summary>
    public void ClearCache()
    {
        try
        {
            foreach (var kvp in spriteCache)
            {
                if (kvp.Value != null && kvp.Value.texture != null)
                {
                    DestroyImmediate(kvp.Value.texture);
                }
            }

            spriteCache.Clear();
            currentRenderHash = "";
            lastRenderedSprite = null;

            LogDebug("[OverViewRenderer] 캐시 정리 완료");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[OverViewRenderer] 캐시 정리 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 렌더러 정보 반환
    /// </summary>
    public string GetRendererInfo()
    {
        var info = new System.Text.StringBuilder();
        info.AppendLine($"[OverViewRenderer 정보]");
        info.AppendLine($"렌더 카메라: {(renderCamera != null ? "설정됨" : "없음")}");
        info.AppendLine($"렌더 텍스처: {(renderTexture != null ? "설정됨" : "없음")}");
        info.AppendLine($"캐시 활성화: {enableCaching}");
        info.AppendLine($"캐시된 스프라이트 수: {spriteCache.Count}");
        info.AppendLine($"현재 해시: {currentRenderHash}");

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
        // 캐시 정리
        ClearCache();

        // 이벤트 초기화
        PartChanged = null;
        OnOverviewRenderedEvent = null;
    }
}
