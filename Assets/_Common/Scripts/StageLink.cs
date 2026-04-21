using UnityEngine;

/* ======================================================================
 * [Class]: StageLink
 * [Role] : 로비에서 스테이지 간 연결 화살표의 잠금/해제 시각 효과를 담당
 *
 * [두 가지 동작 모드]
 *
 * 모드 1. checkSpecificSource = false (기본)
 *   → targetGame의 모든 조건 달성 여부로 화살표 색 결정
 *   → 사용 예: GravitySplit → MirrorDodge 화살표
 *              (GravitySplit의 모든 해금 조건이 충족됐을 때 파란색)
 *
 * 모드 2. checkSpecificSource = true
 *   → targetGame 조건 중 sourceGame 항목 하나만 체크
 *   → 사용 예: InsideOut → GravitySplit 화살표
 *              (InsideOut 점수 조건만 만족해도 파란색, ColorTwin은 무관)
 * ====================================================================== */
public class StageLink : MonoBehaviour
{
    // 이 화살표가 연결하는 게임 (열릴 게임, 항상 필수 설정)
    [SerializeField] private GameType targetGameType;

    // true이면 조건 계산 없이 항상 회색(잠김) 화살표 표시
    // Coming Soon처럼 아직 GameType이 없는 게임으로 향하는 화살표에 사용
    [SerializeField] private bool forceLocked;

    // true이면 sourceGameType 조건 하나만 체크
    // false이면 targetGame의 전체 조건 달성 여부 체크
    [SerializeField] private bool checkSpecificSource;

    // checkSpecificSource가 true일 때만 사용 (어떤 게임의 조건을 체크할지)
    [SerializeField] private GameType sourceGameType;

    [SerializeField] private StageUnLockConfig config;

    [SerializeField] private GameObject unclearArrow; // 회색 화살표 (조건 미달)
    [SerializeField] private GameObject clearArrow;   // 파란 화살표 (조건 달성)

    void Start()
    {
        UpdateArrowState();
    }

    public void UpdateArrowState()
    {
        if (config == null) return;

        // forceLocked가 체크되어 있으면 항상 잠금 (Coming Soon 등에 사용)
        if (forceLocked)
        {
            if (unclearArrow != null) unclearArrow.SetActive(true);
            if (clearArrow != null) clearArrow.SetActive(false);
            return;
        }

        bool isConditionMet = checkSpecificSource
            ? config.IsConditionMet(targetGameType, sourceGameType) // 특정 조건 하나만 체크
            : config.IsUnlocked(targetGameType);                    // 전체 조건 모두 체크

        if (unclearArrow != null) unclearArrow.SetActive(!isConditionMet);
        if (clearArrow != null) clearArrow.SetActive(isConditionMet);
    }
}
