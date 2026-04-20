using System;
using UnityEngine;


/* ======================================================================
/* 게임 전반의 영구 데이터(최고 점수, 튜토리얼 여부)를 PlayerPrefs로 관리하는 싱글톤 클래스
 * * [Method Summary]
 * 1. CheckAndSaveBestScore : 입력된 점수가 기존 최고 점수보다 높을 경우 갱신하여 저장
 * 2. GetBestScore          : 특정 게임 타입의 저장된 최고 점수를 로드하여 반환
 * 3. IsFirstSeen           : 해당 게임의 튜토리얼을 처음 진입하는지 여부 확인
 * 4. SetTutorialSeen       : 해당 게임의 튜토리얼 시청 완료 상태를 저장(도장 찍기)
/* ====================================================================== */

public class DataManager : PersistentSingleton<DataManager>
{
    // 점수 저장 및 불러오기
    public bool CheckAndSaveBestScore(GameType type, int score)
    {
        int currentScore = GetBestScore(type);
        if (score > currentScore)
        {
            PlayerPrefs.SetInt("BestScore_" + type.ToString(), score);
            PlayerPrefs.Save();
            return true;
        }
        return false;
    }
    public int GetBestScore(GameType type)
    {
        return PlayerPrefs.GetInt("BestScore_" + type.ToString(), 0);
    }


    // 튜토리얼을 봤는지 확인
    public bool IsFirstSeen(GameType type)
    {
        // 예) "Tutorial_GravitySplit" 키가 0이면 처음 본 거, 1이면 이미 본 거
        return PlayerPrefs.GetInt("Tutorial_" + type.ToString(), 0) == 0;
    }

    // 튜토리얼 봤다고 도장 찍기
    public void SetTutorialSeen(GameType type)
    {
        PlayerPrefs.SetInt("Tutorial_" + type.ToString(), 1);
        PlayerPrefs.Save();
    }

}