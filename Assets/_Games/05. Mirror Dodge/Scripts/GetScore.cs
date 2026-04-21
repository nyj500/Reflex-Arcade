using UnityEngine;

/* ======================================================================
 * [Class]: GetScore
 * [Role] : 장애물이 ScoreWall 콜라이더를 통과하면 점수 추가
 *          GravitySplit의 GetScore.cs와 동일한 역할
 * ====================================================================== */
namespace MirrorDodge
{
    public class GetScore : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Obstacle"))
                MirrorDodgeManager.Instance.AddScore(1);
        }
    }
}
