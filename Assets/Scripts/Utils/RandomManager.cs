using UnityEngine;

public static class RandomManager
{
    private static System.Random rng = new System.Random();

    private static readonly float[] triangularOptions = { 7f, 7.5f, 8f, 8.5f, 9f, 9.5f, 10f, 10.5f };

    // 벌레 수명 관련 설정
    private const int MIN_LIFESPAN_DAYS = 14;  // 최소 14일
    private const int MAX_LIFESPAN_DAYS = 21;  // 최대 21일
    private const int DEFAULT_LIFESPAN_DAYS = 14; // 기본 14일

    public static int GetRandomIndex(int maxExclusive)
    {
        return rng.Next(0, maxExclusive);
    }

    public static float GetRandomTriangularStep(float min, float max, float step)
    {
        int steps = Mathf.RoundToInt((max - min) / step) + 1;
        float[] values = new float[steps];
        for (int i = 0; i < steps; i++)
            values[i] = min + (i * step);

        float a = values[rng.Next(values.Length)];
        float b = values[rng.Next(values.Length)];
        return a + b;
    }

    public static string GetRandomElement(string[] array)
    {
        return array[rng.Next(array.Length)];
    }

    /// <summary>
    /// 벌레의 랜덤 수명 생성 (분 단위)
    /// 7~10.5일 사이의 이산적 값들의 조합으로 14~21일 수명 생성
    /// </summary>
    public static int GenerateWormLifespan()
    {
        // triangularOptions 배열에서 두 값을 랜덤 선택하여 합산
        float lifespanDays = GetRandomTriangularStep(7f, 10.5f, 0.5f);
        return Mathf.RoundToInt(lifespanDays * 24 * 60); // 일 → 분 변환
    }

    /// <summary>
    /// 벌레의 랜덤 수명 생성 (일 단위 지정)
    /// </summary>
    public static int GenerateWormLifespan(int minDays, int maxDays)
    {
        int lifespanDays = rng.Next(minDays, maxDays + 1);
        return lifespanDays * 24 * 60; // 일 → 분 변환
    }

    /// <summary>
    /// 기본 수명 반환 (분 단위)
    /// </summary>
    public static int GetDefaultLifespan()
    {
        return DEFAULT_LIFESPAN_DAYS * 24 * 60; // 14일 → 분 변환
    }

    public static void SetSeed(int seed)
    {
        rng = new System.Random(seed);
    }
}
