using UnityEngine;

/* ======================================================================
 * [Class]: BallController
 * [Role] : 공의 중력 반전 및 충돌 처리만 담당
 *          입력은 MirrorDodgeInputHandler 이벤트를 구독하여 수신
 * * [Method Summary]
 * 1. Flip             : 현재 중력 방향을 반전시켜 반대쪽 벽으로 이동
 * 2. OnCollisionEnter2D : 벽(Line) 충돌 시 속도 초기화 / 장애물 충돌 시 게임 오버
 * ====================================================================== */
namespace MirrorDodge
{
    public class BallController : MonoBehaviour
    {
        // 인스펙터에서 위쪽 공이면 true, 아래쪽 공이면 false로 설정
        [SerializeField] private bool isTopBall;

        // 중력 크기 — *=-1 대신 이 값으로 직접 설정해서 인스펙터에서 속도 조절 가능
        // (값이 클수록 빠르게 이동. 라인을 뚫는 현상이 생기면 줄이거나 Rigidbody2D의
        //  Collision Detection을 Continuous로 변경)
        [SerializeField] private float gravityMagnitude = 5f;

        private Rigidbody2D rb;

        // 라인에 붙어있는 상태인지 추적 — false이면 Flip() 차단 (연속 터치 방지)
        private bool isOnLine = true;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
        }

        private void Start()
        {
            // Start에서 구독 → Awake 실행 순서와 무관하게 싱글톤이 확실히 존재한 뒤 연결
            MirrorDodgeInputHandler.Instance.OnTopZoneTapped += HandleTopZoneTap;
            MirrorDodgeInputHandler.Instance.OnBottomZoneTapped += HandleBottomZoneTap;
        }

        private void OnDestroy()
        {
            // 씬 언로드 시 구독 해제 → 이벤트에 죽은 참조가 남는 것을 방지
            if (MirrorDodgeInputHandler.Instance == null) return;
            MirrorDodgeInputHandler.Instance.OnTopZoneTapped -= HandleTopZoneTap;
            MirrorDodgeInputHandler.Instance.OnBottomZoneTapped -= HandleBottomZoneTap;
        }

        // ----------------------------------------------------------------------
        // [Input 이벤트 핸들러]
        // ----------------------------------------------------------------------

        private void HandleTopZoneTap()
        {
            // 위쪽 공만 반응 — 아래쪽 공은 이 이벤트를 무시
            if (isTopBall) Flip();
        }

        private void HandleBottomZoneTap()
        {
            // 아래쪽 공만 반응 — 위쪽 공은 이 이벤트를 무시
            if (!isTopBall) Flip();
        }

        // ----------------------------------------------------------------------
        // [공 동작]
        // ----------------------------------------------------------------------

        private void Flip()
        {
            if (!MirrorDodgeManager.Instance.IsGameRunning) return;

            // 라인에 닿지 않은 이동 중 상태면 입력 무시 → 연속 터치로 공중에 머무는 현상 방지
            if (!isOnLine) return;

            isOnLine = false;

            // 이전 속도를 먼저 제거하지 않으면 반전 직후 관성이 남아 의도와 다르게 움직임
            rb.linearVelocity = Vector2.zero;

            // 현재 중력 방향 반대로 고정 크기 설정
            // *=-1 대신 이렇게 하면 인스펙터에서 gravityMagnitude 하나로 속도 조절 가능
            rb.gravityScale = rb.gravityScale > 0 ? -gravityMagnitude : gravityMagnitude;
        }

        // ----------------------------------------------------------------------
        // [충돌 처리]
        // ----------------------------------------------------------------------

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Line"))
            {
                // 벽에 닿은 순간 속도를 0으로 → 튀거나 미끄러지지 않고 벽에 딱 붙음
                rb.linearVelocity = Vector2.zero;

                // 라인에 안착했으므로 다시 Flip 허용
                isOnLine = true;
            }
            else if (collision.gameObject.CompareTag("Obstacle"))
            {
                HitEffect.Instance.PlayHighlight(collision.transform);
                MirrorDodgeManager.Instance.GameOver();
            }
        }
    }
}
