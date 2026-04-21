using System.Collections.Generic;
using UnityEngine;

/* ======================================================================
 * [Class]: ObstacleManager
 * [Role] : 위/아래 두 그룹 장애물의 초기 생성과 무한 재배치를 관리
 * * [Method Summary]
 * 1. SpawnGroup    : 게임 시작 시 그룹별 장애물을 초기 생성
 * 2. RecycleGroup  : (Update) 화면 밖으로 나간 장애물을 반대쪽 끝으로 재배치
 * 3. Reposition    : 그룹 내 가장 뒤에 있는 장애물 기준으로 새 위치 계산
 * 4. ApplyPlacement: 위치 + 회전 적용 (연속 같은 라인 제한 포함)
 * ====================================================================== */
namespace MirrorDodge
{
    public class ObstacleManager : MonoBehaviour
    {
        [Header("프리팹")]
        [SerializeField] private GameObject topObstaclePrefab;
        [SerializeField] private GameObject bottomObstaclePrefab;

        [Header("장애물 개수")]
        [SerializeField] private int obstacleCount = 5;

        // ---------------------------------------------------------------
        // Y 위치 후보 + 회전값 — 인덱스가 1:1 대응되어야 함
        //
        // 위쪽 존:
        //   [0] CenterLine 근처 → 정방향 △  (rotation  0°)
        //   [1] TopLine 근처    → 뒤집힌 ▽  (rotation 180°)
        //
        // 아래쪽 존:
        //   [0] CenterLine 근처  → 뒤집힌 ▽  (rotation 180°)
        //   [1] BottomLine 근처  → 정방향 △  (rotation  0°)
        // ---------------------------------------------------------------
        [Header("Y 위치 후보 (씬에 맞게 조정)")]
        [SerializeField] private float[] topYCandidates    = { 0.4f,  1.2f };
        [SerializeField] private float[] topRotations      = { 0f,   180f  };

        [SerializeField] private float[] bottomYCandidates = { -0.4f, -1.2f };
        [SerializeField] private float[] bottomRotations   = { 180f,    0f  };

        [Header("장애물 간격")]
        [SerializeField] private float minGap = 3f;
        [SerializeField] private float maxGap = 4f;

        // ---------------------------------------------------------------
        // 같은 라인 연속 제한
        // 예) maxConsecutiveSameLine = 2 → 같은 라인 최대 2번 연속까지 허용
        // ---------------------------------------------------------------
        [Header("같은 라인 연속 제한")]
        [SerializeField] private int maxConsecutiveSameLine = 2;

        // 각 그룹의 마지막 선택 인덱스와 연속 횟수 추적
        private int topLastIndex       = -1;
        private int topConsecutiveCount = 0;
        private int bottomLastIndex       = -1;
        private int bottomConsecutiveCount = 0;

        // 화면 경계 — GravitySplit과 동일한 기준값 사용
        private const float LeftBound  = -3f;
        private const float RightBound =  3f;

        private List<GameObject> topObstacles    = new List<GameObject>();
        private List<GameObject> bottomObstacles = new List<GameObject>();

        private void Start()
        {
            SpawnGroup(topObstaclePrefab, topObstacles, topYCandidates, topRotations,
                       startX: 3f, spaceRight: true,
                       ref topLastIndex, ref topConsecutiveCount);

            SpawnGroup(bottomObstaclePrefab, bottomObstacles, bottomYCandidates, bottomRotations,
                       startX: -3.5f, spaceRight: false,
                       ref bottomLastIndex, ref bottomConsecutiveCount);
        }

        private void Update()
        {
            RecycleGroup(topObstacles, topYCandidates, topRotations,
                         movesLeft: true, ref topLastIndex, ref topConsecutiveCount);

            RecycleGroup(bottomObstacles, bottomYCandidates, bottomRotations,
                         movesLeft: false, ref bottomLastIndex, ref bottomConsecutiveCount);
        }

        // ---------------------------------------------------------------
        // [초기 생성]
        // ---------------------------------------------------------------
        private void SpawnGroup(GameObject prefab, List<GameObject> group,
                                float[] yCandidates, float[] rotations,
                                float startX, bool spaceRight,
                                ref int lastIndex, ref int consecutiveCount)
        {
            float currentX = startX;

            for (int i = 0; i < obstacleCount; i++)
            {
                GameObject obj = Instantiate(prefab);
                ApplyPlacement(obj, currentX, yCandidates, rotations, ref lastIndex, ref consecutiveCount);
                group.Add(obj);

                float gap = Random.Range(minGap, maxGap);
                currentX += spaceRight ? gap : -gap;
            }
        }

        // ---------------------------------------------------------------
        // [재배치 체크]
        // ---------------------------------------------------------------
        private void RecycleGroup(List<GameObject> group, float[] yCandidates,
                                  float[] rotations, bool movesLeft,
                                  ref int lastIndex, ref int consecutiveCount)
        {
            foreach (var obj in group)
            {
                bool isOffScreen = movesLeft
                    ? obj.transform.position.x < LeftBound
                    : obj.transform.position.x > RightBound;

                if (isOffScreen)
                    Reposition(obj, group, yCandidates, rotations, movesLeft, ref lastIndex, ref consecutiveCount);
            }
        }

        // ---------------------------------------------------------------
        // [재배치 위치 계산]
        // ---------------------------------------------------------------
        private void Reposition(GameObject target, List<GameObject> group,
                                float[] yCandidates, float[] rotations, bool movesLeft,
                                ref int lastIndex, ref int consecutiveCount)
        {
            float gap = Random.Range(minGap, maxGap);

            if (movesLeft)
            {
                float maxX = -Mathf.Infinity;
                foreach (var obj in group)
                    if (obj.transform.position.x > maxX) maxX = obj.transform.position.x;

                ApplyPlacement(target, maxX + gap, yCandidates, rotations, ref lastIndex, ref consecutiveCount);
            }
            else
            {
                float minX = Mathf.Infinity;
                foreach (var obj in group)
                    if (obj.transform.position.x < minX) minX = obj.transform.position.x;

                ApplyPlacement(target, minX - gap, yCandidates, rotations, ref lastIndex, ref consecutiveCount);
            }
        }

        // ---------------------------------------------------------------
        // [위치 + 회전 적용]
        // 같은 라인이 maxConsecutiveSameLine번 연속이면 강제로 다른 라인 선택
        // ---------------------------------------------------------------
        private void ApplyPlacement(GameObject obj, float x,
                                    float[] yCandidates, float[] rotations,
                                    ref int lastIndex, ref int consecutiveCount)
        {
            int index = Random.Range(0, yCandidates.Length);

            if (index == lastIndex && consecutiveCount >= maxConsecutiveSameLine)
                index = (index + 1) % yCandidates.Length;

            consecutiveCount = (index == lastIndex) ? consecutiveCount + 1 : 1;
            lastIndex = index;

            obj.transform.position = new Vector3(x, yCandidates[index], 0);
            obj.transform.rotation = Quaternion.Euler(0, 0, rotations[index]);
        }
    }
}
