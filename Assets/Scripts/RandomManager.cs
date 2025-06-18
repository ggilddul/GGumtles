using UnityEngine;

public static class RandomManager
{
    private static System.Random rng = new System.Random();

    private static readonly float[] triangularOptions = { 7f, 7.5f, 8f, 8.5f, 9f, 9.5f, 10f, 10.5f };

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

    public static void SetSeed(int seed)
    {
        rng = new System.Random(seed);
    }
}
