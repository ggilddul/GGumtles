using UnityEngine;

public class AchievementTabManager : MonoBehaviour
{
    [SerializeField] private Transform contentRoot; // ScrollView Content
    [SerializeField] private GameObject achievementPrefab;
    [SerializeField] private Sprite[] achievementIcons; // 업적 아이콘 배열

    private void OnEnable()
    {
        ShowAchievements();
    }

    private void ShowAchievements()
    {
        // 기존 자식들 모두 삭제 (필요 시, 풀링 고려)
        foreach (Transform child in contentRoot)
        {
            Destroy(child.gameObject);
        }

        var definitions = AchievementManager.Instance.GetAllDefinitions();
        var statuses = AchievementManager.Instance.GetAllStatuses();

        int count = Mathf.Min(definitions.Count, statuses.Count);

        for (int i = 0; i < count; i++)
        {
            var definition = definitions[i];
            var status = statuses[i];

            GameObject item = Instantiate(achievementPrefab, contentRoot);
            if (item.TryGetComponent<AchievementButtonUI>(out var ui))
            {
                Sprite icon = GetIconFor(i);
                ui.Set(definition, icon, i, status);
            }
            else
            {
                Debug.LogWarning($"AchievementPrefab에 AchievementButtonUI 컴포넌트가 없습니다.");
            }
        }
    }

    private Sprite GetIconFor(int index)
    {
        return (achievementIcons != null && index >= 0 && index < achievementIcons.Length)
            ? achievementIcons[index]
            : (achievementIcons != null && achievementIcons.Length > 0 ? achievementIcons[0] : null);
    }
}
