using UnityEngine;

/* ======================================================================
 * [Class]: Obstacle
 * [Role] : 개별 장애물의 이동만 담당
 *          위쪽 장애물은 왼쪽으로, 아래쪽 장애물은 오른쪽으로 이동
 * ====================================================================== */
namespace MirrorDodge
{
    public class Obstacle : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 4f;

        // true = 왼쪽 이동 (위쪽 존), false = 오른쪽 이동 (아래쪽 존)
        // 프리팹에서 각 존에 맞게 설정
        [SerializeField] private bool movesLeft = true;

        private void Update()
        {
            if (!MirrorDodgeManager.Instance.IsGameRunning) return;

            Vector2 direction = movesLeft ? Vector2.left : Vector2.right;
            // Space.World: 로컬 좌표계가 아닌 월드 기준으로 이동
            // 180도 회전된 장애물도 방향이 뒤집히지 않음
            transform.Translate(direction * moveSpeed * Time.deltaTime, Space.World);
        }
    }
}
