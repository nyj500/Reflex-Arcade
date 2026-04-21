using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/* ==================================================================================
 * [Class]: StagePopupManager (Singleton)
 * [Role] : 해금 조건 팝업(UI)을 총괄하는 관리자
 *
 * [변경 이유]
 * 기존: Show()가 List<GameType>을 받아 Config에서 점수를 다시 조회
 *       → 같은 게임이라도 대상에 따라 점수가 다른 새 구조와 맞지 않음
 * 변경: Show()가 List<UnlockCondition>을 직접 받음
 *       → 이미 어떤 게임에서 몇 점이 필요한지 담겨있어 Config 재조회 불필요
 *       → Config가 없어도 팝업 표시 가능 (의존성 감소)
 *
 * [Feature]
 * 1. Dynamic UI : 조건 개수만큼 Row 프리팹을 동적으로 생성
 * 2. Data Injection : 조건별 아이콘/점수/달성여부를 각 Row에 주입
 * 3. Auto Layout : Content Size Fitter를 통해 팝업 크기 자동 조절
 * ================================================================================== */
public class StagePopupManager : Singleton<StagePopupManager>
{
    [SerializeField] private GameObject popupObject;          // 켜고 끌 팝업 오브젝트
    [SerializeField] private RectTransform popupStartPos;    // 팝업 위치 기준점
    [SerializeField] private Vector3 offset = new Vector3(0, 50, 0); // 잠금 아이콘보다 살짝 위에 표시

    [SerializeField] private Transform contentContainer;     // Row들이 쌓일 컨테이너
    [SerializeField] private ConditionRow rowPrefab;         // 조건 한 줄 프리팹

    // 게임 타입과 아이콘 스프라이트를 연결하는 데이터베이스
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
        // 팝업이 열려있는 동안 클릭하면 닫기
        if (popupObject.activeSelf && Input.GetMouseButtonDown(0))
            Hide();
    }

    // 팝업 표시
    // targetPos: 잠금 아이콘의 월드 좌표
    // conditions: StageLockView가 config에서 가져온 조건 목록
    public void Show(Vector3 targetPos, List<StageUnLockConfig.UnlockCondition> conditions)
    {
        // 이전 Row들 제거
        foreach (Transform child in contentContainer)
            Destroy(child.gameObject);

        // 조건 목록 순서대로 Row 생성
        foreach (var condition in conditions)
        {
            ConditionRow newRow = Instantiate(rowPrefab, contentContainer);

            Sprite icon = GetIcon(condition.sourceGame);

            // 내 점수와 요구 점수 비교하여 달성 여부 판단
            int myBestScore = DataManager.Instance != null
                ? DataManager.Instance.GetBestScore(condition.sourceGame)
                : 0;
            bool isCleared = myBestScore >= condition.requiredScore;

            newRow.SetData(icon, condition.requiredScore, condition.sourceGame.ToString(), isCleared);
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
            if (data.gameType == gameType) return data.icon;
        return null;
    }
}
