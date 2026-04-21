using System.Collections.Generic;
using UnityEngine;

/* ======================================================================
 * [Class]: StageUnLockConfig (ScriptableObject)
 * [Role] : 각 게임의 잠금 해제 조건을 관리하는 데이터 컨테이너
 *
 * [핵심 구조 변경 이유]
 * 기존: 게임마다 점수 하나 고정 → InsideOut은 항상 120점
 * 변경: 열릴 게임마다 조건 목록 → GravitySplit 해금엔 120점, MirrorDodge 해금엔 150점
 *
 * [데이터 구조]
 * UnlockCondition : sourceGame(어떤 게임에서) + requiredScore(몇 점)
 *                   → 조건 하나를 표현하는 단위
 * UnlockRule      : targetGame(열릴 게임) + conditions(조건 목록)
 *                   → 하나의 게임을 열기 위한 전체 조건 묶음
 *
 * [예시 설정]
 * GravitySplit 열기 조건 → InsideOut 120점, ColorTwin 80점
 * MirrorDodge  열기 조건 → InsideOut 150점, ColorTwin 100점, GravitySplit 80점
 * ====================================================================== */
[CreateAssetMenu(fileName = "StageUnLockConfig", menuName = "Scriptable Objects/Stage UnLock Config")]
public class StageUnLockConfig : ScriptableObject
{
    // ─── 데이터 구조 정의 ───────────────────────────────────────────────

    // 조건 하나: 어떤 게임에서 몇 점이 필요한가
    [System.Serializable]
    public struct UnlockCondition
    {
        public GameType sourceGame;   // 점수를 확인할 게임 (예: InsideOut)
        public int requiredScore;     // 필요한 점수 (예: 120)
    }

    // 규칙 하나: 어떤 게임을 열기 위해 어떤 조건들이 필요한가
    [System.Serializable]
    public struct UnlockRule
    {
        public GameType targetGame;             // 열릴 게임 (예: GravitySplit)
        public List<UnlockCondition> conditions; // 조건 목록 (여러 게임의 점수 조건)
    }

    // 인스펙터에서 직접 입력하는 규칙 목록
    [SerializeField] private List<UnlockRule> rules;

    // 빠른 검색을 위해 List → Dictionary로 변환 (키: targetGame)
    private Dictionary<GameType, List<UnlockCondition>> ruleDict;

    // ─── 초기화 ─────────────────────────────────────────────────────────

    private void OnEnable()
    {
        // 에셋이 로드될 때 리스트를 딕셔너리로 변환
        ruleDict = new Dictionary<GameType, List<UnlockCondition>>();
        foreach (var rule in rules)
            ruleDict[rule.targetGame] = rule.conditions;
    }

    // ─── 공개 메서드 ─────────────────────────────────────────────────────

    // targetGame을 열기 위한 조건 목록 반환
    // 등록된 규칙이 없으면 null 반환
    public List<UnlockCondition> GetConditions(GameType targetGame)
    {
        if (ruleDict == null) OnEnable();
        return ruleDict.TryGetValue(targetGame, out var conditions) ? conditions : null;
    }

    // targetGame을 열기 위한 모든 조건을 달성했는지 확인
    // 조건 목록이 없거나 비어있으면 항상 false (잠금 유지)
    public bool IsUnlocked(GameType targetGame)
    {
        var conditions = GetConditions(targetGame);

        // 조건이 없거나 비어있으면 잠금 유지
        if (conditions == null || conditions.Count == 0) return false;

        foreach (var condition in conditions)
        {
            int myScore = DataManager.Instance != null
                ? DataManager.Instance.GetBestScore(condition.sourceGame)
                : 0;

            // 하나라도 미달이면 즉시 false
            if (myScore < condition.requiredScore) return false;
        }

        return true; // 모든 조건 달성
    }

    // targetGame을 열기 위한 조건 중, sourceGame 항목 하나만 달성했는지 확인
    // StageLink의 개별 화살표가 이 메서드를 사용
    // (예: InsideOut → GravitySplit 화살표는 InsideOut 조건만 체크)
    public bool IsConditionMet(GameType targetGame, GameType sourceGame)
    {
        var conditions = GetConditions(targetGame);
        if (conditions == null) return false;

        foreach (var condition in conditions)
        {
            if (condition.sourceGame != sourceGame) continue;

            int myScore = DataManager.Instance != null
                ? DataManager.Instance.GetBestScore(sourceGame)
                : 0;

            return myScore >= condition.requiredScore;
        }

        // targetGame의 조건 목록에 sourceGame이 없으면 false
        return false;
    }
}
