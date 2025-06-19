[System.Serializable]
public class WormData
{
    public int wormId;
    public int gen;             // 세대
    public int age;             // 현재 나이 (분 단위)
    public int lifespan;        // 전체 수명 (분 단위)
    public int lifeStage;       // 생애 주기 (0=알, 1=L1, ..., 5=성체, 6=사망 등)
    public string name;

    public string hatItemId;
    public string faceItemId;
    public string costumeItemId;
}
