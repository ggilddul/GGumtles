using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class WormData
{
    [Header("기본 정보")]
    public int wormId = -1;
    public string name = "";
    public int generation = 1;           // 세대

    [Header("생명 정보")]
    public int age = 0;                  // 현재 나이 (분 단위)
    public int lifespan = 0;             // 전체 수명 (분 단위, RandomManager에서 설정)
    public int lifeStage = 0;            // 생애 주기 (0=알, 1=제1유충기, 2=제2유충기, 3=제3유충기, 4=제4유충기, 5=성체, 6=사망)

    [Header("장착 아이템")]
    public string hatItemId = "";
    public string faceItemId = "";
    public string costumeItemId = "";

    public bool isAlive = true;

    [Header("통계 정보")]
    public WormStatistics statistics = new WormStatistics();

    // 통계 클래스
    [System.Serializable]
    public class WormStatistics
    {
        public int totalEatCount = 0;        // 총 먹이 횟수
        public int totalPlayCount = 0;       // 총 놀이 횟수
        public int achievementCount = 0;     // 총 업적 수
    }
}
