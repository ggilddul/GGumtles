using System.Collections.Generic;
using UnityEngine;

public class WormManager : MonoBehaviour
{
    public static WormManager Instance { get; private set; }

    private readonly string[] adjectives = new string[]
    {
        "반짝이는", "말랑말랑한", "재빠른", "작은", "따뜻한",
        "귀여운", "푸른", "부드러운", "신나는", "조용한",
        "빛나는", "무시무시한", "행복한", "장난꾸러기", "포근한",
        "대단한", "상큼한", "졸린", "용감한", "엄청난"
    };

    private readonly string[] nouns = new string[]
    {
        "꿈틀이", "꼬물이", "말랑이", "쫄랑이", "비비미",
        "꼬마", "토실이", "무지개", "나비", "별빛",
        "토끼", "햇살", "방울", "파랑새", "풍선",
        "별똥별", "구름", "사탕", "바람", "도토리"
    };

    private List<WormData> wormList = new();
    private WormData currentWorm;
    public WormFamilyManager wormFamilyManager;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    // 저장된 웜 리스트로 초기화 및 현재 웜 설정
    public void Initialize(List<WormData> savedWormList)
    {
        wormList = savedWormList ?? new List<WormData>();

        if (wormList.Count > 0)
        {
            currentWorm = wormList[wormList.Count - 1];
        }
        else
        {
            currentWorm = null;
        }
    }

    public string GenerateRandomWormName()
    {
        return $"{RandomManager.GetRandomElement(adjectives)} {RandomManager.GetRandomElement(nouns)}";
    }

    public int GenerateRandomLifespan()
    {
        float days = RandomManager.GetRandomTriangularStep(7f, 10.5f, 0.5f);
        return Mathf.RoundToInt(days * 1440);
    }

    public WormData CreateNewWorm(int generation)
    {
        WormData newWorm = new WormData
        {
            gen = generation,
            age = 0,
            lifeStage = 0,
            name = GenerateRandomWormName(),
            lifespan = GenerateRandomLifespan(),
            hatItemId = "100",
            faceItemId = "200",
            costumeItemId = "300"
        };

        wormList.Add(newWorm);

        currentWorm = newWorm;

        if (generation != 1)
        {
            wormFamilyManager.AddGeneration(newWorm);
        }

        PopupManager.Instance?.OpenEggPopup();

        return newWorm;
    }
    public void EvolveCurrentWorm()
    {
        Debug.Log($"[Evolve] age={currentWorm.age}, stage={currentWorm.lifeStage}, lifespan={currentWorm.lifespan}");
        if (currentWorm == null) return;

        // 사망 조건
        if (currentWorm.age >= currentWorm.lifespan)
        {
            currentWorm.lifeStage = 6; // 6 = 사망 상태로 가정
            OnWormDied(currentWorm);

            // 새 꿈틀이 생성 (세대 증가)
            int newGen = currentWorm.gen + 1;

            CreateNewWorm(newGen);
            return;
        }

        // 진화 조건
        int[] thresholds = { 30, 120, 300, 720, 1440 }; // 알, 1~4 유충기, 성체로 진화 기준 (분 단위 예시)
        int currentStage = currentWorm.lifeStage;

        if (currentStage < thresholds.Length && currentWorm.age >= thresholds[currentStage])
        {
            currentWorm.lifeStage++;
            OnWormEvolved(currentWorm);
        }
    }

    public WormData GetCurrentWorm()
    {
        return currentWorm;
    }

    public List<WormData> GetAllWorms()
    {
        return wormList;
    }

    private void OnWormDied(WormData worm)
    {
        PopupManager.Instance?.OpenDiePopup(worm);
        // 업적 체크, 애니메이션 등도 가능
    }

    private void OnWormEvolved(WormData worm)
    {
        PopupManager.Instance?.OpenEvolvePopup(worm);
        AudioManager.Instance?.PlayButtonSound(6);
    }
}
