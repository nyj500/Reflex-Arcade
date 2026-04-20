using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/* ==================================================================================
 * [Class]: StagePopupManager (Singleton)
 * [Role] : 해금 조건 팝업(UI)을 총괄하는 관리자
 * * [Feature]
 * 1. Dynamic UI : 요청받은 게임 리스트 개수만큼 Row 프리팹을 동적으로 생성
 * 2. Data Injection : 아이콘 데이터베이스와 Config를 조회하여 각 Row에 데이터 주입
 * 3. Auto Layout : Content Size Fitter 등을 통해 팝업 크기 자동 조절
 * ================================================================================== */

public class StagePopupManager : Singleton<StagePopupManager>
{

    [SerializeField] private GameObject popupObject; // 켜고 끌 파란 상자 (Popup_UI)
    [SerializeField] RectTransform popupStartPos; // 위치를 옮길 파란 상자의 Transform
    [SerializeField] private Vector3 offset = new Vector3(0, 50, 0); // 잠김 아이콘보다 살짝 위에 뜨게

    [SerializeField] private Transform contentContainer; // 줄(Row)들이 쌓일 투명 상자 (Condition_Container)

    [SerializeField] private ConditionRow rowPrefab; // 줄(Row) 프리팹 (Condition_Row)

    [SerializeField] private StageUnLockConfig config; // 점수


    // 게임 이름(Enum)과 아이콘(Sprite)을 짝지어주는 데이터베이스
    [System.Serializable]
    public struct GameIconData
    {
        public GameType gameType;
        public Sprite icon;
    }
    [SerializeField] private List<GameIconData> iconDataBase;

    protected override void Awake()
    {
        base.Awake();
        Hide();
    }
    void Update()
    {
        if (popupObject.activeSelf && Input.GetMouseButtonDown(0))
        {
            Hide();
        }
    }
    public void Show(Vector3 targetPos, List<GameType> requiredGames)
    {
        foreach (Transform child in contentContainer)
        {
            Destroy(child.gameObject);
        }

        foreach (var gameType in requiredGames)
        {
            ConditionRow newRow = Instantiate(rowPrefab, contentContainer);

            Sprite icon = GetIcon(gameType);
            int requiredScore = config.GetRequiedScore(gameType);

            int myBestScore = (DataManager.Instance != null) ? DataManager.Instance.GetBestScore(gameType) : 0;
            bool isCleared = myBestScore >= requiredScore;

            newRow.SetData(icon, requiredScore, gameType.ToString(), isCleared);
        }

        popupObject.SetActive(true);
        popupStartPos.position = targetPos + offset;
    }


    public void Hide()
    {
        popupObject.SetActive(false);
    }

    private Sprite GetIcon(GameType gameType)
    {
        foreach (var data in iconDataBase)
        {
            if (data.gameType == gameType)
            {
                return data.icon;
            }
        }
        return null;
    }

}
